using Microsoft.Extensions.Options;
using SchoolMS.DTO;
using SchoolMS.Models;
using SchoolMS.Settings;
using System.ComponentModel;
using System.Net.Http.Headers;

namespace SchoolMS.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly WhatsAppSettings _settings;

        public WhatsAppService(IOptions<WhatsAppSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task<bool> SendMessage(string mobile, string language, string template, List<WhatsAppComponent>? components = null)
        {
            using HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.Token);

            WhatsAppRequest body = new WhatsAppRequest()
            {
                to = mobile,
                template = new Template
                {
                    name = template,
                    language = new Language { code = language }
                }
            };

            if (components is not null) 
            {
                body.template.components = components;
            }

            HttpResponseMessage response =
                await httpClient.PostAsJsonAsync(new Uri(_settings.ApiUrl), body);

            return response.IsSuccessStatusCode;
        }
    }
}
