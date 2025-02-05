//*********************************************************
// DBとの接続を管理するContext
// Author:Rui Enomoto
//*********************************************************
using Microsoft.EntityFrameworkCore;
using Server.Model.Entity;

namespace Server.Model.Context
{
    /// <summary>
    /// Context配下にDB毎にクラス作成
    /// </summary>
    public class GameDbContext : DbContext
    {
        // テーブル数分のプロパティを用意する[変数名はテーブル名と同じにする]
        public DbSet<User> Users { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<NGWord> NG_Words { get; set; }

        // DBの接続先設定
        readonly string connectionString = $@"
        server={DbConnectionSettings.connectionBuilder.Server};
        database={DbConnectionSettings.connectionBuilder.Database};
        user={DbConnectionSettings.connectionBuilder.UserID};
        password={DbConnectionSettings.connectionBuilder.Password};";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString,new MySqlServerVersion(new Version(8,0)));
        }
    }
}
