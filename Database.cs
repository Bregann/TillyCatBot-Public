using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatBot
{
    public class Database
    {
        private static readonly string _sqlConnectionString = "Data Source=Database/cats.sqlite;Version=3;PRAGMA journal_mode=WAL";

        public static async Task InsertCatImageToDatabase(string fileName, string url, string subredditName)
        {
            using (var sqlConnection = new SQLiteConnection(_sqlConnectionString))
            using (var sqlCommand = new SQLiteCommand($"INSERT INTO cats (fileName, subRedditName, redditLink, timesPosted) VALUES ('{fileName}', '{subredditName}','{url}', 0)", sqlConnection))
            {
                await sqlConnection.OpenAsync();
                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> HasCatBeenDownloaded(string permalink)
        {
            using (var sqlConnection = new SQLiteConnection(_sqlConnectionString))
            using (var sqlCommand = new SQLiteCommand($"SELECT * FROM cats WHERE redditLink = @name", sqlConnection))
            {
                sqlCommand.Parameters.Add("@name", DbType.String).Value = permalink;

                await sqlConnection.OpenAsync();
                await sqlCommand.ExecuteNonQueryAsync();

                var result = await sqlCommand.ExecuteScalarAsync();

                if (result == DBNull.Value || result == null)
                {
                    return false;
                }

                return true;
            }
        }

        public static async Task<long> GetCatImageRowName()
        {
            using (var sqlConnection = new SQLiteConnection(_sqlConnectionString))
            using (var sqlCommand = new SQLiteCommand($"SELECT COUNT(*) FROM cats", sqlConnection))
            {
                await sqlConnection.OpenAsync();
                await sqlCommand.ExecuteNonQueryAsync();

                var result = await sqlCommand.ExecuteScalarAsync();

                if (result == DBNull.Value || result == null)
                {
                    return 0;
                }
                var fieldCount = long.Parse(result.ToString());
                return fieldCount + 2;
            }
        }

        public static async Task UpdateUsage(string fileName)
        {
            using (var sqlConnection = new SQLiteConnection(_sqlConnectionString))
            using (var sqlCommand = new SQLiteCommand($"UPDATE cats SET timesPosted = timesPosted + 1 WHERE filename='{fileName}'", sqlConnection))
            {
                await sqlConnection.OpenAsync();
                await sqlCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
