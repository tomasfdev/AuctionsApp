using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityService.Pages.Account.Register
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; }    //bind properties from RegisterViewModel to use them in Index.cshtml.cs

        [BindProperty]
        public bool RegisterSuccess { get; set; }

        //what to do when user gets register page, return register page
        public IActionResult OnGet(string returnUrl)
        {
            Input = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (Input.Button is not "register") return Redirect("~/");  //redirect to home page

            if (ModelState.IsValid)
            {
                var newUser = new ApplicationUser   //create new user
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, Input.Password);   //create new user

                if (result.Succeeded)
                {
                    await _userManager.AddClaimsAsync(newUser, new Claim[]  //add claim
                    {
                        new Claim(JwtClaimTypes.Name, Input.Fullname)   //add claimType.Name
                    });

                    RegisterSuccess = true;
                }
            }

            return Page();
        }
    }
}
