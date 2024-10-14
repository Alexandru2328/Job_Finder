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
    public class Indeed
    {
        private readonly AppDbContext _context;
        private readonly AutoApplyLinkedinService _autoApplyService;
        private IWebDriver _driver;
        public Indeed(AppDbContext context, IWebDriver driver)
        {
            _context = context;
            _driver = driver;

        }
        private async Task ScrollIndeed()
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
        private async Task saveAdsIndeed()
        {
            try
            {
                var node = _driver.FindElements(By.ClassName("job_seen_beacon"));
                foreach (var li in node)
                {
                    if (li != null)
                    {
                        var link = li.FindElement(By.ClassName("jcs-JobTitle"));
                        string href = link.GetAttribute("href");
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].setAttribute('href', arguments[1]);", link, href);
                        var titleElement = li.FindElement(By.ClassName("jobTitle"));
                        var trackingId = link.GetAttribute("id");
                        var companyEl = li.FindElement(By.ClassName("css-63koeb"));
                        var detailsEl = li.FindElement(By.ClassName("css-qvloho"));

                        Job job = new Job()
                        {
                            Id_Traking = trackingId,
                            Link = href,
                            Title = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", titleElement),
                            Company = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", companyEl),
                            Details = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return arguments[0].outerHTML;", detailsEl),
                            Platform = "Indeed",
                            Data = DateTime.Now,
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
            }
            catch (Exception ex) { }
        }
        public async Task SearchIndeed(string search)
        {
            string url = "https://ro.indeed.com/jobs?q=" + search + "&l=Romania&fromage=14&vjk=c40b2d58a9f7ba4f";
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);
            try
            {
                while (true)
                {
                    await Task.Delay(600);
                    try
                    {
                        var close = _driver.FindElement(By.ClassName("css-yi9ndv"));
                        close.Click();
                    }
                    catch (Exception ex) { }
                    await ScrollIndeed();
                    await Task.Delay(600);
                    await saveAdsIndeed();
                    try
                    {
                        var nextBtn = _driver.FindElement(By.CssSelector($"[aria-label='Next Page']"));
                        nextBtn.Click();
                    }
                    catch (NoSuchElementException ex)
                    {
                        break;
                    }
                }

            }
            catch (Exception ex) { }
        }

    }
}