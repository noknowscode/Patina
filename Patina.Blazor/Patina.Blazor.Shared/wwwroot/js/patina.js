// Patina — the JS the shared UI needs. Loaded as a module by Blazor (IJSRuntime "import").
// Three jobs: theme and motion attributes on <html>, file download, and the Leaflet map.

export function applyTheme(theme, reduceMotion) {
  const root = document.documentElement;
  if (theme === "Light" || theme === "Dark") root.setAttribute("data-theme", theme.toLowerCase());
  else root.removeAttribute("data-theme");
  if (reduceMotion) root.setAttribute("data-reduce-motion", "true");
  else root.removeAttribute("data-reduce-motion");
}

// Hands the viewer a file: used by the web head for export (the MAUI head shares a file natively).
export async function downloadFile(fileName, contentType, streamRef) {
  const buffer = await streamRef.arrayBuffer();
  const blob = new Blob([buffer], { type: contentType });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

// ---------- map (Leaflet, loaded by the host page) ----------
const maps = new Map();

export function createMap(element, dotnet, centre, zoom) {
  if (!window.L || !element) return false;
  const map = L.map(element, { zoomControl: true, attributionControl: true });
  L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: "&copy; OpenStreetMap contributors",
  }).addTo(map);
  map.setView([centre.lat, centre.lon], zoom);
  const layer = L.layerGroup().addTo(map);
  maps.set(element, { map, layer, dotnet });
  map.on("click", () => dotnet.invokeMethodAsync("OnMapClick"));
  return true;
}

export function setPins(element, pins, selectedId) {
  const entry = maps.get(element);
  if (!entry) return;
  entry.layer.clearLayers();
  for (const p of pins) {
    const selected = p.id === selectedId;
    const icon = L.divIcon({
      className: "",
      html: `<div class="pin${selected ? " is-selected" : ""}" style="background:${p.color}" title="${escapeHtml(p.title)}"></div>`,
      iconSize: selected ? [22, 22] : [14, 14],
      iconAnchor: selected ? [11, 11] : [7, 7],
    });
    const marker = L.marker([p.lat, p.lon], { icon, keyboard: true, title: p.title, zIndexOffset: selected ? 1000 : 0 });
    marker.on("click", (e) => {
      L.DomEvent.stopPropagation(e);
      entry.dotnet.invokeMethodAsync("OnPinClick", p.id);
    });
    marker.addTo(entry.layer);
  }
}

export function panTo(element, lat, lon) {
  const entry = maps.get(element);
  if (entry) entry.map.panTo([lat, lon]);
}

export function invalidateSize(element) {
  const entry = maps.get(element);
  if (entry) entry.map.invalidateSize();
}

export function destroyMap(element) {
  const entry = maps.get(element);
  if (!entry) return;
  entry.map.remove();
  maps.delete(element);
}

function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}

// Brings a list row into view when its pin is selected on the map.
export function scrollIntoView(element) {
  element?.scrollIntoView({ block: "nearest", behavior: "smooth" });
}
