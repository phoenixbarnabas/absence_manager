namespace Entities.Models
{
    public class Desk
    {
        public string Id { get; set; }
        public int RoomId { get; set; }
        public string Name { get; set; }
        public Room Room { get; set; }
    }
}
