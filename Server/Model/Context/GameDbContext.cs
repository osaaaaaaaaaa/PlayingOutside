using Microsoft.EntityFrameworkCore;
using Server.Model.Entity;

namespace Server.Model.Context
{
    /// <summary>
    /// Context配下にDB毎にクラス作成
    /// </summary>
    public class GameDbContext : DbContext
    {
        // テーブル数分のプロパティを用意する
        public DbSet<User> Users { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        // DBの接続先設定
        //readonly string connectionString = "server=db-ge-04.mysql.database.azure.com;database=realtime_game;user=student;password=Yoshidajobi2023;";
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
