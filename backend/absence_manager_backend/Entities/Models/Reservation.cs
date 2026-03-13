namespace Entities.Models
{
    public class Reservation
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DeskId { get; set; }
        public DateOnly Date { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public AppUser User { get; set; }
        public Desk Desk { get; set; }
        public Reservation()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
