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

        /// <summary>
        /// Общая страница всех ползователей
        /// </summary>
        /// <returns>возвращает список ползователей</returns>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View(_db.Users.ToList());
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
        }
        
        /// <summary>
        /// Страница предварительной регистрации пользователя
        /// </summary>
        /// <returns>возвращает предварительную страницу для добавления нового ползователя</returns>
        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                return View(new RegisterViewModel());
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }
        
        /// <summary>
        /// Экшн для добавления нового пользователя
        /// </summary>
        /// <param name="model">контейнер с данными пользователя</param>
        /// <returns>в случае успешного добавления нового пользователя, админ будет перенаправлен на страницу всех обращений</returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
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
                        RoleDisplay = model.Role
                    };
                    
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
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
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }

        /// <summary>
        /// Экшн для предварительной авторизации пользователями, доступна для всех.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                return View();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }
        
        /// <summary>
        /// Экшн для авторизации пользователями, доступна для всех.
        /// </summary>
        /// <param name="model">контейнер с данными (логин и пароль)</param>
        /// <returns>в случае успешной авторизации пользователь будет перенаправлен на список всех обращений</returns>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
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
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }

        /// <summary>
        /// Экшн для выхода из системы
        /// </summary>
        /// <returns>перенаправляет на страницу для авторизации</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }

        /// <summary>
        /// Экшн для предварительного редактирования пользователя
        /// </summary>
        /// <param name="userId">искомый id пользователя</param>
        /// <returns></returns>
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
                    AvatarPath = user.AvatarPath,
                    Role = user.RoleDisplay
                };

                ViewBag.UserId = user.Id;
                ViewBag.BirthDay = BirthDate(user.DateOfBirth);
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
        }

        /// <summary>
        /// Экшн для редактирования пользователя
        /// </summary>
        /// <param name="model">контейнер с данными пользователя</param>
        /// <param name="userId">искомый id пользователя</param>
        /// <returns>в случае успеха, пользователь будет перенаправлен в список пользователей</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model, string userId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string path = Path.Combine(_environment.ContentRootPath, "/Images/Avatars");

                    User user = _userManager.FindByIdAsync(userId).Result;
                    user.Name = model.Name;
                    user.Surname = model.Surname;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.DateOfBirth = model.DateOfBirth;
                    
                    if (user.RoleDisplay != model.Role)
                    {
                        await _userManager.RemoveFromRoleAsync(user, user.RoleDisplay);
                        await _userManager.AddToRoleAsync(user, model.Role);
                        user.RoleDisplay = model.Role;
                    }

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
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
        }

        /// <summary>
        /// Экшн для детального просмотра информации пользователя
        /// </summary>
        /// <param name="userId">искомый id пользователя</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Detail(string userId)
        {
            try
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
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }

        /// <summary>
        /// Экшн по предварительному удалению пользователя
        /// </summary>
        /// <param name="userId">искомый id пользователя</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(string userId)
        {
            try
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
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
        }

        /// <summary>
        /// Экшн для удаления пользователя
        /// </summary>
        /// <param name="userId">искомый id пользователя</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete(string userId)
        {
            try
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
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }

        /// <summary>
        /// Экшн для предварительного изменения пароля администратором пользователю
        /// </summary>
        /// <param name="email">искомый email пользователя</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ChangePassAdmin(string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        ChangePassAdminViewModel model = new ChangePassAdminViewModel()
                        {
                            Id = user.Id,
                            Email = email
                        };

                        return View(model);
                    }

                    return NotFound();
                }

                return NotFound();
}
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }
        
        /// <summary>
        /// Экшн для изменения пароля администратором пользователю
        /// </summary>
        /// <param name="model">контейнер с паролями</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangePassAdmin(ChangePassAdminViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = await _userManager.FindByIdAsync(model.Id);
                    if (user != null)
                    {
                        var passwordValidator = HttpContext.RequestServices.GetService(typeof(IPasswordValidator<User>)) as IPasswordValidator<User>;
                        var passwordHasher = HttpContext.RequestServices.GetService(typeof(IPasswordHasher<User>)) as IPasswordHasher<User>;
                        var result = await passwordValidator.ValidateAsync(_userManager, user, model.NewPassword);
                        if (result.Succeeded)
                        {
                            user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);
                            await _userManager.UpdateAsync(user);
                            
                            return View("SuccessChangePassword");
                        }

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }

        /// <summary>
        /// Экшн для предварительного изменения пароля пользователем
        /// </summary>
        /// <param name="email">искомый email пользователя</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ChangePassUser(string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        ChangePassUserViewModel model = new ChangePassUserViewModel()
                        {
                            Id = user.Id,
                            Email = email
                        };

                        return View(model);
                    }

                    return NotFound();
                }

                return NotFound();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
            
        }

        /// <summary>
        /// Экшн для изменения пароля пользователем
        /// </summary>
        /// <param name="model">искомый email пользователя</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangePassUser(ChangePassUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = await _userManager.FindByIdAsync(model.Id);
                    if (user != null)
                    {
                        IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                        if(result.Succeeded)
                        {
                            return View("SuccessChangePassword");
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Пользователь не найден");
                    }
                }
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
        }

        [NonAction]
        private string BirthDate(DateTime date)
        {
            return $"{date.Year}-{CheckNum(date.Month)}-{CheckNum(date.Day)}";
        }

        [NonAction]
        private string CheckNum(int num)
        {
            if (num < 10)
            {
                return "0" + 1;
            }

            return num.ToString();
        }
    }
}