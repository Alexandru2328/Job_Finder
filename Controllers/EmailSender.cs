using Microsoft.AspNetCore.Mvc;
using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Job_Finder.Services;
using Job_Finder.Services.AutoApplyService;
using System.Net.Mail;
using Microsoft.SqlServer.Management.Smo;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace Job_Finder.Controllers
{
    public class EmailSender : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailSender(IHttpContextAccessor httpContextAccessor, AppDbContext context,
            UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
        { 
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.Email = user.MailCompany;
                ViewBag.UserSubjecMail = user.UserSubjecMail;
                ViewBag.CVPath = user.CvPath;
                ViewBag.CvName = user.UserCv;
            }
            return View();
        }
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Text = user.MailCompany;

            return View();
        }

        public async Task<IActionResult> EditSubject()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Text = user.UserSubjecMail;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Save(string emailText)
        {
            var user = await _userManager.GetUserAsync(User);
            user.MailCompany = emailText;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> SaveSubject(string emailSubject)
        {
            var user = await _userManager.GetUserAsync(User);
            user.UserSubjecMail = emailSubject;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Preview(string mailAddress, string positions, string companiName)
        {
            var user = await _userManager.GetUserAsync(User);
            string email = "";
            if (user != null)
            {
                email = user.MailCompany;
                if (!string.IsNullOrEmpty(email))
                {
                    email = email.Replace("(companiName)", companiName);
                    email = email.Replace("(positions)", positions);
                }
            }
            string subject = user.UserSubjecMail + companiName;
            ViewBag.MailAddress = mailAddress;
            ViewBag.Email = email;
            ViewBag.Subject = subject;

            return View();
        }

        public async Task<IActionResult> SendMail(string mailAddress, string subject, string email)
        {
            var user = await _userManager.GetUserAsync(User);
            string filePath = user.UserCv; 
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

        public async Task<IActionResult> UploadCv(IFormFile cvFile)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || cvFile == null || cvFile.Length == 0)
            {
                return BadRequest("Invalid user or file.");
            }
            string userFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", user.Id);
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            string filePath = Path.Combine(userFolder, Path.GetFileName(cvFile.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(stream);
            }

            user.CvPath = $"/uploads/{user.Id}/{Path.GetFileName(cvFile.FileName)}";
            user.UserCv = cvFile.FileName;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index"); 
        }
    }
}
