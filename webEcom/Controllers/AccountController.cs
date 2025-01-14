using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using webEcom.Data;
using webEcom.Models;
using webEcom.ViewModels;


namespace webEcom.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        // private readonly YourDbContext _context;
        private readonly _IdentityDbContext _context;
        private readonly webEcomDBContext _context1;


        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, _IdentityDbContext context, webEcomDBContext context1)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;
            _context1 = context1;
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<Users> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await GetUserByEmailAsync(model.UserEmail);
            if (user.UserCategory == "Admin")
            {
                model.RememberMe = false;
            }

            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.UserEmail, model.UserPassword, model.RememberMe, false);

                if (result.Succeeded)
                {
                    if (user != null)
                    {
                        // Check the UserCategory
                        if (user.UserCategory == "User")
                        {
                            // Redirect to the home page for "User" category
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            // Handle other categories or redirect to a specific page
                            // For example, redirect to an admin dashboard:
                            return RedirectToAction("Index", "Admin");
                        }
                    }
                    else
                    {
                        // Handle the case where the user is not found
                        ModelState.AddModelError("", "User not found.");
                        return View(model);
                    }
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
                byte[] ProfileImage = await DefaultProfile();

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
                    UserProfile = ProfileImage,
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

        public async Task<byte[]> DefaultProfile()
        {
            byte[] profileImage = null;

            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Dafault_User_Profile.png");

            if (System.IO.File.Exists(imagePath))
            {
                profileImage = await System.IO.File.ReadAllBytesAsync(imagePath);
            }
            else
            {
                // Handle the case where the default image does not exist
                throw new FileNotFoundException("Default profile image not found.");
            }

            return profileImage; // Return the profile image byte array
        }

        public async Task<IActionResult> Logout()
        {
            var currentUser = await userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var productsInCart = _context1.PRODUCT
                    .Where(p => p.ProductUserName == $"{currentUser.UserFirstName} {currentUser.UserSurname}" && p.ProductStatus == "In Cart")
                    .ToList();

                foreach (var product in productsInCart)
                {
                    product.ProductUserTel = null;
                    product.ProductUserName = null;
                    product.ProductUserAddress = null;
                    product.ProductStatus = "Market";
                    product.ProductUserCart = null;
                }

                _context1.UpdateRange(productsInCart);
                await _context1.SaveChangesAsync();
            }

            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> EditUser()
        {
            var user = await userManager.GetUserAsync(User);
            ViewBag.UserFirstName = user.UserFirstName;
            ViewBag.UserSurname = user.UserSurname;
            ViewBag.Email = user.Email;
            ViewBag.UserPhoneNumber = user.UserPhoneNumber;
            ViewBag.UserAddress = user.UserAddress;
            ViewBag.UserProfile = Convert.ToBase64String(user.UserProfile);
            return View();

		}
        public async Task<IActionResult> EditUser_Name()
		{
            var user = await userManager.GetUserAsync(User);
            ViewBag.UserFirstName = user.UserFirstName;
            return View();
		}

        [HttpPost]
        public async Task<IActionResult> EditUser_Name(EditUser_Name model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    user.UserFirstName = model.UserFirstName; // Update the property

                    var result = await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Update successful, handle success
                        return RedirectToAction("EditUser", "Account");
                    }
                    else
                    {
                        // Handle update errors
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Handle the case where the user is not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> EditUser_Surname()
		{
            var user = await userManager.GetUserAsync(User);
            ViewBag.UserSurname = user.UserSurname;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditUser_Surname(EditUser_Surname model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    user.UserSurname = model.UserSurname; // Update the property

                    var result = await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Update successful, handle success
                        return RedirectToAction("EditUser", "Account");
                    }
                    else
                    {
                        // Handle update errors
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Handle the case where the user is not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> EditUser_Email()
		{
            var user = await userManager.GetUserAsync(User);
            ViewBag.Email = user.Email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditUser_Email(EditUser_Email model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    user.Email = model.Email; // Update the property
                    user.UserName = model.Email; // Bug fix

                    var result = await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Update successful, handle success
                        return RedirectToAction("EditUser", "Account");
                    }
                    else
                    {
                        // Handle update errors
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Handle the case where the user is not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> EditUser_PhoneNumber()
		{
            var user = await userManager.GetUserAsync(User);
            ViewBag.UserPhoneNumber = user.UserPhoneNumber;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditUser_PhoneNumber(EditUser_PhoneNumber model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    user.UserPhoneNumber = model.UserPhoneNumber; // Update the property

                    var result = await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Update successful, handle success
                        return RedirectToAction("EditUser", "Account");
                    }
                    else
                    {
                        // Handle update errors
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Handle the case where the user is not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> EditUser_Address()
        {
            var user = await userManager.GetUserAsync(User);
            ViewBag.UserAddress = user.UserAddress;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditUser_Address(EditUser_Address model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user != null)
                {
                    user.UserAddress = model.UserAddress; // Update the property

                    var result = await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Update successful, handle success
                        return RedirectToAction("EditUser", "Account");
                    }
                    else
                    {
                        // Handle update errors
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Handle the case where the user is not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }


        public async Task<IActionResult> EditUser_Profile()
        {
            var user = await userManager.GetUserAsync(User);
            ViewBag.UserProfile = Convert.ToBase64String(user.UserProfile);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditUser_Profile(EditUser_Profile model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                
                if (user != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.UserProfile_IFormFile.CopyToAsync(memoryStream);
                        user.UserProfile = memoryStream.ToArray(); // Store the byte array directly
                    }

                    var result = await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Update successful, handle success
                        return RedirectToAction("EditUser", "Account");
                    }
                    else
                    {
                        // Handle update errors
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Handle the case where the user is not found
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }
    }
}

