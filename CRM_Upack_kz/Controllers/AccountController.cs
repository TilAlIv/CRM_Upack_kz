using System.IO;
using System.Threading.Tasks;
using CRM_Upack_kz.Models;
using CRM_Upack_kz.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Services;

namespace CRM_Upack_kz.Controllers
{
    
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

        [Authorize]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [Authorize]
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
                    await _signInManager.SignInAsync(user, false);
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
        
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
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




    }
}