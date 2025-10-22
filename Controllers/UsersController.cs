using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Models;
using ShopInventory.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Users/ActivityLog/{id}
        [HttpGet]
        public async Task<IActionResult> ActivityLog(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var logs = _context.ActivityLogs
                .Where(l => l.UserId == id)
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            return View("~/Views/Reports/ActivityLogs.cshtml", logs);
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new List<ShopInventory.Models.UserListItemViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var lastLog = _context.ActivityLogs.Where(a => a.UserId == u.Id).OrderByDescending(a => a.Timestamp).Select(a => (DateTime?)a.Timestamp).FirstOrDefault();
                var status = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow ? "معطل" : "نشط";

                model.Add(new ShopInventory.Models.UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName ?? string.Empty,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    LastLogin = lastLog,
                    Status = status
                });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                        await _userManager.AddToRoleAsync(user, model.Role);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roles = _roleManager.Roles;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Role = roles.Count > 0 ? roles[0] : string.Empty
            };

            ViewBag.Roles = _roleManager.Roles;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FullName = model.FullName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!string.IsNullOrEmpty(model.Role))
                        await _userManager.AddToRoleAsync(user, model.Role);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roles = _roleManager.Roles;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, "Password123!");

            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(Edit), new { id });
        }
    }
}