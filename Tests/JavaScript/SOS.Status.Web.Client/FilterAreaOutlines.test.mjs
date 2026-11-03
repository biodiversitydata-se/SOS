import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';

const polygon = { type: 'Polygon', coordinates: [[[15, 59], [15, 60], [16, 60], [15, 59]]] };
const area = id => ({ featureType: 'FilterArea.Area', areaType: 'County', featureId: id, geometryUrl: `/external-api/areas/County/${id}/geometry` });
const response = (geometry = polygon) => ({ ok: true, json: async () => ({ type: 'Feature', geometry, properties: {} }) });

function load(fetchResult = async () => response()) {
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/Pages/LocationsSearch.razor.js', import.meta.url), 'utf8')
        .replace(/^\s*import .*;$/m, '').replace(/export /g, '');
    const calls = [], notifications = [], circleCalls = [], formatOptions = [];
    let now = 100000;
    class Feature {
        constructor(value) { this.value = value; this.properties = value.properties ?? {}; }
        get(key) { return this.properties[key]; }
        getGeometry() { return { getType: () => this.value.geometry.type }; }
    }
    class VectorSource {
        features = [];
        addFeatures(values) { this.features.push(...values); }
        getFeatures() { return this.features; }
    }
    class Layer {
        constructor(options) { Object.assign(this, options); this.properties = {}; }
        set(key, value) { this.properties[key] = value; }
        get(key) { return this.properties[key]; }
        setZIndex(value) { this.zIndex = value; }
        setSource(value) { this.source = value; }
        getSource() { return this.source; }
        setVisible(value) { this.visible = value; }
    }
    const style = class { constructor(options) { Object.assign(this, options); } };
    const map = {
        layers: [], getAllLayers() { return this.layers; },
        addLayer(layer) { this.layers.push(layer); },
        getView: () => ({ getProjection: () => 'EPSG:3857' }), setTarget() {}
    };
    const context = vm.createContext({
        console: { error() {} }, URL, AbortController, document: { baseURI: 'https://status.example.test/app/' },
        Date: { now: () => now },
        fetch: async (url, options) => { calls.push({ url: url.href, options }); return fetchResult(url, options); },
        openLayersMap: {
            removeLayer(map, id) { map.layers = map.layers.filter(layer => layer.get('id') !== id); },
            createGeodesicCircleCoordinates(...args) { circleCalls.push(args); return polygon.coordinates[0]; }
        },
        ol: {
            source: { Vector: VectorSource }, layer: { Vector: Layer },
            style: { Style: style, Stroke: style },
            format: { GeoJSON: class {
                constructor(options) { formatOptions.push(options); }
                readFeatures(collection) {
                    const validate = geometry => {
                        assert.ok(['Point', 'MultiPoint', 'LineString', 'MultiLineString', 'Polygon', 'MultiPolygon', 'GeometryCollection'].includes(geometry.type),
                            'OpenLayers requires canonical GeoJSON geometry type names');
                        if (geometry.type === 'GeometryCollection') geometry.geometries.forEach(validate);
                        else assert.ok(Array.isArray(geometry.coordinates), 'OpenLayers reads the lowercase coordinates member');
                    };
                    return collection.features.map(value => { validate(value.geometry); return new Feature(value); });
                }
            } }
        }
    });
    vm.runInContext(source + `
        globalThis.api = { updateFilterAreaLayer, setFilterAreaOutlinesVisible, destroyMap,
            attachMap(value, callback) { map = value; dotNetRef = { invokeMethodAsync: callback }; } };
    `, context);
    context.api.attachMap(map, async method => { notifications.push(method); });
    return {
        ...context.api, calls, notifications, circleCalls, formatOptions, map, Layer,
        advance(ms) { now += ms; },
        layer() { return map.layers.find(layer => layer.get('id') === 'filter-areas'); }
    };
}

test('area geometries use the same-origin proxy and get an outline-only non-interactive layer', async () => {
    const api = load();
    await api.updateFilterAreaLayer([area('a b')], '{}');
    assert.equal(api.calls[0].url, 'https://status.example.test/app/api/areas/County/a%20b/geometry');
    assert.equal(api.calls[0].options.headers.Accept, 'application/geo+json');
    const layer = api.layer();
    assert.equal(layer.get('sosNonInteractive'), true);
    assert.equal(layer.zIndex, 25);
    assert.equal(layer.style.fill, undefined);
    assert.equal(layer.style.stroke.color, '#00695c');
    assert.deepEqual(Array.from(layer.style.stroke.lineDash), [6, 4]);
    assert.equal(layer.source.features.length, 1);
    assert.equal(api.formatOptions[0].dataProjection, 'EPSG:4326');
    assert.equal(api.formatOptions[0].featureProjection, 'EPSG:3857');
});

test('pan, zoom and non-geographic filter changes reuse the layer for one hour before revalidation', async () => {
    const api = load();
    await api.updateFilterAreaLayer([area('1')], '{"taxonIds":[1]}');
    const layer = api.layer(), source = layer.source;
    await api.updateFilterAreaLayer([area('1')], '{"taxonIds":[1]}');
    await api.updateFilterAreaLayer([area('1')], '{"taxonIds":[2]}');
    assert.equal(api.calls.length, 1);
    assert.equal(api.layer(), layer);
    assert.equal(api.layer().source, source);
    api.advance(600001);
    await api.updateFilterAreaLayer([area('1')], '{"taxonIds":[2]}');
    assert.equal(api.calls.length, 1, 'ten minutes does not expire the cached outlines');
    api.advance(2999998);
    await api.updateFilterAreaLayer([area('1')], '{"taxonIds":[2]}');
    assert.equal(api.calls.length, 1, 'outlines remain reusable just before one hour');
    api.advance(2);
    await api.updateFilterAreaLayer([area('1')], '{"taxonIds":[2]}');
    assert.equal(api.calls.length, 2);
    assert.equal(api.layer(), layer);
    assert.notEqual(api.layer().source, source);
});

test('one own-geometries reference renders every polygon and multipolygon with their holes without HTTP requests', async () => {
    const api = load();
    const geometry = { type: 'MultiPolygon', coordinates: [polygon.coordinates] };
    const hole = [[15.1, 59.8], [15.2, 59.8], [15.2, 59.9], [15.1, 59.9], [15.1, 59.8]];
    const polygonWithHole = { type: 'Polygon', coordinates: [polygon.coordinates[0], hole] };
    const filter = JSON.stringify({ geographics: { geometries: [polygonWithHole, geometry] } });
    await api.updateFilterAreaLayer([{ featureType: 'FilterArea.OwnGeometries' }], filter);
    assert.equal(api.calls.length, 0);
    assert.equal(api.layer().source.features.length, 2);
    assert.equal(api.layer().source.features[0].value.geometry.type, 'Polygon');
    assert.equal(api.layer().source.features[1].value.geometry.type, 'MultiPolygon');
    assert.equal(api.layer().source.features[0].value.geometry.coordinates.length, 2);
    assert.deepEqual(JSON.parse(JSON.stringify(api.layer().source.features[0].value.geometry.coordinates[1])), hole);
    assert.equal(api.layer().style.fill, undefined);
});

test('the reported search filter with a lowercase polygon draws its outline without fetching geometry', async () => {
    const api = load();
    const filter = {
        taxon: { ids: [100077] },
        geographics: {
            geometries: [{ type: 'polygon', coordinates: [[
                [15.07063, 57.92573], [15.0051, 58.16108], [14.58003, 58.10148],
                [14.64143, 57.93294], [15.07063, 57.92573]
            ]] }],
            considerObservationAccuracy: true
        },
        includeRealCount: true
    };
    await api.updateFilterAreaLayer([{ featureType: 'FilterArea.OwnGeometries' }], JSON.stringify(filter));
    assert.equal(api.notifications.length, 0);
    assert.equal(api.layer().source.features.length, 1);
    assert.equal(api.layer().source.features[0].value.geometry.type, 'Polygon');
    assert.deepEqual(JSON.parse(JSON.stringify(api.layer().source.features[0].value.geometry.coordinates)), filter.geographics.geometries[0].coordinates);
    assert.equal(api.layer().style.fill, undefined);
    assert.equal(api.calls.length, 0);
    assert.equal(filter.geographics.geometries[0].type, 'polygon');
});

test('a search circle still uses only the first request point and its radius', async () => {
    const api = load();
    await api.updateFilterAreaLayer([{ featureType: 'FilterArea.Polygon', maxDistanceFromPoint: 500 }],
        JSON.stringify({ geographics: { geometries: [{ type: 'Point', coordinates: [15, 59] }, polygon] } }));
    assert.deepEqual(api.circleCalls[0], [15, 59, 500, 'EPSG:4326']);
    assert.equal(api.circleCalls.length, 1);
    assert.equal(api.layer().source.features.length, 1);
    assert.equal(api.layer().source.features[0].value.geometry.type, 'Polygon');
    assert.equal(api.calls.length, 0);
});

for (const geometry of [
    { type: 'POLYGON', coordinates: polygon.coordinates },
    { Type: 'pOlYgOn', Coordinates: polygon.coordinates },
    { type: 'multipolygon', coordinates: [polygon.coordinates] },
    { Type: 'MultiPolygon', Coordinates: [polygon.coordinates] },
    { type: 'geometrycollection', geometries: [{ Type: 'polygon', Coordinates: polygon.coordinates }] }
]) {
    test(`search geometry ${geometry.type ?? geometry.Type} is normalized before OpenLayers reads it`, async () => {
        const api = load();
        const filter = JSON.stringify({ Geographics: { Geometries: [geometry] } });
        await api.updateFilterAreaLayer([{ featureType: 'FilterArea.OwnGeometries' }], filter);
        assert.equal(api.notifications.length, 0);
        assert.equal(api.layer().source.features.length, 1);
        assert.equal(api.layer().style.fill, undefined);
        assert.equal(api.calls.length, 0);
    });
}

test('a lowercase point with PascalCase coordinates still renders a search circle', async () => {
    const api = load();
    await api.updateFilterAreaLayer([{ featureType: 'FilterArea.Polygon', maxDistanceFromPoint: 500 }],
        JSON.stringify({ geographics: { geometries: [{ Type: 'point', Coordinates: [15, 59] }] } }));
    assert.equal(api.notifications.length, 0);
    assert.deepEqual(Array.from(api.circleCalls[0]), [15, 59, 500, 'EPSG:4326']);
    assert.equal(api.layer().source.features.length, 1);
});

test('removing geographic filters clears their outlines without touching observation results', async () => {
    const api = load();
    const observations = new api.Layer({}); observations.set('id', 'locations'); api.map.addLayer(observations);
    await api.updateFilterAreaLayer([area('1')], '{}');
    await api.updateFilterAreaLayer([], '{}');
    assert.equal(api.layer().source.features.length, 0);
    assert.ok(api.map.layers.includes(observations));
});

test('changing a later own geometry replaces the contours even though the common reference stays the same', async () => {
    const api = load();
    const references = [{ featureType: 'FilterArea.OwnGeometries' }];
    await api.updateFilterAreaLayer(references, JSON.stringify({ geographics: { geometries: [polygon, polygon] } }));
    const firstSource = api.layer().source;
    const moved = { ...polygon, coordinates: [[[16, 60], [16, 61], [17, 61], [16, 60]]] };
    await api.updateFilterAreaLayer(references, JSON.stringify({ geographics: { geometries: [polygon, moved] } }));
    assert.notEqual(api.layer().source, firstSource);
    assert.equal(api.layer().source.features.length, 2);
    assert.equal(api.layer().source.features[1].value.geometry.coordinates[0][0][0], 16);
    assert.equal(api.calls.length, 0);
});

test('stored areas and a common own-geometries reference share one contour layer', async () => {
    const api = load();
    await api.updateFilterAreaLayer([area('1'), { featureType: 'FilterArea.OwnGeometries' }],
        JSON.stringify({ geographics: { geometries: [polygon, polygon] } }));
    assert.equal(api.map.layers.length, 1);
    assert.equal(api.layer().source.features.length, 3);
    assert.equal(api.calls.length, 1);
    assert.equal(api.layer().style.fill, undefined);
});

test('a late response for an old filter cannot replace outlines of the latest filter', async () => {
    let resolveFirst, resolveSecond;
    const first = new Promise(resolve => { resolveFirst = resolve; });
    const second = new Promise(resolve => { resolveSecond = resolve; });
    const api = load(url => url.pathname.includes('/first/') ? first : second);
    const oldLoad = api.updateFilterAreaLayer([area('first')], '{}');
    const newLoad = api.updateFilterAreaLayer([area('second')], '{}');
    assert.equal(api.calls[0].options.signal.aborted, true);
    const newerPolygon = { ...polygon, coordinates: [[[16, 60], [16, 61], [17, 61], [16, 60]]] };
    resolveSecond(response(newerPolygon)); await newLoad;
    resolveFirst(response()); await oldLoad;
    assert.equal(api.layer().source.features[0].value.geometry.coordinates[0][0][0], 16);
    assert.equal(api.notifications.length, 0);
});

test('same-filter concurrent renders share a pending request and visibility changes need no refetch', async () => {
    let resolve;
    const api = load(() => new Promise(done => { resolve = done; }));
    const first = api.updateFilterAreaLayer([area('1')], '{}');
    const second = api.updateFilterAreaLayer([area('1')], '{}');
    assert.equal(first, second);
    api.setFilterAreaOutlinesVisible(false);
    resolve(response()); await first;
    assert.equal(api.layer().visible, false);
    api.setFilterAreaOutlinesVisible(true);
    assert.equal(api.layer().visible, true);
    assert.equal(api.calls.length, 1);
});

test('failed requests are retried and report a single notification per geographic filter', async () => {
    let failures = 2;
    const api = load(async () => failures-- > 0 ? { ok: false, status: 404 } : response());
    await api.updateFilterAreaLayer([area('1')], '{}');
    await api.updateFilterAreaLayer([area('1')], '{}');
    assert.equal(api.notifications.length, 1);
    assert.equal(api.layer(), undefined);
    await api.updateFilterAreaLayer([area('1')], '{}');
    assert.equal(api.calls.length, 3);
    assert.equal(api.layer().source.features.length, 1);
});

test('destroying the map cancels pending geometry work and prevents a late layer update', async () => {
    let resolve;
    const api = load(() => new Promise(done => { resolve = done; }));
    const loading = api.updateFilterAreaLayer([area('1')], '{}');
    api.destroyMap();
    assert.equal(api.calls[0].options.signal.aborted, true);
    resolve(response()); await loading;
    assert.equal(api.layer(), undefined);
    assert.equal(api.notifications.length, 0);
});
