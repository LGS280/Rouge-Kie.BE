using Rogue_Kie.BE.Contracts.RunHistory;
using Rogue_Kie.BE.DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.RunHistoryService
{
    public interface IRunHistoryService
    {
        Task<RunHistory> LogRunHistoryAsync(int userId, CreateRunHistoryRequest request);
        Task<List<RunHistoryResponse>> GetUserRunsAsync(int userId);
    }
}
