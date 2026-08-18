using System.Collections.Concurrent;

namespace Rogue_Kie.BE.API.Hubs.Models
{
    /// <summary>
    /// Quản lý bộ nhớ tạm thời (In-Memory Thread-Safe) cho toàn bộ các phòng chơi Co-op đang hoạt động và bộ đếm CCU.
    /// </summary>
    public static class RoomManager
    {
        /// <summary>
        /// Quản lý các phòng chơi đang hoạt động theo RoomCode
        /// </summary>
        public static ConcurrentDictionary<string, RoomSession> ActiveRooms = new ConcurrentDictionary<string, RoomSession>();

        /// <summary>
        /// Tra cứu nhanh mã phòng theo SignalR ConnectionId người chơi
        /// </summary>
        public static ConcurrentDictionary<string, string> ConnectionToRoom = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Bộ sưu tập lưu vết các tài khoản đang giữ kết nối SignalR thời gian thực
        /// </summary>
        public static ConcurrentDictionary<string, byte> ConnectedUsers = new ConcurrentDictionary<string, byte>();

        /// <summary>
        /// Lấy tổng số lượng người chơi đang Online thời gian thực (Concurrent Users - CCU)
        /// </summary>
        public static int GetCCU() => ConnectedUsers.Count;
    }
}
