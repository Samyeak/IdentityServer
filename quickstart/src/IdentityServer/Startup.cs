// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var list = env.EnvironmentName.ToList();
            string IdentityStoreType = System.Environment.GetEnvironmentVariable("IDENTITY_STORE_TYPE");


            var builder = services.AddIdentityServer();
            if (IdentityStoreType == "Postgres")
            {
                const string connectionString = "Server = 127.0.0.1; Port = 5432; Database = IdentityServer; User Id = postgres; Password = badministrator;";

                builder.AddTestUsers(TestUsers.Users)
                    .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
                    })
                    .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
                    })
                    ;
            }
            else
            {
                builder.AddInMemoryClients(Config.Clients)
                    .AddInMemoryApiResources(Config.Apis)
                    .AddInMemoryIdentityResources(Config.Ids)
                    .AddTestUsers(TestUsers.Users)
                    ;
            }

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
        }

        public async void Configure(IApplicationBuilder app)
        {
            await InitializeDatabase(app);

            //if (Environment.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseExceptionHandler(app =>
            {
                app.Run(ExceptionHandler);
            });


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

        private async Task InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!(await context.Clients.AnyAsync()))
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    await context.SaveChangesAsync();
                }

                if (!(await context.IdentityResources.AnyAsync()))
                {
                    foreach (var identity in Config.Ids)
                    {
                        context.IdentityResources.Add(identity.ToEntity());
                    }
                    await context.SaveChangesAsync();
                }

                if (!(await context.ApiResources.AnyAsync()))
                {
                    foreach (var resource in Config.Apis)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    await context.SaveChangesAsync();
                }

            }
        }


        private async Task ExceptionHandler(HttpContext context)
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            Console.WriteLine("---------------------------------- ERROR OCCURED ----------------------------------");
            await context.Response.WriteHtmlAsync(exceptionHandlerPathFeature?.Error?.Message ?? "Looks like an exception");
        }
    }
}
