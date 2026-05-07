using ASC.Model.BaseTypes;

namespace ASC.Model.Models
{
    /// <summary>
    /// Lưu tin nhắn chat giữa Customer/Admin và Engineer
    /// PartitionKey = ServiceRequestId (hoặc "general" cho chat không liên quan SR)
    /// RowKey = Guid mới
    /// </summary>
    public class ChatMessage : BaseEntity, IAuditTracker
    {
        public ChatMessage() { }

        public ChatMessage(string serviceRequestId)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = serviceRequestId;
        }

        /// <summary>Email của người gửi</summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>Tên hiển thị của người gửi</summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>Role của người gửi: User | Engineer | Admin</summary>
        public string SenderRole { get; set; } = string.Empty;

        /// <summary>Nội dung tin nhắn</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>ServiceRequest ID liên quan (PartitionKey)</summary>
        public string ServiceRequestId { get; set; } = string.Empty;

        /// <summary>Đã đọc chưa</summary>
        public bool IsRead { get; set; } = false;
    }
}
