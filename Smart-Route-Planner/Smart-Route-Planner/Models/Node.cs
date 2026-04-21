namespace Smart_Route_Planner.Models
{
    public class Node
    {
        public long Id { get; set; }
        public long OsmId { get; set; }         // OSM id
        public double Lat { get; set; }
        public double Lng { get; set; }

        public virtual List<Apartment>? Apartments { get; set; }
    }
}
