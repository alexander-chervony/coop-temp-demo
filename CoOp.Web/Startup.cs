/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using Amazon.SQS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using CoOp.Domain;
using CoOp.Domain.Queries.InMemory;
using CoOp.Domain.Sqs;
using CoOp.Web.Data;
using CoOp.Web.Extensions;
using CoOp.Web.Identity;
using CoOp.Web.Infrastructure.JsonSerialization;
using CoOp.Web.Models;
using CoOp.Web.Services;
using EventFlow;
using EventFlow.AspNetCore.Extensions;
using EventFlow.AspNetCore.Middlewares;
using EventFlow.Autofac.Extensions;
using EventFlow.Extensions;
using EventFlow.MsSql;
using EventFlow.MsSql.Extensions;
using EventFlow.ReadStores;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace CoOp.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            
            services
                .Configure<CookiePolicyOptions>(
                    options =>
                    {
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                    })
                .Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("ru-RU");

                    var supportedCultures = new[]
                    {
                        new CultureInfo("ru-RU")
                        {
                            DateTimeFormat = {DateSeparator = "."},
                            NumberFormat = {CurrencySymbol = "р."},
                        },
                        new CultureInfo("be-BY")
                        {
                            DateTimeFormat = {DateSeparator = "."},
                            NumberFormat = {CurrencySymbol = "р."}
                        }
                    };

                    options.DefaultRequestCulture = new RequestCulture("ru-RU");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                    options.RequestCultureProviders.Clear();
                    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
                })
                .AddLocalization(
                    options => { options.ResourcesPath = "Resources"; });
                //.AddTransient<IStringLocalizer, StringLocalizer<string>>();

            services.AddTransient<ClaimsPrincipalAccessor>();

            // Configure Compression level
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

            // Add Response compression services
            services
                .AddResponseCompression(options =>
                {
                    options.Providers.Add<GzipCompressionProvider>();
                    options.EnableForHttps = true;
                })
                .AddMvc(
                    options => options.EnableEndpointRouting = false )
                .AddViewLocalization()
                
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                // Maintain property names during serialization. See:
                // https://github.com/aspnet/Announcements/issues/194
                .AddNewtonsoftJson(options =>
                {
                    //var provider = services.BuildServiceProvider();
                    //options.SerializerSettings.ContractResolver = new ConditionalSerializerContractResolver(
                    //    provider.GetService<IAuthorizationService>(), 
                    //    provider.GetService<ClaimsPrincipalAccessor>());
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.SerializationBinder = new DefaultSerializationBinder();
                    options.SerializerSettings.Formatting = Formatting.Indented;

                })
                .AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<Startup>());

            services
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddScoped(it =>
                    it
                        .GetRequiredService<IUrlHelperFactory>()
                        .GetUrlHelper(it.GetRequiredService<IActionContextAccessor>().ActionContext));

            var containerBuilder = new ContainerBuilder();

            EventFlowOptions.New
                .UseAutofacContainerBuilder(containerBuilder)
                .UseMssqlEventStore()
                .ConfigureMsSql(MsSqlConfiguration.New.SetConnectionString(
                  Configuration.GetConnectionString("EventFlowConnection")))
                .AddAspNetCore()
                .Configure(c => c.ThrowSubscriberExceptions = true)
                .AddDefaults(typeof(CoOpAR).GetTypeInfo().Assembly)
                .AddDefaults(typeof(To1CEventsSender).GetTypeInfo().Assembly)
                .UseConsoleLog()
                .UseInMemoryReadStoreFor<CoOpReadModel>();

            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-3.1&tabs=visual-studio
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddVkontakte(o =>
                {
                    o.ClientId = Configuration["Authentication:VK:AppId"];
                    o.ClientSecret = Configuration["Authentication:VK:AppSecret"];
                })
                .AddOdnoklassniki(o =>
                {
                    o.ClientId = Configuration["Authentication:Odnoklassniki:AppId"];
                    o.ApplicationKey = Configuration["Authentication:Odnoklassniki:AppId"];
                    o.ClientSecret = Configuration["Authentication:Odnoklassniki:AppSecret"];
                });


            services.AddDbContext<IdentityDataContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("IdentityDataContextConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false) // todo: set to true and configure emails
                .AddEntityFrameworkStores<IdentityDataContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.SlidingExpiration = true;
                // The TimeSpan after which the authentication ticket stored inside the cookie expires. ExpireTimeSpan is added to the current time to create the expiration time for the ticket. The ExpiredTimeSpan value always goes into the encrypted AuthTicket verified by the server. It may also go into the Set-Cookie header, but only if IsPersistent is set. To set IsPersistent to true, configure the AuthenticationProperties passed to SignInAsync. And by default this is passed only if checkbox "Remember Me" is checked during login.
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            });
            
            services.Configure<IISServerOptions>(options => 
            {
                options.AutomaticAuthentication = false;
            });
            
            ConfigureAuthorization(services);

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddControllersWithViews();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<SqsQueueUrlBuilder>();
            services.AddSingleton<IAmazonSQS>(new SqsClientFactory(Configuration).CreateSqsClient());

            services.AddSingleton<From1CEventsReceiver>();
            
            services.AddAutoMapper(typeof(Startup));

            containerBuilder.Populate(services);

            var container = containerBuilder.Build();
            
            //var msSqlDatabaseMigrator = container.Resolve<IMsSqlDatabaseMigrator>();
            //EventFlowEventStoresMsSql.MigrateDatabase(msSqlDatabaseMigrator);
            
            // todo: add only once along with other coop conf values
            container.Resolve<ICommandBus>().Publish(new AddInflationRateCommand(CoOpId.BlrRealty, 
               0.06, DateTime.Now.AddDays(-60), "BYN"));
            
            var readModelPopulator = container.Resolve<IReadModelPopulator>();
            readModelPopulator.PopulateAsync<CoOpReadModel>(CancellationToken.None).Wait(30000);
            
            var from1CEventsReceiver = container.Resolve<From1CEventsReceiver>();
            from1CEventsReceiver.StartPolling();

            return new AutofacServiceProvider(container);
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Member", policy => policy.RequireClaim("MemberId"));
                options.AddPolicy("ImmovablesAdded", policy => policy.RequireClaim("ImmovablesAdded"));
                options.AddPolicy("ActivePaidMember", policy => policy.RequireClaim("ActivePaidMember"));
                options.AddPolicy("Founder", policy => policy.RequireAssertion(context => new[] {
                        "alexander.chervony@gmail.com", 
                        "sergey.chervony@gmail.com"
                    }.Any(n => n.Equals(context.User.Identity.Name))));
            });
            
            services.AddTransient<ClaimsManager>();

            services.AddTransient<IClaimsTransformation, MemberClaimsTransformer>();
         }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            //app.UseRequestLocalization(locOptions.Value);
            app.UseRequestLocalization();
            
            app.UseResponseCompression();

            app.UseRouting(); // auth
            if (env.IsDevOrLocal())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication(); // auth
            app.UseAuthorization();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>  // auth
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(name: "defaultIndex", pattern: "{controller=Home}/{id?}", defaults: new { action = "Index" });
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(name: "addPayment", pattern: "{controller=CoOp}/{action=AddPayment}/{immovablesId}/{paymentPercent}");
                endpoints.MapControllerRoute(name: "applyForCredit", pattern: "{controller=ApplyForCredit}/{action=Index}");
                endpoints.MapHealthChecks("/health");
            });

            
            app.UseMvc();

            app.UseMiddleware<CommandPublishMiddleware>();
        }
    }
}
