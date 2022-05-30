using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM_Upack_kz.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CRM_Upack_kz.Jobs
{
    public class DataJob : IJob
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public DataJob(IServiceScopeFactory serviceScopeFactory,ILogger<EmailSender> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<UpackContext>();
                    foreach (User manager in db.Users.ToList())
                    {
                        string text = null;
                        foreach (var app in db.Applications.ToList())
                        {
                            if (app.Manager.Id == manager.Id && app.WaitingDate.ToShortDateString() == DateTime.Now.ToShortDateString())
                            {
                                text += $"№: <b>{app.NumberApplication}</b>" +
                                        $"Вы: <b>{app.Manager.Surname} {app.Manager.Name}</b> &nbsp;" +
                                        $"Дата создания: <b>{app.CreateDate.ToShortDateString()}</b> &nbsp;" +
                                        $"Дата ожидания: <b>{app.WaitingDate.ToShortDateString()}</b> &nbsp;" +
                                        $"Код клиента: <b>{app.Client.CodeClient}</b> &nbsp;" +
                                        $"Клиент: <b>{app.Client.Title}</b> &nbsp;" +
                                        $"Артикул: <b>{app.ArticleNumber}</b> &nbsp;" +
                                        $"Количество: <b>{app.Quantity}</b><br>  ";
                            }
                        }
                        
                        if (!String.IsNullOrEmpty(text))
                        {
                            var emailsender = scope.ServiceProvider.GetService<IEmailSender>(); 
                            await emailsender.SendEmailAsync(manager.Email, $"Внимание, отказы на {DateTime.Now.ToShortDateString()}", text);
                           
                           _logger.LogInformation($"Сообщение для {manager.Surname} {manager.Name} на {manager.Email} email было отправлено успешно!"); 
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.GetBaseException().Message);
            }
            
        }
        
    }
}