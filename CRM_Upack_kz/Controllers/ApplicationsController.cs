using System;
using System.Linq;
using System.Threading.Tasks;
using CRM_Upack_kz.Models;
using CRM_Upack_kz.ViewModel;
using CRM_Upack_kz.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Services;
using X.PagedList;

namespace CRM_Upack_kz.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private UpackContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHostEnvironment _environment;
        private readonly FileUploadService _uploadService;
        private ILogger<AccountController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();

        public ApplicationsController(UpackContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IHostEnvironment environment, FileUploadService uploadService, ILogger<AccountController> iLogger)
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
        public IActionResult Index()
        {
            try
            {
                return View(_db.Applications.ToList());
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                ApplViewModel model = new ApplViewModel();
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                return NotFound();
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(ApplViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User manager = await _userManager.FindByEmailAsync(User.Identity.Name);
                    if (manager != null)
                    {
                        Client client = _db.Clients.FirstOrDefault(c => c.CodeClient == model.CodeClient);
                        CheckClient(manager, client, model); 
                        return RedirectToAction("Index");
                    }
                    return Content("Не найден менеджер");
                } 
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                return Content($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
            }
        }


        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoClientAjax(string codeClient)
        {
          var client = await _db.Clients.FirstOrDefaultAsync(c => c.CodeClient == codeClient);
          if (client != null)
          {
              return Content(client.Title);
          }

          return Content("Клиент не найден, заполните клиента");
        }


        [NonAction]
        private void SaveApplication(User manager, Client client, ApplViewModel model)
        {
            Application application = new Application()
            {
                ManagerId = manager.Id,
                Manager = manager,
                ClientId = client.Id,
                Client = client,
                Price = model.Price,
                Quantity = model.Quantity,
                Comment = model.Comment,
                ArticleNumber = model.ArticleNumber.ToUpper(),
                CreateDate = DateTime.Now,
                Amount = model.Price * model.Quantity,
            };
            _db.Applications.Add(application);
            _db.SaveChanges();
        } 
    
        
        [NonAction]
        private void CheckClient(User manager, Client client, ApplViewModel model)
        {
            if (client != null)
            {
                SaveApplication(manager, client, model);
            }
            else
            {
                Client newClient = new Client()
                {
                    CodeClient = model.CodeClient.ToUpper(),
                    Title = model.NameClient
                };

                _db.Clients.Add(newClient);
                _db.SaveChanges();
                SaveApplication(manager, newClient, model);

            }
        }

    }
}