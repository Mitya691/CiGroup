using DesktopClient.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.Services
{
    public class SQLRepository : ISQLRepository
    {
        private readonly string _cs;
        public SQLRepository(string connectionString) => _cs = connectionString;

        /// <summary>
        /// Собирает подготовленные данные для прокидывания на UI
        /// </summary>
        public async Task<List<Card>> GetLast30CardsAsync(CancellationToken ct = default)
        {
            using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            var sql = @"SELECT * FROM cards
                        ORDER BY EndTs DESC
                        LIMIT 30;";

            using var cmd = new MySqlCommand(sql, conn) { CommandTimeout = 5 };
            using var r = await cmd.ExecuteReaderAsync(ct);

            var list = new List<Card>();
            while (await r.ReadAsync(ct))
            {
                list.Add(new Card
                {
                    Id = r.GetInt32("Id"),
                    StartTime = r.GetDateTime("StartTs"),
                    EndTime = r.GetDateTime("EndTs"),
                    Weight1 = r.IsDBNull("Weight1") ? (decimal?)null : r.GetDecimal("Weight1"),
                    Weight2 = r.IsDBNull("Weight2") ? (decimal?)null : r.GetDecimal("Weight2"),
                    TotalWeight = r.IsDBNull("TotalWeight") ? (decimal?)null : r.GetDecimal("TotalWeight"),
                    Direction = r.IsDBNull("Direction") ? null : r.GetString("Direction"),
                    SourceSilo = r.IsDBNull("SourceSilo") ? null : r.GetString("SourceSilo"),
                    TargetSilo = r.IsDBNull("TargetSilo") ? null : r.GetString("TargetSilo")
                });
            }
            return list;
        }

        public async Task<List<Card>> GetCardsForInterval(DateTime? filterStart, DateTime? filterStop, CancellationToken ct = default)
        {
            using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            var sql = @"SELECT *
                        FROM cards
                        WHERE StartTs >= @filterStart AND EndTs <= @filterStop
                        ORDER BY EndTs DESC;";

            using var cmd = new MySqlCommand(sql, conn) { CommandTimeout = 5 };
            cmd.Parameters.AddWithValue("@filterStart", filterStart); 
            cmd.Parameters.AddWithValue("@filterStop", filterStop);
            using var r = await cmd.ExecuteReaderAsync(ct);

            var list = new List<Card>();
            while (await r.ReadAsync(ct))
            {
                list.Add(new Card
                {
                    Id = r.GetInt32("Id"),
                    StartTime = r.GetDateTime("StartTs"),
                    EndTime = r.GetDateTime("EndTs"),
                    Weight1 = r.IsDBNull("Weight1") ? (decimal?)null : r.GetDecimal("Weight1"),
                    Weight2 = r.IsDBNull("Weight2") ? (decimal?)null : r.GetDecimal("Weight2"),
                    TotalWeight = r.IsDBNull("TotalWeight") ? (decimal?)null : r.GetDecimal("TotalWeight"),
                    Direction = r.IsDBNull("Direction") ? null : r.GetString("Direction"),
                    SourceSilo = r.IsDBNull("SourceSilo") ? null : r.GetString("SourceSilo"),
                    TargetSilo = r.IsDBNull("TargetSilo") ? null : r.GetString("TargetSilo")
                });
            }
            return list;
        }

        /// <summary>
        /// Делает вставку подготовленных данных в чистую таблицу cards для удобной выборки
        /// </summary>
        /// <param name="lastEndInterval"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task InsertNewCard(DateTime lastEndInterval, CancellationToken ct = default)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            const string sql = @"INSERT INTO cards(TrendID, StartTs, EndTs, SourceSilo, Direction, TargetSilo, Weight1, Weight2)
                                    WITH c AS (
                                        SELECT TrendID, DateSet AS StartTs,
                                            LEAD(DateSet)  OVER (PARTITION BY TrendID ORDER BY DateSet) AS EndTs,
                                            TagValue, LEAD(TagValue) OVER (PARTITION BY TrendID ORDER BY DateSet) AS NextVal
                                        FROM int_archive
                                        WHERE DateSet >= @lastEndInterval - INTERVAL 1 DAY
                                    )

                                    SELECT c.TrendID, c.StartTs, c.EndTs,
                                        t.Name        AS SourceSilo,  
                                        t.Direction   AS Direction,   
                                        NULL          AS TargetSilo,   
                                        SUM(CASE WHEN d.TrendID = 1 THEN d.TagValue ELSE 0 END) AS Weight1,
                                        SUM(CASE WHEN d.TrendID = 2 THEN d.TagValue ELSE 0 END) AS Weight2  
                                    FROM c
                                    LEFT JOIN double_archive d
                                        ON d.DateSet >= c.StartTs
                                        AND d.DateSet <  c.EndTs
                                    LEFT JOIN trends t
                                        ON t.TagID = c.TrendID
                                        WHERE c.TagValue = 1 AND c.NextVal  = 0
                                              AND c.EndTs IS NOT NULL
                                              AND c.EndTs > @lastEndInterval
                                        GROUP BY c.TrendID, c.StartTs, c.EndTs, t.Name, t.Direction
                                        ORDER BY c.EndTs DESC
                                LIMIT 1;";

            await using var cmd = new MySqlCommand( sql, conn) { CommandTimeout = 5};
            cmd.Parameters.AddWithValue("@lastEndInterval", lastEndInterval);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        /// <summary>
        /// Метод обновляет целевой силос в карточке
        /// </summary>
        public async Task UpdateCardTargetSiloAsync(long id, string targetSilo, CancellationToken ct = default)
        {
            await using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            const string sql = @"UPDATE cards
                         SET TargetSilo=@target, UpdatedAt=NOW()
                         WHERE Id=@id;";

            await using var cmd = new MySqlCommand(sql, conn) { CommandTimeout = 5 };
            cmd.Parameters.AddWithValue("@target", targetSilo);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        /// <summary>
        /// Запрос проверяет наличие нового закрытого перегона, который еще не был обработан
        /// </summary>
        public async Task<bool> CheckCompletedIntervalAsync(DateTime lastEndInterval, CancellationToken ct = default)
        {
            using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            var sql = @" WITH e AS (
                            SELECT TrendID,
                            DateSet AS StartTs,
                            LEAD(DateSet)  OVER (PARTITION BY TrendID ORDER BY DateSet) AS EndTs,
                            TagValue,
                            LEAD(TagValue) OVER (PARTITION BY TrendID ORDER BY DateSet) AS NextVal
                            FROM int_archive)
                            SELECT EXISTS (
                            SELECT 1
                            FROM e
                            WHERE TagValue = 1    
                            AND NextVal  = 0       
                            AND EndTs IS NOT NULL    
                            AND EndTs > @lastEndInterval  
                            ) AS HasClosed;";   

            using var cmd = new MySqlCommand(sql, conn) { CommandTimeout = 5 };
            cmd.Parameters.AddWithValue("@lastEndInterval", lastEndInterval);

            using var r = await cmd.ExecuteReaderAsync(ct);

            await r.ReadAsync(ct);

            return r.GetBoolean("HasClosed");
        }
    }
}
