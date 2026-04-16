// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                // Kiểm tra user tồn tại và đã xác nhận email (theo ảnh d5ee67)
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Đừng tiết lộ user không tồn tại, chuyển hướng để báo đã gửi mail (giả)
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Tạo Token reset mật khẩu
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Mã hóa code để an toàn khi đưa lên URL
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Tạo đường dẫn callback trỏ về trang ResetPassword
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email = Input.Email },
                    protocol: Request.Scheme);

                // Gửi email chứa link reset
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                // Chuyển hướng đến trang thông báo "Vui lòng kiểm tra email"
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}