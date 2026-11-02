import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';

class Geometry {
    constructor(type = 'Point', width = 0, height = 0) { Object.assign(this, { type, width, height }); }
    getType() { return this.type; }
    getExtent() { return [0, 0, this.width, this.height]; }
    getArea() { return this.width * this.height; }
    clone() { return new Geometry(this.type, this.width, this.height); }
}

function feature(count, groupingType = 'singleLocation', properties = {}, geometry = new Geometry()) {
    const values = { observationCount: count, groupingType, ...properties };
    return {
        geometry,
        geometryChanges: 0,
        getId: () => properties.id,
        get: key => values[key],
        set: (key, value) => { values[key] = value; },
        getGeometry() { return this.geometry; },
        setGeometry(value) { this.geometry = value; this.geometryChanges++; },
        setStyle(value) { this.style = value; },
        changed() {}
    };
}

function load() {
    const style = class { constructor(options) { Object.assign(this, options); } };
    const ol = {
        style: { Style: style, Circle: style, Fill: style, Stroke: style },
        source: { Vector: class {
            features = [];
            addFeatures(features) { this.features.push(...features); }
            getFeatures() { return this.features; }
        } },
        layer: { Vector: class extends style {
            properties = {};
            redraws = 0;
            get(key) { return this.properties[key]; }
            set(key, value) { this.properties[key] = value; }
            getSource() { return this.source; }
            setSource(value) { this.source = value; }
            setStyle(value) { this.style = value; }
            setZIndex() {}
            changed() { this.redraws++; }
        } },
        format: { GeoJSON: class { readFeatures(features) { return features; } } },
        geom: { Point: class extends Geometry { constructor() { super(); } } },
        extent: {
            getWidth: extent => extent[2] - extent[0],
            getHeight: extent => extent[3] - extent[1],
            getCenter: extent => [(extent[2] + extent[0]) / 2, (extent[3] + extent[1]) / 2]
        }
    };
    const context = vm.createContext({ console, ol, window: { ol } });
    const sharedSource = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/wwwroot/openLayersMap.js', import.meta.url), 'utf8').replace(/export /g, '');
    vm.runInContext(`globalThis.openLayersMap = (() => { ${sharedSource}; return { removeLayer, configurePolygonRendering }; })();`, context);
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/Pages/LocationsSearch.razor.js', import.meta.url), 'utf8')
        .replace(/^\s*import .*;$/m, '').replace(/export /g, '');
    vm.runInContext(source + '\nglobalThis.api = { renderMapResult, configurePointSizing, configureCompactClusterRendering, getPointStyle, getObservationStyle, setHighlightedLocationIds, calculateObservationCountBreaks, getObservationCountClass, attachMap(value) { map = value; } };', context);
    const view = { resolution: 1, getResolution() { return this.resolution; }, getProjection: () => 'EPSG:3857', on: (_event, listener) => { view.listener = listener; return {}; } };
    const map = {
        layers: [], viewReads: 0, renders: 0,
        getAllLayers() { return this.layers; },
        addLayer(layer) { this.layers.push(layer); },
        removeLayer(layer) { this.layers = this.layers.filter(value => value !== layer); },
        getView() { this.viewReads++; return view; },
        render() { this.renders++; },
        getSize: () => null
    };
    const api = context.api;
    api.attachMap(map);
    return { api, map, view, newResult(features, resultType = 'locations', polygons = 'No', options = {}) {
        return JSON.parse(JSON.stringify(api.renderMapResult('locations', features, {
            resultType, createPolygons: polygons, polygonToPointThresholdPx: 5, ...options
        })));
    } };
}

// Only counts from the supplied 44-point example; no locations or observation details.
const sampleCounts = [[1, 17], [2, 4], [3, 8], [4, 1], [5, 2], [7, 3], [8, 1], [10, 4], [17, 1], [18, 1], [27, 1], [73, 1]]
    .flatMap(([count, frequency]) => Array(frequency).fill(count));
const automaticBreakOptions = { lowPercentile: 0.75, highPercentile: 0.9, useRangeFallback: false };
const countSizeModes = ['ByObservationCount', 'ByObservationCountFixedBreaks'];

test('fixed sizing is the default and both count modes use the same three diameters for every grouping', () => {
    const { api, newResult } = load();
    const features = ['singleLocation', 'coLocatedLocations', 'meterCluster', 'pixelCluster']
        .flatMap(group => [1, 8, 73].map(count => feature(count, group)));
    assert.deepEqual(newResult(sampleCounts.map(count => feature(count))), []);
    for (const f of features) {
        const style = api.getPointStyle(f);
        assert.equal(style.image.radius, f.get('groupingType').endsWith('Cluster') ? 6 : 5);
        assert.equal(style.image.fill.color, { meterCluster: '#fff04a', pixelCluster: 'dodgerblue' }[f.get('groupingType')] ?? 'yellow');
        assert.equal(style.image.stroke.color, 'black');
    }

    for (const mode of countSizeModes) {
        api.configurePointSizing(mode);
        for (const f of features) {
            assert.equal(api.getPointStyle(f).image.radius * 2, { 1: 10, 8: 14, 73: 18 }[f.get('observationCount')]);
            assert.equal(api.getPointStyle(f).image.stroke.width, 1);
        }
    }
    api.configurePointSizing('Fixed');
    for (const f of features) assert.equal(api.getPointStyle(f).image.radius, f.get('groupingType').endsWith('Cluster') ? 6 : 5);
});

test('classification preserves the grid ranks, boundaries and fallback for ties', () => {
    const { api } = load();
    for (const [counts, expected] of [
        [[], null], [[NaN, Infinity], null], [[42], { low: 42, high: 42 }],
        [[7, 7, 7], { low: 7, high: 7 }],
        [[1, 2, 3, 4, 5, 6, 7], { low: 3, high: 5 }],
        [[0, 300], { low: 100, high: 200 }],
        [[1, 1, 1, 1, 1, 1000], { low: 334, high: 667 }]
    ]) {
        assert.deepEqual(JSON.parse(JSON.stringify(api.calculateObservationCountBreaks(counts))), expected);
    }
    const breaks = api.calculateObservationCountBreaks([1, 2, 3, 4, 5, 6, 7]);
    for (const [count, expected] of [[1, 0], [3, 0], [4, 1], [5, 1], [6, 2], [7, 2]])
        assert.equal(api.getObservationCountClass(count, breaks), expected);
});

test('legend contains only occupied classes and their actual observation ranges', () => {
    const { api, newResult } = load();
    api.configurePointSizing('ByObservationCount');
    assert.deepEqual(newResult([1, 2, 3, 4, 5, 6, 7].map(count => feature(count))), [
        { diameterPx: 10, minCount: 1, maxCount: 5 },
        { diameterPx: 14, minCount: 6, maxCount: 6 },
        { diameterPx: 18, minCount: 7, maxCount: 7 }
    ]);
    assert.deepEqual(newResult([feature(1), feature(1000)]), [
        { diameterPx: 10, minCount: 1, maxCount: 1 },
        { diameterPx: 18, minCount: 1000, maxCount: 1000 }
    ]);
    const identical = [feature(356), feature(356, 'pixelCluster')];
    assert.deepEqual(newResult(identical), [{ diameterPx: 10, minCount: 356, maxCount: 356 }]);
    identical.forEach(f => assert.equal(api.getPointStyle(f).image.radius, 5));
    const fixedLegend = api.configurePointSizing('ByObservationCountFixedBreaks');
    assert.deepEqual(JSON.parse(JSON.stringify(fixedLegend)), [{ diameterPx: 18, minCount: 51, maxCount: null }]);
    identical.forEach(f => assert.equal(api.getPointStyle(f).image.radius, 9));
});

test('invalid counts are excluded and render small, while representative summaries use total count', () => {
    const { api, newResult } = load();
    const invalid = [null, undefined, '', ' ', false, -1, NaN, Infinity, 'invalid', [], {}].map(count => feature(count));
    const representative = feature(356, 'singleLocation', { isSummaryApproximated: true, presentation: { status: 'Present', statusCounts: { Present: 1 } } });
    for (const mode of countSizeModes) {
        api.configurePointSizing(mode);
        assert.deepEqual(newResult(invalid), []);
        invalid.forEach(f => assert.equal(api.getPointStyle(f).image.radius, 5));
        newResult([feature(0), feature(10), representative, ...invalid]);
        assert.equal(api.getPointStyle(representative).image.radius, 9);
        invalid.forEach(f => assert.equal(api.getPointStyle(f).image.radius, 5));
    }
});

test('small Yes polygons participate after rendering, while surfaces and filter areas are excluded', () => {
    const { api, newResult, view } = load();
    const small = feature(100, 'singleLocation', {}, new Geometry('Polygon', 2, 2));
    const large = feature(10000, 'singleLocation', {}, new Geometry('Polygon', 100, 100));
    const filterArea = feature(100000, 'singleLocation', { FeatureType: 'FilterArea.Area' });
    const otherFeature = feature(200000, null);
    api.configurePointSizing('ByObservationCount');
    const counts = newResult([feature(1), feature(10), small, large, filterArea, otherFeature], 'locations', 'Yes');
    assert.equal(small.getGeometry().getType(), 'Point');
    assert.equal(large.getGeometry().getType(), 'Polygon');
    assert.deepEqual(counts.map(entry => entry.maxCount), [10, 100]);
    assert.equal(api.getPointStyle(small).image.radius, 9);
    assert.equal(api.getPointStyle(filterArea).image.radius, 5);
    const largeStyle = api.getObservationStyle(large, { resultType: 'locations' });
    api.configurePointSizing('Fixed');
    assert.equal(api.getObservationStyle(large, { resultType: 'locations' }), largeStyle);
    api.configurePointSizing('ByObservationCount');
    view.resolution = 100;
    view.listener();
    assert.equal(large.getGeometry().getType(), 'Point');
    assert.equal(api.getPointStyle(small).image.radius, 9, 'zoom does not recompute the result breaks');
});

test('colors, compact cluster styling and cached symbols survive size mode changes', () => {
    const { api, newResult } = load();
    const features = [
        feature(100, 'singleLocation', { presentation: { status: 'PresentSensitive' } }),
        feature(100, 'meterCluster', { cluster: { radiusMetersMin: 10 } }),
        feature(100, 'pixelCluster', { cluster: { radiusMetersMin: 10 } })
    ];
    newResult([...sampleCounts.map(count => feature(count)), ...features]);
    api.configureCompactClusterRendering(50);
    const fixedStyles = features.map(f => api.getPointStyle(f));
    for (const mode of countSizeModes) {
        api.configurePointSizing(mode);
        const countStyles = features.map(f => api.getPointStyle(f));
        features.forEach((f, i) => {
            assert.equal(countStyles[i].image.radius, 9);
            assert.equal(countStyles[i].image.fill.color, fixedStyles[i].image.fill.color);
            assert.deepEqual(countStyles[i].image.stroke, fixedStyles[i].image.stroke);
            assert.equal(api.getPointStyle(f), countStyles[i]);
        });
        assert.deepEqual(countStyles.map(s => s.image.fill.color), ['#d7191c', '#ffd54f', '#fff04a']);
    }
    api.configurePointSizing('Fixed');
    features.forEach((f, i) => assert.equal(api.getPointStyle(f), fixedStyles[i]));
});

test('switching size mode refreshes selected symbols without moving the map or loading data', () => {
    const { api, map, newResult } = load();
    const selected = feature(30, 'pixelCluster', { id: 'selected' });
    newResult([feature(1), feature(10), selected]);
    api.setHighlightedLocationIds([], ['selected']);
    assert.equal(selected.style.image.radius, 7);
    const layer = map.layers[0];
    const viewReads = map.viewReads;
    const redraws = layer.redraws;
    const automaticLegend = api.configurePointSizing('ByObservationCount');
    assert.equal(selected.style.image.radius, 11);
    assert.equal(selected.style.image.stroke.color, '#d500f9');
    assert.equal(map.layers[0], layer);
    assert.equal(map.viewReads, viewReads);
    assert.ok(layer.redraws > redraws);
    assert.equal(api.configurePointSizing('ByObservationCount'), automaticLegend, 'same mode reuses the legend');
    const fixedLegend = api.configurePointSizing('ByObservationCountFixedBreaks');
    assert.equal(selected.style.image.radius, 9);
    assert.deepEqual(JSON.parse(JSON.stringify(fixedLegend)), [
        { diameterPx: 10, minCount: null, maxCount: 5 },
        { diameterPx: 14, minCount: 6, maxCount: 50 }
    ]);
    assert.equal(api.configurePointSizing('ByObservationCount'), automaticLegend, 'switching modes reuses the original classification');
    assert.equal(selected.style.image.radius, 11);
    assert.equal(api.configurePointSizing('Fixed').length, 0);
    assert.equal(selected.style.image.radius, 7);
    assert.equal(map.viewReads, viewReads);
    api.setHighlightedLocationIds([], []);
    assert.equal(selected.style, undefined);
});

test('new results replace breaks and grid retains its colors with no point legend', () => {
    const { api, map, newResult } = load();
    api.configurePointSizing('ByObservationCount');
    const sameCount = feature(100);
    newResult([feature(1), feature(10), sameCount]);
    assert.equal(api.getPointStyle(sameCount).image.radius, 9);
    newResult([sameCount, feature(1000), feature(10000)]);
    assert.equal(api.getPointStyle(sameCount).image.radius, 5);

    const grid = [1, 10, 100].map(count => feature(undefined, null, { ObservationsCount: count }, new Geometry('Polygon', 100, 100)));
    assert.deepEqual(newResult(grid, 'geogrid'), []);
    const colors = grid.map(f => map.layers[0].style(f).fill.color);
    assert.deepEqual(colors, ['rgba(255, 213, 128, 0.60)', 'rgba(255, 145, 0, 0.66)', 'rgba(220, 38, 38, 0.72)']);
    api.configurePointSizing('Fixed');
    assert.deepEqual(grid.map(f => map.layers[0].style(f).fill.color), colors);
    assert.equal(api.configurePointSizing('ByObservationCountFixedBreaks').length, 0);
    assert.deepEqual(grid.map(f => map.layers[0].style(f).fill.color), colors);
    assert.deepEqual(newResult([], 'locations'), []);
    api.configurePointSizing('ByObservationCount');
    assert.deepEqual(newResult([feature(1)], 'locations'), [{ diameterPx: 10, minCount: 1, maxCount: 1 }]);
});

test('the 44-point example uses 75th and 90th percentiles or fixed 5/50 breaks', () => {
    const { api, newResult } = load();
    assert.equal(sampleCounts.length, 44);
    assert.deepEqual(JSON.parse(JSON.stringify(api.calculateObservationCountBreaks(sampleCounts, automaticBreakOptions))), { low: 7, high: 10 });
    const features = sampleCounts.map(count => feature(count));
    newResult(features);
    for (const [mode, expected] of [['ByObservationCount', [35, 5, 4]], ['ByObservationCountFixedBreaks', [32, 11, 1]]]) {
        api.configurePointSizing(mode);
        assert.deepEqual([5, 7, 9].map(radius => features.filter(f => api.getPointStyle(f).image.radius === radius).length), expected);
    }
});

test('fixed breaks include 5 and 50 in the smaller classes and retain stable legend labels', () => {
    const { api, newResult } = load();
    api.configurePointSizing('ByObservationCountFixedBreaks');
    const features = [0, 5, 6, 50, 51, 1000].map(count => feature(count));
    const legend = newResult(features);
    assert.deepEqual(features.map(f => api.getPointStyle(f).image.radius * 2), [10, 10, 14, 14, 18, 18]);
    assert.deepEqual(legend, [
        { diameterPx: 10, minCount: null, maxCount: 5 },
        { diameterPx: 14, minCount: 6, maxCount: 50 },
        { diameterPx: 18, minCount: 51, maxCount: null }
    ]);
    assert.deepEqual(newResult([feature(2), feature(20), feature(9999)]), legend);
    assert.deepEqual(newResult([feature(51)]), [{ diameterPx: 18, minCount: 51, maxCount: null }]);
    assert.equal(api.getPointStyle(feature(51)).image.radius, 9);
});

test('automatic ties allow an empty middle class without the grid range fallback', () => {
    const { api, newResult } = load();
    api.configurePointSizing('ByObservationCount');
    const counts = [1, 1, 1, 1, 1, 1000];
    assert.deepEqual(JSON.parse(JSON.stringify(api.calculateObservationCountBreaks(counts, automaticBreakOptions))), { low: 1, high: 1 });
    assert.deepEqual(newResult(counts.map(count => feature(count))), [
        { diameterPx: 10, minCount: 1, maxCount: 1 },
        { diameterPx: 18, minCount: 1000, maxCount: 1000 }
    ]);
    const values = [1, 2, 3, 4, 5, 6, 7].map(count => feature(count));
    newResult(values);
    assert.deepEqual(values.map(f => api.getPointStyle(f).image.radius * 2), [10, 10, 10, 10, 10, 14, 18]);
});

for (const type of ['Polygon', 'MultiPolygon']) {
    test(`${type} geometry changes only when crossing the point/surface boundary`, () => {
        const { newResult, view } = load();
        const polygon = feature(1, 'singleLocation', {}, new Geometry(type, 10, 10));
        const original = polygon.getGeometry();
        newResult([polygon], 'locations', 'Yes');
        for (const resolution of [0.1, 0.5, 0.99, 1]) {
            view.resolution = resolution;
            view.listener();
            assert.equal(polygon.getGeometry(), original);
        }
        assert.equal(polygon.geometryChanges, 0);

        view.resolution = 1.01;
        view.listener();
        const point = polygon.getGeometry();
        assert.equal(point.getType(), 'Point');
        for (const resolution of [1.1, 2, 10, 100]) {
            view.resolution = resolution;
            view.listener();
            assert.equal(polygon.getGeometry(), point);
        }
        assert.equal(polygon.geometryChanges, 1);

        view.resolution = 1;
        view.listener();
        const restored = polygon.getGeometry();
        assert.equal(restored.getType(), type);
        for (const resolution of [0.9, 0.5, 0.1]) {
            view.resolution = resolution;
            view.listener();
            assert.equal(polygon.getGeometry(), restored);
        }
        assert.equal(polygon.geometryChanges, 2);
    });
}

for (const mode of ['No', 'WhenNeeded']) {
    test(`switching from Yes to ${mode} restores the surface once and keeps it during zoom`, () => {
        const { newResult, view } = load();
        const polygon = feature(1, 'singleLocation', {}, new Geometry('Polygon', 2, 2));
        newResult([polygon], 'locations', 'Yes');
        assert.equal(polygon.getGeometry().getType(), 'Point');
        newResult([polygon], 'locations', mode);
        const restored = polygon.getGeometry();
        assert.equal(restored.getType(), 'Polygon');
        for (const resolution of [0.1, 1, 100]) {
            view.resolution = resolution;
            view.listener();
            assert.equal(polygon.getGeometry(), restored);
        }
        assert.equal(polygon.geometryChanges, 2);
    });
}

test('batched rendering sizes polygons shown as points before painting selection once', () => {
    const { api, newResult, map } = load();
    const selected = feature(73, 'singleLocation', { id: 'selected', locations: { ids: ['a'] } }, new Geometry('Polygon', 2, 2));
    map.getSize = () => { throw new Error('Replacement results must not move the map'); };
    const legend = newResult([feature(1), feature(10), selected], 'locations', 'Yes', {
        pointSizeMode: 'ByObservationCountFixedBreaks',
        highlight: { locationIds: ['a'], featureIds: [], selection: { featureCount: 1, locationIds: ['a'], featureIds: ['selected'] }, clickVersion: 1 }
    });
    assert.equal(selected.getGeometry().getType(), 'Point');
    assert.equal(selected.style.image.radius, 11);
    assert.equal(selected.style.image.stroke.color, '#d500f9');
    assert.deepEqual(legend.map(entry => entry.diameterPx), [10, 14, 18]);
    assert.equal(map.renders, 1);

    const replacement = feature(10, 'pixelCluster', { id: 'new-cluster', locations: { ids: ['a', 'b'] } });
    newResult([replacement], 'locations', 'No');
    assert.equal(map.renders, 2);
    assert.equal(replacement.style.image.radius, 9);
    // An older acknowledgement in the next batch cannot remove the newer selection.
    newResult([replacement], 'locations', 'No', {
        highlight: { locationIds: [], featureIds: [], selection: { featureCount: 0, locationIds: [], featureIds: [] }, clickVersion: 0 }
    });
    assert.equal(map.renders, 3);
    assert.equal(replacement.style.image.stroke.color, '#d500f9');
    api.setHighlightedLocationIds([], [], false, null, 2);
    assert.equal(replacement.style, undefined);
});
