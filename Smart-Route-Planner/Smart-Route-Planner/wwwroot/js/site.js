function initIndexPage() {
    const mapElement = document.getElementById("map");
    const coordsText = document.getElementById("coords-text");
    const coordsBox = document.getElementById("coords-box");
    const findBtn = document.getElementById("find-btn");
    const instruction = document.getElementById("instruction");

    if (!mapElement || !coordsText || !coordsBox || !findBtn || !instruction || typeof L === "undefined") return;

    const map = L.map("map", {
        center: [31.4415, 31.6547],
        zoom: 14,
        zoomControl: true,
    });

    L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
        subdomains: "abcd",
        maxZoom: 20,
    }).addTo(map);

    let marker = null;
    let selectedLat = null;
    let selectedLng = null;

    const pinIcon = L.divIcon({
        className: "",
        html: `
      <div style="position:relative;width:40px;height:40px;display:flex;align-items:center;justify-content:center;">
        <div style="
          width:16px;height:16px;border-radius:50%;
          background:#4f8ef7;
          box-shadow:0 0 0 4px rgba(79,142,247,0.25), 0 0 20px rgba(79,142,247,0.6);
          position:relative;
        ">
          <div style="
            position:absolute;inset:-6px;border-radius:50%;
            border:2px solid rgba(79,142,247,0.5);
            animation:pulse-ring 1.6s ease-out infinite;
          "></div>
        </div>
      </div>`,
        iconSize: [40, 40],
        iconAnchor: [20, 20],
    });

    map.on("click", (e) => {
        selectedLat = e.latlng.lat.toFixed(6);
        selectedLng = e.latlng.lng.toFixed(6);

        if (marker) {
            marker.setLatLng(e.latlng);
        } else {
            marker = L.marker(e.latlng, { icon: pinIcon }).addTo(map);
        }

        coordsText.innerHTML = `${selectedLat}, ${selectedLng}`;
        coordsBox.classList.add("active");
        findBtn.disabled = false;
        instruction.innerHTML = '<i class="bi bi-check-circle-fill" style="color:#34d399"></i> Location pinned — click again to change';
    });

    findBtn.addEventListener("click", () => {
        if (!selectedLat || !selectedLng) return;
        window.location.href = `/home/result?lat=${selectedLat}&lng=${selectedLng}`; // TODO: Add the coordinates to the URL
    });
}

function initResultsPage() {
    const mapElement = document.getElementById("map");
    const listEl = document.getElementById("apt-list");
    const userCoordsEl = document.getElementById("user-coords");
    const aptCountEl = document.getElementById("apt-count");

    if (!mapElement || !listEl || !userCoordsEl || !aptCountEl || typeof L === "undefined") return;

    const params = new URLSearchParams(window.location.search);
    const userLat = parseFloat(params.get("lat")) || 31.4415;
    const userLng = parseFloat(params.get("lng")) || 31.6547;

    userCoordsEl.textContent = `${userLat.toFixed(5)}, ${userLng.toFixed(5)}`;

    const map = L.map("map", {
        center: [userLat, userLng],
        zoom: 14,
    });

    L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
        subdomains: "abcd",
        maxZoom: 20,
    }).addTo(map);

    const userIcon = L.divIcon({
        className: "",
        html: `<div style="
          width:18px;height:18px;border-radius:50%;
          background:#4f8ef7;
          box-shadow: 0 0 0 4px rgba(79,142,247,0.25), 0 0 22px rgba(79,142,247,0.7);
          border: 2px solid #fff;
        "></div>`,
        iconSize: [18, 18],
        iconAnchor: [9, 9],
    });

    L.marker([userLat, userLng], { icon: userIcon })
        .addTo(map)
        .bindPopup('<b style="font-family:Syne,sans-serif">Your Location</b>');

    function makeAptIcon(selected = false) {
        const color = selected ? "#f7794f" : "#7c5cfc";
        return L.divIcon({
            className: "",
            html: `<div style="
        position:relative;
        display:flex;align-items:center;justify-content:center;
      ">
        <div style="
          width:13px;height:13px;border-radius:50%;
          background:${color};
          box-shadow: 0 0 0 3px rgba(124,92,252,0.25), 0 0 14px ${color}99;
          border: 2px solid rgba(255,255,255,0.6);
        "></div>
      </div>`,
            iconSize: [13, 13],
            iconAnchor: [7, 7],
        });
    }

    const aptMarkers = {};
    apartments.forEach((apt) => {
        aptMarkers[apt.id] = L.marker([apt.lat, apt.lng], { icon: makeAptIcon() })
            .addTo(map)
            .bindPopup(`<b style="font-family:Syne,sans-serif">${apt.name}</b><br/>${apt.type} · ${apt.distance}m away`);
    });

    let activePolyline = null;
    let selectedId = null;

    aptCountEl.textContent = apartments.length;

    apartments.forEach((apt) => {
        const card = document.createElement("div");
        card.className = "apt-card";
        card.dataset.id = apt.id;
        card.innerHTML = `
      <div class="apt-row-top">
        <div class="apt-name">${apt.name}</div>
        <span class="apt-type">${apt.type}</span>
      </div>
      <div class="apt-meta">
        <div class="apt-meta-item distance">
          <i class="bi bi-signpost-split-fill"></i>
          ${apt.distance} m away
        </div>
        <div class="apt-meta-item price">
          <i class="bi bi-tag-fill"></i>
          EGP ${apt.price.toLocaleString()}/mo
        </div>
      </div>
      <div class="route-btn">
        <i class="bi bi-arrow-right-circle-fill"></i>
        Show route on map
      </div>
    `;
        card.addEventListener("click", () => selectApartment(apt));
        listEl.appendChild(card);
    });

    async function fetchRoute(startLat, startLng, endLat, endLng) {
        const url =
            `https://router.project-osrm.org/route/v1/driving/` +
            `${startLng},${startLat};${endLng},${endLat}` +
            `?overview=full&geometries=geojson`;
        const res = await fetch(url);
        if (!res.ok) throw new Error("OSRM request failed");
        const data = await res.json();
        if (!data.routes || data.routes.length === 0) throw new Error("No route found");
        return data.routes[0];
    }

    function osrmCoordsToLatLng(coords) {
        return coords.map(([lng, lat]) => [lat, lng]);
    }

    function selectApartment(apt) {
        if (selectedId) {
            document.querySelector(`.apt-card[data-id="${selectedId}"]`)?.classList.remove("selected");
            aptMarkers[selectedId].setIcon(makeAptIcon(false));
        }

        if (activePolyline) {
            map.removeLayer(activePolyline);
            activePolyline = null;
        }

        const card = document.querySelector(`.apt-card[data-id="${apt.id}"]`);
        card?.classList.add("selected");
        selectedId = apt.id;

        aptMarkers[apt.id].setIcon(makeAptIcon(true));
        aptMarkers[apt.id].openPopup();

        const info = document.getElementById("route-info");
        info.classList.add("visible", "loading");
        document.getElementById("route-name").textContent = apt.name;
        document.getElementById("route-dist").textContent = "...";

        fetchRoute(userLat, userLng, apt.lat, apt.lng)
            .then((route) => {
                const latlngs = osrmCoordsToLatLng(route.geometry.coordinates);
                activePolyline = L.polyline(latlngs, {
                    color: "#4f8ef7",
                    weight: 4,
                    opacity: 0.9,
                    lineJoin: "round",
                    lineCap: "round",
                }).addTo(map);

                map.fitBounds(activePolyline.getBounds(), { padding: [60, 60], maxZoom: 16 });

                const distM = Math.round(route.distance);
                const distLabel = distM >= 1000 ? (distM / 1000).toFixed(1) + " km" : distM + " m";
                document.getElementById("route-dist").textContent = distLabel;
                info.classList.remove("loading");
            })
            .catch(() => {
                activePolyline = L.polyline([[userLat, userLng], [apt.lat, apt.lng]], {
                    color: "#4f8ef7",
                    weight: 2.5,
                    opacity: 0.7,
                    dashArray: "8 6",
                }).addTo(map);
                map.fitBounds([[userLat, userLng], [apt.lat, apt.lng]], { padding: [60, 60], maxZoom: 16 });
                document.getElementById("route-dist").textContent = `${apt.distance} m (est.)`;
                info.classList.remove("loading");
            });
    }
}

document.addEventListener("DOMContentLoaded", () => {
    initIndexPage();
    initResultsPage();
});
