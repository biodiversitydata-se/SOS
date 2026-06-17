﻿const originalPolygonGeometries = new WeakMap();
const centroidGeometries = new WeakMap();

export function getMetersPerPixel(map) {
    const view = map && typeof map.getView === "function"
        ? map.getView()
        : null;
    const resolution = view && typeof view.getResolution === "function"
        ? view.getResolution()
        : null;

    if (!Number.isFinite(resolution) || resolution <= 0) {
        return null;
    }

    const projection = view && typeof view.getProjection === "function"
        ? view.getProjection()
        : null;
    const center = view && typeof view.getCenter === "function"
        ? view.getCenter()
        : null;

    if (window.ol?.proj &&
        typeof window.ol.proj.getPointResolution === "function" &&
        projection &&
        Array.isArray(center)) {
        const pointResolution = window.ol.proj.getPointResolution(projection, resolution, center, "m");
        if (Number.isFinite(pointResolution) && pointResolution > 0) {
            return pointResolution;
        }
    }

    const projectionCode = projection && typeof projection.getCode === "function"
        ? projection.getCode()
        : null;

    if (projectionCode === "EPSG:3857" && Array.isArray(center)) {
        const earthRadius = 6378137;
        const latitudeRadians = 2 * Math.atan(Math.exp(center[1] / earthRadius)) - Math.PI / 2;
        const metersPerPixel = resolution * Math.cos(latitudeRadians);
        if (Number.isFinite(metersPerPixel) && metersPerPixel > 0) {
            return metersPerPixel;
        }
    }

    if (projectionCode === "EPSG:4326" && Array.isArray(center)) {
        const latitudeRadians = center[1] * Math.PI / 180;
        const longitudeMetersPerPixel = Math.abs(resolution * 111320 * Math.cos(latitudeRadians));
        const latitudeMetersPerPixel = Math.abs(resolution * 110574);
        const metersPerPixel = Math.max(longitudeMetersPerPixel, latitudeMetersPerPixel);
        if (Number.isFinite(metersPerPixel) && metersPerPixel > 0) {
            return metersPerPixel;
        }
    }

    return resolution;
}

export function configurePolygonRendering(map, layerId, createPolygons, resultType, polygonToPointThresholdPx) {
    if (!map || !layerId) {
        return;
    }

    if (typeof resultType === "number" && polygonToPointThresholdPx === undefined) {
        polygonToPointThresholdPx = resultType;
        resultType = "";
    }

    map.__sosPolygonRenderingOptions = map.__sosPolygonRenderingOptions || {};
    map.__sosPolygonRenderingOptions[layerId] = {
        createPolygons: createPolygons || "No",
        resultType: typeof resultType === "string" ? resultType.toLowerCase() : "",
        polygonToPointThresholdPx: Number.isFinite(polygonToPointThresholdPx) && polygonToPointThresholdPx > 0
            ? polygonToPointThresholdPx
            : 5
    };

    if (!map.__sosPolygonRenderingResolutionListener) {
        map.__sosPolygonRenderingResolutionListener = map.getView().on("change:resolution", function () {
            Object.keys(map.__sosPolygonRenderingOptions || {}).forEach(function (configuredLayerId) {
                applyPolygonRendering(map, configuredLayerId);
            });
        });
    }

    applyPolygonRendering(map, layerId);
}

export function applyPolygonRendering(map, layerId) {
    const options = map?.__sosPolygonRenderingOptions?.[layerId];
    const layer = getLayerById(map, layerId);
    const source = layer && typeof layer.getSource === "function" ? layer.getSource() : null;
    const features = source && typeof source.getFeatures === "function" ? source.getFeatures() : [];
    const view = map && typeof map.getView === "function" ? map.getView() : null;
    const resolution = view && typeof view.getResolution === "function" ? view.getResolution() : null;

    if (!options || !Number.isFinite(resolution) || resolution <= 0) {
        return;
    }

    if (options.resultType !== "locations") {
        return;
    }

    features.forEach(function (feature) {
        const currentGeometry = feature.getGeometry && feature.getGeometry();
        const currentGeometryType = currentGeometry && currentGeometry.getType && currentGeometry.getType();
        let originalGeometry = originalPolygonGeometries.get(feature);

        if (!originalGeometry && isPolygonGeometryType(currentGeometryType)) {
            originalGeometry = currentGeometry.clone();
            originalPolygonGeometries.set(feature, originalGeometry);
        }

        if (!originalGeometry) {
            return;
        }

        if (options.createPolygons !== "Yes") {
            if (!isSameGeometry(feature.getGeometry(), originalGeometry)) {
                feature.setGeometry(originalGeometry.clone());
            }
            return;
        }

        const useCentroid = shouldRenderPolygonAsCentroid(
            originalGeometry,
            resolution,
            options.polygonToPointThresholdPx);
        if (useCentroid) {
            feature.setGeometry(getCentroidGeometry(feature, originalGeometry, map));
        } else {
            feature.setGeometry(originalGeometry.clone());
        }
    });
}

export function configureCoordinateUncertaintyLayer(map, layerId, geoJson, visible) {
    if (!map || !layerId ||
        !window.ol?.layer?.Vector ||
        !window.ol?.source?.Vector ||
        !window.ol?.geom?.Polygon ||
        !window.ol?.style?.Style ||
        !window.ol?.style?.Stroke) {
        return;
    }

    removeLayer(map, layerId);

    const featureCollection = parseGeoJson(geoJson);
    const source = new ol.source.Vector();

    if (featureCollection?.features && Array.isArray(featureCollection.features)) {
        const projection = map.getView().getProjection();

        featureCollection.features.forEach(function (geoJsonFeature) {
            const uncertaintyFeature = createCoordinateUncertaintyFeature(geoJsonFeature, projection);
            if (uncertaintyFeature) {
                source.addFeature(uncertaintyFeature);
            }
        });
    }

    const layer = new ol.layer.Vector({
        source: source,
        style: new ol.style.Style({
            stroke: new ol.style.Stroke({
                color: "#AAAAAA",
                width: 1
            })
        })
    });

    layer.set("id", layerId);
    layer.set("sosNonInteractive", true);
    layer.setVisible(visible !== false);
    layer.setZIndex(0);
    map.addLayer(layer);
}

export function configureClusterRadiusLayer(map, layerId, geoJson, radiusPropertyName, visible) {
    if (!map || !layerId || !radiusPropertyName ||
        !window.ol?.layer?.Vector ||
        !window.ol?.source?.Vector ||
        !window.ol?.geom?.Polygon ||
        !window.ol?.style?.Style ||
        !window.ol?.style?.Stroke) {
        return;
    }

    removeLayer(map, layerId);

    const featureCollection = parseGeoJson(geoJson);
    const source = new ol.source.Vector();

    if (featureCollection?.features && Array.isArray(featureCollection.features)) {
        const projection = map.getView().getProjection();

        featureCollection.features.forEach(function (geoJsonFeature) {
            const radiusFeature = createClusterRadiusFeature(geoJsonFeature, radiusPropertyName, projection);
            if (radiusFeature) {
                source.addFeature(radiusFeature);
            }
        });
    }

    const layer = new ol.layer.Vector({
        source: source,
        style: new ol.style.Style({
            stroke: new ol.style.Stroke({
                color: "#FF0000",
                width: 1.5
            })
        })
    });

    layer.set("id", layerId);
    layer.set("sosNonInteractive", true);
    layer.setVisible(visible === true);
    layer.setZIndex(0);
    map.addLayer(layer);
}

export function setLayerZIndex(map, layerId, zIndex) {
    const layer = getLayerById(map, layerId);
    if (layer && typeof layer.setZIndex === "function" && Number.isFinite(zIndex)) {
        layer.setZIndex(zIndex);
    }
}

export function setLayerVisible(map, layerId, visible) {
    const layer = getLayerById(map, layerId);
    if (layer && typeof layer.setVisible === "function") {
        layer.setVisible(visible === true);
    }
}

export function removeLayer(map, layerId) {
    const layer = getLayerById(map, layerId);
    if (map && layer && typeof map.removeLayer === "function") {
        map.removeLayer(layer);
    }
}

function getLayerById(map, layerId) {
    if (!map || typeof map.getAllLayers !== "function") {
        return null;
    }

    return map.getAllLayers().find(function (layer) {
        return layer.get && layer.get("id") === layerId;
    });
}

function isPolygonGeometryType(geometryType) {
    return geometryType === "Polygon" || geometryType === "MultiPolygon";
}

function isSameGeometry(featureGeometry, referenceGeometry) {
    return featureGeometry && referenceGeometry && featureGeometry.getType && featureGeometry.getType() === referenceGeometry.getType();
}

function shouldRenderPolygonAsCentroid(geometry, resolution, polygonToPointThresholdPx) {
    const extent = geometry.getExtent();
    const widthPx = Math.abs(ol.extent.getWidth(extent)) / resolution;
    const heightPx = Math.abs(ol.extent.getHeight(extent)) / resolution;
    const areaPx = typeof geometry.getArea === "function"
        ? Math.abs(geometry.getArea()) / (resolution * resolution)
        : widthPx * heightPx;
    const minimumRadiusPx = Number.isFinite(polygonToPointThresholdPx) && polygonToPointThresholdPx > 0
        ? polygonToPointThresholdPx
        : 5;
    const minimumDiameterPx = minimumRadiusPx * 2;
    const minimumAreaPx = Math.PI * minimumRadiusPx * minimumRadiusPx;

    return widthPx < minimumDiameterPx ||
        heightPx < minimumDiameterPx ||
        areaPx < minimumAreaPx;
}

function getCentroidGeometry(feature, originalGeometry, map) {
    const existing = centroidGeometries.get(feature);
    if (existing) {
        return existing.clone();
    }

    const locations = feature.get("locations") || feature.get("Locations");
    const location = getPropertyValue(locations, "location", "Location") ||
        feature.get("location") ||
        feature.get("Location");
    const longitude = location && Number(location.decimalLongitude);
    const latitude = location && Number(location.decimalLatitude);
    let coordinate = null;

    if (Number.isFinite(longitude) && Number.isFinite(latitude)) {
        const projection = map.getView().getProjection();
        coordinate = ol.proj.transform([longitude, latitude], "EPSG:4326", projection);
    } else if (originalGeometry.getType() === "Polygon" && typeof originalGeometry.getInteriorPoint === "function") {
        coordinate = originalGeometry.getInteriorPoint().getCoordinates();
    } else if (originalGeometry.getType() === "MultiPolygon" && typeof originalGeometry.getInteriorPoints === "function") {
        const interiorPoints = originalGeometry.getInteriorPoints().getCoordinates();
        coordinate = interiorPoints && interiorPoints.length > 0 ? interiorPoints[0] : null;
    }

    if (!coordinate) {
        coordinate = ol.extent.getCenter(originalGeometry.getExtent());
    }

    const centroidGeometry = new ol.geom.Point(coordinate);
    centroidGeometries.set(feature, centroidGeometry);
    return centroidGeometry.clone();
}

function parseGeoJson(geoJson) {
    if (!geoJson) {
        return null;
    }

    if (typeof geoJson === "object") {
        return geoJson;
    }

    try {
        return JSON.parse(geoJson);
    } catch {
        return null;
    }
}

function createCoordinateUncertaintyFeature(geoJsonFeature, projection) {
    const geometry = geoJsonFeature?.geometry;
    const properties = geoJsonFeature?.properties || {};
    const location = getLocationProperty(properties);

    if (!geometry || geometry.type !== "Point" || !location || toBoolean(properties.isCluster)) {
        return null;
    }

    const locationType = getPropertyValue(location, "type", "Type");
    if (isPolygonLocationType(locationType)) {
        return null;
    }

    const radiusMeters = Number(getPropertyValue(location, "coordinateUncertaintyInMeters", "CoordinateUncertaintyInMeters"));
    if (!Number.isFinite(radiusMeters) || radiusMeters <= 0) {
        return null;
    }

    const longitude = Number(getPropertyValue(location, "decimalLongitude", "DecimalLongitude"));
    const latitude = Number(getPropertyValue(location, "decimalLatitude", "DecimalLatitude"));
    if (!Number.isFinite(longitude) || !Number.isFinite(latitude)) {
        return null;
    }

    const polygonCoordinates = createGeodesicCircleCoordinates(longitude, latitude, radiusMeters, projection);
    if (!polygonCoordinates) {
        return null;
    }

    const feature = new ol.Feature({
        geometry: new ol.geom.Polygon([polygonCoordinates])
    });
    feature.set("sosNonInteractive", true);
    return feature;
}

function createClusterRadiusFeature(geoJsonFeature, radiusPropertyName, projection) {
    const geometry = geoJsonFeature?.geometry;
    const properties = geoJsonFeature?.properties || {};

    if (!geometry || geometry.type !== "Point" || !toBoolean(properties.isCluster)) {
        return null;
    }

    const radiusMeters = Number(
        getNestedPropertyValue(properties, radiusPropertyName) ??
        getPropertyValue(properties, radiusPropertyName, radiusPropertyName));
    if (!Number.isFinite(radiusMeters) || radiusMeters <= 0) {
        return null;
    }

    const coordinates = geometry.coordinates;
    if (!Array.isArray(coordinates) || coordinates.length < 2) {
        return null;
    }

    const longitude = Number(coordinates[0]);
    const latitude = Number(coordinates[1]);
    if (!Number.isFinite(longitude) || !Number.isFinite(latitude)) {
        return null;
    }

    const polygonCoordinates = createGeodesicCircleCoordinates(longitude, latitude, radiusMeters, projection);
    if (!polygonCoordinates) {
        return null;
    }

    const feature = new ol.Feature({
        geometry: new ol.geom.Polygon([polygonCoordinates])
    });
    feature.set("sosNonInteractive", true);
    return feature;
}

function getPropertyValue(source, camelName, pascalName) {
    if (!source) {
        return null;
    }

    if (Object.prototype.hasOwnProperty.call(source, camelName)) {
        return source[camelName];
    }

    if (Object.prototype.hasOwnProperty.call(source, pascalName)) {
        return source[pascalName];
    }

    return null;
}

function getNestedPropertyValue(source, propertyPath) {
    if (!source || !propertyPath || typeof propertyPath !== "string") {
        return null;
    }

    const parts = propertyPath.split(".");
    let current = source;
    for (const part of parts) {
        if (!current) {
            return null;
        }

        current = getPropertyValue(current, part, toPascalCase(part));
    }

    return current;
}

function getLocationProperty(properties) {
    const locations = getPropertyValue(properties, "locations", "Locations");
    return getPropertyValue(locations, "location", "Location") ||
        getPropertyValue(properties, "location", "Location");
}

function toPascalCase(value) {
    return value.length === 0
        ? value
        : value.charAt(0).toUpperCase() + value.slice(1);
}

function toBoolean(value) {
    if (typeof value === "boolean") {
        return value;
    }

    if (typeof value === "string") {
        return value.toLowerCase() === "true";
    }

    return value === 1;
}

function isPolygonLocationType(locationType) {
    if (typeof locationType === "number") {
        return locationType === 2;
    }

    if (typeof locationType !== "string") {
        return false;
    }

    return locationType.toLowerCase() === "polygon" || Number(locationType) === 2;
}

function createGeodesicCircleCoordinates(longitude, latitude, radiusMeters, projection) {
    const segmentCount = 96;
    const earthRadiusMeters = 6378137;
    const angularDistance = radiusMeters / earthRadiusMeters;
    const latitudeRadians = latitude * Math.PI / 180;
    const longitudeRadians = longitude * Math.PI / 180;
    const coordinates = [];

    for (let i = 0; i <= segmentCount; i++) {
        const bearing = 2 * Math.PI * i / segmentCount;
        const pointLatitude = Math.asin(
            Math.sin(latitudeRadians) * Math.cos(angularDistance) +
            Math.cos(latitudeRadians) * Math.sin(angularDistance) * Math.cos(bearing));
        const pointLongitude = longitudeRadians + Math.atan2(
            Math.sin(bearing) * Math.sin(angularDistance) * Math.cos(latitudeRadians),
            Math.cos(angularDistance) - Math.sin(latitudeRadians) * Math.sin(pointLatitude));
        const lonLat = [
            normalizeLongitude(pointLongitude * 180 / Math.PI),
            pointLatitude * 180 / Math.PI
        ];

        coordinates.push(ol.proj.transform(lonLat, "EPSG:4326", projection));
    }

    return coordinates;
}

function normalizeLongitude(longitude) {
    return ((longitude + 540) % 360) - 180;
}
