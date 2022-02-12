using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using EmployeeManagement.Security;

namespace EmployeeManagement
{
    public class Startup

    {
        private IConfiguration _config;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
                    options.Password.RequiredLength = 10;
                    options.Password.RequiredUniqueChars = 3;
                    options.SignIn.RequireConfirmedEmail = true;
                    options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                }
                ).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders().AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");
            services.AddRouting();
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(_config.GetConnectionString("EmployeeDbConnection")));
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddXmlDataContractSerializerFormatters();
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();


            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireAssertion(context => context.User.IsInRole("Super Admin")
                    || context.User.HasClaim("Delete Role", "true") && context.User.HasClaim("Edit User", "true")));



                //policy => policy.RequireClaim("delete role").RequireClaim("Create Role")
                //.RequireAssertion(context => context.User.IsInRole("Super Admin")));

                options.AddPolicy("EditRolePolicy",
                    policy => policy.AddRequirements(new ManageAdminRolesAndClaims()));
                options.InvokeHandlersAfterFailure = false;
                options.AddPolicy("AdminRolePolicy",
                   policy => policy.RequireRole("admin"));
            });
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "840750449807-o61m86bk3gn9ibru6ogu67lne2q1gkcf.apps.googleusercontent.com";
                options.ClientSecret = "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";
            });
            services.AddAuthentication().AddFacebook(options =>
            {
                options.ClientId = "345628694139972";
                options.ClientSecret = "7811c26776904c2f514c6e28e22c6422";
            });

            services.Configure<CustomEmailConfirmtionTokenProviderOptions>
                (o => o.TokenLifespan = TimeSpan.FromDays(3));




            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeString>();




        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {

                app.UseDeveloperExceptionPage();
            }
            else
            {

                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");


                //app.UseStatusCodePages();

            }
            app.UseDatabaseErrorPage();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}

