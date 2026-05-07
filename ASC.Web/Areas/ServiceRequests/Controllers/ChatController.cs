using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Controllers;
using ASC.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ChatController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public ChatController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Hiển thị trang chat cho một ServiceRequest cụ thể.
        /// Customer xem chat SR của chính mình.
        /// Engineer chỉ thấy SR được gán.
        /// Admin thấy tất cả.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Chat(string serviceRequestId)
        {
            if (string.IsNullOrEmpty(serviceRequestId))
                return BadRequest("ServiceRequest ID không được để trống.");

            var currentUser = HttpContext.User.GetCurrentUserDetails();
            if (currentUser == null) return Unauthorized();

            // Tìm ServiceRequest theo RowKey
            var serviceRequest = await _db.ServiceRequests
                .FirstOrDefaultAsync(s => s.RowKey == serviceRequestId && !s.IsDeleted);

            if (serviceRequest == null)
                return NotFound("Không tìm thấy yêu cầu dịch vụ.");

            // Phân quyền
            bool isAdmin = HttpContext.User.IsInRole(Roles.Admin.ToString());
            bool isEngineer = HttpContext.User.IsInRole(Roles.Engineer.ToString());
            bool isCustomer = HttpContext.User.IsInRole(Roles.User.ToString());

            if (isCustomer && !isAdmin && serviceRequest.PartitionKey != currentUser.Email)
                return Forbid();

            if (isEngineer && !isAdmin && serviceRequest.ServiceEngineer != currentUser.Email)
                return Forbid();

            // Lấy lịch sử chat
            var messages = await _db.ChatMessages
                .Where(m => m.PartitionKey == serviceRequestId && !m.IsDeleted)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();

            ViewBag.ServiceRequestId = serviceRequestId;
            ViewBag.ServiceRequest = serviceRequest;
            ViewBag.CurrentUserEmail = currentUser.Email;
            ViewBag.CurrentUserName = currentUser.Name;
            ViewBag.CurrentUserRole = isAdmin ? "Admin" : (isEngineer ? "Engineer" : "User");

            return View(messages);
        }

        /// <summary>
        /// API: Lấy danh sách ServiceRequests có thể chat (cho Admin/Engineer xem toàn bộ danh sách)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ChatList()
        {
            var currentUser = HttpContext.User.GetCurrentUserDetails();
            bool isAdmin = HttpContext.User.IsInRole(Roles.Admin.ToString());
            bool isEngineer = HttpContext.User.IsInRole(Roles.Engineer.ToString());

            List<ServiceRequest> requests;

            if (isAdmin)
            {
                requests = await _db.ServiceRequests
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.CreatedDate)
                    .Take(50)
                    .ToListAsync();
            }
            else if (isEngineer)
            {
                requests = await _db.ServiceRequests
                    .Where(s => !s.IsDeleted && s.ServiceEngineer == currentUser!.Email)
                    .OrderByDescending(s => s.CreatedDate)
                    .Take(50)
                    .ToListAsync();
            }
            else
            {
                // Customer: xem SR của chính mình
                requests = await _db.ServiceRequests
                    .Where(s => !s.IsDeleted && s.PartitionKey == currentUser!.Email)
                    .OrderByDescending(s => s.CreatedDate)
                    .ToListAsync();
            }

            return View(requests);
        }
    }
}
