namespace Entities.Dtos.AppUserDtos
{
    public class LeaveBalanceDto
    {
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int SickLeaveDays { get; set; }
        public int PendingDays { get; set; }
        public int RemainingDays { get; set; }
    }
}
