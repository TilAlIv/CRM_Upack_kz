using System.Threading.Tasks;

namespace CRM_Upack_kz.Jobs
{
    public interface IEmailSender
    { 
        Task SendEmailAsync(string email, string subject, string message);
        
    }
}