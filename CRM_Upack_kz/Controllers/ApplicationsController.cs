using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CRM_Upack_kz.Enums;
using CRM_Upack_kz.Models;
using CRM_Upack_kz.Services;
using CRM_Upack_kz.ViewModels;
using CRM_Upack_kz.ViewModels;
using ExcelLibrary.BinaryFileFormat;
using ExcelLibrary.SpreadSheet;
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
        public IActionResult Index(int? page, string userId, AppSort sort = AppSort.NumApplDesc)
        {
            try
            {
                List<Application> applications = GetSortApplications(sort, userId);
                
                int pageSize = 13;
                int pageNumber = page ?? 1;
                return View(applications.ToPagedList(pageNumber, pageSize));
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
                        Client client = _db.Clients.FirstOrDefault(c => c.CodeClient == model.CodeClient || c.Title == model.NameClient.Trim());
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
        public IActionResult Edit(string id)
        {
            if (id != null)
            {
                Application appl = _db.Applications.FirstOrDefault(a => a.Id == id);
                if (appl != null)
                {
                    ApplViewModel model = new ApplViewModel()
                    {
                        CodeClient = appl.Client.CodeClient,
                        NameClient = appl.Client.Title,
                        ArticleNumber = appl.ArticleNumber,
                        Quantity = (int)appl.Quantity,
                        Price = appl.Price,
                        Comment = appl.Comment
                    };
                    ViewBag.ApplId = appl.Id;
                    ViewBag.StateAppl = appl.AppState;
                    return View(model);
                }
                return Content("Не найдено обращение");
            }
            return Content("Неверный Id");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ApplViewModel model, string applId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Application appl = _db.Applications.FirstOrDefault(ap => ap.Id == applId);
                    if (appl != null)
                    {
                        Client client = await _db.Clients.FirstOrDefaultAsync(c => c.CodeClient == model.CodeClient || c.Title == model.NameClient.Trim());
                        if (client != null)
                        {
                            UpdateApplication(appl, client, model);
                            return RedirectToAction("Index");
                        }
                        
                        Client newClient = new Client() {CodeClient = model.CodeClient.ToUpper(), Title = model.NameClient};
                        _db.Clients.Add(newClient);
                        await _db.SaveChangesAsync();

                        UpdateApplication(appl, newClient, model);
                        return RedirectToAction("Index");
                    }

                    return Content("Обращение не найдено");

                }

                ViewBag.ApplId = applId;
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
                return Content($"Внимание ошибка: {e.TargetSite}: {e.Message} | {e.StackTrace}");
            }
        }

        [HttpPost]
        public IActionResult ChangeDateWaite(string applId, DateTime date, string comment, string state)
        {
            if (applId != null)
            {
                Application appl = _db.Applications.FirstOrDefault(a => a.Id == applId);
                if (appl != null)
                {
                    switch (state)
                    {
                        case "Новая":
                            appl.AppState = AppState.Ожидается;
                            break;
                        
                        case "Ожидается":
                            appl.AppState = AppState.Закрыта;
                            break;
                        
                        case "Закрыта":
                            return Content("Данное обращение было успешно закрыто");
                    }
                    
                    appl.WaitingDate = date;
                    appl.Comment = comment;
                    
                    _db.Applications.Update(appl);
                    _db.SaveChanges();
                    
                    return RedirectToAction("Index");
                }
                
                return Content("По указанному Id не найдено обращения");
            }
            
            return Content("Не найдено Id обращения");
        }
        
        
        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            if (id != null)
            {
                Application appl = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id);
                if (appl != null)
                {
                    return View(appl);
                }
                return Content("Данное обращение не найденно");
            }
            return Content("Неверный Id");
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (id != null)
            {
                Application appl = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id);
                if (appl != null)
                {
                    _db.Entry(appl).State = EntityState.Deleted;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                return Content("Данное обращение не найденно");
            }
            return Content("Неверный Id");
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
        
        /// <summary>
        /// Экшн по поиску информации в обращениях
        /// </summary>
        /// <param name="find">искомый параметр</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Find(string find)
        {
            if (!string.IsNullOrEmpty(find))
            {
                return PartialView("PartialViews/GetFindResultPartialView", FindResult(find).OrderByDescending(a => a.NumberApplication).ToList());
            }

            return Content("null");
        }
        
        /// <summary>
        /// Сохранение обращения
        /// </summary>
        /// <param name="manager">менеджер создавший новое обращение</param>
        /// <param name="client">клиент указанный менеджером</param>
        /// <param name="model">контейнер с данными для создания нового обращения</param>
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
                NumberApplication = _db.CountApplications.Count() + 1
            };
            _db.CountApplications.Add(new CountApplication());
            _db.Applications.Add(application);
            _db.SaveChanges();
        } 
        
        /// <summary>
        /// Обновление обращения при редактировании
        /// </summary>
        /// <param name="appl">само обращение</param>
        /// <param name="client">клиент</param>
        /// <param name="model">контейнер со значениями</param>
        [NonAction]
        private void UpdateApplication(Application appl, Client client, ApplViewModel model)
        {
            client.Title = model.NameClient;
            
            appl.Client = client;
            appl.ClientId = client.Id;
            appl.ArticleNumber = model.ArticleNumber.ToUpper();
            appl.Comment = model.Comment;
            appl.Price = model.Price;
            appl.Quantity = model.Quantity;
            appl.Amount = model.Price * model.Quantity;

            _db.Clients.Update(client);
            _db.Applications.Update(appl);
            _db.SaveChanges();
        }

        /// <summary>
        /// Проверка клиента на его наличие, если клиент существует уже в БД, то он просто сохраняетя, если это он отсутствует,
        /// то создадим нового клиента и сохраним новое обращение.
        /// </summary>
        /// <param name="manager">менеджер создающий новое обращение</param>
        /// <param name="client">проверяемый клиент на его наличие в БД</param>
        /// <param name="model">передаваемый контейнер с параметрами</param>
        [NonAction]
        private void CheckClient(User manager, Client client, ApplViewModel model)
        {
            if (client != null)
            {
                SaveApplication(manager, client, model);
            }
            else
            {
                Client newClient = new Client() {CodeClient = model.CodeClient.ToUpper(), Title = model.NameClient};
                _db.Clients.Add(newClient);
                _db.SaveChanges();
                SaveApplication(manager, newClient, model);
            }
        }
        
        /// <summary>
        /// Экшн для выгрузки отфильтрованного пользователем Excel файла.
        /// </summary>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">конечная дата</param>
        /// <param name="managerId">передаваемый id менеджера</param>
        /// <param name="codeClient">передаваемый код клиента</param>
        /// <returns></returns>
        public VirtualFileResult GetVirtualFile(DateTime startDate, DateTime endDate, string managerId, string codeClient)
        {
            List<Application> applications = FilterApplications(startDate, endDate, managerId, codeClient);
            CreateExcelFile(applications);
            
            var filepath = Path.Combine("~/Files", "Выгрузка.xls");
            return File(filepath, "text/plain", "Выгрузка.xls");
        }

        /// <summary>
        /// Метод для фильтрации обращений по выбранному способу пользователем. 
        /// </summary>
        /// <param name="startDate">Начальная указаная дата польз-ем или же по умолчанию 01.01.0001</param>
        /// <param name="endDate">Конечная дата, если эта дата не указана польз-ем необходимо установить текущюю дату и время + 1 день включительно</param>
        /// <param name="managerId">id менеджера</param>
        /// <param name="codeClient">код клиента</param>
        /// <returns></returns>
        [NonAction]
        private List<Application> FilterApplications(DateTime startDate, DateTime endDate, string managerId, string codeClient)
        {
            List<Application> applications = new List<Application>();
            endDate = endDate == new DateTime(0001, 01, 01) ? DateTime.Now : endDate;
            
            if (managerId != null & codeClient == null)
            {
                applications = _db.Applications
                    .Where(a => a.CreateDate >= startDate && endDate.AddDays(1) >= a.CreateDate)
                    .Where(a => a.Manager.Id == managerId).ToList();
            }
            else if (managerId == null & codeClient != null)
            {
                applications = _db.Applications
                    .Where(a => a.CreateDate >= startDate && endDate.AddDays(1) >= a.CreateDate)
                    .Where(a => a.Client.CodeClient == codeClient).ToList();
            }
            else if(managerId != null & codeClient != null)
            {
                applications = _db.Applications
                    .Where(a => a.CreateDate >= startDate && endDate.AddDays(1) >= a.CreateDate)
                    .Where(a => a.Manager.Id == managerId)
                    .Where(a => a.Client.CodeClient == codeClient).ToList();
            }
            else
            {
                applications = _db.Applications.Where(a => a.CreateDate >= startDate && endDate.AddDays(1) >= a.CreateDate).ToList();
            }
            
            return applications;
        }

        /// <summary>
        /// Метод создает ексель файл, сначала происходит инициализация файла, далее задаётся ширина столбцов.
        /// </summary>
        /// <param name="applications">полученный список обращений</param>
        [NonAction]
        private void CreateExcelFile(List<Application> applications)
        {
            string file = Path.Combine(_environment.ContentRootPath, "wwwroot/Files/Выгрузка.xls"); 
            Workbook workbook = new Workbook();
            Worksheet worksheet = new Worksheet("Страница 1");

            List<string> headers = new List<string>() {"ОБРАЩЕНИЕ", "МЕНЕДЖЕР", "ДАТА СОЗДАНИЯ", "ОЖИДАЕТСЯ", "КОД КЛИЕНТА", "КЛИЕНТ", "АРТИКУЛ", "КОЛ-ВО", "ЦЕНА", "СУММА", "КОМ-ИЙ"};

            for (int i = 0; i < 100; i++)
            {
               worksheet.Cells[i,0] = new Cell("");
               if (i < 11)
               {
                  worksheet.Cells.ColumnWidth[0, (ushort)i] = 5000;
                  worksheet.Cells[0, i] = new Cell(headers[i]);
               }
            }


            int count = 1;
            foreach (var appl in applications.OrderByDescending(a => a.NumberApplication))
            {
                worksheet.Cells[count, 0] = new Cell("№ " + appl.NumberApplication);
                worksheet.Cells[count, 1] = new Cell($"{appl.Manager.Surname} {appl.Manager.Name}");
                worksheet.Cells[count, 2] = new Cell(appl.CreateDate.ToShortDateString() + "   " + appl.CreateDate.ToShortTimeString(), @"DD-MM-YYYY");
                if (appl.WaitingDate != new DateTime(0001, 01, 01))
                {
                    worksheet.Cells[count, 3] = new Cell(appl.WaitingDate.ToShortDateString(), @"DD-MM-YYYY");
                }
                worksheet.Cells[count, 4] = new Cell(appl.Client.CodeClient);
                worksheet.Cells[count, 5] = new Cell(appl.Client.Title);
                worksheet.Cells[count, 6] = new Cell(appl.ArticleNumber);
                worksheet.Cells[count, 7] = new Cell(appl.Quantity);
                worksheet.Cells[count, 8] = new Cell(appl.Price);
                worksheet.Cells[count, 9] = new Cell(appl.Amount);
                worksheet.Cells[count, 10] = new Cell(appl.Comment);
                count++;
            }

            workbook.Worksheets.Add(worksheet);
            workbook.Save(file);
        }
        
        /// <summary>
        /// Метод для сортировки обращений.
        /// </summary>
        /// <param name="sort">способ сортировки</param>
        /// <returns></returns>
        [NonAction]
        private List<Application> GetSortApplications(AppSort sort, string userId)
        {
            ViewBag.NumAppl = sort == AppSort.NumApplAsc ? AppSort.NumApplDesc : AppSort.NumApplAsc;
            ViewBag.Manager = sort == AppSort.ManagerAsc ? AppSort.ManagerDesc : AppSort.ManagerAsc;
            ViewBag.CreateDate = sort == AppSort.CreateDateAsc ? AppSort.CreateDateDesc : AppSort.CreateDateAsc;
            ViewBag.CodeClient = sort == AppSort.CodeClientAsc ? AppSort.CodeClientDesc : AppSort.CodeClientAsc;
            ViewBag.NameClient = sort == AppSort.NameClientAsc ? AppSort.NameClientDesc : AppSort.NameClientAsc;
            ViewBag.ArtNum = sort == AppSort.ArtNumAsc ? AppSort.ArtNumDesc : AppSort.ArtNumAsc;
            ViewBag.Quntity = sort == AppSort.QuntityAsc ? AppSort.QuntityDesc : AppSort.QuntityAsc;
            ViewBag.Price = sort == AppSort.PriceAsc ? AppSort.PriceDesc : AppSort.PriceAsc;
            ViewBag.Amount = sort == AppSort.AmountAsc ? AppSort.AmountDesc : AppSort.AmountAsc;
            
            List<Application> applications = new List<Application>();
            switch (sort)
            {
                case AppSort.NumApplAsc:
                    applications = _db.Applications.OrderBy(c => c.NumberApplication).ToList();
                    ViewBag.sort = AppSort.NumApplAsc;
                    break;
                
                case AppSort.NumApplDesc:
                    applications = _db.Applications.OrderByDescending(c => c.NumberApplication).ToList();
                    ViewBag.sort = AppSort.NumApplDesc;
                    break;

                case AppSort.ManagerAsc:
                    applications = _db.Applications.OrderBy(c => c.Manager.Surname).ToList();
                    ViewBag.sort = AppSort.ManagerAsc;
                    break;

                case AppSort.ManagerDesc:
                    applications = _db.Applications.OrderByDescending(c => c.Manager.Surname).ToList();
                    ViewBag.sort = AppSort.ManagerDesc;
                    break;

                case AppSort.CreateDateAsc:
                    applications = _db.Applications.OrderBy(c => c.CreateDate).ToList();
                    ViewBag.sort = AppSort.CreateDateAsc;
                    break;

                case AppSort.CreateDateDesc:
                    applications = _db.Applications.OrderByDescending(c => c.CreateDate).ToList();
                    ViewBag.sort = AppSort.CreateDateDesc;
                    break;

                case AppSort.CodeClientAsc:
                    applications = _db.Applications.OrderBy(c => c.Client.CodeClient).ToList();
                    ViewBag.sort = AppSort.CodeClientAsc;
                    break;

                case AppSort.CodeClientDesc:
                    applications = _db.Applications.OrderByDescending(c => c.Client.CodeClient).ToList();
                    ViewBag.sort = AppSort.CodeClientDesc;
                    break;

                case AppSort.NameClientAsc:
                    applications = _db.Applications.OrderBy(c => c.Client.Title).ToList();
                    ViewBag.sort = AppSort.NameClientAsc;
                    break;

                case AppSort.NameClientDesc:
                    applications = _db.Applications.OrderByDescending(c => c.Client.Title).ToList();
                    ViewBag.sort = AppSort.NameClientDesc;
                    break;

                case AppSort.ArtNumAsc:
                    applications = _db.Applications.OrderBy(c => c.ArticleNumber).ToList();
                    ViewBag.sort = AppSort.ArtNumAsc;
                    break;

                case AppSort.ArtNumDesc:
                    applications = _db.Applications.OrderByDescending(c => c.ArticleNumber).ToList();
                    ViewBag.sort = AppSort.ArtNumDesc;
                    break;

                case AppSort.QuntityAsc:
                    applications = _db.Applications.OrderBy(c => c.Quantity).ToList();
                    ViewBag.sort = AppSort.QuntityAsc;
                    break;

                case AppSort.QuntityDesc:
                    applications = _db.Applications.OrderByDescending(c => c.Quantity).ToList();
                    ViewBag.sort = AppSort.QuntityDesc;
                    break;

                case AppSort.PriceAsc:
                    applications = _db.Applications.OrderBy(c => c.Price).ToList();
                    ViewBag.sort = AppSort.PriceAsc;
                    break;
                
                case AppSort.PriceDesc:
                    applications = _db.Applications.OrderByDescending(c => c.Price).ToList();
                    ViewBag.sort = AppSort.PriceDesc;
                    break;
                
                case AppSort.AmountAsc:
                    applications = _db.Applications.OrderBy(c => c.Amount).ToList();
                    ViewBag.sort = AppSort.AmountAsc;
                    break;
                
                case AppSort.AmountDesc:
                    applications = _db.Applications.OrderByDescending(c => c.Amount).ToList();
                    ViewBag.sort = AppSort.AmountDesc;
                    break;
            }

            if (!String.IsNullOrEmpty(userId))
            {
                applications = applications.Where(a => a.Manager.Id == userId).OrderByDescending(a => a.NumberApplication).ToList();
            }

            return applications;
        }

        /// <summary>
        /// Метод для поиска информации с различными способами поиска
        /// </summary>
        /// <param name="find">искомый параметр</param>
        /// <returns></returns>
        [NonAction]
        private List<Application> FindResult(string find)
        {
            List<Application> applications = new List<Application>();
            
            if (!string.IsNullOrEmpty(find))
            {
                // жесткий поиск по номеру обращения
                applications = _db.Applications.Where(a => a.NumberApplication.ToString() == find.Trim()).ToList();
                
                // поиск по id менеджера
                if (applications.Any() == false)
                {
                    applications = _db.Applications.Where(a => a.Manager.Id == find & a.AppState == AppState.Новая).ToList();
                }

                // жесткий поиск по фамилии, имени, коду клиента
                if (applications.Any() == false)
                {
                    applications = _db.Applications
                        .Where(a => find.ToLower().Trim().Contains(a.Manager.Surname.ToLower()))
                        .Where(a => find.ToLower().Trim().Contains(a.Manager.Name.ToLower()))
                        .Where(a => find.ToUpper().Trim().Contains(a.Client.CodeClient)).ToList();
                }
                
                // жесткий поиск по фамилии, имени, названию клиента
                if (applications.Any() == false)
                {
                    applications = _db.Applications
                        .Where(a => find.ToLower().Trim().Contains(a.Manager.Surname.ToLower()))
                        .Where(a => find.ToLower().Trim().Contains(a.Manager.Name.ToLower()))
                        .Where(a => find.ToUpper().Trim().Contains(a.Client.Title)).ToList();
                }
                
                // жесткий поиск по фамилии и коду клиента
                if (applications.Any() == false)
                {
                    applications = _db.Applications
                        .Where(a => find.ToLower().Trim().Contains(a.Manager.Surname.ToLower()))
                        .Where(a => find.ToUpper().Trim().Contains(a.Client.CodeClient)).ToList();
                }

                // 1 способ, мягкий поиск
                if (applications.Any() == false)
                {
                    applications = _db.Applications.Where(a =>
                        find.ToLower().Contains(a.Manager.Surname.ToLower()) ||
                        find.ToLower().Contains(a.Manager.Name.ToLower()) ||
                        find.ToLower().Contains(a.NumberApplication.ToString().ToLower()) ||
                        find.ToLower().Contains(a.Client.Title.ToLower()) ||
                        find.ToUpper().Contains(a.Client.CodeClient)).ToList();
                }

                // 2 способ, мягкий поиск
                if (applications.Any() == false)
                {
                    applications = _db.Applications.Where(a => 
                        a.Manager.Surname.ToLower().Contains(find.ToLower().Trim()) ||
                        a.Manager.Name.ToLower().Contains(find.ToLower().Trim()) || 
                        a.NumberApplication.ToString().ToLower().Contains(find.ToLower().Trim()) ||
                        a.Client.Title.ToLower().Contains(find.ToLower().Trim()) ||
                        a.Client.CodeClient.Contains(find.ToUpper().Trim())).ToList();
                }

                return applications;
            }

            return applications;
        }
    }
}