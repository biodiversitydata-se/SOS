import * as openLayersMap from "/openLayersMap.js";

let map = null;
let dotNetRef = null;
let moveEndKey = null;
let singleClickKey = null;

const locationsLayerId = "locations";
const pointStylesByLocationDisplayStatus = new Map();
const polygonStylesByLocationDisplayStatus = new Map();
let highlightedLocationIds = new Set();
let highlightedFeatureIds = new Set();
let previouslyHighlightedMapFeatures = [];

const displayStatusStyleDefinitions = {
    present: {
        pointFill: "yellow",
        pointStroke: "black",
        polygonFill: "rgba(255, 255, 0, 0.25)",
        polygonStroke: "black",
        diffusedFill: "rgba(255, 0, 0, 0.20)",
        diffusedStroke: "rgba(180, 0, 0, 0.85)"
    },
    presentsensitive: {
        pointFill: "#d7191c",
        pointStroke: "black",
        polygonFill: "rgba(255, 0, 0, 0.28)",
        polygonStroke: "rgba(180, 0, 0, 0.90)",
        diffusedFill: "rgba(120, 0, 0, 0.34)",
        diffusedStroke: "rgba(95, 0, 0, 0.90)"
    },
    treated: {
        pointFill: "#f97316",
        pointStroke: "black",
        polygonFill: "rgba(249, 115, 22, 0.32)",
        polygonStroke: "rgba(194, 65, 12, 0.90)",
        diffusedFill: "rgba(249, 115, 22, 0.32)",
        diffusedStroke: "rgba(194, 65, 12, 0.90)"
    },
    notrecovered: {
        pointFill: "#8a8a8a",
        pointStroke: "black",
        polygonFill: "rgba(128, 128, 128, 0.30)",
        polygonStroke: "rgba(92, 92, 92, 0.90)",
        diffusedFill: "rgba(128, 128, 128, 0.30)",
        diffusedStroke: "rgba(92, 92, 92, 0.90)"
    },
    notpresent: {
        pointFill: "black",
        pointStroke: "black",
        polygonFill: "rgba(0, 0, 0, 0.30)",
        polygonStroke: "rgba(0, 0, 0, 0.90)",
        diffusedFill: "rgba(0, 0, 0, 0.30)",
        diffusedStroke: "rgba(0, 0, 0, 0.90)"
    },
    dividedbyyearuntil: {
        pointFill: "#ff69b4",
        pointStroke: "black",
        polygonFill: "rgba(255, 105, 180, 0.30)",
        polygonStroke: "rgba(214, 51, 132, 0.90)",
        diffusedFill: "rgba(255, 105, 180, 0.30)",
        diffusedStroke: "rgba(214, 51, 132, 0.90)"
    },
    cluster: {
        pointFill: "dodgerblue",
        pointStroke: "black",
        polygonFill: "rgba(30, 144, 255, 0.25)",
        polygonStroke: "rgba(30, 96, 200, 0.90)",
        diffusedFill: "rgba(30, 144, 255, 0.25)",
        diffusedStroke: "rgba(30, 96, 200, 0.90)"
    }
};

const numericDisplayStatusKeys = {
    0: "present",
    1: "presentsensitive",
    2: "treated",
    3: "notrecovered",
    4: "notpresent",
    5: "dividedbyyearuntil",
    6: "cluster"
};

export function initMap(container, blazorDotNetRef, zoom, centerLon, centerLat) {
    if (!window.ol || !container) {
        return;
    }

    destroyMap();

    dotNetRef = blazorDotNetRef;

    const baseLayer = new ol.layer.Tile({
        source: new ol.source.OSM()
    });
    baseLayer.set("id", "osm-base");

    map = new ol.Map({
        target: container,
        layers: [baseLayer],
        view: new ol.View({
            center: ol.proj.fromLonLat([centerLon, centerLat]),
            zoom: zoom,
            projection: "EPSG:3857"
        })
    });

    moveEndKey = map.on("moveend", notifyExtentChanged);
    singleClickKey = map.on("singleclick", onSingleClick);

    requestAnimationFrame(function () {
        map?.updateSize();
        notifyExtentChanged();
    });
}

export function destroyMap() {
    if (moveEndKey) {
        ol.Observable.unByKey(moveEndKey);
        moveEndKey = null;
    }

    if (singleClickKey) {
        ol.Observable.unByKey(singleClickKey);
        singleClickKey = null;
    }

    if (map) {
        map.setTarget(null);
        map = null;
    }

    dotNetRef = null;
    highlightedLocationIds = new Set();
    highlightedFeatureIds = new Set();
    previouslyHighlightedMapFeatures = [];
}

export function getExtent() {
    if (!map) {
        return null;
    }

    const view = map.getView();
    const size = map.getSize();
    if (!view || !Array.isArray(size)) {
        return null;
    }

    const extent3857 = view.calculateExtent(size);
    const bottomLeft = ol.proj.toLonLat([extent3857[0], extent3857[1]]);
    const topRight = ol.proj.toLonLat([extent3857[2], extent3857[3]]);

    return {
        x1: bottomLeft[0],
        y1: bottomLeft[1],
        x2: topRight[0],
        y2: topRight[1],
        zoom: view.getZoom() ?? 0
    };
}

export function getMetersPerPixel() {
    return openLayersMap.getMetersPerPixel(map);
}

export function configurePolygonRendering(layerId, createPolygons, resultType, polygonToPointThresholdPx) {
    openLayersMap.configurePolygonRendering(
        map,
        layerId,
        createPolygons,
        resultType,
        polygonToPointThresholdPx);
}

export function configureCoordinateUncertaintyLayer(layerId, geoJson, visible) {
    openLayersMap.configureCoordinateUncertaintyLayer(map, layerId, geoJson, visible);
}

export function configureClusterRadiusLayer(layerId, geoJson, radiusPropertyName, visible) {
    openLayersMap.configureClusterRadiusLayer(map, layerId, geoJson, radiusPropertyName, visible);
}

export function setLayerZIndex(layerId, zIndex) {
    openLayersMap.setLayerZIndex(map, layerId, zIndex);
}

export function setLayerVisible(layerId, visible) {
    openLayersMap.setLayerVisible(map, layerId, visible);
}

export function setHighlightedLocationIds(locationIds, featureIds) {
    highlightedLocationIds = new Set(
        (Array.isArray(locationIds) ? locationIds : [])
            .map(normalizeLocationId)
            .filter(Boolean));
    highlightedFeatureIds = new Set(
        (Array.isArray(featureIds) ? featureIds : [])
            .map(normalizeLocationId)
            .filter(Boolean));

    applyHighlightedLocationStyles();
}

export function loadGeoJsonLayer(layerId, geoJson, resultType, lowObservationCountBreak, highObservationCountBreak) {
    if (!map || !layerId) {
        return;
    }

    removeLayer(layerId);

    if (!geoJson) {
        return;
    }

    const source = new ol.source.Vector();
    const format = new ol.format.GeoJSON({
        dataProjection: "EPSG:4326",
        featureProjection: map.getView().getProjection()
    });

    let features = [];
    try {
        features = format.readFeatures(geoJson);
    } catch (error) {
        console.error("LocationsSearch: failed to parse GeoJSON", error);
        return;
    }

    source.addFeatures(features);

    const normalizedResultType = typeof resultType === "string"
        ? resultType.toLowerCase()
        : "";

    const styleContext = {
        resultType: normalizedResultType,
        lowObservationCountBreak: toFiniteNumber(lowObservationCountBreak),
        highObservationCountBreak: toFiniteNumber(highObservationCountBreak)
    };

    const layer = new ol.layer.Vector({
        source: source,
        style: function (feature) {
            return getObservationStyle(feature, styleContext);
        }
    });

    layer.set("id", layerId);
    layer.setZIndex(20);
    map.addLayer(layer);

    if (layerId === locationsLayerId) {
        applyHighlightedLocationStyles();
    }
}

function notifyExtentChanged() {
    const extent = getExtent();
    if (!extent || !dotNetRef) {
        return;
    }

    dotNetRef.invokeMethodAsync("OnExtentChangedFromJs", extent).catch(function () {
    });
}

function onSingleClick(event) {
    if (!map || !dotNetRef) {
        return;
    }

    const clickedFeatures = getInteractiveObservationFeaturesAtPixel(event.pixel);
    const payload = clickedFeatures.map(toFeatureClickPayload);

    dotNetRef.invokeMethodAsync(
        "OnFeaturesClickedFromJs",
        payload).catch(function () {
        });
}

function getInteractiveObservationFeaturesAtPixel(pixel) {
    const features = [];
    if (!map) {
        return features;
    }

    map.forEachFeatureAtPixel(
        pixel,
        function (feature, layer) {
            if (!isInteractiveObservationFeature(feature, layer)) {
                return undefined;
            }

            features.push(feature);
            return undefined;
        },
        { hitTolerance: 4 });

    return features;
}

function isInteractiveObservationFeature(feature, layer) {
    if (!feature || !layer || layer.get("sosNonInteractive") || feature.get("sosNonInteractive")) {
        return false;
    }

    return layer.get("id") === locationsLayerId;
}

function toFeatureClickPayload(feature) {
    const id = feature.getId();
    const featureId = id === null || id === undefined ? null : id.toString();
    if (featureId) {
        return { featureId: featureId };
    }

    const properties = { ...feature.getProperties() };
    delete properties.geometry;

    return {
        featureId: null,
        properties: properties
    };
}

export function removeLayer(layerId) {
    openLayersMap.removeLayer(map, layerId);
}

function getLayerById(layerId) {
    if (!map || typeof map.getAllLayers !== "function") {
        return null;
    }

    return map.getAllLayers().find(function (layer) {
        return layer.get && layer.get("id") === layerId;
    });
}

function applyHighlightedLocationStyles() {
    if (!map) {
        return;
    }

    for (const feature of previouslyHighlightedMapFeatures) {
        feature.setStyle?.(undefined);
        feature.changed?.();
    }
    previouslyHighlightedMapFeatures = [];

    const locationsLayer = getLayerById(locationsLayerId);
    const locationsSource = locationsLayer?.getSource?.();
    if (!locationsSource || typeof locationsSource.getFeatures !== "function") {
        return;
    }

    for (const feature of locationsSource.getFeatures()) {
        if (featureMatchesHighlightedLocation(feature)) {
            feature.setStyle?.(getHighlightedLocationStyle(feature));
            previouslyHighlightedMapFeatures.push(feature);
            feature.changed?.();
        }
    }

    locationsLayer.changed?.();
    ensureHighlightedFeaturesVisible();
}

function ensureHighlightedFeaturesVisible() {
    if (!map || previouslyHighlightedMapFeatures.length === 0) {
        return;
    }

    const size = map.getSize?.();
    const view = map.getView?.();
    if (!Array.isArray(size) || !view) {
        return;
    }

    const highlightedExtent = ol.extent.createEmpty();
    for (const feature of previouslyHighlightedMapFeatures) {
        const featureExtent = feature.getGeometry?.()?.getExtent?.();
        if (Array.isArray(featureExtent)) {
            ol.extent.extend(highlightedExtent, featureExtent);
        }
    }

    if (ol.extent.isEmpty(highlightedExtent)) {
        return;
    }

    const currentExtent = view.calculateExtent(size);
    if (ol.extent.containsExtent(currentExtent, highlightedExtent)) {
        return;
    }

    const fitOptions = {
        size: size,
        padding: [40, 40, 40, 40],
        duration: 250
    };
    const currentZoom = view.getZoom?.();
    if (Number.isFinite(currentZoom)) {
        fitOptions.maxZoom = currentZoom;
    }

    view.fit(highlightedExtent, fitOptions);
}

function featureMatchesHighlightedLocation(feature) {
    const featureId = normalizeLocationId(feature?.getId?.());
    if (featureId && highlightedFeatureIds.has(featureId)) {
        return true;
    }

    return getFeatureLocationIds(feature).some(locationId => highlightedLocationIds.has(locationId));
}

function getFeatureLocationIds(feature) {
    const result = new Set();
    addLocationIdValue(feature?.getId?.(), result);
    addLocationContainer(feature?.get?.("locations") ?? feature?.get?.("Locations"), result);
    addLocationIdValue(feature?.get?.("locationIds") ?? feature?.get?.("LocationIds"), result);
    addLocationContainer(feature?.get?.("location") ?? feature?.get?.("Location"), result);
    return Array.from(result);
}

function addLocationContainer(value, result) {
    const parsedValue = parseJsonValue(value);
    if (!parsedValue) {
        return;
    }

    if (Array.isArray(parsedValue)) {
        addLocationIdValue(parsedValue, result);
        return;
    }

    if (typeof parsedValue !== "object") {
        return;
    }

    addLocationIdValue(getObjectProperty(parsedValue, "ids", "Ids"), result);
    addLocationIdValue(getObjectProperty(parsedValue, "locationIds", "LocationIds"), result);
    addLocationIdValue(getObjectProperty(parsedValue, "locationId", "LocationId"), result);

    const location = getObjectProperty(parsedValue, "location", "Location");
    if (location && location !== parsedValue) {
        addLocationContainer(location, result);
    }
}

function addLocationIdValue(value, result) {
    const parsedValue = parseJsonValue(value);
    if (parsedValue === null || parsedValue === undefined) {
        return;
    }

    if (Array.isArray(parsedValue)) {
        for (const item of parsedValue) {
            addLocationIdValue(item, result);
        }
        return;
    }

    if (typeof parsedValue === "object") {
        addLocationContainer(parsedValue, result);
        return;
    }

    const normalizedValue = normalizeLocationId(parsedValue);
    if (normalizedValue) {
        result.add(normalizedValue);
    }
}

function parseJsonValue(value) {
    if (typeof value !== "string") {
        return value;
    }

    const trimmedValue = value.trim();
    if (!(trimmedValue.startsWith("[") || trimmedValue.startsWith("{"))) {
        return value;
    }

    try {
        return JSON.parse(trimmedValue);
    } catch {
        return value;
    }
}

function normalizeLocationId(value) {
    if (value === null || value === undefined) {
        return "";
    }

    return value.toString().trim().toLowerCase();
}

function getHighlightedLocationStyle(feature) {
    const geometryType = feature.getGeometry()?.getType();
    if (geometryType === "Point" || geometryType === "MultiPoint") {
        return new ol.style.Style({
            zIndex: 1000,
            image: new ol.style.Circle({
                radius: 7,
                fill: new ol.style.Fill({ color: "rgba(213, 0, 249, 0.38)" }),
                stroke: new ol.style.Stroke({ color: "#d500f9", width: 2 })
            })
        });
    }

    return new ol.style.Style({
        zIndex: 1000,
        stroke: new ol.style.Stroke({ color: "#d500f9", width: 2 }),
        fill: new ol.style.Fill({ color: "rgba(213, 0, 249, 0.18)" })
    });
}

function getObservationStyle(feature, context) {
    if (context.resultType === "geogrid") {
        return getGeoGridStyle(feature, context);
    }

    const geometryType = feature.getGeometry()?.getType();
    if (geometryType === "Point" || geometryType === "MultiPoint") {
        return getPointStyle(feature);
    }

    return getLocationPolygonStyle(feature);
}

function getPointStyle(feature) {
    const isCluster = toBoolean(feature.get("isCluster")) || getDisplayStatusKey(feature) === "cluster";
    const displayStatusKey = isCluster ? "cluster" : getDisplayStatusKey(feature);
    const cacheKey = `${displayStatusKey}:${isCluster ? "cluster" : "point"}`;
    let style = pointStylesByLocationDisplayStatus.get(cacheKey);
    if (!style) {
        style = createPointStyle(displayStatusKey, isCluster);
        pointStylesByLocationDisplayStatus.set(cacheKey, style);
    }

    return style;
}

function createPointStyle(displayStatusKey, isCluster) {
    const styleDefinition = getDisplayStatusStyleDefinition(displayStatusKey);
    return new ol.style.Style({
        image: new ol.style.Circle({
            radius: isCluster ? 7 : 5,
            fill: new ol.style.Fill({
                color: styleDefinition.pointFill
            }),
            stroke: new ol.style.Stroke({
                color: styleDefinition.pointStroke,
                width: 1
            })
        })
    });
}

function getLocationPolygonStyle(feature) {
    const displayStatusKey = getDisplayStatusKey(feature);
    const isDiffused = isDiffusedLocation(feature);
    const cacheKey = `${isDiffused ? "diffused" : "polygon"}:${displayStatusKey}`;
    let style = polygonStylesByLocationDisplayStatus.get(cacheKey);
    if (!style) {
        style = createLocationPolygonStyle(displayStatusKey, isDiffused);
        polygonStylesByLocationDisplayStatus.set(cacheKey, style);
    }

    return style;
}

function createLocationPolygonStyle(displayStatusKey, isDiffused) {
    const styleDefinition = getDisplayStatusStyleDefinition(displayStatusKey);
    return new ol.style.Style({
        stroke: new ol.style.Stroke({
            color: isDiffused ? styleDefinition.diffusedStroke : styleDefinition.polygonStroke,
            width: 1
        }),
        fill: new ol.style.Fill({
            color: isDiffused ? styleDefinition.diffusedFill : styleDefinition.polygonFill
        })
    });
}

function getDisplayStatusStyleDefinition(displayStatusKey) {
    return displayStatusStyleDefinitions[displayStatusKey] ?? displayStatusStyleDefinitions.present;
}

function getDisplayStatusKey(feature) {
    const presentation = getFeatureProperty(feature, "presentation", "Presentation");
    return normalizeDisplayStatus(
        getObjectProperty(presentation, "status", "Status") ??
        getFeatureProperty(feature, "displayStatus", "DisplayStatus"));
}

function normalizeDisplayStatus(value) {
    if (typeof value === "number") {
        return numericDisplayStatusKeys[value] ?? "present";
    }

    if (typeof value === "string") {
        const normalizedValue = value.trim().toLowerCase().replace(/[\s_-]/g, "");
        return displayStatusStyleDefinitions[normalizedValue] ? normalizedValue : "present";
    }

    return "present";
}

function getFeatureProperty(feature, ...propertyNames) {
    for (const propertyName of propertyNames) {
        const value = feature.get(propertyName);
        if (value !== null && value !== undefined && value !== "") {
            return value;
        }
    }

    return null;
}

function getGeoGridStyle(feature, context) {
    const observationsCount = getNumericProperty(
        feature,
        "ObservationsCount",
        "observationsCount",
        "ObservationCount",
        "observationCount",
        "Count",
        "count");

    return new ol.style.Style({
        stroke: new ol.style.Stroke({
            color: "rgba(120, 45, 0, 0.85)",
            width: 1
        }),
        fill: new ol.style.Fill({
            color: getGeoGridFillColor(observationsCount, context)
        })
    });
}

function getGeoGridFillColor(observationsCount, context) {
    const lowColor = "rgba(255, 213, 128, 0.60)";
    const mediumColor = "rgba(255, 145, 0, 0.66)";
    const highColor = "rgba(220, 38, 38, 0.72)";

    if (!Number.isFinite(context.lowObservationCountBreak) ||
        !Number.isFinite(context.highObservationCountBreak)) {
        return lowColor;
    }

    if (observationsCount <= context.lowObservationCountBreak) {
        return lowColor;
    }

    return observationsCount <= context.highObservationCountBreak
        ? mediumColor
        : highColor;
}

function getNumericProperty(feature, ...propertyNames) {
    for (const propertyName of propertyNames) {
        const value = Number(feature.get(propertyName));
        if (Number.isFinite(value)) {
            return value;
        }
    }

    return 0;
}

function isDiffusedLocation(feature) {
    const renderMode = feature.get("renderMode");
    if (typeof renderMode === "string" && renderMode.toLowerCase() === "diffused") {
        return true;
    }

    const locations = getFeatureProperty(feature, "locations", "Locations");
    const location = getObjectProperty(locations, "location", "Location") ??
        getFeatureProperty(feature, "location", "Location");
    return toBoolean(getObjectProperty(location, "isGeneralized", "IsGeneralized"));
}

function getObjectProperty(source, ...propertyNames) {
    if (!source || typeof source !== "object") {
        return null;
    }

    for (const propertyName of propertyNames) {
        if (Object.prototype.hasOwnProperty.call(source, propertyName)) {
            return source[propertyName];
        }
    }

    return null;
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

function toFiniteNumber(value) {
    if (value === null || value === undefined) {
        return null;
    }

    const numberValue = Number(value);
    return Number.isFinite(numberValue) ? numberValue : null;
}
