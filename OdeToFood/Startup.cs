using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OdeToFood.Data;
using OdeToFood.Services;

namespace OdeToFood
{

    public class Startup
    {
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(options =>
            {
                _configuration.Bind("AzureAd", options);
            })
            .AddCookie();


            services.AddSingleton<IGreeter, Greeter>();
            services.AddDbContext<OdeToFoodDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("OdeToFood")));
            services.AddScoped<IRestaurantData, SqlRestaurantData>();
            services.AddMvc();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFoodCom",
                    c => c.WithOrigins("https://localhost:44337/"));
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IGreeter greeter, ILogger<Startup> logger)
        {
            app.UseCsp(options => options.DefaultSources(s => s.Self()));

            app.UseXfo(o => o.Deny());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //This will not allow app to ever run again insecurely if ran once. Just a heads up. :)
            //The preload will make the browser access this page securely from the start
            //if (!env.IsDevelopment()){ app.UseHsts(h => h.MaxAge(days: 365).Preload()); }

            app.UseRewriter(new RewriteOptions().AddRedirectToHttpsPermanent());

            app.UseStaticFiles();

            app.UseCors("AllowFoodCom");

            app.UseNodeModules(env.ContentRootPath);

            app.UseAuthentication();

            app.UseMvc(ConfigureRoutes);

            
        }

        private void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            // /Home/Index/4

            routeBuilder.MapRoute("Default", "{controller=Home}/{action=Index}/{id?}");

        }
    }
}
