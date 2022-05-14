﻿using System;
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
        public IActionResult Index(int? page, int count)
        {
            try
            {
                int pageSize = 13;
                int pageNumber = (page ?? 1);
                return View(_db.Applications.ToPagedList(pageNumber, pageSize));
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
                        Client client = await _db.Clients.FirstOrDefaultAsync(c => c.CodeClient == model.CodeClient);
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
        private void UpdateApplication(Application appl, Client client, ApplViewModel model)
        {
            appl.Client = client;
            appl.ClientId = client.Id;
            appl.ArticleNumber = model.ArticleNumber.ToUpper();
            appl.Comment = model.Comment;
            appl.Price = model.Price;
            appl.Quantity = model.Quantity;
            appl.Amount = model.Price * model.Quantity;

            _db.Applications.Update(appl);
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
                Client newClient = new Client() {CodeClient = model.CodeClient.ToUpper(), Title = model.NameClient};
                _db.Clients.Add(newClient);
                _db.SaveChanges();
                SaveApplication(manager, newClient, model);
            }
        }
        
        
        public VirtualFileResult GetVirtualFile(DateTime startDate, DateTime endDate, string managerId, string codeClient)
        {
            List<Application> applications = FilterApplications(startDate, endDate, managerId, codeClient);
            CreateExcelFile(applications);
            
            var filepath = Path.Combine("~/Files", "Выгрузка.xls");
            return File(filepath, "text/plain", "Выгрузка.xls");
        }

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

        [NonAction]
        private void CreateExcelFile(List<Application> applications)
        {
            string file = Path.Combine(_environment.ContentRootPath, "wwwroot/Files/Выгрузка.xls"); 
            Workbook workbook = new Workbook();
            Worksheet worksheet = new Worksheet("Страница 1");
            
            for(int i = 0;i < 100; i++)
                worksheet.Cells[i,0] = new Cell("");
            
            int count = 0;
            foreach (var appl in applications)
            {
                for (int i = 0; i < _db.Applications.ToList().Count; i++)
                {
                    worksheet.Cells.ColumnWidth[0, (ushort)i] = 5000;
                }
                
                worksheet.Cells[count, 0] = new Cell($"{appl.Manager.Surname} {appl.Manager.Name}");
                worksheet.Cells[count, 1] = new Cell(appl.CreateDate.ToShortDateString() + "   " + appl.CreateDate.ToShortTimeString(), @"DD-MM-YYYY");
                if (appl.WaitingDate != new DateTime(0001, 01, 01))
                {
                    worksheet.Cells[count, 2] = new Cell(appl.WaitingDate.ToShortDateString(), @"DD-MM-YYYY");
                }
                worksheet.Cells[count, 3] = new Cell(appl.Client.CodeClient);
                worksheet.Cells[count, 4] = new Cell(appl.Client.Title);
                worksheet.Cells[count, 5] = new Cell(appl.ArticleNumber);
                worksheet.Cells[count, 6] = new Cell(appl.Quantity);
                worksheet.Cells[count, 7] = new Cell(appl.Price);
                worksheet.Cells[count, 8] = new Cell(appl.Amount);
                worksheet.Cells[count, 9] = new Cell(appl.Comment);
                count++;
            }

            workbook.Worksheets.Add(worksheet);
            workbook.Save(file);
        }
    }
}