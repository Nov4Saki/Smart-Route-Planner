namespace Smart_Route_Planner.Models
{
    public class Apartment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }

        public long NodeId { get; set; }
        public virtual Node? Node { get; set; }
    }
}
