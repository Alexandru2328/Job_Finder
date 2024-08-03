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

namespace Job_Finder.Services
{
    public class WebScrapingService
    {
        private readonly ApplicationDbContext _context;
        private readonly AutoApplyLinkedinService _autoApplyService ;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IWebDriver _driver;
        public WebScrapingService(ApplicationDbContext context, AutoApplyLinkedinService autoApplyService,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _autoApplyService = autoApplyService;
            
        }

        public async Task SearchJobs()
        {
            _driver = new ChromeDriver();
            Linkedin linkedin = new Linkedin(_context, _driver);
            EJobs eJobs = new EJobs(_context, _driver);
            BestJobs bestJobs = new BestJobs(_context, _driver);
            Indeed indeed = new Indeed(_context, _driver);
            List<string> keyWords = new List<string>{"junior web developer", "software developer",
                ".Net Developer"};
            foreach (string key in keyWords)
            {
                await eJobs.searchEJobs(key);
                await linkedin.searchLinkedin(key);
                await bestJobs.searchBestJobs(key);
                await indeed.SearchIndeed(key);
            }
        }

        public async Task<List<Job>> GetAllJobs()
        {
            return await _context.Jobs.ToListAsync();
        }
       
    }
}
