namespace Entities.Models
{
    public class Desk
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public string Name { get; set; }
        public Room Room { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
