import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
const filename = new URL('../../../Src/SOS.Status.Web.Client/Pages/LocationsSearch.razor.js', import.meta.url);
function load() {
    const source = fs.readFileSync(filename, 'utf8').replace(/^\s*import .*;$/m, '').replace(/export /g, '');
    const style = class { constructor(options) { Object.assign(this, options); } };
    const context = vm.createContext({ console, ol: { style: { Style: style, Circle: style, Fill: style, Stroke: style } } });
    vm.runInContext(source + '\nglobalThis.api = { getFeatureLocationIds, toFeatureClickPayload, setHighlightedLocationIds, isDiffusedLocation, getPointStyle, getObservationStyle, configureCompactClusterRendering, attachLayersForTest(layers) { map = { getAllLayers: () => layers, getSize: () => null, getView: () => null }; } };', context);
    return context.api;
}
function feature(id, locations, groupingType = 'coLocatedLocations', summary = {}, cluster, presentation) {
    const result = {
        getId: () => id,
        get: key => ({ locations, groupingType, observationSummary: summary, cluster, presentation })[key],
        getGeometry: () => ({ getType: () => 'Point' }),
        setStyle: style => { result.style = style; },
        changed: () => {}
    };
    return result;
}
test('click carries map identity while searchable ids contain every original member', () => {
    const api = load();
    const ids = Array.from({length: 100}, (_, i) => `site-${i}`);
    const f = feature('cluster-hash', { ids, items: ids.slice(0, 5).map(locationId => ({ locationId })) });
    assert.deepEqual(Array.from(api.getFeatureLocationIds(f)), ids);
    assert.equal(api.toFeatureClickPayload(f).featureId, 'cluster-hash');
    assert.ok(!api.getFeatureLocationIds(f).includes('cluster-hash'));
    const coincident = feature('point', { ids: [' A ', 'a', 'B', '', ' '] });
    assert.deepEqual(Array.from(api.getFeatureLocationIds(coincident)), ['a', 'b']);
    assert.deepEqual(Array.from(api.getFeatureLocationIds(feature('grid'))), []);
});
test('generalized members outside the five items still affect geometry presentation', () => {
    const api = load();
    assert.equal(api.isDiffusedLocation(feature('point', { items: [{ isGeneralized: false }] }, 'coLocatedLocations', { statusCounts: { diffusedCount: 1 } })), true);
});

test('presentation generalization takes precedence over legacy summary and location details', () => {
    const api = load();
    const make = (generalized, legacyGeneralized) => feature('polygon',
        { items: [{ isGeneralized: legacyGeneralized }] }, 'coLocatedLocations',
        { statusCounts: { diffusedCount: legacyGeneralized ? 1 : 0 } }, undefined,
        { generalized });
    assert.equal(api.isDiffusedLocation(make(true, false)), true);
    assert.equal(api.isDiffusedLocation(make(false, true)), false);
});

test('presentation generalization changes polygon color while preserving point and cluster styles', () => {
    const api = load();
    for (const geometryType of ['Polygon', 'MultiPolygon', 'Point']) {
        const make = generalized => {
            const f = feature('site', {}, 'singleLocation', {}, undefined, { generalized });
            f.getGeometry = () => ({ getType: () => geometryType });
            return f;
        };
        const normal = api.getObservationStyle(make(false), {});
        const generalized = api.getObservationStyle(make(true), {});
        if (geometryType === 'Point') assert.equal(generalized, normal);
        else {
            assert.notEqual(generalized.fill.color, normal.fill.color);
            assert.notEqual(generalized.stroke.color, normal.stroke.color);
        }
    }
    for (const grouping of ['meterCluster', 'pixelCluster']) {
        const make = generalized => feature('cluster', {}, grouping, {}, undefined, { generalized });
        assert.equal(api.getPointStyle(make(true)), api.getPointStyle(make(false)));
    }
});

test('points and clusters with generalized members use presentation.generalized false', () => {
    const api = load();
    for (const groupingType of ['singleLocation', 'meterCluster', 'pixelCluster']) {
        const f = feature('point', { items: [{ isGeneralized: true }] }, groupingType,
            { statusCounts: { diffusedCount: 1 } }, undefined, { generalized: false });
        assert.equal(api.isDiffusedLocation(f), false);
        const ordinary = feature('ordinary', {}, groupingType, {}, undefined, { generalized: false });
        assert.equal(api.getObservationStyle(f, {}), api.getObservationStyle(ordinary, {}));
    }
});

for (const [groupingType, compactColor, defaultColor, defaultRadius] of [
    ['meterCluster', '#ffd54f', '#fff04a', 6],
    ['pixelCluster', '#fff04a', 'dodgerblue', 6]
]) {
    test(`${groupingType} uses its compact color at or below the enclosing radius limit`, () => {
        const api = load();
        const make = radiusMetersMin => feature('cluster', { ids: ['a', 'b'] }, groupingType, {}, { radiusMetersMin });
        assert.equal(api.getPointStyle(make(20)).image.fill.color, defaultColor, 'empty setting is off by default');
        api.configureCompactClusterRendering(50);
        for (const radius of [49.999, 50]) {
            const style = api.getPointStyle(make(radius));
            assert.equal(style.image.fill.color, compactColor);
            assert.equal(style.image.radius, 6);
            assert.equal(style.image.stroke.color, 'black');
            assert.equal(style.image.stroke.width, 1);
        }
        const outside = api.getPointStyle(make(50.001));
        assert.equal(outside.image.fill.color, defaultColor);
        assert.equal(outside.image.radius, defaultRadius);
        for (const radius of [null, undefined, -1, NaN, Infinity, 'invalid', '', false])
            assert.equal(api.getPointStyle(make(radius)).image.fill.color, defaultColor);
        assert.equal(api.getPointStyle(feature('no-details', {}, groupingType)).image.fill.color, defaultColor);
        api.configureCompactClusterRendering(null);
        assert.equal(api.getPointStyle(make(20)).image.fill.color, defaultColor);
    });
}

test('zero is an active limit and invalid limits disable compact rendering', () => {
    const api = load();
    const zero = feature('zero', {}, 'pixelCluster', {}, { radiusMetersMin: 0 });
    const positive = feature('positive', {}, 'pixelCluster', {}, { radiusMetersMin: 0.001 });
    api.configureCompactClusterRendering(0);
    assert.equal(api.getPointStyle(zero).image.fill.color, '#fff04a');
    assert.equal(api.getPointStyle(positive).image.fill.color, 'dodgerblue');
    for (const limit of [null, undefined, -1, NaN, Infinity, '', false]) {
        api.configureCompactClusterRendering(50);
        api.configureCompactClusterRendering(limit);
        assert.equal(api.getPointStyle(zero).image.fill.color, 'dodgerblue');
    }
});

test('compact and regular styles remain separate when limits change', () => {
    const api = load();
    const small = feature('small', {}, 'meterCluster', {}, { radiusMetersMin: 20 });
    const large = feature('large', {}, 'meterCluster', {}, { radiusMetersMin: 60 });
    api.configureCompactClusterRendering(50);
    const compact = api.getPointStyle(small);
    const regular = api.getPointStyle(large);
    assert.notEqual(compact, regular);
    api.configureCompactClusterRendering(100);
    assert.equal(api.getPointStyle(large), compact);
    api.configureCompactClusterRendering(null);
    assert.equal(api.getPointStyle(small), regular);
});

test('ordinary locations and geogrid keep their presentation', () => {
    const api = load();
    const gridFeature = feature('grid', {}, 'pixelCluster', {}, { radiusMetersMin: 1 });
    const gridContext = { resultType: 'geogrid', lowObservationCountBreak: 10, highObservationCountBreak: 100 };
    const before = api.getObservationStyle(gridFeature, gridContext);
    api.configureCompactClusterRendering(50);
    for (const groupingType of ['singleLocation', 'coLocatedLocations']) {
        const style = api.getPointStyle(feature('location', {}, groupingType, {}, { radiusMetersMin: 1 }));
        assert.equal(style.image.radius, 5);
        assert.equal(style.image.fill.color, 'yellow');
    }
    assert.deepEqual(api.getObservationStyle(gridFeature, gridContext), before);
});

test('grid cells reuse exactly three styles across counts, redraws and changing breaks', () => {
    const api = load();
    const cell = count => ({ get: key => key === 'observationCount' ? count : undefined });
    const context = { resultType: 'geogrid', lowObservationCountBreak: 5, highObservationCountBreak: 50 };
    const styles = [1, 6, 51].map(count => api.getObservationStyle(cell(count), context));
    assert.equal(new Set(styles).size, 3);
    for (let count = 0; count < 1000; count++) {
        assert.equal(api.getObservationStyle(cell(count), context), styles[count <= 5 ? 0 : count <= 50 ? 1 : 2]);
    }
    assert.equal(api.getObservationStyle(cell(100), { ...context, lowObservationCountBreak: 100, highObservationCountBreak: 200 }), styles[0]);
    assert.equal(styles[0].fill.color, 'rgba(255, 213, 128, 0.60)');
    assert.equal(styles[1].fill.color, 'rgba(255, 145, 0, 0.66)');
    assert.equal(styles[2].fill.color, 'rgba(220, 38, 38, 0.72)');
});

test('changing a limit redraws the existing layer while preserving selection', () => {
    const api = load();
    const selected = feature('selected', { ids: ['a', 'b'] }, 'pixelCluster', {}, { radiusMetersMin: 20 });
    let redraws = 0;
    api.attachLayersForTest([{
        get: key => key === 'id' ? 'locations' : undefined,
        getSource: () => ({ getFeatures: () => [selected] }),
        changed: () => { redraws++; }
    }]);
    api.setHighlightedLocationIds([], ['selected']);
    const highlight = selected.style;
    assert.equal(highlight.image.stroke.color, '#d500f9');
    const before = redraws;
    api.configureCompactClusterRendering(50);
    assert.equal(redraws, before + 1);
    assert.equal(selected.style, highlight);
    api.configureCompactClusterRendering(50);
    assert.equal(redraws, before + 1);
    api.setHighlightedLocationIds([], []);
    assert.equal(selected.style, undefined);
    assert.equal(api.getPointStyle(selected).image.fill.color, '#fff04a');
    api.configureCompactClusterRendering(null);
    assert.equal(api.getPointStyle(selected).image.fill.color, 'dodgerblue');
});

test('radius overlays recognize both cluster types and use individual location uncertainty', () => {
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/wwwroot/openLayersMap.js', import.meta.url), 'utf8').replace(/export /g, '');
    const context = vm.createContext({ ol: {
        Feature: class { constructor(options) { Object.assign(this, options); } set(key, value) { this[key] = value; } },
        geom: { Polygon: class { constructor(coordinates) { this.coordinates = coordinates; } } },
        proj: { transform: coordinates => coordinates }
    } });
    vm.runInContext(source + '\nglobalThis.api = { createClusterRadiusFeature, createCoordinateUncertaintyFeature };', context);
    const item = { decimalLongitude: 15, decimalLatitude: 59, coordinateUncertaintyInMeters: 100, type: 'Point' };
    const f = { geometry: { type: 'Point', coordinates: [15, 59] }, properties: { cluster: { radiusMeters: 200, radiusMetersMin: 300 } } };
    for (const groupingType of ['meterCluster', 'pixelCluster']) {
        f.properties.groupingType = groupingType;
        const radius = context.api.createClusterRadiusFeature(f, 'cluster.radiusMeters', 'EPSG:4326');
        const enclosure = context.api.createClusterRadiusFeature(f, 'cluster.radiusMetersMin', 'EPSG:4326');
        assert.equal(radius.sosNonInteractive, true);
        assert.ok(enclosure.geometry.coordinates[0][0][1] > radius.geometry.coordinates[0][0][1]);
        assert.equal(context.api.createCoordinateUncertaintyFeature(f, 'EPSG:4326', item), null);
    }
    f.properties.groupingType = 'coLocatedLocations';
    assert.equal(context.api.createClusterRadiusFeature(f, 'cluster.radiusMeters', 'EPSG:4326'), null);
    const small = context.api.createCoordinateUncertaintyFeature(f, 'EPSG:4326', item);
    const large = context.api.createCoordinateUncertaintyFeature(f, 'EPSG:4326', { ...item, coordinateUncertaintyInMeters: 500 });
    assert.ok(large.geometry.coordinates[0][0][1] > small.geometry.coordinates[0][0][1]);
});

test('polygon rendering uses a ten pixel diameter and the actual polygon area', () => {
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/wwwroot/openLayersMap.js', import.meta.url), 'utf8').replace(/export /g, '');
    const context = vm.createContext({ ol: { extent: {
        getWidth: extent => extent[2] - extent[0],
        getHeight: extent => extent[3] - extent[1]
    } } });
    vm.runInContext(source + '\nglobalThis.shouldRenderPolygonAsCentroid = shouldRenderPolygonAsCentroid;', context);
    for (const [width, height, area, expectedPoint] of [
        [9.99, 12, 119.88, true],
        [12, 9.99, 119.88, true],
        [10, 10, 100, false],
        [11, 11, 121, false],
        [200, 5, 1000, true],
        [12, 12, 44, true], // Polygon with a hole.
        [10, 10, Math.PI * 25, false]
    ]) {
        for (const resolution of [1, 100]) {
            const geometry = {
                getExtent: () => [0, 0, width * resolution, height * resolution],
                getArea: () => area * resolution * resolution
            };
            assert.equal(context.shouldRenderPolygonAsCentroid(geometry, resolution, 5), expectedPoint);
        }
    }
});
