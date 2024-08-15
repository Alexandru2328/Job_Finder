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
    public class EJobs
    {
        private readonly AppDbContext _context;
        private readonly AutoApplyLinkedinService _autoApplyService;
        private IWebDriver _driver;
        public EJobs(AppDbContext context, IWebDriver driver)
        {
            _context = context;
            _driver = driver;
           
        }
        private async Task scrollEJobs()
        {
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
        private async Task saveAdsEJobs()
        {
            try
            {
                var jobCards = _driver.FindElements(By.ClassName("JobCard"));
                foreach (var card in jobCards)
                {
                    var titleElement = card.FindElement(By.ClassName("JCContentMiddle__Title"));
                    var companyElement = card.FindElement(By.ClassName("JCContentMiddle__Info"));
                    var detailsElement = card.FindElement(By.ClassName("JCContentTop__Date"));
                    var auxElement = titleElement.FindElement(By.TagName("a"));
                    string link = auxElement.GetAttribute("href");
                    var job = new Job()
                    {
                        Title = titleElement.Text,
                        Company = companyElement.Text,
                        Details = detailsElement.Text,
                        Link = link,
                        Id_Traking = link,
                        Platform = "Ejobs",
                        Data = DateTime.Now,
                    };
                    bool jobExists = await _context.Jobs.AnyAsync(j => j.Id_Traking == link);
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
            catch (Exception ex) { }
        }
        public async Task searchEJobs(string search)
        {
            string url = "https://www.ejobs.ro/locuri-de-munca/" + search;
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);
            await Task.Delay(500);
            try
            {
                var close = _driver.FindElement(By.ClassName("CookiesPopup__AcceptButton"));
                close.Click();
            }
            catch (Exception ex) { }
            await Task.Delay(500);
            while (true)
            {
                await scrollEJobs();
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("window.scrollBy(0,-300)", "");
                saveAdsEJobs();
                try
                {
                    var nextPageButton = _driver.FindElement(By.CssSelector("a.JLPButton--Next"));
                    nextPageButton.Click();
                }
                catch (NoSuchElementException ex)
                {
                    break;
                } catch (Exception ex) { }
                await Task.Delay(500);
            }

        }
    }
}