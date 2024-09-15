using SchoolMS.DTO;
using SchoolMS.Models;
using System.ComponentModel;

namespace SchoolMS.Services
{
    public interface IWhatsAppService
    {
        Task<bool> SendMessage(string mobile, string language, string template, List<WhatsAppComponent>? components = null);
    }
}
