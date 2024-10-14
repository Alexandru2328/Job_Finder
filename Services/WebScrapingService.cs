using Job_Finder.Models;
using Job_Finder.WebScraping;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.Identity;
using Job_Finder.Services.AutoApplyService;
using System.Security.Claims;

namespace Job_Finder.Services
{
    public class WebScrapingService
    {
        private readonly AppDbContext _context;
        private readonly AutoApplyLinkedinService _autoApplyService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IWebDriver _driver;
        public WebScrapingService(AppDbContext context, AutoApplyLinkedinService autoApplyService,
            UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _autoApplyService = autoApplyService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<AppUser> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _userManager.FindByIdAsync(userId);
        }
        public List<string> getListWords(string keyWordsString)
        { 
            return keyWordsString.Split(',').ToList(); 
        }
        public async Task SearchJobs()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                user.LastDataSearch = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            await Task.Delay(1000);
            var options = new ChromeOptions();
            //options.AddArgument("--headless");
            //options.AddArgument("--start-maximized");
            //options.AddArgument("--disable-dev-shm-usage");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--disable-blink-features=AutomationControlled");
            //options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");


            _driver = new ChromeDriver(options);
            Linkedin linkedin = new Linkedin(_context, _driver);
            EJobs eJobs = new EJobs(_context, _driver);
            BestJobs bestJobs = new BestJobs(_context, _driver);
            Indeed indeed = new Indeed(_context, _driver);

            List<string> keyWords = getListWords(user.KeywordList);


            foreach (string key in keyWords)
            {
                await eJobs.searchEJobs(key);
                await linkedin.searchLinkedin(key);
                await bestJobs.searchBestJobs(key);
                await indeed.SearchIndeed(key);
            }

            _driver.Quit();
        }


        public async Task<List<Job>> GetAllJobs()
        {
            return await _context.Jobs.ToListAsync();
        }
       
    }
}
