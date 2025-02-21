using JobBot.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;

namespace JobBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddTransient<Service>();

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

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            // Learn more about configuring Swagger/ OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(x =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                x.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddSpaYarp();

            var host = builder.Build();

            host.UseSwagger();
            host.UseSwaggerUI();

            host.UseRouting();
            //host.UseHttpsRedirection();
            host.UseStaticFiles();
            host.UseAuthorization();

            host.MapControllers();

            if (host.Environment.IsDevelopment())
                host.UseSpaYarp();
            else
                host.MapFallbackToFile("index.html");

            var factory = host.Services.GetRequiredService<IDbContextFactory<DataContext>>();
            var context = factory.CreateDbContext();
            context.Database.Migrate();

            host.Run();
        }
    }
}