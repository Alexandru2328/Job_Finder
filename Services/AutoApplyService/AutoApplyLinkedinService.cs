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
using Job_Finder.Controllers;


namespace Job_Finder.Services.AutoApplyService
{
    public class AutoApplyLinkedinService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IWebDriver _driver;
        private readonly SaveJobs _saveJobs;
        private readonly NotificationServices _notification;


        public AutoApplyLinkedinService(AppDbContext context, UserManager<AppUser> userManager,
            IHttpContextAccessor httpContextAccessor, SaveJobs saveJobs, NotificationServices notification)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _saveJobs = saveJobs;
            _notification = notification;
        }



        public async Task<AppUser> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _userManager.FindByIdAsync(userId);
        }
        public async Task ApplyLinkedInJobs(int id)
        {
            var user = await GetCurrentUserAsync();
            var options = new ChromeOptions();
            //options.AddArgument("--headless");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1500);
            _driver.Navigate().GoToUrl("https://www.linkedin.com/home");
            await Task.Delay(1000);
            bool isLogin = await IsUserLoggedOnLinkedIn();
            if (!isLogin && user.AutoAplly)
            {
                await AuthenticateLinkedIn();
            }
            await Task.Delay(9000);
            var job = await _context.Jobs.FindAsync(id);
            _driver.Navigate().GoToUrl($"{job.Link}");
            bool applyStatus = await AutoApply();
            await Task.Delay(1000);
            if (applyStatus)
            {
                await _saveJobs.SaveAsAppliedAsync(job);
                Console.WriteLine("sa salvat");
            }
            else
            {
                //await _saveJobs.SaveAsNotAppliedAsync(job);
            }
            _driver.Close();

            //await SaveCookies();
        }

        public async Task ApplyLinkedInJobs()
        {
            var user = await GetCurrentUserAsync();
            var options = new ChromeOptions();
            //options.AddArgument("--headless");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            options.AddArgument("--start-minimaiz");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1500);
            _driver.Navigate().GoToUrl("https://www.linkedin.com/home");
            await Task.Delay(1000);
            await LoadCookies();
            await Task.Delay(2000);
            bool isLogin = await IsUserLoggedOnLinkedIn();
            if (!isLogin && user.AutoAplly)
            {
                await AuthenticateLinkedIn();
            }
            else if (! isLogin)
            {
                //await DeleteCookei();
                await SaveUserCookei();
                await LoadCookies();
            }
            await Task.Delay(2000);
            var jobList = await _context.Jobs.Where(j => j.Platform == "Linkedin").ToListAsync();
            var filteredJobs = await _context.Jobs.OrderByDescending(j => j.Data)
                    .ToListAsync();
            foreach (var job in filteredJobs)
            {
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
                        await _notification.CreateNotification(user, job);
                    }
                }
            }
            _driver.Close();
            ////await SaveCookies();
        }

        private async Task AutoCompletationLinkedin()
        {
            var user = await GetCurrentUserAsync();
             //<= insert text to input element =>
                var input = _driver.FindElements(By.TagName("input"));
                foreach (var inp in input)
                {
                    try
                    {
                        string getInputText = inp.GetAttribute("value");
                        if (getInputText.IsNullOrEmpty())
                        {
                            inp.SendKeys(user.DomainExperience.ToString());
                        }

                    } catch (WebDriverArgumentException ex){ }
                }
                await Task.Delay(500);
             try 
            {
            
            } catch (NoSuchElementException ex) {}
            // <= insert text to textarea element =>
            try
            {
                var textareaElement = _driver.FindElements(By.TagName("textarea"));
                foreach (var textarea in textareaElement)
                {
                    string textareaText = textarea.GetAttribute("value");
                    if (textareaText.IsNullOrEmpty())
                    {
                        textarea.SendKeys(user.Message);
                    }
                }
            } catch (NoSuchElementException ex) { }
            // <=  pres ckeck box =>
            try
            {
                var radioButtons = _driver.FindElements(By.CssSelector("label[data-test-text-selectable-option__label='Yes']"));
                foreach (var radioButton in radioButtons)
                {
                    await Task.Delay(1500);
                    radioButton.Click();
                }
            } catch (NoSuchElementException ex) { }
            // <= select option for select =>
            try
            {
                List<string> keywords = new List<string> { "romania", "yes", "da", "male", "intermediate", "yes, i am a citizen.", "LinkedIn" };

                var selectElements = _driver.FindElements(By.TagName("select"));
                foreach (var select in selectElements)
                {
                    var selectElement = new SelectElement(select);

                    bool optionFound = false;
                    foreach (var option in selectElement.Options)
                    {
                        foreach (var keyword in keywords)
                        {
                            if (option.Text.ToLower() == keyword || option.GetAttribute("value").ToLower() == keyword)
                            {
                                option.Click();
                                optionFound = true;
                                break;
                            }
                        }
                        if (optionFound)
                        {
                            break;
                        }
                    }
                }

            } catch (NoSuchElementException ex) { }
            // <= I Agree Terms & Conditions =>
            int nrOfTims = 0;
            try
            {
           
                var label = _driver.FindElement(By.CssSelector("label[data-test-text-selectable-option__label]"));
                label.Click();
                var erorrMessge = _driver.FindElement(By.ClassName("artdeco-inline-feedback__message"));
                while (erorrMessge.Displayed)
                {
                    Console.WriteLine("need to be select I Agree Terms & Conditions ");
                    await Task.Delay(2000);
                    label.Click();
                    ++nrOfTims;
                    if(nrOfTims == 10)
                    {
                        break ;
                    }
                }

            }
            catch (NoSuchElementException ex){ }
            await Task.Delay(1000);
            try
            {
                var nextStep = _driver.FindElement(By.XPath("//*[@aria-label='Examinați candidatura dvs.']"));
            } catch (Exception ex) { }
        }

        public async Task<bool> IsUserLoggedOnLinkedIn()
        {
            try
            {
                var loginButton = _driver.FindElement(By.ClassName("nav__button-secondary"));
                if (loginButton != null || loginButton.Displayed)
                {
                    return false;
                }
                return true;
            }
            catch (Exception EX)
            {
                return true;
            }
        }


        private async Task AuthenticateLinkedIn()
        {
            var user = await GetCurrentUserAsync();
            try
            {
                var applyBtn = _driver.FindElement(By.ClassName("nav__button-secondary"));
                applyBtn.Click();
                await Task.Delay(500);
            }
            catch (Exception) { }

            try
            {
                var goToLogin = _driver.FindElement(By.LinkText("Intră în cont"));
                goToLogin.Click();
            }
            catch (Exception) { }

            try
            {
                var emailInput = _driver.FindElement(By.Id("username"));
                emailInput.SendKeys(user.UserPlatformEmail);
            }
            catch (Exception) { }

            try
            {
                var passwordInput = _driver.FindElement(By.Id("password"));
                passwordInput.SendKeys(user.UserPlatformPassword);
            }
            catch (Exception) { }

            try
            {
                var login = _driver.FindElement(By.ClassName("btn__primary--large"));
                login.Click();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during login: " + ex.Message);
            }

            await Task.Delay(500);
        }


        private async Task<bool> AutoApply()
        {
            await Task.Delay(2500);
            var applyBtn = _driver.FindElements(By.CssSelector(".jobs-apply-button.artdeco-button.artdeco-button--3.artdeco-button--primary.ember-view"));
            //int initialWindowCount = _driver.WindowHandles.Count;
            int isNotFinde = 0;
            try
            {
                var notAccepted = _driver.FindElement(By.ClassName("artdeco-inline-feedback__icon"));
                if (notAccepted.Displayed) 
                {
                    Console.WriteLine("Not accepted");
                    return true;
                }

            } catch (Exception ex) { }
            // <= Pres Apply button =>
            foreach (var ele in applyBtn)
            {

                try
                {
                    var role = ele.GetAttribute("role");
                    await Task.Delay(1500);
                    if (role == "link")
                    {
                        Console.WriteLine("Nu este potrivit");
                       return false;
                    } else
                    {
                        ele.Click();
                        ++isNotFinde;
                    }
                }
                catch (NoSuchElementException ex)
                {
                }
                catch (ElementNotInteractableException et) { }
            }
            Console.WriteLine(isNotFinde);         
            if (isNotFinde == 0)
            {
                Console.WriteLine("nu se gaseste butonul");
                return true;
            }

            List<string> name = new List<string>()
            {
                "Continue to next step",
                "Continuați la pasul următor"
            };

            await Task.Delay(1500);
            // <= Pres next btn until is not available =>
            int nrTimse = 0;
            while (true)
            {
                
                IWebElement nextStep = null;
                foreach (var ele in name)
                {
                    try
                    {
                        nextStep = _driver.FindElement(By.XPath("//*[@aria-label='" + ele + "']"));
                        nextStep.Click();
                        await Task.Delay(1500);
                        var needToByCompleteds = _driver.FindElement(By.ClassName("artdeco-inline-feedback__message"));
                        if (needToByCompleteds.Displayed)
                        {
                            Console.WriteLine("trebuie completat inainte");
                            await AutoCompletationLinkedin();
                            await Task.Delay(1500);
                        }
                    }
                    catch (Exception ex) { }
                }
                if (nextStep == null)
                {
                    break;
                }
                ++nrTimse;
                if (nrTimse == 30)
                {
                    return false;
                }
            }

            await Task.Delay(1500);
            // <= press review btn =>
            List<string> nameReview = new List<string>()
            {
                "Review your application",
                "Examinați candidatura dvs."
            };

            foreach (var ele in nameReview)
            {
                int nrTimes = 0; 
                try
                {
                    var nextStep = _driver.FindElement(By.XPath("//*[@aria-label='" + ele + "']"));
                    nextStep.Click();

               
                    await Task.Delay(2000);

                    try
                    {
                        while (nextStep != null)
                        {
                            nextStep.Click();
                            Console.WriteLine("trebuie completat aici");
                            await AutoCompletationLinkedin();
                            nextStep.Click();
                            ++nrTimes;
                            if (nrTimes == 26)
                            {
                                return false;
                            }
                            await Task.Delay(1000);
                        }

                    } catch (StaleElementReferenceException ex) { }

                } catch (NoSuchElementException ex) { }
            }

            List<string> sendName = new List<string>()
            {
                "Trimiteți candidatura",
                "Submit application"
            };
            await Task.Delay(1500);
            bool applySend = false;
            // <= press submit btn =>
            foreach (var ele in sendName)
            {
                try
                {
                    var nextStep = _driver.FindElement(By.XPath("//*[@aria-label='" + ele + "']"));
                    nextStep.Click();
                    await Task.Delay(1500);
                    applySend = true;
                }
                catch (Exception ex) { }
            }
            await Task.Delay(2000);
            if (applySend)
            {
                try
                {
                    var confirmApplication = _driver.FindElement(By.ClassName("artdeco-button"));
                    confirmApplication.Click();
                    return true;
                }
                catch (Exception) { }

            }
            return false;
        }
        public async Task SaveUserCookei()
        {
            IWebDriver loginDriver = new ChromeDriver();
            loginDriver.Navigate().GoToUrl("https://www.linkedin.com/home");
            await Task.Delay(20000);
            Console.WriteLine("1");
            await Task.Delay(20000);
            Console.WriteLine("2");
            await Task.Delay(20000);
            Console.WriteLine("3");

            await SaveCookies(loginDriver);
            loginDriver.Close();
        }

        public async Task DeleteCookei()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                throw new InvalidOperationException("User is not logged in.");
            }

            var cookies = await _context.UserCookies.Where(c => c.UserName == currentUser.UserName).ToListAsync();
            foreach (var userCookie in cookies)
            {
                if (userCookie.Platform == "Linkedin")
                {
                   _context.UserCookies.Remove(userCookie);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task SaveCookies(IWebDriver diever)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                throw new InvalidOperationException("User is not logged in.");
            }

            var cookies = diever.Manage().Cookies.AllCookies;
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
                    Platform = "Linkedin"
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
                if (userCookie.Platform == "Linkedin")
                {
                                     
                    try
                    {
                        var cookie = new Cookie(userCookie.Name, userCookie.Value, userCookie.Domain, userCookie.Path,
                            userCookie.Expiry, userCookie.Secure, userCookie.HttpOnly, userCookie.SameSite);
                        _driver.Manage().Cookies.AddCookie(cookie);
                    }
                    catch (Exception) { }

                }
            }
        }

    }
}
