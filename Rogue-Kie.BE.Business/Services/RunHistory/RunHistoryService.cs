using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Business.Services.RunHistoryService;
using Rogue_Kie.BE.Contracts.RunHistory;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.RunHistoryService
{
    public class RunHistoryService : IRunHistoryService
    {
        private readonly AppDbContext _context;

        public RunHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RunHistory> LogRunHistoryAsync(int userId, CreateRunHistoryRequest request)
        {
            // 1. Tạo một GameSession tự động cho lượt chơi đơn này
            var session = new GameSession
            {
                HostUserId = userId,
                RoomCode = "SINGLE_PLAYER_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                Status = "Completed",
                MaxPlayers = 1,
                CreatedAt = DateTime.UtcNow.AddSeconds(-request.DurationSeconds),
                EndedAt = DateTime.UtcNow
            };

            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync(); // Lưu để lấy SessionId

            // 2. Lưu lịch sử đấu
            var runHistory = new RunHistory
            {
                UserId = userId,
                SessionId = session.SessionId,
                CharacterId = request.CharacterId,
                WavesSurvived = request.WavesSurvived,
                EnemiesKilled = request.EnemiesKilled,
                DamageDealt = request.DamageDealt,
                CurrencyEarned = request.CurrencyEarned,
                DurationSeconds = request.DurationSeconds,
                PlayedAt = DateTime.UtcNow
            };

            _context.RunHistories.Add(runHistory);

            // 3. Cập nhật hồ sơ người chơi (PlayerProfile)
            var profile = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                var user = await _context.Users.FindAsync(userId);
                profile = new PlayerProfile
                {
                    UserId = userId,
                    DisplayName = user?.Username ?? "Player",
                    StandardCurrency = 0,
                    PremiumCurrency = 0,
                    TotalRuns = 0,
                    HighestWave = 0,
                    TotalKills = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PlayerProfiles.Add(profile);
            }

            profile.TotalRuns += 1;
            profile.StandardCurrency += request.CurrencyEarned;
            profile.TotalKills += request.EnemiesKilled;
            
            if (request.WavesSurvived > profile.HighestWave)
            {
                profile.HighestWave = request.WavesSurvived;
            }

            profile.UpdatedAt = DateTime.UtcNow;
            _context.PlayerProfiles.Update(profile);

            // 4. Đồng bộ sang bảng Leaderboard
            var leaderboardEntry = await _context.Leaderboards
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (leaderboardEntry == null)
            {
                leaderboardEntry = new Leaderboard
                {
                    UserId = userId,
                    HighestWave = profile.HighestWave,
                    TotalKills = profile.TotalKills,
                    TotalRuns = profile.TotalRuns,
                    RankPosition = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Leaderboards.Add(leaderboardEntry);
            }
            else
            {
                leaderboardEntry.HighestWave = profile.HighestWave;
                leaderboardEntry.TotalKills = profile.TotalKills;
                leaderboardEntry.TotalRuns = profile.TotalRuns;
                leaderboardEntry.UpdatedAt = DateTime.UtcNow;
                _context.Leaderboards.Update(leaderboardEntry);
            }

            await _context.SaveChangesAsync();
            return runHistory;
        }

        public async Task<List<RunHistoryResponse>> GetUserRunsAsync(int userId)
        {
            var runs = await _context.RunHistories
                .Include(r => r.Character)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.PlayedAt)
                .ToListAsync();

            return runs.Select(r => new RunHistoryResponse
            {
                RunId = r.RunId,
                UserId = r.UserId,
                CharacterId = r.CharacterId,
                CharacterName = r.Character?.Name ?? "Unknown Character",
                WavesSurvived = r.WavesSurvived,
                EnemiesKilled = r.EnemiesKilled,
                DamageDealt = r.DamageDealt,
                CurrencyEarned = r.CurrencyEarned,
                DurationSeconds = r.DurationSeconds,
                PlayedAt = r.PlayedAt
            }).ToList();
        }
    }
}
