import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';

function feature(id, ids = [id], groupingType = 'singleLocation', geometryType = 'Point') {
    const properties = { locations: { ids }, groupingType };
    return {
        getId: () => id,
        get: key => properties[key],
        set: (key, value) => { properties[key] = value; },
        getGeometry: () => ({ getType: () => geometryType }),
        setStyle(value) { this.style = value; },
        changed() {}
    };
}

function load() {
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/Pages/LocationsSearch.razor.js', import.meta.url), 'utf8')
        .replace(/^\s*import .*;$/m, '').replace(/export /g, '');
    const style = class { constructor(options) { Object.assign(this, options); } };
    const fixture = { calls: [], hits: [], features: [], reads: 0, renders: 0, rejected: [] };
    let features, vectorSource;
    // A new result replaces the source, as renderMapResult does in the real map.
    Object.defineProperty(fixture, 'features', {
        get: () => features,
        set(value) {
            features = value;
            vectorSource = { getFeatures: () => { fixture.reads++; return features; } };
        }
    });
    fixture.features = [];
    const layer = {
        get: key => key === 'id' ? 'locations' : undefined,
        getSource: () => vectorSource,
        changed() {}
    };
    const map = {
        getAllLayers: () => [layer],
        getSize: () => { throw new Error('Immediate click feedback must not pan/zoom the map'); },
        forEachFeatureAtPixel: (_pixel, visit) => fixture.hits.forEach(hit => visit(hit, layer)),
        render: () => fixture.renders++
    };
    const dotNet = { invokeMethodAsync(name, ...args) {
        fixture.calls.push({ name, args });
        return new Promise((_resolve, reject) => fixture.rejected.push(reject));
    } };
    const context = vm.createContext({ console, ol: { style: { Style: style, Circle: style, Fill: style, Stroke: style } } });
    vm.runInContext(source + '\nglobalThis.api = { onClickFeedback, onSingleClick, onDoubleClick, setHighlightedLocationIds, attach(value, ref) { map = value; dotNetRef = ref; } };', context);
    fixture.api = context.api;
    fixture.api.attach(map, dotNet);
    fixture.click = hits => { fixture.hits = hits; fixture.api.onClickFeedback({ pixel: [0, 0] }); };
    fixture.commit = () => fixture.api.onSingleClick({ pixel: [0, 0] });
    fixture.confirm = (features, version) => fixture.api.setHighlightedLocationIds(
        features.flatMap(f => f.get('locations').ids), features.map(f => f.getId()), false,
        { featureCount: features.length, locationIds: features.flatMap(f => f.get('locations').ids), featureIds: features.map(f => f.getId()) }, version);
    fixture.selected = f => (f.style?.image?.stroke ?? f.style?.stroke)?.color === '#d500f9';
    return fixture;
}

test('feedback paints at click before singleclick or Blazor, without scanning the result', () => {
    const f = load(); const point = feature('point');
    f.features = [point, ...Array.from({ length: 10000 }, (_, i) => feature(`other-${i}`))];
    f.click([point]);
    assert.equal(f.selected(point), true);
    assert.equal(f.calls.length, 0, 'no callback is needed to display the marker');
    assert.equal(f.reads, 0, 'only the hit features are styled during immediate feedback');
    f.commit();
    assert.equal(f.selected(point), true);
    assert.equal(f.calls.length, 1);
    assert.equal(f.calls[0].name, 'OnFeaturesClickedFromJs');
    assert.equal(f.calls[0].args[0][0].featureId, 'point');
    assert.equal(f.calls[0].args[1], 1);
    assert.ok(f.renders > 0);
});

test('empty selection and deselection do not scan the result or build a highlight index', () => {
    const f = load(); const point = feature('a');
    f.features = [point, ...Array.from({ length: 10000 }, (_, i) => feature(`other-${i}`))];
    f.confirm([], 0);
    assert.equal(f.reads, 0);
    f.click([point]); f.commit();
    f.confirm([], 1);
    assert.equal(f.selected(point), false, 'deselection still removes immediate click feedback');
    assert.equal(f.reads, 0);
});

test('repeated highlights reuse one index and a replacement source gets a fresh index', () => {
    const f = load(); const a = feature('a'), b = feature('b');
    f.features = [a, b, ...Array.from({ length: 10000 }, (_, i) => feature(`other-${i}`))];
    f.confirm([a], 0);
    assert.equal(f.reads, 1);
    f.confirm([b], 0);
    assert.equal(f.selected(a), false);
    assert.equal(f.selected(b), true);
    f.confirm([], 0);
    f.confirm([a], 0);
    assert.equal(f.reads, 1, 'highlight changes only look up the affected ids');
    const cluster = feature('new-cluster', ['a', 'b'], 'pixelCluster');
    f.features = [cluster];
    f.confirm([a], 0);
    assert.equal(f.reads, 2);
    assert.equal(f.selected(cluster), true, 'stable ids follow the replacement cluster');
    f.features = [];
    f.confirm([a], 0);
    assert.equal(f.reads, 3);
    assert.equal(f.selected(cluster), false, 'removed features are cleared');
    const returning = feature('a');
    f.features = [returning];
    f.confirm([a], 0);
    assert.equal(f.reads, 4);
    assert.equal(f.selected(returning), true);
});

test('indexed highlights include every matching feature and deduplicate overlapping ids', () => {
    const f = load();
    const ids = Array.from({ length: 100 }, (_, i) => `site-${i}`);
    const cluster = feature('Cluster-A', ids, 'pixelCluster');
    const polygon = feature('area', ['site-99'], 'singleLocation', 'Polygon');
    const grid = feature('tile', [], 'grid', 'Polygon');
    f.features = [cluster, polygon, grid];
    let clusterPaints = 0;
    const originalSetStyle = cluster.setStyle;
    cluster.setStyle = style => { if (style) clusterPaints++; originalSetStyle.call(cluster, style); };
    f.api.setHighlightedLocationIds(['TILE'], [], false);
    assert.equal(f.selected(grid), false, 'map ids are separate from observation location ids');
    f.api.setHighlightedLocationIds(['SITE-99'], ['cluster-a', 'TILE'], false);
    assert.equal(f.selected(cluster), true);
    assert.equal(f.selected(polygon), true);
    assert.equal(f.selected(grid), true, 'features without location ids still match their map id');
    assert.equal(clusterPaints, 1, 'a feature matching map and location ids is painted once');
    assert.equal(f.reads, 1);
});

test('rapid select, switch and deselect are visible while all callbacks are still pending', () => {
    const f = load(); const a = feature('a'), b = feature('b'); f.features = [a, b];
    f.click([a]); f.commit();
    f.click([b]); assert.equal(f.selected(a), false); assert.equal(f.selected(b), true); f.commit();
    f.confirm([a], 1);
    assert.equal(f.selected(b), true, 'a stale server confirmation cannot revert the newer click');
    f.click([b]); assert.equal(f.selected(b), false); f.commit();
    f.confirm([b], 2);
    assert.equal(f.selected(b), false);
    f.confirm([], 3);
    assert.deepEqual(f.calls.map(c => c.args[1]), [1, 2, 3]);
});

test('double click restores the prior selection and does not start an observation request', () => {
    const f = load(); const a = feature('a'), b = feature('b'); f.features = [a, b];
    f.confirm([a], 0);
    f.click([b]); assert.equal(f.selected(b), true); assert.equal(f.selected(a), false);
    f.click([b]); f.api.onDoubleClick();
    assert.equal(f.selected(a), true);
    assert.equal(f.selected(b), false);
    assert.equal(f.calls.length, 0);
});

test('a background confirmation does not interrupt click feedback awaiting singleclick', () => {
    const f = load(); const a = feature('a'), b = feature('b'); f.features = [a, b];
    f.confirm([a], 0);
    f.click([b]); f.confirm([a], 0);
    assert.equal(f.selected(b), true);
    assert.equal(f.selected(a), false);
    f.commit();
    assert.equal(f.selected(b), true);
});

test('selection comparison uses stable location ids after reclustering and excludes table highlights', () => {
    const f = load(); const old = feature('old-cluster', ['a', 'b'], 'pixelCluster');
    const same = feature('new-cluster', ['b', 'a'], 'pixelCluster'), larger = feature('old-cluster', ['a', 'b', 'c'], 'pixelCluster');
    f.features = [same, larger]; f.confirm([old], 0);
    f.click([same]); assert.equal(f.selected(same), false); f.commit();
    f.confirm([old], 1);
    f.click([larger]); assert.equal(f.selected(larger), true); f.commit();
    const a = feature('a'); f.features = [a];
    f.api.setHighlightedLocationIds(['a'], [], false, { featureCount: 0, locationIds: [], featureIds: [] }, 2);
    f.click([a]); assert.equal(f.selected(a), true, 'an observation row highlight is not a selected map feature');
});

test('a rejected callback restores the confirmed selection and keeps click versions monotonic', async () => {
    const f = load(); const a = feature('a'), b = feature('b'); f.features = [a, b]; f.confirm([a], 4);
    f.click([b]); f.commit(); assert.equal(f.calls[0].args[1], 5);
    f.rejected[0](new Error('Connection unavailable'));
    await Promise.resolve();
    assert.equal(f.selected(a), true); assert.equal(f.selected(b), false);
    f.click([b]); f.commit(); assert.equal(f.calls[1].args[1], 6);
});

test('overlapping hits, clusters, geogrid and blank-map deselection use the same immediate feedback', () => {
    const f = load(); const point = feature('a'), cluster = feature('cluster', ['b', 'c'], 'meterCluster'), grid = feature('tile', [], 'grid');
    f.features = [point, cluster, grid];
    f.click([point, cluster]); assert.equal(f.selected(point), true); assert.equal(f.selected(cluster), true); f.commit();
    f.click([grid]); assert.equal(f.selected(grid), true); assert.equal(f.selected(point), false); f.commit();
    f.click([]); assert.equal(f.selected(grid), false); f.commit();
    assert.equal(f.calls.at(-1).args[0].length, 0);
});

for (const [groupingType, geometryType] of [
    ['singleLocation', 'Point'], ['singleLocation', 'MultiPoint'],
    ['meterCluster', 'Point'], ['pixelCluster', 'Point']
]) {
    test(`${groupingType}/${geometryType} takes precedence over a polygon in feedback, selection and deselection`, () => {
        for (const polygonFirst of [true, false]) {
            const f = load();
            const point = feature('point', ['point-site'], groupingType, geometryType);
            const polygon = feature('polygon', ['polygon-site'], 'singleLocation', 'Polygon');
            f.features = [point, polygon];
            const hits = polygonFirst ? [polygon, point] : [point, polygon];
            f.click(hits);
            assert.equal(f.selected(point), true);
            assert.equal(f.selected(polygon), false);
            f.commit();
            assert.equal(f.calls[0].args[0].length, 1);
            assert.equal(f.calls[0].args[0][0].featureId, 'point');
            f.confirm([point], 1);
            assert.equal(f.selected(polygon), false);
            f.click(hits);
            assert.equal(f.selected(point), false);
            assert.equal(f.selected(polygon), false);
            f.commit();
            assert.equal(f.calls[1].args[0].length, 1);
            f.confirm([], 2);
            assert.equal(f.selected(polygon), false);
        }
    });
}

test('polygon surfaces remain selectable when no point symbol is hit', () => {
    const f = load();
    const polygon = feature('polygon', ['a'], 'singleLocation', 'Polygon');
    const multiPolygon = feature('multi-polygon', ['b'], 'singleLocation', 'MultiPolygon');
    const point = feature('point', ['c']);
    f.features = [polygon, multiPolygon, point];
    f.click([polygon, multiPolygon]);
    assert.equal(f.selected(polygon), true);
    assert.equal(f.selected(multiPolygon), true);
    assert.equal(f.selected(point), false);
    f.commit();
    assert.equal(f.calls[0].args[0].length, 2);
    f.click([polygon, multiPolygon]);
    assert.equal(f.selected(polygon), false);
    assert.equal(f.selected(multiPolygon), false);
});

test('clicking a point within a selected polygon replaces the polygon selection', () => {
    const f = load();
    const polygon = feature('polygon', ['a'], 'singleLocation', 'Polygon');
    const point = feature('point', ['b']);
    f.features = [polygon, point];
    f.confirm([polygon], 0);
    f.click([polygon, point]);
    assert.equal(f.selected(polygon), false);
    assert.equal(f.selected(point), true);
    f.commit();
    assert.equal(f.calls[0].args[0].length, 1);
    assert.equal(f.calls[0].args[0][0].featureId, 'point');
});

test('noninteractive point overlays do not prevent selecting a polygon', () => {
    const f = load();
    const polygon = feature('polygon', ['a'], 'singleLocation', 'Polygon');
    const overlay = feature('overlay', ['b']);
    overlay.set('sosNonInteractive', true);
    f.features = [polygon, overlay];
    f.click([overlay, polygon]);
    assert.equal(f.selected(polygon), true);
    assert.equal(f.selected(overlay), false);
    f.commit();
    assert.equal(f.calls[0].args[0].length, 1);
    assert.equal(f.calls[0].args[0][0].featureId, 'polygon');
});

test('polygons displayed as point symbols participate alongside other points', () => {
    const f = load();
    const centroid = feature('centroid', ['a']);
    centroid.set('locations', { ids: ['a'], items: [{ type: 'Polygon' }] });
    const point = feature('point', ['b']);
    const surface = feature('surface', ['c'], 'singleLocation', 'Polygon');
    f.features = [centroid, point, surface];
    f.click([surface, centroid, point]);
    assert.equal(f.selected(centroid), true);
    assert.equal(f.selected(point), true);
    assert.equal(f.selected(surface), false);
    f.commit();
    assert.equal(f.calls[0].args[0].length, 2);
});
