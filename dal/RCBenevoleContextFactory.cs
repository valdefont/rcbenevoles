using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace dal
{
    public class RCBenevoleContextFactory : IDesignTimeDbContextFactory<RCBenevoleContext>
    {
        public const string CONNECTION_STRING = "User ID=rcbenevoles;Password={1};Host={0};Port={2};Database=rcbenevoles;Pooling=true;";

        public RCBenevoleContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<RCBenevoleContext>();
            builder.UseNpgsql(GetConnectionString());

            var context = new RCBenevoleContext(builder.Options);
            return context;
        }

        public static string GetConnectionString()
        {
            var servername = Environment.GetEnvironmentVariable("APP_DB_SERVER") ?? "localhost";//"host.docker.internal";//
            var passwordname = Environment.GetEnvironmentVariable("APP_DB_PASSWORD") ?? "jw8s0F5";
            var port = Environment.GetEnvironmentVariable("APP_DB_PORT") ?? "5432";

            // Check if running inside Docker
            /*if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            {
                servername = "host.docker.internal";
            }*/
            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            {
                servername = Environment.GetEnvironmentVariable("APP_DB_SERVER") ?? "host.docker.internal";//"localhost";
            }

            return string.Format(CONNECTION_STRING, servername, passwordname, port);
        }
    }
}

