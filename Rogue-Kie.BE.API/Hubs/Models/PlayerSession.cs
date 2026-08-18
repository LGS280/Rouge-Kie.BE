namespace Rogue_Kie.BE.API.Hubs.Models
{
    /// <summary>
    /// Data Model lưu trữ thông tin phiên kết nối của một người chơi trong phòng Co-op ảo.
    /// </summary>
    public class PlayerSession
    {
        /// <summary>
        /// ID kết nối SignalR của người chơi
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// Tên tài khoản người chơi (Username)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Cờ xác định người chơi có phải là Chủ phòng (Host) hay không
        /// </summary>
        public bool IsHost { get; set; }
    }
}
