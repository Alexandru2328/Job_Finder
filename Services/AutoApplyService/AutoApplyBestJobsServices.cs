using Job_Finder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;


namespace Job_Finder.Services.AutoApplyService
{
    public class AutoApplyBestJobsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IWebDriver _driver;
        public readonly SaveJobs _saveJobs;

        public AutoApplyBestJobsService(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor, SaveJobs saveJobs)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _saveJobs = saveJobs;
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _userManager.FindByIdAsync(userId);
        }
        public async Task autoApplyBestjobs(int id)
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1500);
            _driver.Navigate().GoToUrl("https://www.bestjobs.eu/ro/login");

            await LoadCookies();
            await Task.Delay(1000);
            bool isLogin = await IsUserLoggedOnBestjobs();
            await Task.Delay(1000);
            if (!isLogin)
            {
                Console.WriteLine("Nu este logat");
                await AuthenticateBestJobs();
            }
            var job = await _context.Jobs.FindAsync(id);
            _driver.Navigate().GoToUrl($"{job.Link}");
            bool applyStatus = await AutoApply();
            await Task.Delay(1000);
            if (applyStatus)
            {
                await _saveJobs.SaveAsAppliedAsync(job);
            }
            else
            {
                await _saveJobs.SaveAsNotAppliedAsync(job);
            }
            SaveCookies();
        }

            public async Task autoApplyBestjobs()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1500);
            _driver.Navigate().GoToUrl("https://www.bestjobs.eu/ro/login");

            await LoadCookies();
            await Task.Delay(1000);
            bool isLogin = await IsUserLoggedOnBestjobs();
            await Task.Delay(1000);
            if (!isLogin)
            {
                Console.WriteLine("Nu este logat");
                await AuthenticateBestJobs();
            }
            await Task.Delay(2000);
            var user = await GetCurrentUserAsync();
            var jobList = await _context.Jobs.ToListAsync();
            var filteredJobs = jobList.Where(j => j.Platform == "BestJobs");
            foreach (var job in filteredJobs)
            {
                Console.WriteLine("am intrat in for");
                if (job.Data > user.LastDataApply)
                {
                    _driver.Navigate().GoToUrl($"{job.Link}");
                    bool applyStatus = await AutoApply();
                    await Task.Delay(1000);
                    if (applyStatus)
                    {
                        await _saveJobs.SaveAsAppliedAsync(job);
                    }
                    else
                    {
                        await _saveJobs.SaveAsNotAppliedAsync(job);
                    }
                }
            }
            Console.WriteLine("test");
            await SaveCookies();
        }

        public async Task<bool> IsUserLoggedOnBestjobs()
        {
            try
            {
                var close = _driver.FindElement(By.XPath("//button[@aria-label='Close']"));
                close.Click();
            }
            catch (NoSuchElementException) { }
            try
            {
                var goToLogin = _driver.FindElement(By.LinkText("Intră în cont"));
                goToLogin.Click();
                return false;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        }

        private async Task<bool> AutoApply()
        {
            try
            {
                var button = _driver.FindElement(By.ClassName("btn-success"));
                button.Click();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        private async Task AuthenticateBestJobs()
        {
            var user = await GetCurrentUserAsync();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);
            try
            {
                var close = _driver.FindElement(By.XPath("//button[@aria-label='Close']"));
                close.Click();
            }
            catch (NoSuchElementException) { }
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1500);
            try
            {
                var emailInput = _driver.FindElement(By.Id("login_form__username"));
                if (emailInput != null)
                {
                    emailInput.SendKeys(user.UserPlatformEmail);
                }
            }
            catch (Exception) { }

            try
            {
                var passwordInput = _driver.FindElement(By.Id("login_form__password"));
                if (passwordInput != null)
                {
                    passwordInput.SendKeys(user.UserPlatformPassword);
                }
            }
            catch (Exception) { }
            await Task.Delay(1000);

            try
            {
                var login = _driver.FindElement(By.ClassName("btn-success"));
                if (login != null)
                {
                    login.Click();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
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
                    Platform = "BestJobs"
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
                if (userCookie.Platform == "BestJobs")
                {
                    var cookie = new Cookie(userCookie.Name, userCookie.Value, userCookie.Domain, userCookie.Path,
                        userCookie.Expiry, userCookie.Secure, userCookie.HttpOnly, userCookie.SameSite);
                    _driver.Manage().Cookies.AddCookie(cookie);

                }
            }
        }

    }
}
