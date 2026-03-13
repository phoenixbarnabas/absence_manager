namespace Entities.Models
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<Desk> Desks { get; set; } = new List<Desk>();
    }
}
