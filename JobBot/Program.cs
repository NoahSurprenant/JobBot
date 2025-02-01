using JobBot.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JobBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            builder.Services.AddDbContextFactory<DataContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection")
                    ?? throw new Exception("Missing DatabaseConnection in connection strings");

                var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);

                sqliteBuilder.DataSource = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sqliteBuilder.DataSource));

                connectionString = sqliteBuilder.ToString();

                //Log.Logger.Information("ConnectionString: {ConnectionString}", connectionString);

                options.UseSqlite(connectionString);
            });

            var host = builder.Build();

            var factory = host.Services.GetRequiredService<IDbContextFactory<DataContext>>();
            var context = factory.CreateDbContext();
            context.Database.Migrate();

            host.Run();
        }
    }
}