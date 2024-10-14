using Job_Finder.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Support.UI;
using Job_Finder.Services;
using Job_Finder.Controllers;
using Job_Finder.Services.AutoApplyService;

namespace Job_Finder.WebScraping
{
    public class Linkedin
    {
        private readonly AppDbContext _context;
        private readonly AutoApplyLinkedinService _autoApplyService;
        private IWebDriver _driver;
        public Linkedin(AppDbContext context, IWebDriver driver)
        {
            _context = context;
            _driver = driver;
        }

        private async Task saveJobLinkedin()
        {
            try
            {
                var nodes = _driver.FindElements(By.ClassName("base-card"));
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
                            Link = link.GetAttribute("href"),
                            Title = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", title),
                            Company = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", company),
                            Details = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", details),
                            Platform = "Linkedin",
                            
                        };

                        bool jobExists = await _context.Jobs.AnyAsync(j => j.Id_Traking == trackingId);
                        if (!jobExists)
                        {
                            await Task.Delay(1000);
                            job.Data = DateTime.Now;
                            _context.Jobs.Add(job);
                            await _context.SaveChangesAsync();
                        }
                    }
                   
                }
            } catch (Exception ex) { }
        }

        private async Task scrollLinkedin()
        {
            var lastHeight = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.scrollHeight");
            int nrOfEqual = 0;
            while (true)
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("window.scrollBy(0,950)", "");
                await Task.Delay(200);
                var newHeight = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.scrollHeight");
                if (newHeight == lastHeight)
                {
                    nrOfEqual++;
                    Console.WriteLine(newHeight.ToString());
                }
                else
                {
                    nrOfEqual = 0;
                }
                if (newHeight == lastHeight && nrOfEqual >= 13)
                {
                    Console.WriteLine("a ajuns la final" + nrOfEqual);
                    break;
                }
                lastHeight = newHeight;
            }
        }
        public async Task searchLinkedin(string search)
        {
            try
            {
                string url = "https://www.linkedin.com/jobs/search/?currentJobId=3937590388&distance=25&geoId=106670623&keywords="
                    + search + "&origin=JOB_SEARCH_PAGE_KEYWORD_HISTORY&refresh=true";
                _driver.Navigate().GoToUrl(url);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);
                await Task.Delay(1000);
                var lastHeight = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.scrollHeight");
                int numberOfMatches = 0;
                while (true)
                {
                    await scrollLinkedin();
                    await Task.Delay(500);
                    await saveJobLinkedin();
                    try
                    {
                        var nextBtn = _driver.FindElement(By.ClassName("infinite-scroller__show-more-button"));
                        nextBtn.Click();
                        if (nextBtn != null)
                        {
                            nextBtn.Click();
                            ++numberOfMatches;
                        } else 
                        {
                            await Task.Delay(600);
                            await scrollLinkedin();
                        }
                        Console.WriteLine(numberOfMatches);
                        if (numberOfMatches > 3)
                        {
                            break;
                        }
                    }
                    catch (Exception ex) 
                    {
                        break;
                    }
                }
            }
            catch (Exception ex) { }
        }
    }
}