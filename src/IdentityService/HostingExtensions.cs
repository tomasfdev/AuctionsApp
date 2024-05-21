using Duende.IdentityServer;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();   //razorPages to provide login page and views from Identity Server

        builder.Services.AddDbContext<ApplicationDbContext>(options =>      //db service
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()   //ASP.Net Identity service
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                if (builder.Environment.IsEnvironment("Docker"))
                {
                    options.IssuerUri = "identity-svc";
                }

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                //options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients(builder.Configuration))
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>();

        builder.Services.ConfigureApplicationCookie(options =>  //cookie config for http
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        builder.Services.AddAuthentication();
        //.AddGoogle(options =>             //google authentication
        //{
        //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

        //    // register your IdentityServer with Google at https://console.developers.google.com
        //    // enable the Google+ API
        //    // set the redirect URI to https://localhost:5001/signin-google
        //    options.ClientId = "copy client ID from Google here";
        //    options.ClientSecret = "copy client secret from Google here";
        //});

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        //middleware
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}