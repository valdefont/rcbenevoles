using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace web
{
    public static class DbDataSeeder
    {
        // Astuce pour appeler une méthode de SeedData
        // TODO: Move this code when seed data is implemented in EF 7

        public static void SeedData(this IServiceScopeFactory scopeFactory)
        {
            using (var serviceScope = scopeFactory.CreateScope())
            {
                var dbcontext = serviceScope.ServiceProvider.GetService<dal.RCBenevoleContext>();
                dbcontext.SeedData();
            }
        }
    }
}
