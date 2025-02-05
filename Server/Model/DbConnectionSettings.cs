//*********************************************************
// DBの接続先設定
// Author:Rui Enomoto
//*********************************************************
using MySqlConnector;

namespace Server.Model
{
    public class DbConnectionSettings
    {
        // DB接続情報
        public static readonly MySqlConnectionStringBuilder connectionBuilder = new MySqlConnectionStringBuilder
        {
#if DEBUG
            Server = "localhost",
            Database = "realtime_game",
            UserID = "jobi",
            Password = "jobi",
#else
            Server = "db-ge-04.mysql.database.azure.com",
            Database = "realtime_game",
            UserID = "student",
            Password = "Yoshidajobi2023",
            SslMode = MySqlSslMode.Required
#endif
        };

        public async Task<MySqlCommand> CreateCommandAsync(string commandText)
        {
            var conn = new MySqlConnection(connectionBuilder.ConnectionString);
            await conn.OpenAsync();

            // SQL作成
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = commandText;
            return command;
        }
    }
}
