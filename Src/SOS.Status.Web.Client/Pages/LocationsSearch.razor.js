import * as openLayersMap from "/openLayersMap.js";

let map = null;
let dotNetRef = null;
let moveEndKey = null;
let moveStartKey = null;
let singleClickKey = null;
let clickFeedbackKey = null;
let doubleClickKey = null;
let pointerMoveKey = null;
let pointerDragKey = null;
let cursorViewport = null;
let cursorTimer = null;
let hoverPixel = null;
let cursorQuery = null;
let cursorVersion = 0;
let lastCursorQueryTime = -Infinity;
let hoverFeedbackMode = "Outline";
let hoveredFeature = null;
let hoverOutlineLayer = null;
let hoverOutlineFeature = null;
let hoverPointRadiusPx = null;
const hoverOutlineStyles = new Map();
const cursorQueryIntervalMs = 1000 / 30;
let mapTooltipElement = null;
let mapTooltipEnabled = false;
let tooltipFeature = null;
let tooltipTimer = null;
let tooltipStartedAt = 0;
let tooltipPixel = null;
let tooltipContext = null;
let tooltipSize = null;
const tooltipDelayMs = 200;
const tooltipNumberFormat = new Intl.NumberFormat("sv-SE");

const locationsLayerId = "locations";
let locationFeatureCollection = null;
const locationOverlays = [
    { id: "coordinate-uncertainty", radiusPropertyName: null, visible: false },
    { id: "cluster-radius", radiusPropertyName: "cluster.radiusMeters", visible: false },
    { id: "cluster-minimum-enclosing-circle", radiusPropertyName: "cluster.radiusMetersMin", visible: false }
];
const pointStylesByLocationDisplayStatus = new Map();
const polygonStylesByLocationDisplayStatus = new Map();
const geoGridStylesByColor = new Map();
const highlightedPointStylesByRadius = new Map();
let highlightedPolygonStyle = null;
let highlightedLocationIds = new Set();
let highlightedFeatureIds = new Set();
let previouslyHighlightedMapFeatures = [];
let highlightFeatureIndex = null;
let clickFeedbackFeatures = null;
let mapSelection = { featureCount: 0, locationIds: new Set(), featureIds: new Set() };
let nextMapClickVersion = 0;
let latestMapClickVersion = 0;
let confirmedHighlightState = null;
let compactClusterMaxRadiusMeters = null;
let pointSizeMode = "Fixed";
let pointObservationCountBreaks = null;
let automaticPointSizeLegend = [];
let fixedPointSizeLegend = [];

const pointSizeRadii = [5, 7, 9];
const fixedPointObservationCountBreaks = { low: 5, high: 50 };

const locationGroupingTypes = new Set(["singleLocation", "coLocatedLocations", "meterCluster", "pixelCluster"]);
const observationCountPropertyNames = ["ObservationsCount", "observationsCount", "ObservationCount", "observationCount", "Count", "count"];

const compactClusterFillColors = {
    meterCluster: "#ffd54f",
    pixelCluster: "#fff04a" //"#fff59d"
};

const filterAreasLayerId = "filter-areas";
const filterAreaCacheDurationMs = 60 * 60 * 1000;
const filterGeometryTypes = new Map([
    ["point", "Point"], ["multipoint", "MultiPoint"],
    ["linestring", "LineString"], ["multilinestring", "MultiLineString"],
    ["polygon", "Polygon"], ["multipolygon", "MultiPolygon"],
    ["geometrycollection", "GeometryCollection"]
]);
let filterAreaVersion = 0;
let filterAreaAbortController = null;
let filterAreaPending = null;
let filterAreaKey = null;
let filterAreaInputJson = null;
let filterAreaReferencesJson = null;
let filterAreaLoadedAt = 0;
let filterAreaFailedKey = null;
let filterAreaOutlinesVisible = true;

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

export function initMap(container, blazorDotNetRef, zoom, centerLon, centerLat, tooltipElement = null) {
    if (!window.ol || !container) {
        return;
    }

    destroyMap();

    dotNetRef = blazorDotNetRef;
    mapTooltipElement = tooltipElement;
    if (mapTooltipElement) mapTooltipElement.hidden = true;

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
    moveStartKey = map.on("movestart", resetHoverCursor);
    singleClickKey = map.on("singleclick", onSingleClick);
    clickFeedbackKey = map.on("click", onClickFeedback);
    doubleClickKey = map.on("dblclick", onDoubleClick);
    pointerMoveKey = map.on("pointermove", onMapPointerMove);
    pointerDragKey = map.on("pointerdrag", onMapPointerMove);
    cursorViewport = map.getViewport();
    cursorViewport.addEventListener("pointerleave", onMapPointerLeave);
    cursorViewport.addEventListener("pointercancel", onMapPointerLeave);
    cursorViewport.addEventListener("pointerup", onMapPointerUp);
    cursorViewport.addEventListener("pointerover", onMapControlPointerOver);
    setMapCursor("default");

    requestAnimationFrame(function () {
        map?.updateSize();
        notifyExtentChanged();
    });
}

export function destroyMap() {
    filterAreaVersion++;
    filterAreaAbortController?.abort();
    filterAreaAbortController = null;
    filterAreaPending = null;
    filterAreaKey = null;
    filterAreaInputJson = null;
    filterAreaReferencesJson = null;
    filterAreaLoadedAt = 0;
    filterAreaFailedKey = null;
    filterAreaOutlinesVisible = true;
    if (moveEndKey) {
        ol.Observable.unByKey(moveEndKey);
        moveEndKey = null;
    }
    if (moveStartKey) {
        ol.Observable.unByKey(moveStartKey);
        moveStartKey = null;
    }

    if (singleClickKey) {
        ol.Observable.unByKey(singleClickKey);
        singleClickKey = null;
    }
    if (clickFeedbackKey) {
        ol.Observable.unByKey(clickFeedbackKey);
        clickFeedbackKey = null;
    }
    if (doubleClickKey) {
        ol.Observable.unByKey(doubleClickKey);
        doubleClickKey = null;
    }
    if (pointerMoveKey) {
        ol.Observable.unByKey(pointerMoveKey);
        pointerMoveKey = null;
    }
    if (pointerDragKey) {
        ol.Observable.unByKey(pointerDragKey);
        pointerDragKey = null;
    }
    onMapPointerLeave();
    mapTooltipEnabled = false;
    mapTooltipElement = null;
    cursorQuery = null;
    lastCursorQueryTime = -Infinity;
    hoverFeedbackMode = "Outline";
    if (hoverOutlineLayer) map?.removeLayer(hoverOutlineLayer);
    hoverOutlineLayer = null;
    hoverOutlineStyles.clear();
    if (cursorViewport) {
        cursorViewport.removeEventListener("pointerleave", onMapPointerLeave);
        cursorViewport.removeEventListener("pointercancel", onMapPointerLeave);
        cursorViewport.removeEventListener("pointerup", onMapPointerUp);
        cursorViewport.removeEventListener("pointerover", onMapControlPointerOver);
        setMapCursor("");
        cursorViewport = null;
    }

    if (map) {
        map.setTarget(null);
        map = null;
    }

    dotNetRef = null;
    highlightedLocationIds = new Set();
    highlightedFeatureIds = new Set();
    previouslyHighlightedMapFeatures = [];
    highlightFeatureIndex = null;
    clickFeedbackFeatures = null;
    mapSelection = { featureCount: 0, locationIds: new Set(), featureIds: new Set() };
    nextMapClickVersion = 0;
    latestMapClickVersion = 0;
    confirmedHighlightState = null;
    compactClusterMaxRadiusMeters = null;
    pointSizeMode = "Fixed";
    pointObservationCountBreaks = null;
    automaticPointSizeLegend = [];
    fixedPointSizeLegend = [];
    pointStylesByLocationDisplayStatus.clear();
    locationFeatureCollection = null;
    for (const overlay of locationOverlays) overlay.visible = false;
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

export function configureLocationOverlays(showUncertainty, showRadius, showEnclosingRadius) {
    locationOverlays[0].visible = showUncertainty === true;
    locationOverlays[1].visible = showRadius === true;
    locationOverlays[2].visible = showEnclosingRadius === true;
    updateLocationOverlays();
}

function updateLocationOverlays() {
    if (!map) return;

    for (const overlay of locationOverlays) {
        try {
            const existingLayer = getLayerById(overlay.id);
            if (existingLayer) {
                existingLayer.setVisible(overlay.visible);
            } else if (overlay.visible && locationFeatureCollection?.features?.length > 0) {
                // Reuse the parsed result. Invisible overlays need no geometry or vector source;
                // once created, an overlay is retained until the map result changes.
                if (overlay.radiusPropertyName) {
                    openLayersMap.configureClusterRadiusLayer(map, overlay.id, locationFeatureCollection, overlay.radiusPropertyName, true);
                } else {
                    openLayersMap.configureCoordinateUncertaintyLayer(map, overlay.id, locationFeatureCollection, true);
                }
            }
        } catch (error) {
            console.error(`LocationsSearch: failed to update overlay ${overlay.id}`, error);
        }
    }
}

export function configureCompactClusterRendering(maxRadiusMeters) {
    const value = Number.isFinite(maxRadiusMeters) && maxRadiusMeters >= 0 ? maxRadiusMeters : null;
    if (value === compactClusterMaxRadiusMeters) {
        return;
    }

    compactClusterMaxRadiusMeters = value;
    getLayerById(locationsLayerId)?.changed?.();
}

export function configurePointSizing(mode) {
    const value = mode === "ByObservationCount" || mode === "ByObservationCountFixedBreaks" ? mode : "Fixed";
    if (pointSizeMode !== value) {
        pointSizeMode = value;
        // Refresh selected symbols too, without moving the map or triggering a search.
        applyHighlightedLocationStyles();
        refreshHoverGeometry();
    }

    return getPointSizeLegend();
}

function getPointSizeLegend() {
    if (pointSizeMode === "ByObservationCount") return automaticPointSizeLegend;
    if (pointSizeMode === "ByObservationCountFixedBreaks") return fixedPointSizeLegend;
    return [];
}

function initializePointSizeClasses(layerId, resultType) {
    pointObservationCountBreaks = null;
    const layer = getLayerById(layerId);
    const features = layer?.getSource?.()?.getFeatures?.() ?? [];
    // Called once per result, after polygon rendering. Zooming and changing the size mode
    // reuse these breaks even if a polygon subsequently switches between surface and point.
    const counts = resultType?.toLowerCase() === "locations"
        ? features.filter(feature => isLocationObservationFeature(feature) && isPointGeometry(feature))
            .map(getPointObservationCount).filter(Number.isFinite)
        : [];
    pointObservationCountBreaks = calculateObservationCountBreaks(counts, {
        lowPercentile: 0.75, highPercentile: 0.9, useRangeFallback: false
    });
    automaticPointSizeLegend = createPointSizeLegend(counts, pointObservationCountBreaks);
    fixedPointSizeLegend = createPointSizeLegend(counts, fixedPointObservationCountBreaks, true);
    return getPointSizeLegend();
}

function createPointSizeLegend(counts, breaks, fixedLabels = false) {
    const classes = new Map();
    for (const count of counts) {
        const sizeClass = getObservationCountClass(count, breaks);
        const entry = classes.get(sizeClass);
        if (entry) {
            entry.minCount = Math.min(entry.minCount, count);
            entry.maxCount = Math.max(entry.maxCount, count);
        } else {
            classes.set(sizeClass, { diameterPx: pointSizeRadii[sizeClass] * 2, minCount: count, maxCount: count });
        }
    }
    return [...classes.entries()].sort(([a], [b]) => a - b).map(([sizeClass, entry]) => fixedLabels
        ? {
            diameterPx: entry.diameterPx,
            minCount: sizeClass === 0 ? null : sizeClass === 1 ? breaks.low + 1 : breaks.high + 1,
            maxCount: sizeClass === 2 ? null : sizeClass === 0 ? breaks.low : breaks.high
        }
        : entry);
}

function calculateObservationCountBreaks(observationCounts, {
    lowPercentile = 1 / 3, highPercentile = 2 / 3, useRangeFallback = true
} = {}) {
    const values = observationCounts.filter(Number.isFinite).sort((a, b) => a - b);
    if (values.length === 0) return null;

    // Defaults preserve the grid's original classification. Point sizing permits empty classes.
    let low = values[Math.floor((values.length - 1) * lowPercentile)];
    let high = values[Math.floor((values.length - 1) * highPercentile)];
    if (useRangeFallback && low >= high && values[values.length - 1] > values[0]) {
        const range = values[values.length - 1] - values[0];
        low = values[0] + range / 3;
        high = values[0] + range * 2 / 3;
    }
    return { low, high };
}

function getObservationCountClass(count, breaks) {
    if (!Number.isFinite(count) || !breaks || count <= breaks.low) return 0;
    return count <= breaks.high ? 1 : 2;
}

function isLocationObservationFeature(feature) {
    const featureType = getFeatureProperty(feature, "featureType", "FeatureType");
    return !String(featureType ?? "").toLowerCase().startsWith("filterarea.") &&
        locationGroupingTypes.has(feature.get("groupingType"));
}

function isPointGeometry(feature) {
    const type = feature.getGeometry?.()?.getType?.();
    return type === "Point" || type === "MultiPoint";
}

function getPointObservationCount(feature) {
    const count = getNumericPropertyOrNull(feature, "observationCount", "ObservationCount");
    return count !== null && count >= 0 ? count : null;
}

function getPointRadius(feature) {
    if (pointSizeMode !== "Fixed" && isLocationObservationFeature(feature)) {
        const breaks = pointSizeMode === "ByObservationCountFixedBreaks" ? fixedPointObservationCountBreaks : pointObservationCountBreaks;
        return pointSizeRadii[getObservationCountClass(getPointObservationCount(feature), breaks)];
    }
    const groupingType = feature.get("groupingType");
    return groupingType === "meterCluster" || groupingType === "pixelCluster" ? 6 : 5;
}

export function setLayerZIndex(layerId, zIndex) {
    openLayersMap.setLayerZIndex(map, layerId, zIndex);
}

export function setLayerVisible(layerId, visible) {
    openLayersMap.setLayerVisible(map, layerId, visible);
}

export function setHighlightedLocationIds(locationIds, featureIds, selection = null, clickVersion = latestMapClickVersion) {
    if (updateHighlightedLocationIds(locationIds, featureIds, selection, clickVersion)) {
        applyHighlightedLocationStyles();
    }
}

function updateHighlightedLocationIds(locationIds, featureIds, selection, clickVersion) {
    const state = {
        locationIds: normalizedIdSet(locationIds),
        featureIds: normalizedIdSet(featureIds),
        selection: selection ? {
            featureCount: selection.featureCount,
            locationIds: normalizedIdSet(selection.locationIds),
            featureIds: normalizedIdSet(selection.featureIds)
        } : mapSelection,
        version: clickVersion
    };
    if (!confirmedHighlightState || clickVersion >= confirmedHighlightState.version) confirmedHighlightState = state;
    // A slow Blazor callback must not undo a more recent local click.
    if (clickVersion < latestMapClickVersion) return false;
    latestMapClickVersion = clickVersion;
    nextMapClickVersion = Math.max(nextMapClickVersion, clickVersion);
    highlightedLocationIds = state.locationIds;
    highlightedFeatureIds = state.featureIds;
    mapSelection = state.selection;
    return true;
}

function normalizedIdSet(values) {
    return new Set((Array.isArray(values) ? values : []).map(normalizeLocationId).filter(Boolean));
}

export function renderMapResult(layerId, geoJson, options = {}) {
    const resultType = options.resultType;
    if (!loadGeoJsonLayer(layerId, geoJson, resultType, options)) return null;

    try {
        openLayersMap.configurePolygonRendering(
            map, layerId, options.createPolygons, resultType, options.polygonToPointThresholdPx);
    } catch (error) {
        // Optional client-side polygon rendering must not block the server's map result.
        console.error("LocationsSearch: failed to configure polygon rendering", error);
    }

    if (options.pointSizeMode !== undefined) {
        pointSizeMode = options.pointSizeMode === "ByObservationCount" || options.pointSizeMode === "ByObservationCountFixedBreaks"
            ? options.pointSizeMode : "Fixed";
    }
    const legend = initializePointSizeClasses(layerId, resultType);
    const highlight = options.highlight;
    if (highlight) {
        updateHighlightedLocationIds(highlight.locationIds, highlight.featureIds, highlight.selection, highlight.clickVersion);
    }
    // Apply selection once, after displayed geometries and point sizes are final.
    applyHighlightedLocationStyles();
    if (options.overlays) {
        configureLocationOverlays(options.overlays.coordinateUncertainty, options.overlays.clusterRadius, options.overlays.clusterMinimumEnclosingCircle);
    } else {
        updateLocationOverlays();
    }
    return legend;
}

function loadGeoJsonLayer(layerId, geoJson, resultType, options = {}) {
    if (!map || !layerId) {
        return false;
    }

    if (!geoJson) {
        removeLayer(layerId);
        return true;
    }

    const source = new ol.source.Vector();
    const format = new ol.format.GeoJSON({
        dataProjection: "EPSG:4326",
        featureProjection: map.getView().getProjection()
    });

    let features = [];
    let featureCollection;
    try {
        featureCollection = typeof geoJson === "string" ? JSON.parse(geoJson) : geoJson;
        features = format.readFeatures(featureCollection).filter(feature =>
            !String(getFeatureProperty(feature, "featureType", "FeatureType") ?? "").toLowerCase().startsWith("filterarea."));
    } catch (error) {
        console.error("LocationsSearch: failed to parse GeoJSON", error);
        return false;
    }

    source.addFeatures(features);

    const normalizedResultType = typeof resultType === "string"
        ? resultType.toLowerCase()
        : "";

    const gridBreaks = normalizedResultType === "geogrid"
        ? calculateObservationCountBreaks(features.map(feature => getNumericPropertyOrNull(feature, ...observationCountPropertyNames)))
        : null;
    const styleContext = {
        resultType: normalizedResultType,
        lowObservationCountBreak: gridBreaks?.low,
        highObservationCountBreak: gridBreaks?.high
    };

    const style = feature => getObservationStyle(feature, styleContext);
    const layer = getLayerById(layerId);
    // Parse/build the replacement before changing the visible result. Reuse the layer
    // to retain its visibility, order and identity across locations and geogrid results.
    if (layer) {
        layer.setStyle(style);
        layer.setSource(source);
        layer.set("sosResultType", normalizedResultType);
    } else {
        const newLayer = new ol.layer.Vector({ source, style });
        newLayer.set("id", layerId);
        newLayer.set("sosResultType", normalizedResultType);
        newLayer.setZIndex(20);
        map.addLayer(newLayer);
    }

    if (layerId === locationsLayerId) {
        resetLocationResult();
        locationFeatureCollection = normalizedResultType === "locations" ? featureCollection : null;
        if (options.filterJson !== undefined) {
            setFilterAreaOutlinesVisible(options.showFilterAreaOutlines !== false);
            // This independent layer must not delay observation updates or auto-searches.
            void updateFilterAreaLayer(featureCollection.filterAreaReferences ?? [], options.filterJson);
        }
    }
    return true;
}

export function setFilterAreaOutlinesVisible(visible) {
    filterAreaOutlinesVisible = visible;
    getLayerById(filterAreasLayerId)?.setVisible(visible);
}

function normalizeFilterGeometry(geometry) {
    // Search input accepts case-insensitive types/members; OpenLayers expects canonical GeoJSON.
    const rawType = geometry?.type ?? geometry?.Type;
    const type = typeof rawType === "string" ? filterGeometryTypes.get(rawType.toLowerCase()) : undefined;
    if (!type) throw new Error("Unsupported search filter geometry type");
    if (type === "GeometryCollection") {
        return { type, geometries: (geometry.geometries ?? geometry.Geometries).map(normalizeFilterGeometry) };
    }
    // Reuse the coordinate arrays without changing the original request or copying large polygons.
    return { type, coordinates: geometry.coordinates ?? geometry.Coordinates };
}

async function fetchFilterAreaFeature(reference, signal) {
    // Resolve through the same-origin proxy; API-origin URLs need not match this host.
    const path = `api/areas/${encodeURIComponent(reference.areaType)}/${encodeURIComponent(reference.featureId)}/geometry`;
    const response = await fetch(new URL(path, document.baseURI), {
        signal, headers: { Accept: "application/geo+json" }
    });
    if (!response.ok) throw new Error(`Area geometry request failed: ${response.status}`);
    return response.json(); // Browser caching handles Cache-Control and ETag revalidation.
}

function createOwnFilterAreaFeatures(reference, ownGeometries) {
    const searchCircle = reference.featureType === "FilterArea.Polygon";
    if (!searchCircle && reference.featureType !== "FilterArea.OwnGeometries")
        throw new Error("Unsupported filter area reference");
    const geometries = searchCircle ? ownGeometries.slice(0, 1) : ownGeometries;
    if (geometries.length === 0) throw new Error("Referenced search geometries are missing");
    return geometries.map(original => {
        let geometry = normalizeFilterGeometry(original);
        if (searchCircle && reference.maxDistanceFromPoint > 0 && geometry.type === "Point") {
            const center = geometry.coordinates;
            geometry = {
                type: "Polygon", coordinates: [openLayersMap.createGeodesicCircleCoordinates(
                    center[0], center[1], reference.maxDistanceFromPoint, "EPSG:4326")]
            };
        }
        return {
            type: "Feature", geometry, properties: {
                FeatureType: searchCircle ? "FilterArea.Polygon" : "FilterArea.OwnPolygon"
            }
        };
    });
}

function replaceFilterAreaFeatures(features) {
    const format = new ol.format.GeoJSON({ dataProjection: "EPSG:4326", featureProjection: map.getView().getProjection() });
    const polygons = format.readFeatures({ type: "FeatureCollection", features }).filter(feature =>
        ["Polygon", "MultiPolygon", "GeometryCollection"].includes(feature.getGeometry()?.getType()));
    const source = new ol.source.Vector();
    source.addFeatures(polygons);
    let layer = getLayerById(filterAreasLayerId);
    if (layer) layer.setSource(source);
    else {
        layer = new ol.layer.Vector({
            source, style: new ol.style.Style({
                stroke: new ol.style.Stroke({ color: "#00695c", width: 2, lineDash: [6, 4] })
            })
        });
        layer.set("id", filterAreasLayerId);
        layer.set("sosNonInteractive", true);
        layer.setZIndex(25);
        map.addLayer(layer);
    }
    layer.setVisible(filterAreaOutlinesVisible);
}

export function updateFilterAreaLayer(references, filterJson) {
    if (!map) return Promise.resolve();
    const referencesJson = JSON.stringify(references);
    const fresh = Date.now() - filterAreaLoadedAt < filterAreaCacheDurationMs;
    if (filterAreaInputJson === filterJson && filterAreaReferencesJson === referencesJson) {
        if (filterAreaPending) return filterAreaPending;
        if (filterAreaLoadedAt && fresh) return Promise.resolve();
    }

    let ownGeometries;
    let key;
    try {
        const filter = typeof filterJson === "string" ? JSON.parse(filterJson) : filterJson;
        const geography = filter?.geographics ?? filter?.Geographics;
        ownGeometries = geography?.geometries ?? geography?.Geometries ?? [];
        // Changes to taxon/date filters do not invalidate identical geographic outlines.
        const referencedGeometries = references.some(reference => reference.featureType === "FilterArea.OwnGeometries")
            ? ownGeometries
            : references.some(reference => reference.featureType === "FilterArea.Polygon") ? ownGeometries.slice(0, 1) : [];
        key = JSON.stringify([references, referencedGeometries]);
    } catch (error) {
        console.error("LocationsSearch: failed to read search filter geometry", error);
        return Promise.resolve();
    }
    filterAreaInputJson = filterJson;
    filterAreaReferencesJson = referencesJson;
    if (key === filterAreaKey && (filterAreaPending || (filterAreaLoadedAt && fresh)))
        return filterAreaPending ?? Promise.resolve();

    const changed = key !== filterAreaKey;
    const version = ++filterAreaVersion;
    const targetMap = map;
    filterAreaAbortController?.abort();
    filterAreaAbortController = new AbortController();
    const signal = filterAreaAbortController.signal;
    filterAreaKey = key;
    filterAreaLoadedAt = 0;
    if (changed && getLayerById(filterAreasLayerId)) openLayersMap.removeLayer(map, filterAreasLayerId);

    filterAreaPending = (async () => {
        try {
            const features = (await Promise.all(references.map(async reference => {
                if (reference.areaType != null && reference.featureId != null) {
                    return [await fetchFilterAreaFeature(reference, signal)];
                }
                return createOwnFilterAreaFeatures(reference, ownGeometries);
            }))).flat();
            if (map !== targetMap || version !== filterAreaVersion || signal.aborted) return;
            replaceFilterAreaFeatures(features);
            filterAreaLoadedAt = Date.now();
            filterAreaFailedKey = null;
        } catch (error) {
            if (signal.aborted || map !== targetMap || version !== filterAreaVersion) return;
            console.error("LocationsSearch: failed to load filter area outlines", error);
            if (filterAreaFailedKey !== key) {
                filterAreaFailedKey = key;
                dotNetRef?.invokeMethodAsync("OnFilterAreaLoadFailedFromJs").catch(() => { });
            }
        } finally {
            if (version === filterAreaVersion) filterAreaPending = null;
        }
    })();
    return filterAreaPending;
}

function notifyExtentChanged() {
    resetHoverCursor();
    const extent = getExtent();
    if (!extent || !dotNetRef) {
        return;
    }

    dotNetRef.invokeMethodAsync("OnExtentChangedFromJs", extent).catch(function () {
    });
}

export function setHoverFeedbackMode(mode) {
    const value = mode === "Cursor" || mode === "Outline" ? mode : "None";
    if (value === hoverFeedbackMode) return;
    hoverFeedbackMode = value;
    resetHoverCursor();
}

export function setMapTooltipEnabled(enabled) {
    const value = enabled === true;
    if (value === mapTooltipEnabled) return;
    mapTooltipEnabled = value;
    resetHoverCursor();
}

function isHoverDetectionEnabled() {
    return hoverFeedbackMode !== "None" || (mapTooltipEnabled && mapTooltipElement !== null);
}

function applyHoverFeedback(feature, pixel = null, query = null) {
    setMapCursor(hoverFeedbackMode === "Cursor" && feature ? "pointer" : "default");
    setHoveredFeature(hoverFeedbackMode === "Outline" ? feature : null);
    updateMapTooltip(feature, pixel, query);
}

function hideMapTooltip() {
    if (tooltipTimer !== null) clearTimeout(tooltipTimer);
    tooltipTimer = null;
    tooltipFeature = null;
    tooltipContext = null;
    tooltipPixel = null;
    tooltipSize = null;
    if (mapTooltipElement && !mapTooltipElement.hidden) mapTooltipElement.hidden = true;
}

function isCurrentTooltipContext() {
    const query = tooltipContext;
    return mapTooltipEnabled && mapTooltipElement && tooltipFeature && query && map === query.map &&
        getLayerById(locationsLayerId) === query.layer && query.layer.getSource() === query.source &&
        map.getView() === query.view && query.view.getRevision() === query.viewRevision;
}

function updateMapTooltip(feature, pixel, query) {
    if (!mapTooltipEnabled || !mapTooltipElement || !feature || !pixel || !query) {
        hideMapTooltip();
        return;
    }
    if (tooltipFeature !== feature) {
        hideMapTooltip();
        tooltipFeature = feature;
        tooltipStartedAt = performance.now();
        const content = createMapTooltipContent(feature);
        mapTooltipElement.textContent = content.text + (content.hint ? "\n" : "");
        if (content.hint) {
            const hint = mapTooltipElement.ownerDocument.createElement("em");
            hint.textContent = content.hint;
            mapTooltipElement.appendChild(hint);
        }
    }
    tooltipPixel = pixel;
    tooltipContext = query;
    if (!mapTooltipElement.hidden) {
        positionMapTooltip();
        return;
    }
    const remaining = Math.max(0, tooltipDelayMs - (performance.now() - tooltipStartedAt));
    if (remaining === 0) displayMapTooltip();
    else if (tooltipTimer === null) tooltipTimer = setTimeout(() => {
        tooltipTimer = null;
        if (!isCurrentTooltipContext()) { hideMapTooltip(); return; }
        // If a newer position is still being checked, let its confirmed hit display the tooltip.
        if (!cursorQuery && !hoverPixel) displayMapTooltip();
    }, remaining);
}

function displayMapTooltip() {
    if (!isCurrentTooltipContext()) { hideMapTooltip(); return; }
    mapTooltipElement.hidden = false;
    // Measure only when showing new content, not on every pointer movement.
    tooltipSize = [mapTooltipElement.offsetWidth, mapTooltipElement.offsetHeight];
    positionMapTooltip();
}

function positionMapTooltip() {
    const size = map?.getSize();
    if (!size || !tooltipPixel || !tooltipSize) return;
    const [width, height] = tooltipSize;
    let x = tooltipPixel[0] + 12, y = tooltipPixel[1] + 12;
    if (x + width > size[0] - 8) x = tooltipPixel[0] - width - 12;
    if (y + height > size[1] - 8) y = tooltipPixel[1] - height - 12;
    x = Math.max(8, Math.min(x, size[0] - width - 8));
    y = Math.max(8, Math.min(y, size[1] - height - 8));
    mapTooltipElement.style.transform = `translate(${Math.round(x)}px, ${Math.round(y)}px)`;
}

function tooltipCount(value) {
    const number = toFiniteNumber(value);
    return Number.isInteger(number) && number >= 0 ? number : null;
}

function tooltipLocationName(location, fallbackId = null) {
    const name = getObjectProperty(location, "locality", "Locality");
    const id = getObjectProperty(location, "locationId", "LocationId") ?? fallbackId;
    return typeof name === "string" && name.trim() ? name.trim()
        : typeof id === "string" && id.trim() ? id.trim() : "Fyndplats";
}

function createMapTooltipContent(feature) {
    const observations = tooltipCount(getNumericPropertyOrNull(feature, ...observationCountPropertyNames));
    const lines = [];
    if (isGridResult()) lines.push("Gridruta");
    else {
        const locations = getFeatureProperty(feature, "locations", "Locations");
        const details = getObjectProperty(locations, "items", "Items");
        const items = Array.isArray(details) ? details : [];
        const ids = getObjectProperty(locations, "ids", "Ids");
        const fullIds = Array.isArray(ids) ? ids : [];
        const count = tooltipCount(getObjectProperty(locations, "count", "Count")) ??
            (fullIds.length > 0 ? normalizedIdSet(fullIds).size : null);
        const type = String(getFeatureProperty(feature, "groupingType", "GroupingType") ?? "").toLowerCase();
        const cluster = type === "metercluster" || type === "pixelcluster";
        const multiple = cluster || type === "colocatedlocations" || count > 1;
        if (multiple) {
            const total = count === null ? "Fyndplatser" : `${tooltipNumberFormat.format(count)} ${count === 1 ? "fyndplats" : "fyndplatser"}`;
            lines.push(cluster ? count === null ? "Kluster" : `Kluster – ${total}` : `${total} på samma plats`);
        } else lines.push(tooltipLocationName(items[0], fullIds.find(id => typeof id === "string" && id.trim())));
        if (observations !== null) lines.push(`${tooltipNumberFormat.format(observations)} ${observations === 1 ? "observation" : "observationer"}`);
        if (multiple && !cluster) {
            const names = [...new Map(items.map(item => {
                const name = tooltipLocationName(item);
                return [name.toLocaleLowerCase("sv-SE"), name];
            })).values()].sort((a, b) => a.localeCompare(b, "sv-SE")).slice(0, 3);
            if (names.length > 0) lines.push(`Exempel: ${names.join(", ")}`);
        } else if (!multiple && isPointGeometry(feature)) {
            const uncertainty = toFiniteNumber(getObjectProperty(items[0], "coordinateUncertaintyInMeters", "CoordinateUncertaintyInMeters"));
            if (uncertainty > 0) lines.push(`Noggrannhetsradie: ${tooltipNumberFormat.format(uncertainty)} m`);
        }
        return { text: lines.join("\n"), hint: cluster ? "Zooma in för att välja en fyndplats" : null };
    }
    if (observations !== null) lines.push(`${tooltipNumberFormat.format(observations)} ${observations === 1 ? "observation" : "observationer"}`);
    lines.push("Klicka för att zooma in");
    return { text: lines.join("\n"), hint: null };
}

function setHoveredFeature(feature) {
    if (hoveredFeature === feature) return;
    hoveredFeature = feature;
    hoverOutlineLayer?.getSource().clear();
    hoverOutlineFeature = null;
    hoverPointRadiusPx = null;
    if (!feature) return;

    if (!hoverOutlineLayer) {
        hoverOutlineLayer = new ol.layer.Vector({
            source: new ol.source.Vector(),
            style: () => getHoverOutlineStyle()
        });
        hoverOutlineLayer.set("id", "hover-outline");
        hoverOutlineLayer.set("sosNonInteractive", true);
        hoverOutlineLayer.setZIndex(30);
        map.addLayer(hoverOutlineLayer);
    }
    // A geometry-only feature keeps the original feature's selection style out of this layer.
    // Share existing polygon geometry; point uncertainty uses one bounded-size circle geometry.
    hoverOutlineFeature = new ol.Feature({ geometry: createHoverGeometry(feature) });
    hoverOutlineLayer.getSource().addFeature(hoverOutlineFeature);
}

function getVisiblePointRadius(feature) {
    const baseRadius = getPointRadius(feature) + 0.5; // Normal symbols have a 1 px stroke.
    const image = feature.getStyle?.()?.getImage?.();
    const radius = image?.getRadius?.();
    const strokeWidth = image?.getStroke?.()?.getWidth?.() ?? 0;
    return Number.isFinite(radius) ? Math.max(baseRadius, radius + strokeWidth / 2) : baseRadius;
}

function getHoverRadiusMeters(feature) {
    const groupingType = getFeatureProperty(feature, "groupingType", "GroupingType");
    if (groupingType === "meterCluster" || groupingType === "pixelCluster") {
        const cluster = getFeatureProperty(feature, "cluster", "Cluster");
        const radius = toFiniteNumber(getObjectProperty(cluster, "radiusMetersMin", "RadiusMetersMin"));
        return radius > 0 ? radius : 0;
    }

    const locations = getFeatureProperty(feature, "locations", "Locations");
    const items = getObjectProperty(locations, "items", "Items");
    let radius = 0;
    for (const location of Array.isArray(items) ? items : []) {
        const uncertainty = toFiniteNumber(getObjectProperty(location, "coordinateUncertaintyInMeters", "CoordinateUncertaintyInMeters"));
        if (uncertainty > radius) radius = uncertainty;
    }
    return radius;
}

function createHoverGeometry(feature) {
    const geometry = feature.getGeometry();
    if (!isPointGeometry(feature)) return geometry;

    hoverPointRadiusPx = getVisiblePointRadius(feature);
    const view = map.getView();
    const projection = view.getProjection?.();
    const resolution = view.getResolution?.();
    if (!projection || !Number.isFinite(resolution) || resolution <= 0) return geometry;

    const coordinates = geometry.getCoordinates();
    const centers = geometry.getType() === "MultiPoint" ? coordinates : [coordinates];
    const radiusMeters = getHoverRadiusMeters(feature);
    const circles = [];
    for (const center of centers) {
        const metersPerPixel = ol.proj.getPointResolution(projection, resolution, center, "m");
        if (!Number.isFinite(metersPerPixel) || metersPerPixel <= 0) return geometry;
        const [longitude, latitude] = ol.proj.toLonLat(center, projection);
        const radius = Math.max(radiusMeters, hoverPointRadiusPx * metersPerPixel);
        circles.push([openLayersMap.createGeodesicCircleCoordinates(longitude, latitude, radius, projection)]);
    }
    return geometry.getType() === "MultiPoint" ? new ol.geom.MultiPolygon(circles) : new ol.geom.Polygon(circles[0]);
}

function refreshHoverGeometry() {
    if (!hoverOutlineFeature || !hoveredFeature || !isPointGeometry(hoveredFeature)) return;
    if (getVisiblePointRadius(hoveredFeature) !== hoverPointRadiusPx)
        hoverOutlineFeature.setGeometry(createHoverGeometry(hoveredFeature));
}

function getHoverOutlineStyle() {
    if (!hoveredFeature) return null;
    const point = isPointGeometry(hoveredFeature);
    // A missing/invalid map resolution falls back to a small symbol, never a huge pixel canvas.
    const fallbackRadius = point && isPointGeometry(hoverOutlineFeature) ? hoverPointRadiusPx : 0;
    const key = fallbackRadius > 0 ? `symbol:${fallbackRadius}` : point ? "area" : "outline";
    let style = hoverOutlineStyles.get(key);
    if (!style) {
        const stroke = new ol.style.Stroke({ color: "#ff0000", width: 2 });
        const fill = point ? new ol.style.Fill({ color: "rgba(255, 235, 59, 0.12)" }) : undefined;
        style = new ol.style.Style(fallbackRadius > 0
            ? { image: new ol.style.Circle({ radius: fallbackRadius, stroke, fill }) }
            : { stroke, fill });
        hoverOutlineStyles.set(key, style);
    }
    return style;
}

function setMapCursor(cursor) {
    if (cursorViewport && cursorViewport.style.cursor !== cursor) cursorViewport.style.cursor = cursor;
}

function cancelCursorUpdate() {
    if (cursorTimer !== null) clearTimeout(cursorTimer);
    cursorTimer = null;
    hoverPixel = null;
    cursorVersion++;
}

function resetHoverCursor() {
    cancelCursorUpdate();
    applyHoverFeedback(null);
}

function onMapPointerLeave() {
    resetHoverCursor();
}

function onMapPointerUp(event) {
    onMapPointerLeave();
    if (map && event.pointerType !== "touch") onMapPointerMove({ pixel: map.getEventPixel(event), originalEvent: event });
}

function onMapControlPointerOver(event) {
    if (event.target?.closest?.(".ol-control")) resetHoverCursor();
}

function onMapPointerMove(event) {
    if (!map) return;
    if (event.dragging || event.originalEvent?.pointerType === "touch" || event.originalEvent?.target?.closest?.(".ol-control")) {
        resetHoverCursor();
        return;
    }
    if (!isHoverDetectionEnabled()) {
        return;
    }

    hoverPixel = event.pixel;
    cursorVersion++;
    scheduleCursorQuery();
}

function scheduleCursorQuery() {
    if (!map || !isHoverDetectionEnabled() || !hoverPixel || cursorTimer !== null || cursorQuery) return;
    const delay = Math.max(0, Math.ceil(cursorQueryIntervalMs - (performance.now() - lastCursorQueryTime)));
    cursorTimer = setTimeout(updateHoverCursor, delay);
}

function isCurrentCursorQuery(query) {
    return cursorQuery === query && map === query.map && cursorVersion === query.version &&
        isHoverDetectionEnabled() && getLayerById(locationsLayerId) === query.layer &&
        query.layer.getRevision() === query.layerRevision && map.getView() === query.view &&
        query.view.getRevision() === query.viewRevision;
}

async function updateHoverCursor() {
    cursorTimer = null;
    const layer = getLayerById(locationsLayerId);
    const view = map?.getView();
    const pixel = hoverPixel;
    hoverPixel = null;
    if (!pixel || !layer || !layer.isVisible() || layer.get("sosNonInteractive") ||
        view.getAnimating() || view.getInteracting()) {
        applyHoverFeedback(null);
        return;
    }

    const query = {
        map, layer, view, pixel, source: layer.getSource(), version: cursorVersion,
        layerRevision: layer.getRevision(), viewRevision: view.getRevision()
    };
    cursorQuery = query;
    lastCursorQueryTime = performance.now();
    try {
        // The layer's cached hit detection only returns the topmost feature.
        // Keep exact hit detection and its tolerance exclusively for clicks.
        const features = await layer.getFeatures(pixel);
        if (isCurrentCursorQuery(query))
            applyHoverFeedback(isInteractiveObservationFeature(features[0], layer) ? features[0] : null, pixel, query);
    } catch {
        if (isCurrentCursorQuery(query)) applyHoverFeedback(null);
    } finally {
        if (cursorQuery === query) {
            cursorQuery = null;
            // Movements during the query are coalesced into the latest position.
            scheduleCursorQuery();
        }
    }
}

function getMapSelection(features) {
    return {
        featureCount: features.length,
        locationIds: normalizedIdSet(features.flatMap(getFeatureLocationIds)),
        featureIds: normalizedIdSet(features.map(feature => feature.getId?.()))
    };
}

function getNextSelectedFeatures(features) {
    const clicked = getMapSelection(features);
    const sameIds = (a, b) => a.size === b.size && [...a].every(id => b.has(id));
    const sameSelection = mapSelection.featureCount > 0 && mapSelection.featureCount === clicked.featureCount &&
        (mapSelection.locationIds.size > 0 || clicked.locationIds.size > 0
            ? sameIds(mapSelection.locationIds, clicked.locationIds)
            : mapSelection.featureIds.size === mapSelection.featureCount && clicked.featureIds.size === clicked.featureCount &&
            sameIds(mapSelection.featureIds, clicked.featureIds));
    return sameSelection ? [] : features;
}

function onClickFeedback(event) {
    if (!map || !dotNetRef) return;
    if (isGridResult()) {
        clickFeedbackFeatures = null;
        applyHighlightedLocationStyles();
        return;
    }
    // OpenLayers delays singleclick to detect double-click zoom. Paint the feedback now,
    // and let singleclick commit the selection and start the observation request later.
    clickFeedbackFeatures = getNextSelectedFeatures(getInteractiveObservationFeaturesAtPixel(event.pixel));
    paintHighlightedFeatures(clickFeedbackFeatures);
}

function onDoubleClick() {
    clickFeedbackFeatures = null;
    applyHighlightedLocationStyles();
}

function onSingleClick(event) {
    if (!map || !dotNetRef) {
        return;
    }

    const clickedFeatures = getInteractiveObservationFeaturesAtPixel(event.pixel);
    if (isGridResult() && clickedFeatures.length > 0) {
        clickFeedbackFeatures = null;
        zoomToGridCell(clickedFeatures, event.coordinate);
        return;
    }
    const payload = clickedFeatures.map(toFeatureClickPayload);
    const selection = getNextSelectedFeatures(clickedFeatures);
    mapSelection = getMapSelection(selection);
    highlightedLocationIds = mapSelection.locationIds;
    highlightedFeatureIds = mapSelection.featureIds;
    clickFeedbackFeatures = null;
    const clickVersion = ++nextMapClickVersion;
    latestMapClickVersion = clickVersion;
    const clickedMap = map;
    paintHighlightedFeatures(selection);

    dotNetRef.invokeMethodAsync(
        "OnFeaturesClickedFromJs",
        payload,
        clickVersion).catch(function () {
            if (map !== clickedMap || latestMapClickVersion !== clickVersion) return;
            // If the callback cannot be delivered, return to the latest confirmed state.
            highlightedLocationIds = confirmedHighlightState?.locationIds ?? new Set();
            highlightedFeatureIds = confirmedHighlightState?.featureIds ?? new Set();
            mapSelection = confirmedHighlightState?.selection ?? { featureCount: 0, locationIds: new Set(), featureIds: new Set() };
            latestMapClickVersion = confirmedHighlightState?.version ?? 0;
            applyHighlightedLocationStyles();
        });
}

function isGridResult() {
    return getLayerById(locationsLayerId)?.get("sosResultType") === "geogrid";
}

function zoomToGridCell(features, coordinate) {
    // At shared borders hit tolerance can include an adjacent cell; prefer the cell containing the click.
    const feature = features.find(feature => coordinate && feature.getGeometry()?.intersectsCoordinate?.(coordinate)) ?? features[0];
    const extent = feature?.getGeometry()?.getExtent?.();
    if (!Array.isArray(extent) || extent.length !== 4 || !extent.every(Number.isFinite) ||
        extent[0] >= extent[2] || extent[1] >= extent[3]) return;

    map.getView().fit(extent, { padding: [20, 20, 20, 20], duration: 250 });
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

    // Prefer the symbols the user clicked over any polygon surfaces underneath.
    // Use the displayed geometry so polygons rendered as points behave like other points.
    const points = features.filter(isPointGeometry);
    return points.length > 0 ? points : features;
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
    if (layerId === locationsLayerId) {
        resetLocationResult();
    }
    openLayersMap.removeLayer(map, layerId);
}

function resetLocationResult() {
    resetHoverCursor();
    highlightFeatureIndex = null;
    clickFeedbackFeatures = null;
    locationFeatureCollection = null;
    for (const overlay of locationOverlays) openLayersMap.removeLayer(map, overlay.id);
    pointObservationCountBreaks = null;
    automaticPointSizeLegend = [];
    fixedPointSizeLegend = [];
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

    const locationsLayer = getLayerById(locationsLayerId);
    const locationsSource = locationsLayer?.getSource?.();
    if (!locationsSource || typeof locationsSource.getFeatures !== "function") {
        return;
    }

    paintHighlightedFeatures(clickFeedbackFeatures ?? findHighlightedFeatures(locationsSource));
}

function paintHighlightedFeatures(features) {
    for (const feature of previouslyHighlightedMapFeatures) {
        feature.setStyle?.(undefined);
        feature.changed?.();
    }
    previouslyHighlightedMapFeatures = [];
    for (const feature of features) {
        feature.setStyle?.(getHighlightedLocationStyle(feature));
        previouslyHighlightedMapFeatures.push(feature);
        feature.changed?.();
    }
    refreshHoverGeometry();
    getLayerById(locationsLayerId)?.changed?.();
    // A selection style can invalidate an in-flight hit. Recheck the same position
    // once it settles, unless navigation, new results or a newer movement superseded it.
    if (cursorQuery?.map === map && cursorQuery.version === cursorVersion && !hoverPixel)
        hoverPixel = cursorQuery.pixel;
    map?.render?.();
}

function findHighlightedFeatures(source) {
    if (highlightedLocationIds.size === 0 && highlightedFeatureIds.size === 0) return [];

    // Build on demand, once per source. Geometry changes during zoom do not change membership.
    if (highlightFeatureIndex?.source !== source) {
        const byFeatureId = new Map();
        const byLocationId = new Map();
        for (const feature of source.getFeatures()) {
            const featureId = normalizeLocationId(feature.getId?.());
            if (featureId) addIndexedFeature(byFeatureId, featureId, feature);
            for (const locationId of getFeatureLocationIds(feature)) {
                addIndexedFeature(byLocationId, locationId, feature);
            }
        }
        highlightFeatureIndex = { source, byFeatureId, byLocationId };
    }

    const matches = new Set();
    for (const id of highlightedFeatureIds) {
        for (const feature of highlightFeatureIndex.byFeatureId.get(id) ?? []) matches.add(feature);
    }
    for (const id of highlightedLocationIds) {
        for (const feature of highlightFeatureIndex.byLocationId.get(id) ?? []) matches.add(feature);
    }
    return Array.from(matches);
}

function addIndexedFeature(index, id, feature) {
    const features = index.get(id);
    if (features) features.push(feature);
    else index.set(id, [feature]);
}

function getFeatureLocationIds(feature) {
    const ids = feature?.get?.("locations")?.ids;
    return Array.isArray(ids)
        ? Array.from(new Set(ids.filter(id => typeof id === "string").map(normalizeLocationId).filter(Boolean)))
        : [];
}

function normalizeLocationId(value) {
    if (value === null || value === undefined) {
        return "";
    }

    return value.toString().trim().toLowerCase();
}

function getHighlightedLocationStyle(feature) {
    const geometryType = feature.getGeometry()?.getType();
    if (geometryType !== "Point" && geometryType !== "MultiPoint") {
        highlightedPolygonStyle ??= new ol.style.Style({
            zIndex: 1000,
            stroke: new ol.style.Stroke({ color: "#d500f9", width: 2 }),
            fill: new ol.style.Fill({ color: "rgba(213, 0, 249, 0.18)" })
        });
        return highlightedPolygonStyle;
    }

    const radius = pointSizeMode !== "Fixed" && isLocationObservationFeature(feature)
        ? getPointRadius(feature) + 2 : 7;
    let style = highlightedPointStylesByRadius.get(radius);
    if (!style) {
        style = new ol.style.Style({
            zIndex: 1000,
            image: new ol.style.Circle({
                radius,
                fill: new ol.style.Fill({ color: "rgba(213, 0, 249, 0.38)" }),
                stroke: new ol.style.Stroke({ color: "#d500f9", width: 2 })
            })
        });
        highlightedPointStylesByRadius.set(radius, style);
    }

    return style;
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
    const groupingType = feature.get("groupingType");
    const radiusMetersMin = getObjectProperty(feature.get("cluster"), "radiusMetersMin");
    const isCompactCluster = compactClusterMaxRadiusMeters !== null &&
        (groupingType === "meterCluster" || groupingType === "pixelCluster") &&
        Number.isFinite(radiusMetersMin) && radiusMetersMin >= 0 &&
        radiusMetersMin <= compactClusterMaxRadiusMeters;
    const displayStatusKey = groupingType === "meterCluster" ? "present"
        : groupingType === "pixelCluster" ? "cluster" : getDisplayStatusKey(feature);
    const radius = getPointRadius(feature);
    const cacheKey = `${displayStatusKey}:${groupingType ?? "point"}:${isCompactCluster ? "compact" : "default"}:${radius}`;
    let style = pointStylesByLocationDisplayStatus.get(cacheKey);
    if (!style) {
        style = createPointStyle(displayStatusKey, groupingType, isCompactCluster, radius);
        pointStylesByLocationDisplayStatus.set(cacheKey, style);
    }

    return style;
}

function createPointStyle(displayStatusKey, groupingType, isCompactCluster, radius) {
    const styleDefinition = getDisplayStatusStyleDefinition(displayStatusKey);
    return new ol.style.Style({
        image: new ol.style.Circle({
            radius: radius,
            fill: new ol.style.Fill({
                color: isCompactCluster ? compactClusterFillColors[groupingType]
                    : groupingType === "meterCluster" ? "#fff04a" : styleDefinition.pointFill
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
    const observationsCount = getNumericPropertyOrNull(feature, ...observationCountPropertyNames) ?? 0;
    const color = getGeoGridFillColor(observationsCount, context);
    if (geoGridStylesByColor.has(color)) return geoGridStylesByColor.get(color);
    const style = new ol.style.Style({
        stroke: new ol.style.Stroke({
            color: "rgba(120, 45, 0, 0.85)",
            width: 1
        }),
        fill: new ol.style.Fill({
            color
        })
    });
    geoGridStylesByColor.set(color, style);
    return style;
}

function getGeoGridFillColor(observationsCount, context) {
    const lowColor = "rgba(255, 213, 128, 0.60)";
    const mediumColor = "rgba(255, 145, 0, 0.66)";
    const highColor = "rgba(220, 38, 38, 0.72)";

    const breaks = Number.isFinite(context.lowObservationCountBreak) && Number.isFinite(context.highObservationCountBreak)
        ? { low: context.lowObservationCountBreak, high: context.highObservationCountBreak } : null;
    return [lowColor, mediumColor, highColor][getObservationCountClass(observationsCount, breaks)];
}

function getNumericPropertyOrNull(feature, ...propertyNames) {
    for (const propertyName of propertyNames) {
        const value = toFiniteNumber(feature.get(propertyName));
        if (value !== null) {
            return value;
        }
    }

    return null;
}

function isDiffusedLocation(feature) {
    const presentation = getFeatureProperty(feature, "presentation", "Presentation");
    const generalized = getObjectProperty(presentation, "generalized", "Generalized");
    if (generalized !== null && generalized !== undefined) {
        return toBoolean(generalized);
    }

    // Compatibility with responses from API versions without presentation.generalized.
    const renderMode = feature.get("renderMode");
    if (typeof renderMode === "string" && renderMode.toLowerCase() === "diffused") {
        return true;
    }

    const summary = getFeatureProperty(feature, "observationSummary", "ObservationSummary");
    const counts = getObjectProperty(summary, "statusCounts", "StatusCounts");
    if (Number(getObjectProperty(counts, "diffusedCount", "DiffusedCount")) > 0) return true;
    const locations = getFeatureProperty(feature, "locations", "Locations");
    const items = getObjectProperty(locations, "items", "Items") ?? [];
    return items.some(item => toBoolean(getObjectProperty(item, "isGeneralized", "IsGeneralized")));
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
    if (typeof value !== "number" && (typeof value !== "string" || value.trim() === "")) {
        return null;
    }

    const numberValue = Number(value);
    return Number.isFinite(numberValue) ? numberValue : null;
}
