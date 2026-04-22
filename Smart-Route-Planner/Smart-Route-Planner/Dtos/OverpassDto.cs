namespace Smart_Route_Planner.Dtos
{
    public class OverpassResponse
    {
        public List<Element> elements { get; set; } = new();
    }

    public class Element
    {
        public string type { get; set; } = string.Empty; // node or way
        public long id { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public List<long> nodes { get; set; } = new();
    }
}
