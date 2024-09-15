using AutoMapper;
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
        private readonly IMapper _mapper;

        public AuthController(IWhatsAppService whatsAppService, IMapper mapper)
        {
            _whatsAppService = whatsAppService;
            _mapper = mapper;
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
        public async Task<IActionResult> SendOTP(SendOTPDto dto)
        {
            var language = Request.Headers["language"].ToString();

            var components = new List<WhatsAppComponent>
            {
                {
                    new WhatsAppComponent
                    {
                        type = "body",
                        parameters = new List<TextMessageParameter>
                        {
                            new TextMessageParameter {type = "text", text = dto.Name},
                            new TextMessageParameter {type = "text", text = dto.Amount},
                            new TextMessageParameter {type = "text", text = dto.RemainingAmount}
                            //new {type = "text", text = dto.Name},
                            //new {type = "text", text = dto.Amount},
                            //new {type = "text", text = dto.RemainingAmount}
                        }
                    }
                }
            };

            var result = await _whatsAppService.SendMessage(dto.Mobile, "send_payment", language ,components);

            if (!result)
                throw new Exception("Something went wrong!");

            return Ok("Sent successfully");
        }
    }
}
