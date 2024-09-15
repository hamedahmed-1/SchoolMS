using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        public async Task<bool> SendMessage(string mobile, string template, string language, List<WhatsAppComponent>? components = null)
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
                    language = new Language { code = language },
                    components = components
                }
            };

            var jsonBody = JsonConvert.SerializeObject(body, Formatting.Indented);
            Console.WriteLine($"Request Payload: {jsonBody}");

            if (components is not null)
            {
                body.template.components = components;
            }


            HttpResponseMessage response =
                await httpClient.PostAsJsonAsync(new Uri(_settings.ApiUrl), body);

            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");


            //Console.WriteLine($"Response Status: {response.StatusCode}");
            //Console.WriteLine($"Response Body: {await response.Content.ReadAsStringAsync()}");

            return response.IsSuccessStatusCode;
        }
    }
}
