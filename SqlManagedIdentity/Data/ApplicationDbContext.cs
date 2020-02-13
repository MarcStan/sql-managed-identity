using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace SqlManagedIdentity.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            // adapted from https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-connect-msi#modify-aspnet-core
            var con = Database.GetDbConnection();
            if (con is SqlConnection connection)
            {
                if (connection.ConnectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
                    return; // no MSI needed when using local db

                // force sync 
                connection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
            }
        }
    }
}
