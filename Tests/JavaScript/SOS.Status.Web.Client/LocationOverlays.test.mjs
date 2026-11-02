import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';

const overlayIds = ['coordinate-uncertainty', 'cluster-radius', 'cluster-minimum-enclosing-circle'];
function result(longitude = 15) {
    return JSON.stringify({ type: 'FeatureCollection', features: [
        { type: 'Feature', id: 'point', geometry: { type: 'Point', coordinates: [longitude, 59] },
            properties: { groupingType: 'singleLocation', locations: { ids: ['a'], items: [
                { type: 'Point', decimalLongitude: longitude, decimalLatitude: 59, coordinateUncertaintyInMeters: 10 }
            ] } } },
        { type: 'Feature', id: 'cluster', geometry: { type: 'Point', coordinates: [longitude, 59] },
            properties: { groupingType: 'pixelCluster', cluster: { radiusMeters: 20, radiusMetersMin: 30 } } }
    ] });
}

function load() {
    const counters = { parses: 0, circles: 0, errors: [], readObjects: [] };
    const style = class { constructor(options) { Object.assign(this, options); } };
    const ol = {
        style: { Style: style, Stroke: style, Circle: style, Fill: style },
        Feature: class extends style { set(key, value) { this[key] = value; } },
        geom: { Polygon: class { constructor(coordinates) {
            if (counters.failCircles) throw new Error('Circle unavailable');
            counters.circles++; this.coordinates = coordinates;
        } } },
        proj: { transform: coordinates => coordinates },
        source: { Vector: class {
            features = [];
            addFeature(feature) { this.features.push(feature); }
            addFeatures(features) { this.features.push(...features); }
            getFeatures() { return this.features; }
        } },
        layer: { Vector: class extends style {
            properties = {};
            set(key, value) { this.properties[key] = value; }
            get(key) { return this.properties[key]; }
            setZIndex(value) { this.zIndex = value; }
            setVisible(value) { this.visible = value; }
            getSource() { return this.source; }
            setSource(value) { this.source = value; }
            setStyle(value) { this.style = value; }
            changed() {}
        } },
        format: { GeoJSON: class {
            readFeatures(collection) {
                assert.equal(typeof collection, 'object', 'OpenLayers receives the already parsed collection');
                counters.readObjects.push(collection);
                return collection.features.map(f => ({
                    getId: () => f.id,
                    get: key => f.properties[key],
                    getGeometry: () => ({ getType: () => f.geometry.type, getExtent: () => [...f.geometry.coordinates, ...f.geometry.coordinates] }),
                    setStyle(style) { this.style = style; }
                }));
            }
        } }
    };
    const context = vm.createContext({ ol, window: { ol }, console: { error: (...args) => counters.errors.push(args) },
        JSON: { parse: text => { counters.parses++; return JSON.parse(text); } } });
    const sharedSource = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/wwwroot/openLayersMap.js', import.meta.url), 'utf8').replace(/export /g, '');
    vm.runInContext(`globalThis.openLayersMap = (() => { ${sharedSource}; return { removeLayer, configurePolygonRendering, configureCoordinateUncertaintyLayer, configureClusterRadiusLayer }; })();`, context);
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/Pages/LocationsSearch.razor.js', import.meta.url), 'utf8')
        .replace(/^\s*import .*;$/m, '').replace(/export /g, '');
    vm.runInContext(source + '\nglobalThis.api = { renderMapResult, loadGeoJsonLayer(layerId, geoJson, resultType) { return renderMapResult(layerId, geoJson, { resultType }); }, configureLocationOverlays, setHighlightedLocationIds, removeLayer, destroyMap, attachMap(value) { map = value; } };', context);
    const map = {
        layers: [],
        getAllLayers() { return this.layers; },
        addLayer(layer) { this.layers.push(layer); },
        removeLayer(layer) { this.layers = this.layers.filter(item => item !== layer); },
        getView: () => ({ getProjection: () => 'EPSG:4326', getResolution: () => 1, on: () => ({}) }),
        getSize: () => null,
        setTarget() {}
    };
    const api = context.api;
    api.attachMap(map);
    return { api, map, counters, layer: id => map.layers.find(layer => layer.get('id') === id) };
}

test('hidden overlays create no layers or circle geometry and the result is parsed once', () => {
    const { api, map, counters } = load();
    api.configureLocationOverlays(false, false, false);
    api.loadGeoJsonLayer('locations', result(), 'locations');
    assert.deepEqual(map.layers.map(layer => layer.get('id')), ['locations']);
    assert.equal(counters.circles, 0);
    assert.equal(counters.parses, 1);
});

for (const [index, id] of overlayIds.entries()) {
    test(`${id} is created on demand and reused after hiding and showing`, () => {
        const { api, layer, counters } = load();
        api.loadGeoJsonLayer('locations', result(), 'locations');
        const switches = overlayIds.map((_, i) => i === index);
        api.configureLocationOverlays(...switches);
        const overlay = layer(id);
        assert.equal(overlay.visible, true);
        assert.equal(overlay.get('sosNonInteractive'), true);
        assert.equal(overlay.zIndex, 0);
        assert.equal(overlay.getSource().getFeatures().length, 1);
        assert.equal(counters.circles, 1);
        for (const other of overlayIds.filter(value => value !== id)) assert.equal(layer(other), undefined);
        api.configureLocationOverlays(false, false, false);
        assert.equal(overlay.visible, false);
        api.configureLocationOverlays(...switches);
        assert.equal(layer(id), overlay);
        assert.equal(overlay.visible, true);
        assert.equal(counters.circles, 1);
        assert.equal(counters.parses, 1);
    });
}

test('new results rebuild visible overlays and discard hidden ones, using new coordinates', () => {
    const { api, layer, counters } = load();
    api.loadGeoJsonLayer('locations', result(15), 'locations');
    api.configureLocationOverlays(true, true, true);
    const oldRadius = layer('cluster-radius');
    const oldCoordinate = oldRadius.getSource().getFeatures()[0].geometry.coordinates[0][0][0];
    api.configureLocationOverlays(false, true, true);
    api.loadGeoJsonLayer('locations', result(16), 'locations');
    assert.equal(layer('coordinate-uncertainty'), undefined);
    assert.notEqual(layer('cluster-radius'), oldRadius);
    assert.equal(layer('cluster-radius').visible, true);
    assert.ok(layer('cluster-radius').getSource().getFeatures()[0].geometry.coordinates[0][0][0] > oldCoordinate);
    assert.equal(counters.circles, 5);
    assert.equal(counters.parses, 2);
    api.configureLocationOverlays(true, true, true);
    assert.equal(counters.circles, 6);
    assert.equal(counters.parses, 2);
});

for (const [description, geoJson, resultType] of [
    ['empty result', '{"type":"FeatureCollection","features":[]}', 'locations'],
    ['geogrid', result(), 'geogrid'],
    ['cleared result', '', 'locations']
]) {
    test(`${description} clears old overlays and cannot recreate them from stale data`, () => {
        const { api, layer, counters } = load();
        api.loadGeoJsonLayer('locations', result(), 'locations');
        api.configureLocationOverlays(true, true, true);
        api.loadGeoJsonLayer('locations', geoJson, resultType);
        api.configureLocationOverlays(true, true, true);
        for (const id of overlayIds) assert.equal(layer(id), undefined);
        assert.equal(counters.circles, 3);
        api.loadGeoJsonLayer('locations', result(16), 'locations');
        for (const id of overlayIds) assert.equal(layer(id).visible, true);
        assert.equal(counters.circles, 6);
    });
}

test('invalid JSON leaves the visible result and its overlays intact', () => {
    const { api, layer, counters } = load();
    api.loadGeoJsonLayer('locations', result(), 'locations');
    api.configureLocationOverlays(true, true, true);
    const previous = layer('locations');
    const previousSource = previous.getSource();
    const previousOverlays = overlayIds.map(layer);
    assert.equal(api.loadGeoJsonLayer('locations', '{', 'locations'), null);
    assert.equal(layer('locations'), previous);
    assert.equal(previous.getSource(), previousSource);
    assert.deepEqual(overlayIds.map(layer), previousOverlays);
    assert.equal(counters.errors.length, 1);
});

test('results reuse the main layer and preserve visibility and order while replacing its source', () => {
    const { api, layer } = load();
    api.loadGeoJsonLayer('locations', result(), 'locations');
    const previous = layer('locations');
    const previousSource = previous.getSource();
    previous.setVisible(false);
    previous.setZIndex(42);
    api.loadGeoJsonLayer('locations', result(16), 'locations');
    assert.equal(layer('locations'), previous);
    assert.notEqual(previous.getSource(), previousSource);
    assert.equal(previous.visible, false);
    assert.equal(previous.zIndex, 42);
    assert.equal(previous.getSource().getFeatures()[0].getGeometry().getExtent()[0], 16);
    api.loadGeoJsonLayer('locations', result(), 'geogrid');
    assert.equal(layer('locations'), previous);
    assert.equal(previous.style(previous.getSource().getFeatures()[0]).fill.color, 'rgba(255, 213, 128, 0.60)');
    api.loadGeoJsonLayer('locations', { type: 'FeatureCollection', features: [] }, 'locations');
    assert.equal(layer('locations'), previous);
    assert.equal(previous.getSource().getFeatures().length, 0);
});

test('batched rendering builds only the overlays enabled for the new result', () => {
    const { api, layer, counters } = load();
    api.configureLocationOverlays(true, true, true);
    api.renderMapResult('locations', result(), { resultType: 'locations', overlays: {
        coordinateUncertainty: false, clusterRadius: true, clusterMinimumEnclosingCircle: false
    } });
    assert.equal(counters.circles, 1);
    assert.equal(layer('coordinate-uncertainty'), undefined);
    assert.equal(layer('cluster-minimum-enclosing-circle'), undefined);
    assert.equal(layer('cluster-radius').visible, true);
});

test('destroying the map clears retained overlay data and visibility preferences', () => {
    const { api, map, layer, counters } = load();
    api.loadGeoJsonLayer('locations', result(), 'locations');
    api.configureLocationOverlays(true, true, true);
    api.destroyMap();
    map.layers = [];
    api.attachMap(map);
    api.loadGeoJsonLayer('locations', result(16), 'locations');
    for (const id of overlayIds) assert.equal(layer(id), undefined);
    assert.equal(counters.circles, 3);
});

test('overlays enabled before the result arrives are created after the main layer', () => {
    const { api, map, counters } = load();
    api.configureLocationOverlays(true, true, true);
    assert.equal(map.layers.length, 0);
    api.loadGeoJsonLayer('locations', result(), 'locations');
    assert.deepEqual(map.layers.map(layer => layer.get('id')), ['locations', ...overlayIds]);
    assert.equal(counters.circles, 3);
    assert.equal(counters.parses, 1);
});

test('optional overlay failures leave the main layer available and can be retried', () => {
    const { api, layer, counters } = load();
    counters.failCircles = true;
    api.configureLocationOverlays(true, true, true);
    api.loadGeoJsonLayer('locations', result(), 'locations');
    assert.equal(layer('locations').getSource().getFeatures().length, 2);
    assert.equal(counters.errors.length, 3);
    counters.failCircles = false;
    api.configureLocationOverlays(true, true, true);
    for (const id of overlayIds) assert.equal(layer(id).visible, true);
    assert.equal(counters.parses, 1);
});

test('location selection follows new clusters and returning points without moving the viewport', () => {
    const { api, map, layer } = load();
    // Highlight updates initiated by clicks may fit the view; replacement results must not.
    map.getSize = () => { throw new Error('A map reload must not try to fit the selection'); };
    api.loadGeoJsonLayer('locations', result(), 'locations');
    api.setHighlightedLocationIds(['a'], [], false);
    assert.equal(layer('locations').getSource().getFeatures()[0].style.image.stroke.color, '#d500f9');

    const cluster = JSON.parse(result());
    cluster.features = [cluster.features[1]];
    cluster.features[0].properties.locations = { ids: ['a', 'b'] };
    api.loadGeoJsonLayer('locations', cluster, 'locations');
    assert.equal(layer('locations').getSource().getFeatures()[0].style.image.stroke.color, '#d500f9');

    api.loadGeoJsonLayer('locations', { type: 'FeatureCollection', features: [] }, 'locations');
    api.loadGeoJsonLayer('locations', result(16), 'locations');
    assert.equal(layer('locations').getSource().getFeatures()[0].style.image.stroke.color, '#d500f9');
    assert.equal(layer('locations').getSource().getFeatures()[1].style, undefined);
    api.setHighlightedLocationIds([], [], false);
    assert.equal(layer('locations').getSource().getFeatures()[0].style, undefined);
});
