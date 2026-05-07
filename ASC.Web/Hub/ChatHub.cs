using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ASC.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Client gọi để tham gia nhóm chat theo serviceRequestId.
        /// Mỗi ServiceRequest có một group riêng để cô lập tin nhắn.
        /// </summary>
        public async Task JoinServiceRequestRoom(string serviceRequestId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, serviceRequestId);
        }

        /// <summary>
        /// Client gọi để rời nhóm chat.
        /// </summary>
        public async Task LeaveServiceRequestRoom(string serviceRequestId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, serviceRequestId);
        }

        /// <summary>
        /// Gửi tin nhắn trong phòng chat của một ServiceRequest.
        /// Tin nhắn được lưu vào DB trước khi broadcast.
        /// </summary>
        public async Task SendMessage(string serviceRequestId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            var user = Context.User;
            if (user == null) return;

            var currentUser = user.GetCurrentUserDetails();
            if (currentUser == null) return;

            // Xác định role hiển thị
            string senderRole = "User";
            if (user.IsInRole(Roles.Admin.ToString())) senderRole = "Admin";
            else if (user.IsInRole(Roles.Engineer.ToString())) senderRole = "Engineer";

            // Lưu tin nhắn vào DB
            var chatMessage = new ChatMessage(serviceRequestId)
            {
                SenderEmail = currentUser.Email,
                SenderName = currentUser.Name,
                SenderRole = senderRole,
                Message = message.Trim(),
                ServiceRequestId = serviceRequestId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = currentUser.Email,
                UpdatedBy = currentUser.Email,
                IsDeleted = false
            };

            _db.ChatMessages.Add(chatMessage);
            await _db.SaveChangesAsync();

            // Broadcast đến tất cả client trong group
            await Clients.Group(serviceRequestId).SendAsync("ReceiveMessage", new
            {
                rowKey = chatMessage.RowKey,
                senderEmail = chatMessage.SenderEmail,
                senderName = chatMessage.SenderName,
                senderRole = chatMessage.SenderRole,
                message = chatMessage.Message,
                serviceRequestId = chatMessage.ServiceRequestId,
                sentAt = chatMessage.CreatedDate.ToString("HH:mm dd/MM/yyyy")
            });
        }
        /// <summary>
        /// Client gọi khi đang gõ để thông báo cho các client khác trong group.
        /// </summary>
        public async Task NotifyTyping(string serviceRequestId, string senderName)
        {
            await Clients.OthersInGroup(serviceRequestId).SendAsync("UserTyping", senderName);
        }
    }
}
