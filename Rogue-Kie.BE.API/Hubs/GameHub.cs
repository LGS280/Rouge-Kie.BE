using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

            // Đưa người chơi vào Group của SignalR để tiện broadcast
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Caller.SendAsync("OnRoomCreated", roomCode);
        }

        // 2. Tham gia phòng (Join Room)
        public async Task JoinRoom(string roomCode, string username)
        {
            roomCode = roomCode.ToUpper().Trim();

            if (!RoomManager.ActiveRooms.TryGetValue(roomCode, out var room))
            {
                await Clients.Caller.SendAsync("OnJoinRoomFailed", "Phòng không tồn tại!");
                return;
            }

            if (room.Players.Count >= 4) // Giới hạn 4 người chơi co-op
            {
                await Clients.Caller.SendAsync("OnJoinRoomFailed", "Phòng đã đầy!");
                return;
            }

            var newPlayer = new PlayerSession
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                IsHost = false
            };

            room.Players.Add(newPlayer);
            RoomManager.ConnectionToRoom[Context.ConnectionId] = roomCode;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            // Trả về cho người chơi mới danh sách thành viên hiện tại trong phòng
            var currentPlayers = room.Players.Select(p => p.Username).ToList();
            await Clients.Caller.SendAsync("OnJoinRoomSuccess", roomCode, currentPlayers);

            // Thông báo cho các người chơi cũ có người mới tham gia
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

    }
}