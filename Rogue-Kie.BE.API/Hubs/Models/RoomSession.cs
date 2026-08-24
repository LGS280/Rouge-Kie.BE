using System.Collections.Generic;

namespace Rogue_Kie.BE.API.Hubs.Models
{
    /// <summary>
    /// Data Model quản lý thông tin một phòng chơi Co-op Multiplayer trong bộ nhớ RAM.
    /// </summary>
    public class RoomSession
    {
        /// <summary>
        /// Mã định danh phòng chơi (Room Code 6 ký tự)
        /// </summary>
        public string RoomCode { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách người chơi tham gia trong phòng
        /// </summary>
        public List<PlayerSession> Players { get; set; } = new List<PlayerSession>();

        /// <summary>
        /// Tập hợp các ConnectionId của người chơi đã gục ngã/hy sinh trong phòng
        /// </summary>
        public HashSet<string> DeadPlayers { get; set; } = new HashSet<string>();

        /// <summary>
        /// Sĩ số tối đa của phòng chơi (mặc định 4 người)
        /// </summary>
        public int MaxPlayers { get; set; } = 4;

        /// <summary>
        /// Trạng thái phòng đã bắt đầu vào trận đấu hay chưa
        /// </summary>
        public bool IsGameStarted { get; set; } = false;
    }
}
