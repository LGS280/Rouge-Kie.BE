using Microsoft.AspNetCore.SignalR;
using Rogue_Kie.BE.API.Hubs.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Hubs
{
    /// <summary>
    /// Hub SignalR chính xử lý toàn bộ các sự kiện đồng bộ Co-op Multiplayer và vị trí người chơi 60 FPS.
    /// </summary>
    public class GameHub : Hub
    {
        // 1. Tạo phòng mới (Create Room)
        public async Task CreateRoom(string username)
        {
            try
            {
                string roomCode = GenerateRoomCode();

                if (string.IsNullOrWhiteSpace(username))
                {
                    username = $"Host_{Context.ConnectionId.Substring(0, Math.Min(4, Context.ConnectionId.Length))}";
                }

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

                await Clients.Caller.SendAsync("OnRoomCreated", roomCode, true);
                await BroadcastPlayerOrder(roomCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameHub] Lỗi CreateRoom: {ex}");
                await Clients.Caller.SendAsync("OnJoinRoomFailed", $"Lỗi tạo phòng: {ex.Message}");
            }
        }

        // 2. Vào phòng (Join Room)
        public async Task JoinRoom(string roomCode, string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomCode))
                {
                    await Clients.Caller.SendAsync("OnJoinRoomFailed", "Mã phòng không hợp lệ!");
                    return;
                }

                roomCode = roomCode.ToUpper().Trim();

                if (!RoomManager.ActiveRooms.TryGetValue(roomCode, out var room) || room == null)
                {
                    await Clients.Caller.SendAsync("OnJoinRoomFailed", "Phòng không tồn tại hoặc đã giải tán!");
                    return;
                }

                if (room.IsGameStarted)
                {
                    await Clients.Caller.SendAsync("OnJoinRoomFailed", "Phòng đang trong trận đấu!");
                    return;
                }

                if (room.Players == null)
                {
                    room.Players = new List<PlayerSession>();
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    username = $"Player_{Context.ConnectionId.Substring(0, Math.Min(4, Context.ConnectionId.Length))}";
                }

                // Nếu người này đã có trong phòng (tránh trùng ConnectionId khi test lặp lại)
                var existingPlayer = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (existingPlayer != null)
                {
                    existingPlayer.Username = username;
                    RoomManager.ConnectionToRoom[Context.ConnectionId] = roomCode;
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

                    var players = room.Players.Select(p => p.Username).ToList();
                    await Clients.Caller.SendAsync("OnJoinRoomSuccess", roomCode, players, existingPlayer.IsHost);
                    await BroadcastPlayerOrder(roomCode);
                    return;
                }

                if (room.Players.Count >= room.MaxPlayers)
                {
                    await Clients.Caller.SendAsync("OnJoinRoomFailed", $"Phòng đã đầy! (Tối đa {room.MaxPlayers} người)");
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

                await Clients.Caller.SendAsync("OnJoinRoomSuccess", roomCode, currentPlayers, false);
                await Clients.OthersInGroup(roomCode).SendAsync("OnPlayerJoined", username, Context.ConnectionId);
                await BroadcastPlayerOrder(roomCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameHub] Lỗi JoinRoom: {ex}");
                await Clients.Caller.SendAsync("OnJoinRoomFailed", $"Lỗi tham gia phòng: {ex.Message}");
            }
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

        /// <summary>
        /// Rời khỏi phòng hiện tại (khi bấm nút Back từ Sảnh chờ)
        /// </summary>
        public async Task LeaveRoom()
        {
            if (RoomManager.ConnectionToRoom.TryRemove(Context.ConnectionId, out string roomCode))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);

                if (RoomManager.ActiveRooms.TryGetValue(roomCode, out var room))
                {
                    var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                    if (player != null)
                    {
                        room.Players.Remove(player);

                        // Nếu là Host rời đi hoặc phòng không còn ai -> Giải phóng phòng và kick tất cả các player còn lại ngay lập tức
                        if (player.IsHost || room.Players.Count == 0)
                        {
                            // Phát thông báo giải tán phòng tới TOÀN BỘ các thành viên còn lại
                            await Clients.Group(roomCode).SendAsync("OnHostDisconnectedEndGame", player.Username);

                            // Xóa mapping của các player còn lại khỏi ConnectionToRoom
                            foreach (var p in room.Players)
                            {
                                RoomManager.ConnectionToRoom.TryRemove(p.ConnectionId, out _);
                            }

                            RoomManager.ActiveRooms.TryRemove(roomCode, out _);
                        }
                        else
                        {
                            // Nếu là Client thường thoát -> Báo cho các người còn lại cập nhật danh sách
                            await Clients.Group(roomCode).SendAsync("OnPlayerDisconnected", player.Username, Context.ConnectionId);
                            await BroadcastPlayerOrder(roomCode);
                        }
                    }
                }
            }
        }

        // 5. Xử lý khi ngắt kết nối đột ngột
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            RoomManager.ConnectedUsers.TryRemove(Context.ConnectionId, out _);

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
                            // Theo chỉ đạo: Nếu Host thoát khỏi game -> Kết thúc trận/giải tán phòng và kick tất cả các Client còn lại
                            await Clients.Group(roomCode).SendAsync("OnHostDisconnectedEndGame", player.Username);

                            foreach (var p in room.Players)
                            {
                                RoomManager.ConnectionToRoom.TryRemove(p.ConnectionId, out _);
                            }

                            RoomManager.ActiveRooms.TryRemove(roomCode, out _);
                        }
                        else
                        {
                            await BroadcastPlayerOrder(roomCode);
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
                        room.IsGameStarted = true;
                        // Phát thứ tự người chơi cố định trong phòng trước khi chuyển Scene
                        await BroadcastPlayerOrder(roomCode);
                        // Phát lệnh chuyển Scene cho TOÀN BỘ thành viên trong Group mã phòng này
                        await Clients.Group(roomCode).SendAsync("OnGameStarted");
                    }
                }
            }
        }

        /// <summary>
        /// Lấy danh sách các phòng chơi đang mở trên Server (phục vụ hiển thị Lobby Room List UI)
        /// </summary>
        public async Task GetPublicRooms()
        {
            try
            {
                var roomList = RoomManager.ActiveRooms.Values
                    .Where(r => r != null && !string.IsNullOrEmpty(r.RoomCode))
                    .Select(r => new
                    {
                        roomCode = r.RoomCode,
                        hostName = r.Players?.FirstOrDefault(p => p.IsHost)?.Username ?? "Host",
                        currentPlayers = r.Players?.Count ?? 1,
                        maxPlayers = r.MaxPlayers > 0 ? r.MaxPlayers : 4,
                        isGameStarted = r.IsGameStarted
                    }).ToList();

                await Clients.Caller.SendAsync("OnReceivePublicRooms", roomList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameHub] Lỗi GetPublicRooms: {ex}");
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

        // Gửi tọa độ quái vật từ Host tới các Client khác
        public async Task SyncEnemyPosition(string roomId, string enemyId, float x, float y)
        {
            await Clients.OthersInGroup(roomId).SendAsync("OnReceiveEnemyPosition", enemyId, x, y);
        }

        // BỔ SUNG: Gửi yêu cầu chuyển tầng đồng bộ tới toàn bộ người chơi trong phòng Co-op
        public async Task RequestNextFloor(string roomId, int targetFloor)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                if (RoomManager.ActiveRooms.TryGetValue(roomId, out var room))
                {
                    room.DeadPlayers.Clear();
                }

                // Phát lệnh chuyển tầng tới tất cả thành viên trong nhóm phòng chơi
                await Clients.Group(roomId).SendAsync("OnFloorTransitionSynced", targetFloor);
            }
        }

        // BỔ SUNG: Gửi đồng bộ loại súng chính và súng phụ đang cầm tới các người chơi khác trong phòng Co-op
        public async Task SyncEquippedWeapon(string roomId, string activeWeaponName, string secondaryWeaponName)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                // Broadcast tên súng chính và súng phụ cho các người chơi khác trong phòng
                await Clients.OthersInGroup(roomId).SendAsync("OnRemoteWeaponChanged", Context.ConnectionId, activeWeaponName, secondaryWeaponName);
            }
        }

        // BỔ SUNG: Gửi đồng bộ trạng thái đã ghé thăm phòng trên Minimap cho đồng đội
        public async Task SyncRoomVisited(string roomId, string roomUniqueId)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("OnRemoteRoomVisited", roomUniqueId);
            }
        }

        // BỔ SUNG: Gửi đồng bộ sát thương quái đánh trúng người chơi qua mạng
        public async Task SyncPlayerDamaged(string roomId, string targetConnId, float damage)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.Group(roomId).SendAsync("OnPlayerDamaged", targetConnId, damage);
            }
        }

        // BỔ SUNG: Gửi đồng bộ sự kiện người chơi hy sinh (Player Death) tới các đồng đội trong phòng Co-op
        public async Task SyncPlayerDeath(string roomId)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.Group(roomId).SendAsync("OnRemotePlayerDied", Context.ConnectionId);

                if (RoomManager.ActiveRooms.TryGetValue(roomId, out var room))
                {
                    room.DeadPlayers.Add(Context.ConnectionId);
                    // Nếu TẤT CẢ người chơi trong phòng đều đã hy sinh -> Broadcast OnTeamDefeat cho cả room mở Bảng Defeat
                    if (room.DeadPlayers.Count >= room.Players.Count && room.Players.Count > 0)
                    {
                        await Clients.Group(roomId).SendAsync("OnTeamDefeat");
                        room.DeadPlayers.Clear();
                    }
                }
            }
        }

        // BỔ SUNG: Gửi đồng bộ sự kiện hồi sinh người chơi (Player Revive) từ đồng đội trong phòng Co-op
        public async Task SyncPlayerRevive(string roomId, string targetConnId, int reviveHp)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                if (RoomManager.ActiveRooms.TryGetValue(roomId, out var room))
                {
                    room.DeadPlayers.Remove(targetConnId);
                }
                await Clients.Group(roomId).SendAsync("OnPlayerRevived", targetConnId, reviveHp);
            }
        }

        // BỔ SUNG: Gửi đồng bộ sự kiện mở rương vũ khí dùng chung trong phòng Co-op
        public async Task SyncOpenChest(string roomId, string chestId, string weaponName, float spawnX, float spawnY)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("OnChestOpened", chestId, weaponName, spawnX, spawnY);
            }
        }

        // BỔ SUNG: Gửi đồng bộ sự kiện nhặt vũ khí rơi trên sàn (xóa súng trên các máy còn lại)
        public async Task SyncPickupGroundWeapon(string roomId, string groundWeaponId)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("OnGroundWeaponPickedUp", groundWeaponId);
            }
        }

        // BỔ SUNG: Gửi đồng bộ sự kiện vứt vũ khí cũ ra sàn cho đồng đội cùng thấy
        public async Task SyncDropWeapon(string roomId, string weaponName, float posX, float posY, string groundWeaponId)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("OnWeaponDropped", weaponName, posX, posY, groundWeaponId);
            }
        }

        // BỔ SUNG: Gửi đồng bộ sự kiện Boss tấn công (xả đạn) cho các người chơi trong phòng
        public async Task SyncBossAttack(string roomId, string bossId, float targetX, float targetY)
        {
            if (string.IsNullOrEmpty(roomId) && RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string foundRoom))
            {
                roomId = foundRoom;
            }

            if (!string.IsNullOrEmpty(roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("OnBossAttack", bossId, targetX, targetY);
            }
        }

        // BỔ SUNG: Cho phép Client yêu cầu gửi lại thứ tự người chơi trong phòng (Global Player Slot Order)
        public async Task GetPlayerOrder()
        {
            if (RoomManager.ConnectionToRoom.TryGetValue(Context.ConnectionId, out string roomCode))
            {
                await BroadcastPlayerOrder(roomCode);
            }
        }

        private async Task BroadcastPlayerOrder(string roomCode)
        {
            if (RoomManager.ActiveRooms.TryGetValue(roomCode, out var room))
            {
                var playerConnIds = room.Players.Select(p => p.ConnectionId).ToList();
                await Clients.Group(roomCode).SendAsync("OnSyncPlayerOrder", playerConnIds);
            }
        }

        public override async Task OnConnectedAsync()
        {
            RoomManager.ConnectedUsers.TryAdd(Context.ConnectionId, 0);
            await base.OnConnectedAsync();
        }
    }
}