namespace Smart_Route_Planner.Models
{
    public class Edge
    {
        public long Id { get; set; }
        public long FromNodeId { get; set; }
        public long ToNodeId { get; set; }
        public double Distance { get; set; }
    }
}
