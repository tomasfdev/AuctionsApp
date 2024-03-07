using IdentityModel;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityService;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();    //create scope
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); //use scope to get ApplicationDbContext service
        context.Database.Migrate(); //migrate db

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(); //get UserManager service provided by ASP.Net Identity

        if (userMgr.Users.Any()) return;    //check if exists any users in db

        var tomas = userMgr.FindByNameAsync("Tomas").Result;    //check if Tomas exists
        if (tomas == null)  //if null create new user Tomas
        {
            tomas = new ApplicationUser
            {
                UserName = "Tomas",
                Email = "Tomas@email.com",
                EmailConfirmed = true,
            };
            var result = userMgr.CreateAsync(tomas, "Pa$$w0rd123").Result; //create user
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(tomas, new Claim[]{ //add claims for Tomas user
                            new Claim(JwtClaimTypes.Name, "Tomas Ferreira"),
                            //new Claim(JwtClaimTypes.GivenName, "Tomas"),
                            //new Claim(JwtClaimTypes.FamilyName, "Ferreira"),
                            //new Claim(JwtClaimTypes.WebSite, "http://tomas.com"),
                        }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("Tomas created");
        }
        else
        {
            Log.Debug("Tomas already exists");
        }

        var bob = userMgr.FindByNameAsync("bob").Result;
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true
            };
            var result = userMgr.CreateAsync(bob, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            //new Claim(JwtClaimTypes.GivenName, "Bob"),
                            //new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            //new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                            //new Claim("location", "somewhere")
                        }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("bob created");
        }
        else
        {
            Log.Debug("bob already exists");
        }
    }
}