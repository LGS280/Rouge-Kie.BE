using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Hubs
{
    // Model lưu thông tin người chơi trong phòng chơi ảo
    public class PlayerSession
    {
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public bool IsHost { get; set; }
    }

    // Model quản lý Phòng chơi ảo
    public class RoomSession
    {
        public string RoomCode { get; set; }
        public List<PlayerSession> Players { get; set; } = new List<PlayerSession>();
    }

    // Quản lý bộ nhớ tạm (In-Memory) cho các phòng đang hoạt động
    public static class RoomManager
    {
        public static ConcurrentDictionary<string, RoomSession> ActiveRooms = new ConcurrentDictionary<string, RoomSession>();
        public static ConcurrentDictionary<string, string> ConnectionToRoom = new ConcurrentDictionary<string, string>(); // ConnectionId -> RoomCode
    }

    public class GameHub : Hub
    {
        // 1. Tạo phòng mới (Create Room)
        // 1. Trong hàm CreateRoom
        public async Task CreateRoom(string username)
        {
            string roomCode = GenerateRoomCode();

            var room = new RoomSession { RoomCode = roomCode };
            room.Players.Add(new PlayerSession
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                IsHost = true
            });

            RoomManager.ActiveRooms[roomCode] = room;
            RoomManager.ConnectionToRoom[Context.ConnectionId] = roomCode;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            // THAY ĐỔI Ở ĐÂY: Truyền thêm giá trị true (vì người tạo chắc chắn là Host)
            await Clients.Caller.SendAsync("OnRoomCreated", roomCode, true);
        }

        // 2. Trong hàm JoinRoom
        public async Task JoinRoom(string roomCode, string username)
        {
            roomCode = roomCode.ToUpper().Trim();

            if (!RoomManager.ActiveRooms.TryGetValue(roomCode, out var room))
            {
                await Clients.Caller.SendAsync("OnJoinRoomFailed", "Phòng không tồn tại!");
                return;
            }

            if (room.Players.Count >= 4)
            {
                await Clients.Caller.SendAsync("OnJoinRoomFailed", "Phòng đã đầy!");
                return;
            }

            var newPlayer = new PlayerSession
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                IsHost = false // Người vào sau mặc định không phải Host ban đầu
            };

            room.Players.Add(newPlayer);
            RoomManager.ConnectionToRoom[Context.ConnectionId] = roomCode;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            var currentPlayers = room.Players.Select(p => p.Username).ToList();

            // THAY ĐỔI Ở ĐÂY: Truyền thêm giá trị false về cho Caller (vì họ là người vào sau, không phải Host)
            await Clients.Caller.SendAsync("OnJoinRoomSuccess", roomCode, currentPlayers, false);

            await Clients.OthersInGroup(roomCode).SendAsync("OnPlayerJoined", username, Context.ConnectionId);
        }

        // 3. Đồng bộ tọa độ di chuyển (Sync Position)
        public async Task SyncPosition(float x, float y)
        {
            if (RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string roomCode))
            {
                // Chỉ gửi dữ liệu cho những người chơi khác trong phòng (tránh gửi lặp lại chính mình)
                await Clients.OthersInGroup(roomCode).SendAsync("OnReceivePosition", Context.ConnectionId, x, y);
            }
        }

        // 4. Đồng bộ hướng bắn / đạn (Sync Shoot)
        public async Task SyncShoot(float angle, float posX, float posY)
        {
            if (RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string roomCode))
            {
                await Clients.OthersInGroup(roomCode).SendAsync("OnReceiveShoot", Context.ConnectionId, angle, posX, posY);
            }
        }

        // ===================================================================================
        // BỔ SUNG: CÁC PHƯƠNG THỨC XỬ LÝ LỆNH ĐỒNG BỘ PHÒNG (ROOM COMBAT) TỪ CLIENT GỬI LÊN
        // ===================================================================================

        // Khi có bất kỳ ai bước vào một phòng combat, broadcast thông số để lôi kéo đồng đội vào chung phòng
        public async Task TriggerRoomCombat(string matchRoomId, string targetRoomId, float centerX, float centerY)
        {
            // Gửi lệnh xuống TOÀN BỘ người chơi đang có trong trận đấu này
            await Clients.Group(matchRoomId).SendAsync("OnRoomCombatStarted", targetRoomId, centerX, centerY);
        }

        // Khi một máy khách xử lý xong quái và báo phòng đã sạch, phát tín hiệu mở cửa đồng loạt
        public async Task RegisterRoomCleared(string matchRoomId, string targetRoomId)
        {
            // Gửi lệnh xuống TOÀN BỘ người chơi để đồng loạt gọi hàm mở cửa local
            await Clients.Group(matchRoomId).SendAsync("OnRoomClearedFromServer", targetRoomId);
        }

        // ===================================================================================

        // 5. Xử lý khi ngắt kết nối đột ngột
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (RoomManager.ConnectionToRoom.TryRemove(Context.ConnectionId, out string roomCode))
            {
                if (RoomManager.ActiveRooms.TryGetValue(roomCode, out var room))
                {
                    var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                    if (player != null)
                    {
                        room.Players.Remove(player);
                        await Clients.Group(roomCode).SendAsync("OnPlayerDisconnected", player.Username, Context.ConnectionId);

                        // Nếu không còn ai trong phòng, giải phóng bộ nhớ phòng chơi
                        if (room.Players.Count == 0)
                        {
                            RoomManager.ActiveRooms.TryRemove(roomCode, out _);
                        }
                        else if (player.IsHost)
                        {
                            // Nếu Chủ phòng thoát, chuyển quyền Host cho người kế tiếp
                            room.Players[0].IsHost = true;
                            await Clients.Group(roomCode).SendAsync("OnHostChanged", room.Players[0].Username, room.Players[0].ConnectionId);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (RoomManager.ActiveRooms.ContainsKey(code));

            return code;
        }

        public async Task StartGame()
        {
            // Tìm phòng chơi hiện tại dựa trên ConnectionId của người gọi lệnh
            if (RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string roomCode))
            {
                if (RoomManager.ActiveRooms.TryGetValue(roomCode, out var room))
                {
                    // Kiểm tra bảo mật: Chỉ cho phép người là HOST được quyền bắt đầu trận đấu
                    var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                    if (player != null && player.IsHost)
                    {
                        // Phát lệnh chuyển Scene cho TOÀN BỘ thành viên trong Group mã phòng này
                        await Clients.Group(roomCode).SendAsync("OnGameStarted");
                    }
                }
            }
        }

        // Gửi sự kiện bắn súng từ người chơi
        public async Task SendShoot(string roomId, string weaponId, float px, float py, float dx, float dy)
        {
            string playerId = Context.ConnectionId;
            // Phát sóng tới tất cả người chơi khác trong phòng để tự vẽ đạn
            await Clients.OthersInGroup(roomId).SendAsync("OnPlayerShoot", playerId, weaponId, px, py, dx, dy);
        }

        // Gửi sự kiện quái vật nhận sát thương (Xác thực phía Server/Host)
        public async Task RegisterEnemyHit(string roomId, string enemyId, float damage)
        {
            // Trong môi trường Authoritative, Server sẽ kiểm tra lượng HP còn lại của quái
            // Sau đó phát sóng lượng HP mới tới toàn bộ Client
            await Clients.Group(roomId).SendAsync("OnEnemyDamaged", enemyId, damage);
        }

    }
}