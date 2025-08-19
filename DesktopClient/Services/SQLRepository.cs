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
    internal class SQLRepository
    {
        private readonly string _cs;
        public SQLRepository(string connectionString) => _cs = connectionString;

        public async Task<List<Card>> GetLast30CardsAsync(CancellationToken ct = default)
        {
            using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            // здесь можешь подставить свою «боевую» агрегацию.
            var sql = @"

            WITH events AS (
              SELECT TrendID, DateSet, TagValue,
                     LEAD(DateSet) OVER (PARTITION BY TrendID ORDER BY DateSet) AS EndTime
              FROM int_archive
            ),
            closed AS (
              SELECT TrendID, DateSet AS StartTs, NextDate AS EndTs
              FROM events WHERE TagValue=1 AND NextDate IS NOT NULL
            )
            SELECT

              ROW_NUMBER() OVER (ORDER BY EndTs DESC) AS Id,
              TrendID, StartTs, EndTs,

              (SELECT SUM(TagValue) FROM double_archive d
                WHERE d.TrendID=c.TrendID AND d.DateSet>=StartTs AND d.DateSet<EndTs) AS ResultB1,
              NULL AS ResultB2,

              NULL AS Direction, NULL AS MillSilo, NULL AS UserSelection
            FROM closed c
            ORDER BY EndTs DESC
            LIMIT 30;";

            using var cmd = new MySqlCommand(sql, conn) { CommandTimeout = 5 };
            using var r = await cmd.ExecuteReaderAsync(ct);

            var list = new List<Card>();
            while (await r.ReadAsync(ct))
            {
                list.Add(new Card
                {
                    Id = r.GetInt64("Id"),
                    TrendID = r.GetInt32("TrendID"),
                    StartTime = r.GetDateTime("StartTs"),
                    EndTime = r.GetDateTime("EndTs"),
                    ResultB1 = r.IsDBNull("ResultB1") ? (decimal?)null : r.GetDecimal("ResultB1"),
                    ResultB2 = r.IsDBNull("ResultB2") ? (decimal?)null : r.GetDecimal("ResultB2"),
                    Direction = r.IsDBNull("Direction") ? null : r.GetString("Direction"),
                    MillSilo = r.IsDBNull("MillSilo") ? null : r.GetString("MillSilo"),
                    UserSelection = r.IsDBNull("UserSelection") ? null : r.GetString("UserSelection")
                });
            }
            return list;
        }

        public async Task<List<Card>> GetCardsClosedAfterAsync(DateTime lastEndTime, int take = 100, CancellationToken ct = default)
        {
            using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            var sql = @"
            WITH events AS (
              SELECT TrendID, DateSet, TagValue,
                     LEAD(DateSet) OVER (PARTITION BY TrendID ORDER BY DateSet) AS NextDate
              FROM boolean_archive
              WHERE DateSet >= @windowStart
            ),
            closed AS (
              SELECT TrendID, DateSet AS StartTs, NextDate AS EndTs
              FROM events
              WHERE TagValue=1 AND NextDate IS NOT NULL AND NextDate > @lastEnd
            )
            SELECT
              ROW_NUMBER() OVER (ORDER BY EndTs DESC) AS Id,
              TrendID, StartTs, EndTs,
              (SELECT SUM(TagValue) FROM double_archive d
                WHERE d.TrendID=c.TrendID AND d.DateSet>=StartTs AND d.DateSet<EndTs) AS ResultB1,
              NULL AS ResultB2,
              NULL AS Direction, NULL AS MillSilo, NULL AS UserSelection
            FROM closed c
            ORDER BY EndTs DESC
            LIMIT @take;";

            using var cmd = new MySqlCommand(sql, conn) { CommandTimeout = 5 };
            cmd.Parameters.AddWithValue("@lastEnd", lastEndTime);
            cmd.Parameters.AddWithValue("@windowStart", lastEndTime.AddDays(-1)); // небольшой хвост
            cmd.Parameters.AddWithValue("@take", take);

            using var r = await cmd.ExecuteReaderAsync(ct);
            var list = new List<Card>();
            while (await r.ReadAsync(ct))
            {
                list.Add(new Card
                {
                    Id = r.GetInt64("Id"),
                    TrendID = r.GetInt32("TrendID"),
                    StartTime = r.GetDateTime("StartTs"),
                    EndTime = r.GetDateTime("EndTs"),
                    ResultB1 = r.IsDBNull("ResultB1") ? (decimal?)null : r.GetDecimal("ResultB1"),
                    ResultB2 = r.IsDBNull("ResultB2") ? (decimal?)null : r.GetDecimal("ResultB2")
                });
            }
            return list;
        }

        public async Task UpdateCardSelectionAsync(long id, string userSelection, string userName, CancellationToken ct = default)
        {
            using var conn = new MySqlConnection(_cs);
            await conn.OpenAsync(ct);

            // если итоговая таблица есть:
            var sql = @"UPDATE journal_intervals
                    SET UserSelection=@sel, UpdatedBy=@user, UpdatedAt=NOW()
                    WHERE Id=@id;";
            using var cmd = new MySqlCommand(sql, conn) { CommandTimeout = 5 };
            cmd.Parameters.AddWithValue("@sel", userSelection);
            cmd.Parameters.AddWithValue("@user", userName ?? "ui");
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
