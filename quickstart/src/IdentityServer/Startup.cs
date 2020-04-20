// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            /* TODO: Didn't work currently over the google end, so postponing it for later :\ */
            //services.AddAuthentication()
            //    .AddGoogle("Google", options => {
            //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //        options.ClientId = "240XXXXXXX-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            //        options.ClientSecret = "";
            //    });
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            const string connectionString = "Server = 127.0.0.1; Port = 5432; Database = myDataBase; User Id = postgres; Password = badministrator;";
            
            services.AddIdentityServer()
                .AddTestUsers(TestUsers.Users)
                .AddConfigurationStore(options => {
                    options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
                })
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
                })
                ;


            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.Ids)
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddTestUsers(TestUsers.Users)
                ;

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
