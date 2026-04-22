namespace Smart_Route_Planner.Models
{
    public class Node
    {
        public long Id { get; set; }
        public long OsmId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }

        public virtual List<Apartment>? Apartments { get; set; }
        public virtual List<Edge>? EdgesSources { get; set; }
        public virtual List<Edge>? EdgesDistinations { get; set; }
    }
}
