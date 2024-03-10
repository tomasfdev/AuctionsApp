using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(), //provides OpenIdConnect that allow to access Id Token
            new IdentityResources.Profile(),    //provides Profile that allow to access User Profile information
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("AuctionApp", "AuctionApp full access")    //scope
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client  //postman client for development
            {
                ClientId = "postman",
                ClientName = "Postman",
                AllowedScopes = {"openid", "profile", "AuctionApp"},
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
                AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
            }
        };
}
