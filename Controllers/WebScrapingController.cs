using Job_Finder.Models;
using Job_Finder.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
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

        public async Task<IActionResult> Index()
        {
            var jobList = await _context.Jobs.ToListAsync();
            return View(jobList);
        }

        public async Task<IActionResult> Search()
        {
            List<string> keyWords = new List<string>{"junior web developer", "front-end web developer",
                 "back-end web developer", "full-stack web developer"};

            foreach (string key in keyWords)
            {
                await SearchLinkedin(key);
                await SearchIndeed(key);
            }
            return RedirectToAction("ShowResult");
        }

        public async Task<IActionResult> ShowResult()
        {
            var jobList = await _context.Jobs.ToListAsync();
            return View(jobList);
        }

        private async Task SearchLinkedin(string search)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); 
            options.AddArgument("--disable-gpu");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try 
                {
                string url = "https://www.linkedin.com/jobs/search/?currentJobId=3937590388&distance=25&geoId=106670623&keywords="
                    + search+ "&origin=JOB_SEARCH_PAGE_KEYWORD_HISTORY&refresh=true";

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

                foreach (var li in nodes)
                {
                    if (li != null)
                    {
                        var trackingId = li.GetAttribute("data-entity-urn");
                        var link = li.FindElement(By.ClassName("base-card__full-link"));
                        var title = li.FindElement(By.ClassName("base-search-card__title"));
                        var company = li.FindElement(By.ClassName("base-search-card__subtitle"));
                        var details = li.FindElement(By.ClassName("base-search-card__metadata"));

                        Job job = new Job()
                        {
                            Id_Traking = trackingId,
                            Link = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", link),
                            Title = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", title),
                            Company = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", company),
                            Details = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", details),
                        };

                        bool jobExists = await _context.Jobs.AnyAsync(j => j.Id_Traking == trackingId);
                        if (!jobExists)
                        {
                                job.Data = DateTime.Now;
                                _context.Jobs.Add(job);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            } catch (Exception ex) { }
        }
    }

        private async Task SearchIndeed(string search)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); 
            options.AddArgument("--disable-gpu");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                string url = "https://ro.indeed.com/jobs?q=" + search + "&l=Romania&from=searchOnDesktopSerp&vjk=a1b8bf2caba6b830";
                driver.Navigate().GoToUrl(url);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);
                var lastHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight");
                try
                {
                    while (true)
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                        await Task.Delay(1000);
                        try
                        {
                            var close = driver.FindElement(By.ClassName("css-yi9ndv"));
                            close.Click();
                        }
                        catch (Exception ex) { }
                        var node = driver.FindElements(By.ClassName("job_seen_beacon"));

                        foreach (var li in node)
                        {
                            if (li != null)
                            {
                                var link = li.FindElement(By.ClassName("jcs-JobTitle"));
                                string href = link.GetAttribute("href");
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('href', arguments[1]);", link, href);
                                var titleElement = li.FindElement(By.ClassName("jobTitle"));
                                var trackingId = link.GetAttribute("id");
                                var companyEl = li.FindElement(By.ClassName("css-63koeb"));
                                var detailsEl = li.FindElement(By.ClassName("css-qvloho"));

                                Job job = new Job()
                                {
                                    Id_Traking = trackingId,
                                    Link = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", link),
                                    Title = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", titleElement),
                                    Company = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", companyEl),
                                    Details = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", detailsEl),
                                    
                                };

                                bool jobExists = await _context.Jobs.AnyAsync(j => j.Id_Traking == trackingId);
                                if (!jobExists)
                                {
                                    job.Data = DateTime.Now;
                                    _context.Jobs.Add(job);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }

                        var newHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight");
                        if (newHeight == lastHeight)
                        {
                            try
                            {
                                var link = driver.FindElement(By.CssSelector($"[aria-label='Next Page']"));
                                link.Click();
                            }
                            catch
                            {
                                break;
                            }
                        }
                        lastHeight = newHeight;
                    }

                } catch (Exception ex) { }
            }
        }
    }
}
