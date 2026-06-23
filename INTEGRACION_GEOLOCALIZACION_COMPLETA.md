# Geolocalización

## APIs utilizadas
- OpenStreetMap
- Leaflet
- Nominatim
- OSRM

## Agregar en Viajes.cshtml

```html
<link rel="stylesheet" href="https://unpkg.com/leaflet/dist/leaflet.css" />
<div id="map" style="height:400px"></div>
<script src="https://unpkg.com/leaflet/dist/leaflet.js"></script>
```

## JavaScript

```javascript
async function calcularRuta(){
 // Nominatim -> coordenadas
 // OSRM -> distancia y tiempo
}
```

## Nuevas columnas SQL

ALTER TABLE Viajes ADD
LatitudOrigen FLOAT,
LongitudOrigen FLOAT,
LatitudDestino FLOAT,
LongitudDestino FLOAT,
TiempoEstimado INT;
