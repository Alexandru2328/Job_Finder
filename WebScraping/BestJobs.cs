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
namespace Job_Finder.WebScraping
{
    public class BestJobs
    {
        private readonly AppDbContext _context;
        private IWebDriver _driver;
        public BestJobs(AppDbContext context, IWebDriver driver)
        {
            _context = context;
            _driver = driver;

        }
        private async Task scrollBestJobs()
        {
            try
            {
                await Task.Delay(900);
                var lastHeight = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.scrollHeight");
                int nrOfEqual = 0;
                while (true)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    js.ExecuteScript("window.scrollBy(0,850)", "");
                    await Task.Delay(300);
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
                    if (newHeight == lastHeight && nrOfEqual >= 6)
                    {
                        Console.WriteLine("a ajuns la final" + nrOfEqual);
                        break;
                    }
                    lastHeight = newHeight;
                }
            } catch (Exception ex) { }
           
        }

        private async Task<bool> CheckAds(string title)
        {
            List<string> keyWords = new List<string>() {
                "developer", "programator", "web","software", "frontend",
                "software","engineer", ".net", "c#", "javascript", "it", "junior"
            };
            string thisTitle = title.ToLower();
            foreach (string word in keyWords)
            {
                if (thisTitle.Contains(word)) 
                {
                    return true;
                }
            }
            return false;
        }
        private async Task saveAdsBestJobs()
        {
            try
            {
                var node = _driver.FindElements(By.ClassName("job-card"));
                foreach (var li in node)
                {
                    if (li != null)
                    {
                        var link = li.FindElement(By.CssSelector(".stretched-link"));
                        string href = link.GetAttribute("href");
                        var titleElement = li.FindElement(By.CssSelector("h2.h6.truncate-2-line strong"));
                        var trackingId = li.GetAttribute("data-id");
                        var companyEl = li.FindElement(By.CssSelector(".h6.text-muted.text-truncate.py-2"));
                        var companyNameElement = companyEl.FindElement(By.TagName("small"));
                        var detailsEl = li.FindElement(By.ClassName("card-footer"));

                        Job job = new Job()
                        {
                            Id_Traking = trackingId,
                            Link = href,
                            Title = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", titleElement),
                            Company = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", companyNameElement),
                            Details = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", detailsEl),
                            Platform = "BestJobs",
                            Data = DateTime.Now
                        };

                        bool jobExists = await _context.Jobs.AnyAsync(j => j.Id_Traking == trackingId);
                        bool isSuitable = await CheckAds(job.Title);
                        if (!jobExists && isSuitable)
                        {
                            await Task.Delay(1000);
                            job.Data = DateTime.Now;
                            _context.Jobs.Add(job);
                            await _context.SaveChangesAsync();

                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
        public async Task searchBestJobs(string search)
        {
            string url = "https://www.bestjobs.eu/ro/locuri-de-munca-in-bucuresti/fara-experienta,entry-0-2-ani/" + search;
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);
            await Task.Delay(500);
            try
            {
                var close = _driver.FindElement(By.XPath("//button[@aria-label='Close']"));
                close.Click();
            }
            catch (Exception ex) { }

            while (true)
            {
                await scrollBestJobs();
                await Task.Delay(400);
                await saveAdsBestJobs();
                try
                {
                    var nextPage = _driver.FindElement(By.CssSelector("button.js-show-next-page"));
                    nextPage.Click();
                }
                catch (NoSuchElementException ex)
                {
                    break;
                }
            }
        }
    }
}