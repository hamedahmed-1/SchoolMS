using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SchoolMS.DTO;
using SchoolMS.Models;
using SchoolMS.Services;
using SchoolMS.Settings;
using System.Net.Http.Headers;

namespace SchoolMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;

        public AuthController(IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        [HttpPost("send-welcome-message")]
        public async Task<IActionResult> SendWelcomeMessage(SendMessageDto dto)
        {
            var language = Request.Headers["language"].ToString();

            var result = await _whatsAppService.SendMessage(dto.Mobile, language, "hello_world");

            if (!result)
                throw new Exception("Something went wrong!");

            return Ok("Sent successfully");
        }


        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPDto dto,[FromQuery] StudentDTO student,[FromQuery] InstallmentDTO installment)
        {
            var language = Request.Headers["language"].ToString();

            var components = new List<WhatsAppComponent>
            {
                {
                    new WhatsAppComponent
                    {
                        type = "body",
                        parameters = new List<object>
                        {
                            new {type = "text", text = student.ParentsName},
                            new {type = "text", text = installment.AmountPaid},
                            new {type = "text", text = installment.RemainingBalance}
                        }
                    }
                }
            };

            var result = await _whatsAppService.SendMessage(student.PhoneNumber, language, "send_pay", components);

            if (!result)
                throw new Exception("Something went wrong!");

            return Ok("Sent successfully");
        }
    }
}
