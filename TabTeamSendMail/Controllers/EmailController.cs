using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using MailKit.Security;
using TabTeamSendMail.Model;

namespace TabTeamSendMail.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _config;
        public EmailController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailWithAttachments([FromForm] EmailModel model, List<IFormFile> attachments)
        {
            string htmlContent = System.IO.File.ReadAllText("wwwroot/HTML/Templates/new_invoice.html");
            using (var client = new SmtpClient())
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender Name", _config["SmtpSettings:Sender"]));
                message.To.Add(new MailboxAddress("Recipient Name", model.Recipient));

                var attachmentNames = attachments.Select(x => x.FileName.Replace(".pdf", ""));
                var subject = "Deine Rechnung von TabTeam! - " + string.Join(", ", attachmentNames);
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlContent;
                foreach (var file in attachments)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        bodyBuilder.Attachments.Add(file.FileName, fileBytes);
                    }
                }
                message.Body = bodyBuilder.ToMessageBody();
                client.Connect(_config["SmtpSettings:Server"], 587, SecureSocketOptions.StartTls);
                client.Authenticate(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                client.Send(message);
                client.Disconnect(true);
            }
            return Ok();
        }
    }
}
