using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CRM_Upack_kz.Models;
using CRM_Upack_kz.Services;
using CRM_Upack_kz.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;


namespace CRM_Upack_kz.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UpackContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHostEnvironment _environment;
        private readonly FileUploadService _uploadService;
        private ILogger<AccountController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();

        public AccountController(UpackContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, 
            SignInManager<User> signInManager, IHostEnvironment environment, FileUploadService uploadService, ILogger<AccountController> iLogger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _environment = environment;
            _uploadService = uploadService;
            _iLogger = iLogger;
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string path = Path.Combine(_environment.ContentRootPath, "wwwroot/Images/Avatars");
                string avatarPath = $"/Images/Avatars/defaultavatar.jpg";
                
                if (model.File != null)
                {
                    avatarPath = $"/Images/Avatars/{model.File.FileName}";
                    _uploadService.Upload(path, model.File.FileName, model.File);
                }
            
                User user = new User
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    UserName = model.Email,
                    AvatarPath = avatarPath,
                    DateOfBirth = model.DateOfBirth,
                    RoleDisplay = "user"
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "user");
                    return RedirectToAction("Index", "Applications");
                }
                
                foreach (var error in result.Errors)
                {
                    _nLogger.Warn($"Регистрация пользователя: ошибка при регистрации {user.Surname} {user.Name}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Applications");
                    }
                }
                
                ModelState.AddModelError("", "Неправильный логин и (или) пароль");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_db.Users.ToList());
        }
        
        
        [HttpGet]
        public IActionResult Edit(string userId)
        {
            try
            {
                User user = _userManager.FindByIdAsync(userId).Result;

                EditUserViewModel model = new EditUserViewModel
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    PhoneNumber =  user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    AvatarPath = user.AvatarPath
                    
                };

                ViewBag.UserId = user.Id;
                _nLogger.Info($"Открыта страница редактирования профиля пользователя {user.Surname} {user.Name}");
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model, string userId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string path = Path.Combine(_environment.ContentRootPath, "wwwroot/Images/Avatars");

                    User user = _userManager.FindByIdAsync(userId).Result;
                    user.Name = model.Name;
                    user.Surname = model.Surname;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.DateOfBirth = model.DateOfBirth;
                    
                    if (model.File != null)
                    {
                        user.AvatarPath = $"/Images/Avatars/{model.File.FileName}";
                        _uploadService.Upload(path, model.File.FileName, model.File);
                    }

                    await _userManager.UpdateAsync(user);
                    return RedirectToAction("Index");
                }

                return View(model);

            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [HttpGet]
        public IActionResult Detail(string userId)
        {
            if (userId != null)
            {
                User user = _userManager.FindByIdAsync(userId).Result;
                if (user != null)
                {
                    return View(user);
                }

                return Content("Такой пользователь отсутствует");
            }

            return Content("Id пользователя не получен");
        }


        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(string userId)
        {
            if (userId != null)
            {
                User user = _userManager.FindByIdAsync(userId).Result;
                if (user != null)
                {
                    return View(user);
                }

            }

            return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string userId)
        {
            if (userId != null)
            {
                User user = _userManager.FindByIdAsync(userId).Result;
                if (user != null)
                {
                    _db.Entry(user).State = EntityState.Deleted;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                return Content("Такой пользователь отсутствует");
            }

            return Content("Id пользователя не получен");
        }








    }
}