# Smart Route Planner — FastAPI Service

## What it does

Receives a GPS coordinate (`lat`, `lng`), snaps it to the nearest road-graph node,
runs **Dijkstra** across the full graph, and returns the **5 closest apartments**
sorted by actual road distance.

---

## Requirements

- Python **3.10+**
- pip

---

## Setup

You can use a virtual environment or your existing global Python install.

### Option A — Virtual environment (recommended)

```bash
# 1. Navigate to this folder
cd fastapi_service

# 2. Create and activate a virtual environment
python -m venv venv

# Windows
venv\Scripts\activate

# macOS / Linux
source venv/bin/activate

# 3. Install dependencies
pip install -r requirements.txt
```

### Option B — Global / existing environment

```bash
pip install -r requirements.txt
```

> If you already have `fastapi`, `uvicorn`, and `pymssql` installed, you're ready to go.

---

## Run the server

```bash
uvicorn main:app --reload --port 8000
```

The server will:
1. Connect to the database and load the full graph into memory
2. Print how many nodes, edges, and apartments were loaded
3. Start listening on `http://localhost:8000`

---

## API Usage

### Endpoint

```
GET /nearest-apartments?lat=<latitude>&lng=<longitude>
```

### Example

```
GET http://localhost:8000/nearest-apartments?lat=30.05&lng=31.23
```

### Response

```json
{
  "apartments": [
    {
      "id": 21,
      "name": "Apartment Name",
      "type": "Studio",
      "lat": 31.389,
      "lng": 31.688,
      "distance": 7239.52
    },
    ...
  ]
}
```

`distance` = shortest road-path distance (sum of edge weights), **not** straight-line.

---

## Interactive Docs

FastAPI auto-generates docs while the server is running:

| UI | URL |
|---|---|
| Swagger | http://localhost:8000/docs |
| ReDoc | http://localhost:8000/redoc |

