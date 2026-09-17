using System;
using System.Collections.Generic;

namespace Rogue_Kie.BE.Contracts.Admin
{
    public class AdminTransactionDto
    {
        public int TransactionId { get; set; }
        public long OrderCode { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int? ShopItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string CurrencyType { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class DailyRevenueDto
    {
        public string Date { get; set; } = string.Empty;
        public string DayLabel { get; set; } = string.Empty;
        public long Revenue { get; set; }
        public int TransactionCount { get; set; }
    }

    public class WeeklyRevenueDto
    {
        public string WeekLabel { get; set; } = string.Empty;
        public long Revenue { get; set; }
        public long GemVolume { get; set; }
    }

    public class PaymentAnalyticsResponse
    {
        public long TotalRevenueVND { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int CancelledTransactions { get; set; }
        public long TotalGemsInEconomy { get; set; }
        public long TotalCoinsInEconomy { get; set; }
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
        public List<WeeklyRevenueDto> WeeklyRevenue { get; set; } = new();
    }

    public class DailyPlayerActivityDto
    {
        public string DayName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int TotalRuns { get; set; }
        public int SurvivedRuns { get; set; }
        public int DefeatRuns { get; set; }
        public int UniquePlayers { get; set; }
    }

    public class PlayerAnalyticsResponse
    {
        public int TotalRegisteredUsers { get; set; }
        public int TotalRunsPlayed { get; set; }
        public int OnlineCCU { get; set; }
        public int ActiveRooms { get; set; }
        public List<DailyPlayerActivityDto> DailyActivity { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string TimeFormatted { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "info"; // info, success, warning, danger
    }
}
