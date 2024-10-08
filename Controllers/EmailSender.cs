using Microsoft.AspNetCore.Mvc;
using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Job_Finder.Services;
using Job_Finder.Services.AutoApplyService;

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


        public IActionResult Edit()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string MailCompani)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine(MailCompani + "test");
                user.MailCompani = MailCompani;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SandMail(string mailAdress)
        {
            return View();
        }
    }
}
