using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASC.Utilities; // Chứa extension GetCurrentUserDetails()
using Microsoft.AspNetCore.Http;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class InitiateResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public InitiateResetPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public void OnGet()
        {
            // Để trống theo thiết kế
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Tìm User dựa trên Email của người dùng hiện tại
            var userEmail = HttpContext.User.GetCurrentUserDetails().Email;
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // 2. Tạo mã Token để Reset mật khẩu
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 3. Tạo link callback dẫn tới trang ResetPassword thực sự
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { userId = user.Id, code = code },
                protocol: Request.Scheme);

            // 4. Gửi Email thông báo
            await _emailSender.SendEmailAsync(userEmail, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            // 5. Chuyển hướng sang trang xác nhận đã gửi email
            return RedirectToPage("./ResetPasswordEmailConfirmation");
        }
    }
}