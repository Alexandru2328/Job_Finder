using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Job_Finder.Models;
using Job_Finder.Services;

namespace Job_Finder.Controllers
{
    [Authorize]
    public class AutoApplyController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Automation _service;

        public AutoApplyController(UserManager<ApplicationUser> userManager, Automation service)
        {
            _userManager = userManager;
            _service = service;
        }

        [HttpGet]
        public  Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Edit(AutoApply model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                user.UserPlatformEmail = model.UserPlatformEmail;
                user.UserPlatformPassword = model.UserPlatformPassword;
                user.DomainExperience = model.DomainExperience;
                user.Message = model.Message;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("View", "AutoApply");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return RedirectToAction("View", "AutoApply");
        }

        public async Task <IActionResult> View()
        {
            var user = await _userManager.GetUserAsync(User);
            AutoApply model = new AutoApply();
            if (user != null)
            {
                model.UserPlatformEmail = user.UserPlatformEmail;
                model.DomainExperience = user.DomainExperience;
                model.Message = user.Message;
                model.UserPlatformPassword = user.UserPlatformPassword;
            }
            return View(model);
        }
        public async Task AutoApplySession()
        {
            await _service.JobFinderProces();
        }

        public async Task Apply(int id)
        {
            await _service.JobFinderProces(id);
        }
    }
}
