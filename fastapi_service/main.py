import heapq
import math
from collections import defaultdict
from contextlib import asynccontextmanager

import pymssql
from fastapi import FastAPI, Query
from fastapi.middleware.cors import CORSMiddleware

# ─── Database Config ─────────────────────────────────────────────────────────

DB_CONFIG = {
    "server": "db49002.public.databaseasp.net",
    "database": "db49002",
    "user": "db49002",
    "password": "tS?36+bT=R9p",
}

# ─── In-memory graph data (loaded once at startup) ───────────────────────────

nodes: dict[int, tuple[float, float]] = {}        # node_id -> (lat, lng)
adj: dict[int, list[tuple[int, float]]] = defaultdict(list)  # adjacency list
apartments: list[dict] = []                         # list of apartment dicts


def load_graph():
    """Load all nodes, edges, and apartments from the database into memory."""
    conn = pymssql.connect(**DB_CONFIG)
    cursor = conn.cursor(as_dict=True)

    # Load nodes
    cursor.execute("SELECT Id, OsmId, Lat, Lng FROM Nodes")
    for row in cursor:
        nodes[row["Id"]] = (row["Lat"], row["Lng"])

    # Load edges (bidirectional)
    cursor.execute("SELECT Id, FromNodeId, ToNodeId, Distance FROM Edges")
    for row in cursor:
        adj[row["FromNodeId"]].append((row["ToNodeId"], row["Distance"]))
        adj[row["ToNodeId"]].append((row["FromNodeId"], row["Distance"]))

    # Load apartments
    cursor.execute("SELECT Id, Name, Type, Lat, Lng, NodeId FROM Apartments")
    for row in cursor:
        apartments.append({
            "id": row["Id"],
            "name": row["Name"],
            "type": row["Type"],
            "lat": row["Lat"],
            "lng": row["Lng"],
            "node_id": row["NodeId"],
        })

    conn.close()
    print(f"Loaded {len(nodes)} nodes, {sum(len(v) for v in adj.values())} directed edges, {len(apartments)} apartments")


# ─── Haversine distance (to find nearest node to input coords) ───────────────

def haversine(lat1: float, lng1: float, lat2: float, lng2: float) -> float:
    """Return distance in km between two GPS points."""
    R = 6371.0  # Earth radius in km
    dlat = math.radians(lat2 - lat1)
    dlng = math.radians(lng2 - lng1)
    a = (math.sin(dlat / 2) ** 2
         + math.cos(math.radians(lat1)) * math.cos(math.radians(lat2))
         * math.sin(dlng / 2) ** 2)
    return R * 2 * math.atan2(math.sqrt(a), math.sqrt(1 - a))


def find_nearest_node(lat: float, lng: float) -> int:
    """Find the node ID closest to the given lat/lng using Haversine."""
    best_id = -1
    best_dist = float("inf")
    for node_id, (nlat, nlng) in nodes.items():
        d = haversine(lat, lng, nlat, nlng)
        if d < best_dist:
            best_dist = d
            best_id = node_id
    return best_id


# ─── Dijkstra ────────────────────────────────────────────────────────────────

def dijkstra(start_id: int) -> dict[int, float]:
    """Run Dijkstra from start_id. Returns {node_id: shortest_distance}."""
    dist: dict[int, float] = {start_id: 0.0}
    pq = [(0.0, start_id)]

    while pq:
        d, u = heapq.heappop(pq)
        if d > dist.get(u, float("inf")):
            continue
        for v, w in adj.get(u, []):
            nd = d + w
            if nd < dist.get(v, float("inf")):
                dist[v] = nd
                heapq.heappush(pq, (nd, v))

    return dist


# ─── FastAPI App ──────────────────────────────────────────────────────────────

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: load graph into memory
    print("Loading graph from database...")
    load_graph()
    print("Graph loaded!")
    yield
    # Shutdown: nothing to clean up


app = FastAPI(title="Smart Route Planner API", lifespan=lifespan)

# Allow CORS so the C# frontend or any client can call this
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/nearest-apartments")
def get_nearest_apartments(
    lat: float = Query(..., description="Latitude of the user"),
    lng: float = Query(..., description="Longitude of the user"),
):
    """
    1. Find nearest graph node to (lat, lng)
    2. Run Dijkstra from that node
    3. Return the 5 closest apartments by graph distance
    """
    # Step 1: snap to nearest node
    start_node = find_nearest_node(lat, lng)

    # Step 2: shortest paths from start
    distances = dijkstra(start_node)

    # Step 3: rank apartments by graph distance
    results = []
    for apt in apartments:
        node_dist = distances.get(apt["node_id"])
        if node_dist is not None:
            results.append({
                "id": apt["id"],
                "name": apt["name"],
                "type": apt["type"],
                "lat": apt["lat"],
                "lng": apt["lng"],
                "distance": round(node_dist, 6),
            })

    # Sort by distance and take top 5
    results.sort(key=lambda x: x["distance"])
    return {"apartments": results[:5]}
