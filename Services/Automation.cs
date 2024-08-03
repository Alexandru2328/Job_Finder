using Job_Finder.Models;
using Job_Finder.Services.AutoApplyService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using System.Threading;
using System.Threading.Tasks;

namespace Job_Finder.Services
{
    public class Automation 
    {
        private readonly AutoApplyBestJobsService _autoApplyBestJobs;
        private readonly AutoApplyEJobs _autoApplyEJobs;
        private readonly AutoApplyLinkedinService _autoApplyLinkedin;
        private readonly AutoApplyIndeed _autoApplyIndeed;
        private readonly WebScrapingService _webScrapingService;
        private readonly ApplicationDbContext _context;
        private IWebDriver _driver;

        public Automation(WebScrapingService webScrapingService, AutoApplyBestJobsService autoApplyBestJobs,
            AutoApplyEJobs autoApplyEJobs, AutoApplyLinkedinService autoApplyLinkedin, AutoApplyIndeed autoApplyIndeed, ApplicationDbContext context)
        {
            _autoApplyBestJobs = autoApplyBestJobs;
            _webScrapingService = webScrapingService;
            _autoApplyEJobs = autoApplyEJobs;
            _autoApplyLinkedin = autoApplyLinkedin;
            _autoApplyIndeed = autoApplyIndeed;
            _context = context;
        }

        public async Task JobFinderProces()
        {
            //await _webScrapingService.SearchJobs();
            await _autoApplyBestJobs.autoApplyBestjobs();
            await _autoApplyEJobs.ApplyEJobs();
            await _autoApplyLinkedin.ApplyLinkedInJobs();
            //await _autoApplyIndeed.ApplyIndeed();
        }
        public async Task JobFinderProces(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                if (job.Platform == "Linkedin")
                {
                    await _autoApplyLinkedin.ApplyLinkedInJobs(id);
                } else if (job.Platform == "BestJobs") 
                {
                    await _autoApplyBestJobs.autoApplyBestjobs(id);
                } else
                {
                     await _autoApplyEJobs.ApplyEJobs(id);
                }
            }
            //await _autoApplyIndeed.ApplyIndeed();
        }
    }
}
