using Smart_Route_Planner.Dtos;
using Smart_Route_Planner.Models;
using System.Text.Json;

namespace Smart_Route_Planner.Data
{
    public class SeedService
    {
        private readonly HttpClient _http;

        public SeedService(HttpClient http)
        {
            _http = http;
        }


        public async Task SeedAsync(AppDbContext db)
        {
            var query = @"
                [out:json];
                (
                  way[""highway""](31.38,31.65,31.45,31.72);
                );
                (._;>;);
                out body;";

            var response = await _http.PostAsync(
                "https://overpass-api.de/api/interpreter",
                new StringContent(query)
            );

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<OverpassResponse>(json);


            var nodesDict = new Dictionary<long, Node>();
            foreach (var el in data.elements.Where(e => e.type == "node"))
            {
                nodesDict[el.id] = new Node
                {
                    OsmId = el.id,
                    Lat = el.lat,
                    Lng = el.lon
                };
            }


            var edges = new List<Edge>();
            foreach (var way in data.elements.Where(e => e.type == "way"))
            {
                for (int i = 0; i < way.nodes.Count - 1; i++)
                {
                    var from = nodesDict[way.nodes[i]];
                    var to = nodesDict[way.nodes[i + 1]];

                    edges.Add(new Edge
                    {
                        FromNodeId = from.OsmId,
                        ToNodeId = to.OsmId,
                        Distance = Haversine(from, to)
                    });
                }
            }

            db.Nodes.AddRange(nodesDict.Values);
            db.Edges.AddRange(edges);
            await db.SaveChangesAsync();


            var rand = new Random();
            var nodeList = nodesDict.Values.ToList();
            double minLat = 31.38;
            double maxLat = 31.45;
            double minLng = 31.65;
            double maxLng = 31.72;


            var apartments = new List<Apartment>();
            for (int i = 0; i < 20; i++)
            {
                //var randomNode = nodeList[rand.Next(nodeList.Count)];
                double lat = minLat + rand.NextDouble() * (maxLat - minLat);
                double lng = minLng + rand.NextDouble() * (maxLng - minLng);
                var nearestNode = FindNearestNode(lat,lng, nodeList);

                apartments.Add(new Apartment
                {
                    Lat = lat,
                    Lng = lng,
                    NodeId = nearestNode
                });
            }

            db.Apartments.AddRange(apartments);
            await db.SaveChangesAsync();
        }

        private double Haversine(Node a, Node b)
        {
            double R = 6371e3;
            double lat1 = a.Lat * Math.PI / 180;
            double lat2 = b.Lat * Math.PI / 180;
            double dLat = (b.Lat - a.Lat) * Math.PI / 180;
            double dLon = (b.Lng - a.Lng) * Math.PI / 180;

            double x = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(lat1) * Math.Cos(lat2) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(x), Math.Sqrt(1 - x));
            return R * c;
        }

        private long FindNearestNode(double lat, double lng, List<Node> nodes)
        {
            double minDist = double.MaxValue;
            long nearestId = -1;
            Node tempNode = new Node { OsmId = -1, Lat = lat, Lng = lng };

            foreach (var node in nodes)
            {
                double dist = Haversine(tempNode, node);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearestId = node.OsmId;
                }
            }

            return nearestId;
        }
    }
}
