using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Contracts.Leaderboard;
using Rogue_Kie.BE.Contracts.Profile;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;

        public ProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PlayerProfile> GetOrCreateProfileAsync(int userId)
        {
            var profile = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng.");
                }

                profile = new PlayerProfile
                {
                    UserId = userId,
                    DisplayName = user.Username,
                    StandardCurrency = 0,
                    PremiumCurrency = 0,
                    TotalRuns = 0,
                    HighestWave = 0,
                    TotalKills = 0,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PlayerProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return profile;
        }

        public async Task<PlayerProfile?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var profile = await GetOrCreateProfileAsync(userId);

            profile.DisplayName = request.DisplayName ?? profile.DisplayName;
            profile.StandardCurrency = request.StandardCurrency;
            profile.PremiumCurrency = request.PremiumCurrency;
            
            // Chỉ cập nhật kỷ lục mới nếu nó cao hơn kỷ lục cũ
            if (request.HighestWave > profile.HighestWave)
            {
                profile.HighestWave = request.HighestWave;
            }
            if (request.TotalKills > profile.TotalKills)
            {
                profile.TotalKills = request.TotalKills;
            }

            profile.UpdatedAt = DateTime.UtcNow;
            _context.PlayerProfiles.Update(profile);

            // Đồng bộ sang bảng Leaderboard
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
            return profile;
        }

        public async Task<List<LeaderboardResponse>> GetLeaderboardAsync(int topCount = 20)
        {
            // Lấy danh sách sắp xếp theo HighestWave giảm dần, sau đó theo TotalKills giảm dần
            var profiles = await _context.PlayerProfiles
                .Include(p => p.User)
                .OrderByDescending(p => p.HighestWave)
                .ThenByDescending(p => p.TotalKills)
                .Take(topCount)
                .ToListAsync();

            var response = new List<LeaderboardResponse>();
            for (int i = 0; i < profiles.Count; i++)
            {
                response.Add(new LeaderboardResponse
                {
                    Rank = i + 1,
                    Username = profiles[i].User?.Username ?? "N/A",
                    DisplayName = profiles[i].DisplayName,
                    HighestWave = profiles[i].HighestWave,
                    TotalKills = profiles[i].TotalKills,
                    TotalRuns = profiles[i].TotalRuns
                });
            }

            return response;
        }
    }
}
