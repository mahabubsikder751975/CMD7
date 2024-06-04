// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Cloud7CMS.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cloud7CMS.Models;

namespace Cloud7CMS.Areas.Identity.Pages.Account
{
    public class ManageUserRoleModel : PageModel
    {
        private readonly SignInManager<Cloud7CMSUser> _signInManager;
        private readonly UserManager<Cloud7CMSUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<Cloud7CMSUser> _userStore;
        private readonly IUserEmailStore<Cloud7CMSUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public ManageUserRoleModel(
            UserManager<Cloud7CMSUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<Cloud7CMSUser> userStore,
            SignInManager<Cloud7CMSUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;            ;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;

        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }


        public SelectList UserSelectList { get; set; }

        public SelectList RoleSelectList { get; set; }
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]      
            [Display(Name = "Role")]
            public string Role { get; set; }

           

        }


        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            UserSelectList = new SelectList(users, nameof(Cloud7CMSUser.Email), nameof(Cloud7CMSUser.Email));

            var roles = await _roleManager.Roles.ToListAsync();
            RoleSelectList = new SelectList(roles, "Name", "Name");


        }

        public async Task<IActionResult> OnPostAsync()
        {
            Cloud7CMSUser cloud7CMSUser = await _userManager.FindByEmailAsync(Input.Email);

                if (cloud7CMSUser != null)
                {                                        
                    await _userManager.AddToRoleAsync(cloud7CMSUser, Input.Role);
                }


            // Set success message in TempData
            TempData["SuccessMessage"] = "Role assigned successfully.";

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> OnGetUserRoleAsync(string userId)
        {
            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault();
            return Content(userRole); // Return the user's role as plain text
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OnPostGetTime(string name)
        {
            PersonModel person = new PersonModel
            {
                Name = name,
                DateTime = DateTime.Now.ToString()
            };
            return new JsonResult(person);
        }



    }
}
