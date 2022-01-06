using ConferenceTracker.Data;
using ConferenceTracker.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConferenceTracker
{
    public class Startup
    {
        private readonly string _allowOrigins = "_allowedOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public string SecretMessage { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            SecretMessage = Configuration["SecretMessage"].Trim();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("ConferenceTracker"));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddCors(options =>
            {
                options.AddPolicy(_allowOrigins, builder =>
                {
                    //this's name of our cors policy and providing what domains will be premitted
                    builder.WithOrigins("http://pluralsight.com");
                });
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddTransient<IPresentationRepository, PresentationRepository>();
            services.AddTransient<ISpeakerRepository, SpeakerRepository>();



        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                context.Database.EnsureCreated();

            //Set Up the Web Application to Use the HSTS Header
            if (env.IsDevelopment())
            {
                logger.LogInformation("Environment is in development");
                app.UseDeveloperExceptionPage().UseDatabaseErrorPage();
            }
            else
            {

                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseCors(_allowOrigins);
            //Redirect HTTP Request to Use HTTPS Instead
            app.UseHttpsRedirection();

            app.UseCookiePolicy();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}