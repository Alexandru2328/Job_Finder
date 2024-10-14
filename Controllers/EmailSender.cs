using Microsoft.AspNetCore.Mvc;
using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Job_Finder.Services;
using Job_Finder.Services.AutoApplyService;
using System.Net.Mail;
using Microsoft.SqlServer.Management.Smo;
using System.Net;

namespace Job_Finder.Controllers
{
    public class EmailSender : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public EmailSender(IHttpContextAccessor httpContextAccessor, AppDbContext context, UserManager<AppUser> userManager)
        { 
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _context = context;
            
        }
        
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.Email = user.MailCompani;
            }
            return View();
        }
        public async Task<IActionResult> Edit()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Save(string emailText)
        {
            var user = await _userManager.GetUserAsync(User);
            user.MailCompani = emailText;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Preview(string mailAddress, string positions, string companiName)
        {
            var user = await _userManager.GetUserAsync(User);
            string email = "";
            if (user != null)
            {
                email = user.MailCompani;
                if (!string.IsNullOrEmpty(email))
                {
                    email = email.Replace("(companiName)", companiName);
                    email = email.Replace("(positions)", positions);
                }
            }
            string subject = "Aplicație pentru o poziție în cadrul companiei " + companiName;
            ViewBag.MailAddress = mailAddress;
            ViewBag.Email = email;
            ViewBag.Subject = subject;

            return View();
        }

        public async Task<IActionResult> SendMailAsync(string mailAddress, string subject, string email)
        {
            var user = await _userManager.GetUserAsync(User);
            string filePath = @"C:\Users\alexa\Downloads\CV Alexandru Cosercar.pdf"; 
            try
            {
                MailMessage mailMessage = new MailMessage(user.UserPlatformEmail, mailAddress);
                mailMessage.From = new MailAddress(user.UserPlatformEmail);
                mailMessage.To.Add(mailAddress);
                mailMessage.Subject = subject;
                mailMessage.Body = email;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(user.UserPlatformEmail, user.UserEmailKey);
                    smtpClient.EnableSsl = true;

                    if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                    {
                        Attachment attachment = new Attachment(filePath);
                        mailMessage.Attachments.Add(attachment);
                    }
                    else
                    {
                        Console.WriteLine("File does not exist: " + filePath);
                    }

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error sending email: " + ex.Message);
            }

            return View();
        }
    }
}
