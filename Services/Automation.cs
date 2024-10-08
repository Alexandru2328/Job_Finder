using Job_Finder.Models;
using Job_Finder.Services.AutoApplyService;
using Microsoft.AspNetCore.SignalR;
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
        private readonly AppDbContext _context;
        private readonly AutomationProgress _progress;
        private IWebDriver _driver;
        private int Percentage { get; set; }


        public Automation(WebScrapingService webScrapingService, AutoApplyBestJobsService autoApplyBestJobs,
            AutoApplyEJobs autoApplyEJobs, AutoApplyLinkedinService autoApplyLinkedin, AutoApplyIndeed autoApplyIndeed,
            AppDbContext context, AutomationProgress progress )
        {
            _autoApplyBestJobs = autoApplyBestJobs;
            _webScrapingService = webScrapingService;
            _autoApplyEJobs = autoApplyEJobs;
            _autoApplyLinkedin = autoApplyLinkedin;
            _autoApplyIndeed = autoApplyIndeed;
            _context = context; 
            _progress = progress;
        }

        public async Task JobFinderProcess()
        {
            try
            {
                await _progress.SetProgress(10);
                await _webScrapingService.SearchJobs();
            }
            catch (Exception ex) { }

            try
            {
                await _progress.SetProgress(20);
                await _autoApplyBestJobs.autoApplyBestjobs();
            }
            catch (Exception ex) { }

            try
            {
                await _progress.SetProgress(40);
                await _autoApplyEJobs.ApplyEJobs();
            }
            catch (Exception ex) { }

            try
            {
                await _progress.SetProgress(60);
                await _autoApplyLinkedin.ApplyLinkedInJobs();
            } catch ( Exception ex ) { }

            try
            {
                await _progress.SetProgress(80);
                //await _autoApplyIndeed.ApplyIndeed();
                await Task.Delay(2000);
                await _progress.SetProgress(100);
            } catch ( Exception ex ) { }
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
