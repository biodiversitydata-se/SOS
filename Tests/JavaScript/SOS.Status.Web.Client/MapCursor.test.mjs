import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';

function deferred() {
    let resolve, reject;
    const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
    return { promise, resolve, reject };
}

const settle = async () => { for (let i = 0; i < 4; i++) await Promise.resolve(); };
const earthRadius = 6378137;
const project = ([longitude, latitude]) => [longitude * Math.PI / 180 * earthRadius, Math.log(Math.tan(Math.PI / 4 + latitude * Math.PI / 360)) * earthRadius];
const unproject = ([x, y]) => [x / earthRadius * 180 / Math.PI, (2 * Math.atan(Math.exp(y / earthRadius)) - Math.PI / 2) * 180 / Math.PI];
const feature = (nonInteractive = false, type = 'Point', properties = {}, coordinates = project([15, 59])) => {
    const geometry = { getType: () => type, getCoordinates: () => type === 'MultiPoint' ? [coordinates] : coordinates };
    return {
        get: key => key === 'sosNonInteractive' ? nonInteractive : properties[key],
        getId: () => properties.id ?? 'point',
        getGeometry: () => geometry,
        getStyle() { return this.style; },
        setStyle(style) { this.style = style; },
        changed() {}
    };
};

function namedPoint(name = 'Årike', count = 1234, uncertainty = 50) {
    return feature(false, 'Point', { groupingType: 'singleLocation', observationCount: count,
        locations: { count: 1, ids: ['Site-A'], items: [{ locality: name, locationId: 'Site-A', coordinateUncertaintyInMeters: uncertainty }] } });
}

function load(mode = 'Cursor', withTooltip = true) {
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/Pages/LocationsSearch.razor.js', import.meta.url), 'utf8')
        .replace(/^\s*import .*;$/m, '').replace(/export /g, '');
    const frames = new Map(), timers = new Map(), maps = [], calls = [];
    let nextId = 0, now = 0;
    let tooltipText = '';
    const tooltip = {
        hidden: true, style: {}, width: 180, height: 60, textWrites: 0, measurements: 0,
        children: [],
        ownerDocument: { createElement: tag => ({ tagName: tag.toUpperCase(), textContent: '' }) },
        appendChild(child) { this.children.push(child); },
        get textContent() { return tooltipText + this.children.map(child => child.textContent).join(''); },
        set textContent(value) { tooltipText = value; this.children = []; this.textWrites++; },
        set innerHTML(value) { assert.fail('tooltip content must be inserted as literal text'); },
        get offsetWidth() { this.measurements++; return Math.min(this.width, maps.at(-1).getSize()[0] - 16); },
        get offsetHeight() { this.measurements++; return Math.min(this.height, maps.at(-1).getSize()[1] - 16); }
    };
    class Layer {
        properties = {};
        revision = 0;
        visible = true;
        hits = [];
        queries = [];
        constructor(options = {}) { this.source = options.source; this.style = options.style; }
        set(key, value) { this.properties[key] = value; this.revision++; }
        get(key) { return this.properties[key]; }
        getRevision() { return this.revision; }
        getSource() { return this.source; }
        setZIndex(value) { this.zIndex = value; }
        changed() { this.revision++; }
        setSource(source) { this.source = source; this.revision++; }
        setVisible(value) { this.visible = value; this.revision++; }
        isVisible() { return this.visible; }
        getFeatures(pixel) {
            this.queries.push({ pixel: [...pixel], time: now });
            return this.detect ? this.detect(pixel) : Promise.resolve(this.hits.slice(0, 1));
        }
    }
    class VectorSource {
        features = [];
        writes = 0;
        clear() { this.features = []; this.writes++; }
        addFeature(feature) { this.features.push(feature); this.writes++; }
    }
    class MapStub {
        constructor(options) {
            maps.push(this); this.layers = options.layers; this.handlers = new Map();
            this.writes = []; this.listeners = new Map(); this.exactQueries = 0;
            this.size = [800, 600];
            this.view = {
                revision: 0, animating: false, interacting: false,
                resolution: 4, projection: 'EPSG:3857',
                getResolution() { return this.resolution; },
                getProjection() { return this.projection; },
                getRevision() { return this.revision; },
                getAnimating() { return this.animating; },
                getInteracting() { return this.interacting; },
                calculateExtent: () => [0, 0, 10, 10], getZoom: () => 14
            };
            let cursor = '';
            const owner = this;
            this.viewport = {
                style: {
                    get cursor() { return cursor; },
                    set cursor(value) { cursor = value; owner.writes.push(value); }
                },
                addEventListener: (name, handler) => this.listeners.set(name, handler),
                removeEventListener: (name, handler) => { if (this.listeners.get(name) === handler) this.listeners.delete(name); }
            };
        }
        on(type, handler) { this.handlers.set(type, handler); return { owner: this, type }; }
        getViewport() { return this.viewport; }
        getAllLayers() { return this.layers; }
        addLayer(layer) { this.layers.push(layer); }
        removeLayer(layer) { this.layers = this.layers.filter(item => item !== layer); }
        getSize() { return this.size; }
        getView() { return this.view; }
        getEventPixel(event) { return event.pixel; }
        updateSize() {}
        setTarget(value) { this.target = value; }
        forEachFeatureAtPixel() { this.exactQueries++; throw new Error('Hover must not call exact map hit detection'); }
    }
    const Style = class {
        constructor(options) { Object.assign(this, options); }
        getImage() { return this.image; }
        getRadius() { return this.radius; }
        getStroke() { return this.stroke; }
        getWidth() { return this.width; }
    };
    const Polygon = class { constructor(coordinates) { this.coordinates = coordinates; } getType() { return 'Polygon'; } getCoordinates() { return this.coordinates; } };
    const MultiPolygon = class extends Polygon { getType() { return 'MultiPolygon'; } };
    const pointResolutions = [];
    const context = vm.createContext({
        window: { ol: true }, console,
        performance: { now: () => now },
        setTimeout: (callback, delay) => { const id = ++nextId; timers.set(id, { callback, time: now + delay }); return id; },
        clearTimeout: id => timers.delete(id),
        requestAnimationFrame: callback => { const id = ++nextId; frames.set(id, callback); return id; },
        cancelAnimationFrame: id => frames.delete(id),
        openLayersMap: { removeLayer: (map, id) => { map.layers = map.layers.filter(layer => layer.get('id') !== id); } },
        ol: {
            Map: MapStub, View: class {}, layer: { Tile: Layer, Vector: Layer }, source: { OSM: class {}, Vector: VectorSource },
            Feature: class {
                constructor(options) { this.geometry = options.geometry; this.geometryWrites = 0; }
                getGeometry() { return this.geometry; }
                setGeometry(geometry) { this.geometry = geometry; this.geometryWrites++; }
            },
            geom: { Polygon, MultiPolygon },
            style: { Style, Stroke: Style, Circle: Style, Fill: Style },
            proj: {
                fromLonLat: project,
                toLonLat: (x, projection = 'EPSG:3857') => projection === 'EPSG:4326' ? x : unproject(x),
                transform: (x, from, to) => from === to ? x : to === 'EPSG:3857' ? project(x) : unproject(x),
                getPointResolution(projection, resolution, center, units) {
                    pointResolutions.push({ projection, resolution, center, units });
                    return projection === 'EPSG:3857' ? resolution * Math.cos(unproject(center)[1] * Math.PI / 180) : resolution * 111319.490793;
                }
            },
            Observable: { unByKey: key => key.owner.handlers.delete(key.type) }
        }
    });
    const sharedSource = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/wwwroot/openLayersMap.js', import.meta.url), 'utf8').replace(/export /g, '');
    vm.runInContext(`globalThis.openLayersMap = (() => { ${sharedSource}; return { removeLayer, createGeodesicCircleCoordinates }; })();`, context);
    vm.runInContext(source + '\nglobalThis.api = { initMap, destroyMap, removeLayer, setHoverFeedbackMode, configurePointSizing, setHighlightedLocationIds, setMapTooltipEnabled, createMapTooltipContent };', context);
    const flushFrames = () => { const pending = [...frames.values()]; frames.clear(); pending.forEach(callback => callback()); };
    const advance = async (ms = 34) => {
        const target = now + ms;
        while (true) {
            const next = [...timers].filter(([, timer]) => timer.time <= target).sort((a, b) => a[1].time - b[1].time)[0];
            if (!next) break;
            const [id, timer] = next;
            timers.delete(id); now = timer.time; timer.callback(); await settle();
        }
        now = target; await settle();
    };
    const init = () => {
        context.api.initMap({}, { invokeMethodAsync: (...args) => { calls.push(args); return Promise.resolve(); } }, 14, 15, 59, withTooltip ? tooltip : null);
        if (mode) context.api.setHoverFeedbackMode(mode);
        flushFrames(); // Initial sizing/extent notification is independent of hover.
        const map = maps.at(-1), layer = new Layer(); layer.set('id', 'locations'); map.layers.push(layer);
        layer.source = { getFeatures: () => layer.hits };
        return { map, layer };
    };
    const { map, layer } = init(); calls.length = 0;
    const move = (pixel = [1, 2], dragging = false, pointerType = 'mouse') => map.handlers.get(dragging ? 'pointerdrag' : 'pointermove')({ pixel, dragging, originalEvent: { pointerType } });
    const outline = () => map.layers.find(layer => layer.get('id') === 'hover-outline');
    return { api: context.api, map, layer, Layer, calls, timers, advance, init, move, outline, mode, pointResolutions, tooltip };
}

test('background uses the standard arrow and locations or grid use fast layer detection without exact queries or API calls', async () => {
    const f = load();
    assert.equal(f.map.viewport.style.cursor, 'default');
    f.move(); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'default');
    for (const resultType of ['locations', 'geogrid']) {
        f.layer.set('sosResultType', resultType); f.layer.hits = [feature()];
        f.move(); await f.advance();
        assert.equal(f.map.viewport.style.cursor, 'pointer');
    }
    f.layer.hits = []; f.move(); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'default');
    assert.equal(f.map.exactQueries, 0);
    assert.equal(f.calls.length, 0);
});

test('hover coalesces movements at the latest pixel and only writes cursor changes', async () => {
    const f = load(); f.layer.hits = [feature()];
    for (let i = 0; i < 100; i++) f.move([i, i]);
    assert.equal(f.timers.size, 1);
    assert.equal(f.layer.queries.length, 0);
    await f.advance();
    assert.equal(f.layer.queries.length, 1);
    assert.deepEqual(f.layer.queries[0].pixel, [99, 99]);
    f.move(); await f.advance();
    assert.deepEqual(f.map.writes, ['default', 'pointer']);
});

test('120 pointer movements per second start at most 30 queries and a trailing movement is not lost', async () => {
    const f = load();
    for (let i = 0; i < 120; i++) { f.move([i, i]); await f.advance(1000 / 120); }
    assert.ok(f.layer.queries.length <= 30, `started ${f.layer.queries.length} queries in one second`);
    assert.ok(f.layer.queries.length >= 29, 'updates continue while the mouse moves');
    await f.advance();
    assert.deepEqual(f.layer.queries.at(-1).pixel, [119, 119]);
    for (let i = 1; i < f.layer.queries.length; i++)
        assert.ok(f.layer.queries[i].time - f.layer.queries[i - 1].time >= 1000 / 30);
});

test('only one query runs at a time and movements while waiting use the latest position', async () => {
    const f = load(), first = deferred();
    f.layer.detect = () => first.promise;
    f.move(); await f.advance(0);
    for (let i = 0; i < 100; i++) { f.move([i, i]); await f.advance(1); }
    assert.equal(f.layer.queries.length, 1);
    f.layer.detect = null; f.layer.hits = [feature()];
    first.resolve([feature()]); await settle();
    assert.equal(f.map.viewport.style.cursor, 'default', 'the stale hit must not change the cursor');
    await f.advance(0);
    assert.equal(f.layer.queries.length, 2);
    assert.deepEqual(f.layer.queries.at(-1).pixel, [99, 99]);
    assert.equal(f.map.viewport.style.cursor, 'pointer');
});

test('clickable cursor can be toggled immediately without detection or API calls while disabled', async () => {
    const f = load(); f.layer.hits = [feature()];
    for (const resultType of ['locations', 'geogrid']) {
        f.layer.set('sosResultType', resultType); f.api.setHoverFeedbackMode('Cursor');
        f.move(); await f.advance();
        assert.equal(f.map.viewport.style.cursor, 'pointer');
        f.move(); f.api.setHoverFeedbackMode('None');
        assert.equal(f.map.viewport.style.cursor, 'default');
        assert.equal(f.timers.size, 0);
        const queries = f.layer.queries.length;
        f.move(); await f.advance();
        f.map.listeners.get('pointerup')({ pixel: [1, 2], pointerType: 'mouse' }); await f.advance();
        assert.equal(f.map.viewport.style.cursor, 'default');
        assert.equal(f.layer.queries.length, queries);
    }
    f.api.setHoverFeedbackMode('Cursor'); f.move(); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'pointer');
    assert.equal(f.calls.length, 0);
});

test('helper layers, hidden layers and noninteractive features do not give pointer', async () => {
    const f = load(), outlines = new f.Layer(), overlay = new f.Layer();
    outlines.set('id', 'filter-areas'); overlay.set('id', 'cluster-radius');
    outlines.hits = overlay.hits = [feature()]; f.map.layers.push(outlines, overlay);
    f.layer.hits = [feature(true)]; f.move(); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'default');
    assert.equal(outlines.queries.length + overlay.queries.length, 0);
    f.layer.set('sosNonInteractive', true); f.move(); await f.advance();
    assert.equal(f.layer.queries.length, 1, 'noninteractive layers are not queried');
    f.layer.set('sosNonInteractive', false); f.layer.setVisible(false); f.move(); await f.advance();
    assert.equal(f.layer.queries.length, 1, 'hidden layers are not queried');
    f.layer.setVisible(true); f.layer.hits = [feature()]; f.move(); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'pointer');
});

test('dragging cancels queued work, keeps the standard arrow and restores hover feedback after release', async () => {
    const f = load(); f.layer.hits = [feature()];
    f.move(); f.move([3, 4], true); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'default');
    assert.equal(f.layer.queries.length, 0);
    f.map.handlers.get('movestart')(); f.move([4, 5], true);
    f.api.setHoverFeedbackMode('None');
    assert.equal(f.map.viewport.style.cursor, 'default');
    f.api.setHoverFeedbackMode('Cursor');
    f.map.listeners.get('pointerup')({ pixel: [4, 5], pointerType: 'mouse' });
    await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'pointer');
});

test('zoom animation, interaction and touch movements do not query features', async () => {
    const f = load();
    for (const state of ['animating', 'interacting']) {
        f.map.view[state] = true; f.move(); await f.advance(); f.map.view[state] = false;
    }
    f.move([1, 2], false, 'touch'); f.move([1, 2], true, 'touch'); await f.advance();
    assert.equal(f.layer.queries.length, 0);
    assert.equal(f.map.viewport.style.cursor, 'default');
});

for (const mode of ['Cursor', 'Outline']) for (const [name, invalidate] of [
    ['pointer leave', f => f.map.listeners.get('pointerleave')()],
    ['pointer cancel', f => f.map.listeners.get('pointercancel')()],
    ['disabled then reenabled hover', f => { f.api.setHoverFeedbackMode('None'); f.api.setHoverFeedbackMode(f.mode); }],
    ['dragging', f => f.move([3, 4], true)],
    ['pan or zoom starts', f => f.map.handlers.get('movestart')()],
    ['pan or zoom ends', f => f.map.handlers.get('moveend')()],
    ['view changes before move events', f => { f.map.view.revision++; }],
    ['result source is replaced', f => f.layer.setSource({})],
    ['result layer is hidden', f => f.layer.setVisible(false)],
    ['result layer is removed', f => f.api.removeLayer('locations')],
    ['map is destroyed', f => f.api.destroyMap()]
]) {
    test(`late ${mode} hover result cannot change feedback after ${name}`, async () => {
        const f = load(mode), pending = deferred(); f.layer.detect = () => pending.promise;
        f.move(); await f.advance(0);
        assert.equal(f.layer.queries.length, 1);
        invalidate(f);
        const cursor = f.map.viewport.style.cursor;
        pending.resolve([feature()]); await settle(); await f.advance();
        assert.equal(f.map.viewport.style.cursor, cursor);
        assert.equal(f.outline(), undefined, 'a stale hit must not create an outline');
        assert.equal(f.layer.queries.length, 1);
    });
}

test('destroy cancels timers, removes listeners and a late old-map result cannot interfere with a new query', async () => {
    const f = load(), oldQuery = deferred(); f.layer.detect = () => oldQuery.promise;
    f.move(); await f.advance(0);
    const next = f.init(), newQuery = deferred(); next.layer.detect = () => newQuery.promise;
    assert.equal(f.map.handlers.size, 0);
    assert.equal(f.map.listeners.size, 0);
    assert.equal(f.map.viewport.style.cursor, '');
    assert.equal(f.map.target, null);
    assert.equal(next.map.listeners.size, 4);
    next.map.handlers.get('pointermove')({ pixel: [3, 4] }); await f.advance(0);
    oldQuery.resolve([feature()]); await settle();
    assert.equal(next.map.viewport.style.cursor, 'default');
    newQuery.resolve([feature()]); await settle();
    assert.equal(next.map.viewport.style.cursor, 'pointer');
    next.map.handlers.get('pointermove')({ pixel: [4, 5] });
    f.api.destroyMap(); await f.advance();
    assert.equal(f.timers.size, 0);
    assert.equal(next.layer.queries.length, 1);
});

test('the default mode draws a filled point area without changing the standard cursor or original selection style', async () => {
    const f = load(null), point = feature();
    const selectionStyle = { color: 'magenta' };
    point.style = selectionStyle;
    point.setStyle = () => assert.fail('hover must not modify the original style');
    f.layer.hits = [point];
    const revision = f.layer.revision;
    f.move(); await f.advance();
    const outline = f.outline(), proxy = outline.getSource().features[0], style = outline.style();
    assert.equal(f.map.viewport.style.cursor, 'default');
    assert.deepEqual(f.map.writes, ['default']);
    assert.equal(outline.get('sosNonInteractive'), true);
    assert.ok(outline.zIndex > 20);
    assert.notEqual(proxy, point, 'use an independent feature so selected styles cannot override the red ring');
    assert.equal(proxy.getGeometry().getType(), 'Polygon');
    assert.equal(style.stroke.color, '#ff0000');
    assert.equal(style.fill.color, 'rgba(255, 235, 59, 0.12)');
    assert.equal(style.image, undefined, 'use geographic geometry, not a giant circle symbol');
    assert.equal(point.style, selectionStyle);
    assert.equal(f.layer.revision, revision, 'hover must not invalidate the observations layer hit cache');
    assert.equal(f.map.exactQueries, 0);
    assert.equal(f.calls.length, 0);
});

function measuredHoverRadius(f, center = [15, 59]) {
    const geometry = f.outline().getSource().features[0].getGeometry();
    const coordinates = geometry.getCoordinates();
    const vertex = geometry.getType() === 'MultiPolygon' ? coordinates[0][0][0] : coordinates[0][0];
    const target = f.map.view.projection === 'EPSG:3857' ? unproject(vertex) : vertex;
    const radians = Math.PI / 180;
    const dLat = (target[1] - center[1]) * radians, dLon = (target[0] - center[0]) * radians;
    const a = Math.sin(dLat / 2) ** 2 + Math.cos(center[1] * radians) * Math.cos(target[1] * radians) * Math.sin(dLon / 2) ** 2;
    return 2 * earthRadius * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

const minimumRadius = (pixels = 5.5, resolution = 4, latitude = 59) => pixels * resolution * Math.cos(latitude * Math.PI / 180);
const close = (actual, expected) => assert.ok(Math.abs(actual - expected) < 0.001, `expected ${expected} m, got ${actual} m`);

for (const uncertainty of [undefined, null, 0, -10, NaN, Infinity, 'invalid', '', false, 1, minimumRadius(), 100, '200']) {
    test(`uncertainty ${String(uncertainty)} respects the geographic radius and minimum black symbol outline`, async () => {
        const f = load('Outline');
        f.layer.hits = [feature(false, 'Point', { groupingType: 'singleLocation', locations: { items: [{ coordinateUncertaintyInMeters: uncertainty }] } })];
        f.move(); await f.advance();
        const valid = typeof uncertainty === 'number' || typeof uncertainty === 'string' ? Number(uncertainty) : 0;
        close(measuredHoverRadius(f), Number.isFinite(valid) && valid > 0 ? Math.max(valid, minimumRadius()) : minimumRadius());
        assert.equal(f.pointResolutions.length, 1);
        assert.equal(f.pointResolutions[0].units, 'm');
        assert.deepEqual(f.pointResolutions[0].center, project([15, 59]));
        assert.equal(f.outline().style().stroke.width, 2);
        assert.equal(f.outline().style().image, undefined);
    });
}

test('coincident unclustered locations use the largest valid uncertainty already included in the result', async () => {
    const f = load('Outline');
    f.layer.hits = [feature(false, 'Point', { groupingType: 'coLocatedLocations', locations: { count: 200, items: [
        { coordinateUncertaintyInMeters: 20 }, { coordinateUncertaintyInMeters: -1000 }, { CoordinateUncertaintyInMeters: 300 }
    ] } })];
    f.move(); await f.advance();
    close(measuredHoverRadius(f), 300);
    assert.equal(f.calls.length, 0, 'missing location details must not trigger another API call');
});

for (const groupingType of ['meterCluster', 'pixelCluster']) {
    test(`${groupingType} uses its enclosing radius rather than the search radius or member uncertainties`, async () => {
        const f = load('Outline');
        const properties = { groupingType, cluster: { radiusMetersMin: 120, radiusMeters: 5000 }, locations: { items: [{ coordinateUncertaintyInMeters: 10000 }] } };
        f.layer.hits = [feature(false, 'Point', properties)]; f.move(); await f.advance();
        close(measuredHoverRadius(f), 120);
        for (const invalid of [undefined, null, 0, -5, Infinity, 'invalid']) {
            properties.cluster.radiusMetersMin = invalid;
            f.map.handlers.get('movestart')(); f.move(); await f.advance();
            close(measuredHoverRadius(f), minimumRadius(6.5));
        }
    });
}

test('a very large uncertainty uses one fixed-size polygon and fills the viewport without an extra symbol ring', async () => {
    const f = load('Outline');
    f.layer.hits = [feature(false, 'Point', { locations: { items: [{ coordinateUncertaintyInMeters: 100000 }] } })];
    f.move(); await f.advance();
    const source = f.outline().getSource(), ring = source.features[0].getGeometry().getCoordinates()[0];
    assert.equal(source.features.length, 1);
    assert.equal(ring.length, 97, 'large radii must not allocate a larger canvas or more vertices');
    close(measuredHoverRadius(f), 100000);
    const xs = ring.map(p => p[0]), ys = ring.map(p => p[1]);
    assert.ok(Math.max(...xs) - Math.min(...xs) > 800 * f.map.view.resolution);
    assert.ok(Math.max(...ys) - Math.min(...ys) > 600 * f.map.view.resolution);
    assert.equal(f.outline().style().fill.color, 'rgba(255, 235, 59, 0.12)');
    assert.equal(f.outline().style().image, undefined);
    f.layer.hits = []; f.move([50, 50]); await f.advance();
    assert.equal(source.features.length, 0, 'the filled area must not keep itself hovered');
});

test('point size and selection changes resize the existing hover geometry without more hit queries or map movements', async () => {
    const f = load('Outline'), point = feature(false, 'Point', { id: 'point', groupingType: 'singleLocation', observationCount: 100 });
    f.layer.hits = [point]; f.move(); await f.advance();
    const proxy = f.outline().getSource().features[0];
    close(measuredHoverRadius(f), minimumRadius());
    f.api.configurePointSizing('ByObservationCountFixedBreaks');
    close(measuredHoverRadius(f), minimumRadius(9.5));
    f.api.setHighlightedLocationIds([], ['point']);
    close(measuredHoverRadius(f), minimumRadius(12));
    f.api.setHighlightedLocationIds([], []);
    close(measuredHoverRadius(f), minimumRadius(9.5));
    f.api.configurePointSizing('Fixed');
    close(measuredHoverRadius(f), minimumRadius());
    assert.equal(proxy.geometryWrites, 4);
    assert.equal(f.outline().getSource().features[0], proxy);
    assert.equal(f.layer.queries.length, 1);
    assert.equal(f.calls.length, 0);
});

test('selection styling during an in-flight hit retries the same pixel instead of requiring another mouse movement', async () => {
    const f = load('Outline'), pending = deferred();
    const point = feature(false, 'Point', { groupingType: 'singleLocation', locations: { items: [{ coordinateUncertaintyInMeters: 200 }] } });
    f.layer.hits = [point]; f.layer.detect = () => pending.promise;
    f.move([12, 34]); await f.advance(0);
    f.api.setHighlightedLocationIds([], ['point']);
    f.layer.detect = null; pending.resolve([point]); await settle();
    assert.equal(f.outline(), undefined, 'the old renderer hit must still be ignored');
    await f.advance();
    assert.equal(f.layer.queries.length, 2);
    assert.deepEqual(f.layer.queries[1].pixel, [12, 34]);
    assert.ok(f.layer.queries[1].time - f.layer.queries[0].time >= 1000 / 30);
    close(measuredHoverRadius(f), 200);
    assert.equal(f.calls.length, 0);
});

test('zoom recomputes the minimum size on the next hover and projection scale is evaluated at the hovered point', async () => {
    const f = load('Outline');
    f.layer.hits = [feature(false, 'Point', { locations: { items: [{ coordinateUncertaintyInMeters: 20 }] } })];
    f.move(); await f.advance(); close(measuredHoverRadius(f), 20);
    f.map.handlers.get('movestart')(); f.map.view.resolution = 100; f.map.view.revision++;
    f.map.handlers.get('moveend')();
    assert.equal(f.outline().getSource().features.length, 0);
    f.move(); await f.advance(); close(measuredHoverRadius(f), minimumRadius(5.5, 100));
    const center = [20, 65];
    f.layer.hits = [feature(false, 'Point', {}, project(center))];
    f.move(); await f.advance(); close(measuredHoverRadius(f, center), minimumRadius(5.5, 100, 65));
});

test('circle coordinates are transformed to the current projection', async () => {
    const f = load('Outline'); f.map.view.projection = 'EPSG:4326'; f.map.view.resolution = 0.0001;
    f.layer.hits = [feature(false, 'Point', { locations: { items: [{ coordinateUncertaintyInMeters: 500 }] } }, [15, 59])];
    f.move(); await f.advance(); close(measuredHoverRadius(f), 500);
    assert.equal(f.pointResolutions[0].projection, 'EPSG:4326');
});

test('invalid map resolution falls back to a symbol-sized circle without allocating a large image', async () => {
    const f = load('Outline'); f.map.view.resolution = 0;
    f.layer.hits = [feature(false, 'Point', { locations: { items: [{ coordinateUncertaintyInMeters: 100000 }] } })];
    f.move(); await f.advance();
    assert.equal(f.outline().style().image.radius, 5.5);
    assert.equal(f.outline().style().image.fill.color, 'rgba(255, 235, 59, 0.12)');
});

for (const type of ['Polygon', 'MultiPolygon', 'Point', 'MultiPoint']) {
    test(`Outline mode draws the displayed ${type} and only updates when the hit changes`, async () => {
        const f = load('Outline'), hit = feature(false, type);
        f.layer.hits = [hit]; f.move(); await f.advance();
        const outline = f.outline(), source = outline.getSource(), writes = source.writes;
        const style = outline.style();
        const point = type === 'Point' || type === 'MultiPoint';
        assert.equal(style.fill?.color, point ? 'rgba(255, 235, 59, 0.12)' : undefined);
        assert.equal((style.image?.stroke ?? style.stroke).color, '#ff0000');
        if (!point) assert.equal(outline.getSource().features[0].getGeometry(), hit.getGeometry(), 'reuse large polygon geometry');
        assert.equal(f.map.viewport.style.cursor, 'default');
        f.move([2, 3]); await f.advance();
        assert.equal(source.writes, writes, 'moving within the same feature must not rebuild its outline');
        f.layer.hits = [feature(false, type)]; f.move([3, 4]); await f.advance();
        assert.equal(source.features.length, 1);
        assert.equal(outline.style(), style, 'outline styles are reused');
        f.layer.hits = []; f.move([4, 5]); await f.advance();
        assert.equal(source.features.length, 0);
        assert.deepEqual(f.map.writes, ['default']);
    });
}

test('clusters without a radius fall back to the symbol and grid cells keep an unfilled outline', async () => {
    const f = load('Outline');
    f.layer.hits = [feature(false, 'Point', { groupingType: 'pixelCluster' })];
    f.move(); await f.advance();
    assert.equal(f.outline().getSource().features[0].getGeometry().getType(), 'Polygon');
    assert.equal(f.outline().style().fill.color, 'rgba(255, 235, 59, 0.12)');
    f.layer.set('sosResultType', 'geogrid');
    f.layer.hits = [feature(false, 'Polygon')]; f.move(); await f.advance();
    assert.equal(f.outline().style().stroke.color, '#ff0000');
    assert.equal(f.outline().style().fill, undefined);
    assert.equal(f.map.viewport.style.cursor, 'default');
});

test('switching modes clears old feedback immediately, keeps the layer for reuse and disables all hover work in None', async () => {
    const f = load('Outline'); f.layer.hits = [feature()];
    f.move(); await f.advance(); const outline = f.outline();
    f.api.setHoverFeedbackMode('Cursor');
    assert.equal(outline.getSource().features.length, 0);
    f.move(); await f.advance(); assert.equal(f.map.viewport.style.cursor, 'pointer');
    f.api.setHoverFeedbackMode('Outline');
    assert.equal(f.map.viewport.style.cursor, 'default');
    f.move(); await f.advance(); assert.equal(f.outline(), outline);
    f.move(); f.api.setHoverFeedbackMode('None');
    assert.equal(outline.getSource().features.length, 0);
    assert.equal(f.timers.size, 0);
    const queries = f.layer.queries.length;
    f.move(); f.move([1, 2], true);
    f.map.listeners.get('pointerup')({ pixel: [1, 2], pointerType: 'mouse' }); await f.advance();
    assert.equal(f.layer.queries.length, queries);
    assert.equal(f.map.viewport.style.cursor, 'default');
    assert.equal(f.calls.length, 0);
});

for (const [name, clear] of [
    ['pointer leaves the map', f => f.map.listeners.get('pointerleave')()],
    ['dragging starts', f => f.move([2, 3], true)],
    ['pan or zoom starts', f => f.map.handlers.get('movestart')()],
    ['pan or zoom ends', f => f.map.handlers.get('moveend')()],
    ['the result is removed', f => f.api.removeLayer('locations')],
    ['the map is destroyed', f => f.api.destroyMap()]
]) {
    test(`the visible outline is removed when ${name}`, async () => {
        const f = load('Outline'); f.layer.hits = [feature()];
        f.move(); await f.advance(); const outline = f.outline();
        assert.equal(outline.getSource().features.length, 1);
        clear(f);
        assert.equal(outline.getSource().features.length, 0);
        assert.notEqual(f.map.viewport.style.cursor, 'pointer');
        if (name === 'the map is destroyed') assert.equal(f.outline(), undefined);
    });
}

for (const mode of ['Cursor', 'Outline', 'None']) {
    test(`a late Outline result is ignored after switching to ${mode}`, async () => {
        const f = load('Outline'), pending = deferred(); f.layer.detect = () => pending.promise;
        f.move(); await f.advance(0);
        f.api.setHoverFeedbackMode('None'); f.api.setHoverFeedbackMode(mode);
        pending.resolve([feature()]); await settle(); await f.advance();
        assert.equal(f.outline(), undefined);
        assert.equal(f.map.viewport.style.cursor, 'default');
    });
}

test('a rejected query recovers and a late rejection does not overwrite the dragging cursor', async () => {
    const f = load(), pending = deferred(); f.layer.detect = () => pending.promise;
    f.move(); await f.advance(0); f.move([3, 4], true);
    pending.reject(new Error('Renderer unavailable')); await settle();
    assert.equal(f.map.viewport.style.cursor, 'default');
    f.layer.detect = () => Promise.reject(new Error('Renderer unavailable'));
    f.map.listeners.get('pointerup')({ pixel: [3, 4], pointerType: 'mouse' }); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'default');
    f.layer.detect = null; f.layer.hits = [feature()]; f.move(); await f.advance();
    assert.equal(f.map.viewport.style.cursor, 'pointer');
});

for (const mode of ['Cursor', 'Outline', 'None']) for (const enabled of [false, true]) {
    test(`tooltip ${enabled ? 'on' : 'off'} with ${mode} shares the existing hover query and makes no API calls`, async () => {
        const f = load(mode); f.layer.hits = [namedPoint()];
        if (enabled) f.api.setMapTooltipEnabled(true);
        f.move([100, 100]); await f.advance(250);
        assert.equal(f.tooltip.hidden, !enabled);
        assert.equal(f.layer.queries.length, mode === 'None' && !enabled ? 0 : 1);
        assert.equal(f.map.exactQueries, 0);
        assert.equal(f.calls.length, 0);
        assert.equal(f.map.view.revision, 0, 'tooltip must not move the map');
        if (enabled) assert.equal(f.tooltip.textContent, 'Årike\n1\u00a0234 observationer\nNoggrannhetsradie: 50 m');
        assert.equal(Boolean(f.outline()), mode === 'Outline');
    });
}

test('tooltip delay is 200 ms and movements within the same object do not restart it or rewrite its text', async () => {
    const f = load('None'); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint('Samma lokal')];
    f.move([100, 100]); await f.advance(199);
    assert.equal(f.tooltip.hidden, true);
    f.move([105, 110]); await f.advance(1);
    assert.equal(f.tooltip.hidden, false);
    assert.equal(f.tooltip.style.transform, 'translate(117px, 122px)');
    assert.equal(f.tooltip.textWrites, 1);
    assert.equal(f.tooltip.measurements, 2);
    for (let i = 0; i < 100; i++) { f.move([105 + i, 110]); await f.advance(1000 / 120); }
    await f.advance(34);
    assert.equal(f.tooltip.textWrites, 1);
    assert.equal(f.tooltip.measurements, 2, 'dimensions are not measured during pointer movement');
    assert.ok(f.layer.queries.length < 30, 'tooltip must respect the common 30 Hz limit');
    f.layer.hits = [namedPoint('Ny lokal')]; f.move(); await f.advance(34);
    assert.equal(f.tooltip.hidden, true);
    await f.advance(200);
    assert.equal(f.tooltip.hidden, false);
    assert.ok(f.tooltip.textContent.startsWith('Ny lokal\n'));
});

test('tooltip waits for a pending newer hit instead of displaying the previous object', async () => {
    const f = load('None'), pending = deferred();
    f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint('Tidigare lokal')];
    f.move(); await f.advance(190);
    f.layer.detect = () => pending.promise; f.move([99, 99]); await f.advance(10);
    assert.equal(f.tooltip.hidden, true);
    pending.resolve([namedPoint('Senaste lokal')]); await settle();
    await f.advance(199); assert.equal(f.tooltip.hidden, true);
    await f.advance(1); assert.equal(f.tooltip.hidden, false);
    assert.ok(f.tooltip.textContent.startsWith('Senaste lokal\n'));
});

for (const type of ['meterCluster', 'pixelCluster', 'coLocatedLocations']) {
    test(`${type} tooltip uses all location counts and shows the appropriate hint without shared accuracy`, () => {
        const f = load();
        const content = f.api.createMapTooltipContent(feature(false, 'Point', { groupingType: type, observationCount: 12345,
            locations: { count: 150, ids: ['one'], items: ['Ön', 'Berg', 'Berg', 'Äng', 'Ås'].map(locality => ({ locality, coordinateUncertaintyInMeters: 10000 })) } }));
        const { text, hint } = content;
        assert.ok(text.startsWith(type === 'coLocatedLocations' ? '150 fyndplatser på samma plats\n' : 'Kluster – 150 fyndplatser\n'));
        assert.ok(text.includes('12\u00a0345 observationer'));
        if (type === 'coLocatedLocations') {
            assert.ok(text.includes('Exempel: Berg, Ås, Äng'));
            assert.equal(hint, null);
        } else {
            assert.equal(text, 'Kluster – 150 fyndplatser\n12\u00a0345 observationer');
            assert.equal(hint, 'Zooma in för att välja en fyndplats');
        }
        assert.ok(!text.includes('Noggrannhet'));
    });
}

for (const groupingType of ['meterCluster', 'pixelCluster']) {
    test(`${groupingType} renders its zoom hint in italics and removes it for a single finding site`, async () => {
        const f = load('None');
        f.api.setMapTooltipEnabled(true);
        f.layer.hits = [feature(false, 'Point', { groupingType, observationCount: 2,
            locations: { count: 2, items: [{ locality: '<b>Exempelnamn</b>' }] } })];
        f.move(); await f.advance(200);
        assert.equal(f.tooltip.textContent, 'Kluster – 2 fyndplatser\n2 observationer\nZooma in för att välja en fyndplats');
        assert.equal(f.tooltip.children.length, 1);
        assert.equal(f.tooltip.children[0].tagName, 'EM');
        assert.equal(f.tooltip.children[0].textContent, 'Zooma in för att välja en fyndplats');
        f.layer.hits = [namedPoint('<em>Fyndplatsnamn</em>')];
        f.move(); await f.advance(234);
        assert.equal(f.tooltip.children.length, 0);
        assert.ok(f.tooltip.textContent.startsWith('<em>Fyndplatsnamn</em>\n'));
    });
}

test('tooltip handles missing names, PascalCase data, full ids, unknown values and zero observation counts', () => {
    const f = load();
    assert.equal(f.api.createMapTooltipContent(feature(false, 'Point')).text, 'Fyndplats');
    assert.equal(f.api.createMapTooltipContent(namedPoint(' ', 0, 0)).text, 'Site-A\n0 observationer');
    const fallback = feature(false, 'Polygon', { observationCount: -1, locations: { ids: ['Wire-ID'], items: [{ locality: '' }] } });
    assert.equal(f.api.createMapTooltipContent(fallback).text, 'Wire-ID');
    assert.equal(f.api.createMapTooltipContent(feature(false, 'Point', { GroupingType: 'singleLocation', ObservationCount: 1,
        Locations: { Count: 1, Items: [{ Locality: 'Namn', CoordinateUncertaintyInMeters: 3 }] } })).text, 'Namn\n1 observation\nNoggrannhetsradie: 3 m');
    const coincident = feature(false, 'Point', { groupingType: 'coLocatedLocations', locations: { Ids: ['A', 'a', 'B'], Items: [{ Locality: 'Gemensamt namn' }] } });
    assert.ok(f.api.createMapTooltipContent(coincident).text.startsWith('2 fyndplatser på samma plats\n'));
    const singleMemberCluster = feature(false, 'Point', { groupingType: 'pixelCluster', locations: { count: 1 } });
    assert.equal(f.api.createMapTooltipContent(singleMemberCluster).text, 'Kluster – 1 fyndplats');
});

for (const radius of [null, undefined, 0, -1, NaN, Infinity, true, '', 'invalid']) {
    test(`tooltip omits invalid accuracy ${String(radius)} and does not invent its visual minimum`, () => {
        const f = load();
        const point = feature(false, 'Point', { groupingType: 'singleLocation', observationCount: 1,
            locations: { count: 1, items: [{ locality: 'Namn', coordinateUncertaintyInMeters: radius }] } });
        assert.equal(f.api.createMapTooltipContent(point).text, 'Namn\n1 observation');
    });
}

test('polygon and grid tooltips omit point accuracy and grid shows its zoom action', () => {
    const f = load();
    const properties = { groupingType: 'singleLocation', observationCount: 1, locations: { count: 1, items: [{ locality: 'Polygon', coordinateUncertaintyInMeters: 100 }] } };
    assert.equal(f.api.createMapTooltipContent(feature(false, 'Polygon', properties)).text, 'Polygon\n1 observation');
    f.layer.set('sosResultType', 'geogrid');
    assert.equal(f.api.createMapTooltipContent(feature(false, 'Polygon', { ObservationsCount: 0 })).text, 'Gridruta\n0 observationer\nKlicka för att zooma in');
    assert.equal(f.api.createMapTooltipContent(feature(false, 'Polygon')).text, 'Gridruta\nKlicka för att zooma in');
});

test('tooltip renders HTML-like names literally and preserves long names as text', async () => {
    const f = load('None'); f.api.setMapTooltipEnabled(true);
    const name = '<img src=x onerror=alert(1)> ' + 'Långt namn '.repeat(50);
    f.layer.hits = [namedPoint(name)]; f.move(); await f.advance(200);
    assert.equal(f.tooltip.hidden, false);
    assert.ok(f.tooltip.textContent.startsWith(name.trim()));
    assert.equal(f.tooltip.textWrites, 1);
});

test('selection styling does not cancel a pending tooltip for the same location data', async () => {
    const f = load('Outline'); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint()];
    f.move(); await f.advance(100);
    f.api.setHighlightedLocationIds([], ['point']);
    await f.advance(100);
    assert.equal(f.tooltip.hidden, false);
    assert.equal(f.tooltip.textContent, 'Årike\n1\u00a0234 observationer\nNoggrannhetsradie: 50 m');
    assert.equal(f.layer.queries.length, 1);
});

for (const pixel of [[0, 0], [799, 0], [799, 599], [0, 599], [400, 300]]) {
    test(`tooltip stays inside the map with eight-pixel margins at ${pixel}`, async () => {
        const f = load('None'); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint()];
        f.move(pixel); await f.advance(200);
        const [x, y] = f.tooltip.style.transform.match(/-?\d+/g).map(Number);
        assert.ok(x >= 8 && y >= 8);
        assert.ok(x + f.tooltip.width <= 792 && y + f.tooltip.height <= 592);
    });
}

test('tooltip respects the available width in a small map', async () => {
    const f = load('None'); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint()];
    f.map.size = [100, 90]; f.move([95, 85]); await f.advance(200);
    const [x, y] = f.tooltip.style.transform.match(/-?\d+/g).map(Number);
    assert.ok(x >= 8 && y >= 8);
    assert.ok(x + f.tooltip.offsetWidth <= 92 && y + f.tooltip.offsetHeight <= 82);
});

for (const delay of [0, 250]) for (const [name, invalidate] of [
    ['empty hit', async f => { f.layer.hits = []; f.move(); await f.advance(34); }],
    ['pointer leave', f => f.map.listeners.get('pointerleave')()],
    ['pointer cancel', f => f.map.listeners.get('pointercancel')()],
    ['map control', f => f.map.listeners.get('pointerover')({ target: { closest: () => true } })],
    ['drag', f => f.move([2, 3], true)],
    ['touch', f => f.move([2, 3], false, 'touch')],
    ['zoom or pan', f => f.map.handlers.get('movestart')()],
    ['new result', f => f.api.removeLayer('locations')],
    ['disabled tooltip', f => f.api.setMapTooltipEnabled(false)],
    ['map destruction', f => f.api.destroyMap()]
]) {
    test(`${delay ? 'visible' : 'pending'} tooltip is cleared by ${name}`, async () => {
        const f = load('None'); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint()];
        f.move(); await f.advance(delay);
        await invalidate(f); await f.advance(500);
        assert.equal(f.tooltip.hidden, true);
    });
}

test('a late hit cannot resurrect disabled tooltips and reinitializing a map resets the option', async () => {
    const f = load('None'), pending = deferred();
    f.api.setMapTooltipEnabled(true); f.layer.detect = () => pending.promise; f.move(); await f.advance(0);
    f.api.setMapTooltipEnabled(false); f.api.setMapTooltipEnabled(true);
    pending.resolve([namedPoint()]); await settle(); await f.advance(500);
    assert.equal(f.tooltip.hidden, true);
    const next = f.init(); next.layer.hits = [namedPoint()];
    next.map.handlers.get('pointermove')({ pixel: [100, 100] }); await f.advance(500);
    assert.equal(f.tooltip.hidden, true);
    assert.equal(next.layer.queries.length, 0);
});

test('legacy initialization without a tooltip element remains supported', async () => {
    const f = load('None', false); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint()];
    f.move(); await f.advance(500);
    assert.equal(f.layer.queries.length, 0);
});

test('tooltip activation persists across hover feedback mode changes', async () => {
    const f = load('None'); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint()];
    for (const mode of ['None', 'Cursor', 'Outline']) {
        f.api.setHoverFeedbackMode(mode); f.move(); await f.advance(250);
        assert.equal(f.tooltip.hidden, false);
    }
    assert.equal(f.layer.queries.length, 3);
});

test('replacing the source during the display delay does not reveal an old tooltip', async () => {
    const f = load('None'); f.api.setMapTooltipEnabled(true); f.layer.hits = [namedPoint()];
    f.move(); await f.advance(100); f.layer.setSource({}); await f.advance(100);
    assert.equal(f.tooltip.hidden, true);
});

test('a late old-map hit cannot overwrite a new map tooltip', async () => {
    const f = load('None'), pending = deferred(); f.api.setMapTooltipEnabled(true);
    f.layer.detect = () => pending.promise; f.move(); await f.advance(0);
    const next = f.init(); f.api.setMapTooltipEnabled(true); next.layer.hits = [namedPoint('Nya kartan')];
    next.map.handlers.get('pointermove')({ pixel: [100, 100] }); await f.advance(200);
    pending.resolve([namedPoint('Gamla kartan')]); await settle(); await f.advance(200);
    assert.equal(f.tooltip.hidden, false);
    assert.ok(f.tooltip.textContent.startsWith('Nya kartan\n'));
});
