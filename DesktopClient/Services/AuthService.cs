using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using MySqlConnector;

namespace DesktopClient.Services
{
    internal class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(string connectionString, IPasswordHasher passwordHasher)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<bool> SignInAsync(string login, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return false;

            const string SqlQuery = @"
            SELECT PasswordSalt, PasswordHash, IsActive
            FROM users
            WHERE login = @login
            LIMIT 1;"; //переписать запрос, пока что заглушка

            byte[] saltBytes = null;
            byte[] hashBytes = null;
            int iterations = 100000;
            //bool isActive = false;

            //1) Подключаемся к бд

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using (var cmd = new MySqlCommand(SqlQuery, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);

                await using var rd = await cmd.ExecuteReaderAsync(ct);

                if (await rd.ReadAsync(ct))
                {
                    saltBytes = (byte[])rd["PasswordSalt"];
                    hashBytes = (byte[])rd["PasswordHash"];
                }

                var base64Salt = Convert.ToBase64String(saltBytes);
                var base64Hash = Convert.ToBase64String(hashBytes);
                var ok = _passwordHasher.Verify(password, base64Salt, base64Hash);

                return ok;
            }

        }
    }
}