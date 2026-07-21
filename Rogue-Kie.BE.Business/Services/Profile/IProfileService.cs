using Rogue_Kie.BE.Contracts.Leaderboard;
using Rogue_Kie.BE.Contracts.Profile;
using Rogue_Kie.BE.DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.Profile
{
    public interface IProfileService
    {
        Task<PlayerProfile> GetOrCreateProfileAsync(int userId);
        Task<PlayerProfile?> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<List<LeaderboardResponse>> GetLeaderboardAsync(int topCount = 20);
    }
}
