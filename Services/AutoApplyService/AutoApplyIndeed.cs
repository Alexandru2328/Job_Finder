using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace Job_Finder.Services.AutoApplyService
{
    public class AutoApplyIndeed
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IWebDriver _driver;
        private readonly SaveJobs _saveJobs;

        public AutoApplyIndeed(AppDbContext context, UserManager<AppUser> userManager,
            IHttpContextAccessor httpContextAccessor, SaveJobs saveJobs)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _saveJobs = saveJobs;
        }

        public async Task<AppUser> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task ApplyIndeed()
        {

            var options = new ChromeOptions();
            options.AddArgument("--headless");
             options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1500);
            _driver.Navigate().GoToUrl("https://secure.indeed.com/auth?hl=en_US&service=my&co=US&continue=https%3A%2F%2Fwww.indeed.com%2F");

            await LoadCookies();
            await Task.Delay(1000);
            bool isLogin = await IsUserLoggedOnIndeed();
            if (!isLogin)
            {
                Console.WriteLine("nu este logat");
                await AuthenticateIndeed();

            }
            await Task.Delay(2000);
            var user = await GetCurrentUserAsync();
            var jobList = await _context.Jobs.ToListAsync();
            var filteredJobs = jobList.Where(j => j.Platform == "Indeed");
            //foreach (var job in filteredJobs)
            //{
            //    if (job.Data > user.LastDataApply)
            //    {
            //        _driver.Navigate().GoToUrl($"{job.Link}");
            //        bool applyStatus = await AutoApply();
            //        await Task.Delay(1000);
            //        Console.WriteLine(applyStatus);
            //        if (applyStatus)
            //        {
            //            Console.WriteLine();
            //            await _saveJobs.SaveAsApplide(job);
            //        }
            //        else
            //        {
            //            Console.WriteLine("go to not save");
            //            await _saveJobs.SaveAsNotApplide(job);
            //        }
            //    }
            //}
            await SaveCookies();
        }
        public async Task<bool> IsUserLoggedOnIndeed()
        {
            await Task.Delay(1000);
            try
            {
                var cookies = _driver.FindElement(By.ClassName("CookiesPopup__AcceptButton"));
                cookies.Click();
            }
            catch (Exception ex) { }
            await Task.Delay(1000);
            var loginButton = _driver.FindElement(By.ClassName("dd-privacy-allow"));
            try
            {
                return false;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        }
        private async Task AuthenticateIndeed()
        {
            Console.WriteLine("Need to login");
            try
            {
                var inputEmail = _driver.FindElement(By.ClassName("css-1rv184k"));
                inputEmail.SendKeys("alexandru.romario23@gmail.com");
            }
            catch (Exception ex) { }
            await Task.Delay(1000);

            var continueButton = _driver.FindElement(By.CssSelector("button.dd-privacy-allow.css-1u4g79g.e8ju0x50"));
            continueButton.Click();
            await Task.Delay(1000);
            var linkToCode = _driver.FindElement(By.ClassName("css-1518o02"));
            linkToCode.Click();
            //var loginBTn = _driver.FindElement(By.ClassName("css-1ril527"));
            int test = 0;
            while (true)
            {
                ++test;
                await Task.Delay(25000);
                if (test >= 4)
                {
                    break;
                }
            }
        }

        private async Task<bool> AutoApply()
        {

            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("window.scrollBy(0,950)", "");
                await Task.Delay(500);
                int initialWindowCount = _driver.WindowHandles.Count;
                var applyButton = _driver.FindElement(By.CssSelector("button.ApplySection__PrimaryBtn.eButton.eButton--Primary"));
                applyButton.Click();
                await Task.Delay(3500);
                int newWindowCount = _driver.WindowHandles.Count;
                if (newWindowCount > initialWindowCount)
                {
                    Console.WriteLine("Sa deschis un now tab");
                    for (int i = initialWindowCount; i < newWindowCount; i++)
                    {
                        string newWindowHandle = _driver.WindowHandles[i];
                        _driver.SwitchTo().Window(newWindowHandle);
                        _driver.Close();
                    }
                    _driver.SwitchTo().Window(_driver.WindowHandles[0]);
                    return false;
                }

                return true;
            }
            catch (NoSuchElementException ex)
            {
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public async Task SaveCookies()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                throw new InvalidOperationException("User is not logged in.");
            }

            var cookies = _driver.Manage().Cookies.AllCookies;
            foreach (var cookie in cookies)
            {
                var userCookie = new UserCookie
                {
                    UserName = currentUser.UserName,
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Domain = cookie.Domain,
                    Path = cookie.Path,
                    Expiry = cookie.Expiry,
                    Secure = cookie.Secure,
                    HttpOnly = cookie.IsHttpOnly,
                    SameSite = cookie.SameSite.ToString(),
                    Platform = "Indeed"
                };
                _context.UserCookies.Add(userCookie);
            }
            await _context.SaveChangesAsync();
        }

        public async Task LoadCookies()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                throw new InvalidOperationException("User is not logged in.");
            }

            var cookies = await _context.UserCookies.Where(c => c.UserName == currentUser.UserName).ToListAsync();
            foreach (var userCookie in cookies)
            {
                if (userCookie.Platform == "Indeed")
                {
                    var cookie = new Cookie(userCookie.Name, userCookie.Value, userCookie.Domain, userCookie.Path,
                        userCookie.Expiry, userCookie.Secure, userCookie.HttpOnly, userCookie.SameSite);
                    _driver.Manage().Cookies.AddCookie(cookie);
                }
            }
        }
    }
}

