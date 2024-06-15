using Job_Finder.Models;
using Job_Finder.Services;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAppScraping.Controllers
{
    public class WebScrapingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WebScrapingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(Search search)
        {
            IWebDriver driver = new ChromeDriver();
            string url = "https://www.linkedin.com/jobs/search/?currentJobId=3937590388&distance=25&geoId=106670623&keywords="
                + search.Text + "&origin=JOB_SEARCH_PAGE_KEYWORD_HISTORY&refresh=true";

            driver.Navigate().GoToUrl(url);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);
            var lastHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight");

            while (true)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                await Task.Delay(2000);

                var newHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight");
                if (newHeight == lastHeight)
                {
                    break;
                }
                lastHeight = newHeight;
            }

            var nodes = driver.FindElements(By.ClassName("base-card"));
            var contentList = new List<string>();

            foreach (var li in nodes)
            {
                if (li != null)
                {
                    var trackingId = li.GetAttribute("data-entity-urn");
                    var link = li.FindElement(By.ClassName("base-card__full-link"));
                    var title = li.FindElement(By.ClassName("base-search-card__title"));
                    var company = li.FindElement(By.ClassName("base-search-card__subtitle"));
                    var detalis = li.FindElement(By.ClassName("base-search-card__metadata"));

                    Job job = new Job()
                    {
                        Id_Traking = trackingId,
                        Link = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", link),
                        Title = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", title),
                        Company = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", company),
                        Details = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", detalis),
                    };
                    _context.Jobs.AddRange(job);
                    await _context.SaveChangesAsync();
                }
            }
            driver.Quit();
            var jobList = _context.Jobs.ToList();
            return View(jobList);
        }
    }
}
