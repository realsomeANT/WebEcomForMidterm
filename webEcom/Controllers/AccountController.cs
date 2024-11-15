using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webEcom.Models;
using webEcom.ViewModels;

namespace webEcom.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.UserEmail, model.UserPassword, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email or password is incorrect.");
                    return View(model);
                }
            }
            return View(model);
        }


        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users user = new Users()
                {
                    UserName = model.UserEmail,
                    UserFirstName = model.UserFirstName,
                    UserSurname = model.UserSurname,
                    Email = model.UserEmail,
                    UserPassword = model.UserPassword,
                    UserConfirmPassword = model.UserConfirmPassword,
                    UserAddress = model.UserAddress,
                    UserPhoneNumber = model.UserPhoneNumber,
                    UserCategory = "User",
                };

                var result = await userManager.CreateAsync(user, model.UserPassword);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}

