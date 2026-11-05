import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';

function load() {
    const source = fs.readFileSync(new URL('../../../Src/SOS.Status.Web.Client/Pages/LocationsSearch.razor.js', import.meta.url), 'utf8')
        .replace(/^\s*import .*;$/m, '').replace(/export /g, '');
    const frames = new Map(), maps = [], calls = [];
    let nextFrame = 0;
    class Layer {
        properties = {};
        set(key, value) { this.properties[key] = value; }
        get(key) { return this.properties[key]; }
    }
    class MapStub {
        constructor(options) {
            maps.push(this); this.layers = options.layers; this.handlers = new Map(); this.hits = [];
            this.queries = []; this.visits = 0; this.writes = []; this.listeners = new Map();
            let cursor = '';
            const owner = this;
            this.viewport = { style: {
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
        getSize() { return [800, 600]; }
        getView() { return { calculateExtent: () => [0, 0, 10, 10], getZoom: () => 14 }; }
        getEventPixel(event) { return event.pixel; }
        updateSize() {}
        setTarget(value) { this.target = value; }
        forEachFeatureAtPixel(pixel, visit, options) {
            this.queries.push({ pixel, options });
            for (const { feature, layer } of this.hits) {
                if (options.layerFilter && !options.layerFilter(layer)) continue;
                this.visits++;
                const result = visit(feature, layer);
                if (result) return result;
            }
        }
    }
    const context = vm.createContext({
        window: { ol: true }, console,
        requestAnimationFrame: callback => { frames.set(++nextFrame, callback); return nextFrame; },
        cancelAnimationFrame: id => frames.delete(id),
        ol: {
            Map: MapStub, View: class {}, layer: { Tile: Layer }, source: { OSM: class {} },
            proj: { fromLonLat: x => x, toLonLat: x => x },
            Observable: { unByKey: key => key.owner.handlers.delete(key.type) }
        }
    });
    vm.runInContext(source + '\nglobalThis.api = { initMap, destroyMap, setClickableCursorEnabled };', context);
    const flush = () => { const pending = [...frames.values()]; frames.clear(); pending.forEach(callback => callback()); };
    const init = () => {
        context.api.initMap({}, { invokeMethodAsync: (...args) => { calls.push(args); return Promise.resolve(); } }, 14, 15, 59);
        flush(); // Initial sizing/extent notification is independent of hover.
        const map = maps.at(-1), layer = new Layer(); layer.set('id', 'locations'); map.layers.push(layer);
        return { map, layer };
    };
    const { map, layer } = init(); calls.length = 0;
    const hit = (nonInteractive = false, targetLayer = layer) => ({ feature: { get: key => key === 'sosNonInteractive' && nonInteractive }, layer: targetLayer });
    const move = (pixel = [1, 2], dragging = false, pointerType = 'mouse') => map.handlers.get(dragging ? 'pointerdrag' : 'pointermove')({ pixel, dragging, originalEvent: { pointerType } });
    return { api: context.api, map, layer, Layer, calls, frames, flush, init, hit, move };
}

test('background uses grab and every clickable observation geometry uses pointer without API calls', () => {
    const f = load();
    assert.equal(f.map.viewport.style.cursor, 'grab');
    f.move(); f.flush();
    assert.equal(f.map.viewport.style.cursor, 'grab');
    for (const resultType of ['locations', 'geogrid']) {
        f.layer.set('sosResultType', resultType);
        f.map.hits = [f.hit()];
        f.move(); f.flush();
        assert.equal(f.map.viewport.style.cursor, 'pointer');
        assert.equal(f.map.queries.at(-1).options.hitTolerance, 4);
    }
    f.map.hits = []; f.move(); f.flush();
    assert.equal(f.map.viewport.style.cursor, 'grab');
    assert.equal(f.calls.length, 0);
});

test('hover coalesces moves into one query at the latest pixel and only writes cursor changes', () => {
    const f = load(); f.map.hits = [f.hit()];
    for (let i = 0; i < 100; i++) f.move([i, i]);
    assert.equal(f.frames.size, 1);
    assert.equal(f.map.queries.length, 0);
    f.flush();
    assert.equal(f.map.queries.length, 1);
    assert.deepEqual(Array.from(f.map.queries[0].pixel), [99, 99]);
    f.move(); f.flush();
    assert.deepEqual(f.map.writes, ['grab', 'pointer']);
});

test('clickable cursor can be toggled immediately without hit detection or API calls while disabled', () => {
    const f = load(); f.map.hits = [f.hit()];
    for (const resultType of ['locations', 'geogrid']) {
        f.layer.set('sosResultType', resultType);
        f.api.setClickableCursorEnabled(true);
        f.move(); f.flush();
        assert.equal(f.map.viewport.style.cursor, 'pointer');
        f.move();
        f.api.setClickableCursorEnabled(false);
        assert.equal(f.map.viewport.style.cursor, 'grab');
        assert.equal(f.frames.size, 0, 'disabling cancels the pending hit detection');
        const queries = f.map.queries.length;
        f.move(); f.flush();
        f.map.listeners.get('pointerup')({ pixel: [1, 2], pointerType: 'mouse' });
        f.flush();
        assert.equal(f.map.viewport.style.cursor, 'grab');
        assert.equal(f.map.queries.length, queries);
    }
    f.api.setClickableCursorEnabled(true);
    f.move(); f.flush();
    assert.equal(f.map.viewport.style.cursor, 'pointer');
    assert.equal(f.calls.length, 0);
});

test('disabling clickable cursors preserves the panning cursor and new maps restore the default', () => {
    const f = load();
    f.move([1, 2], true);
    f.api.setClickableCursorEnabled(false);
    assert.equal(f.map.viewport.style.cursor, 'grabbing');
    f.move([2, 3], true);
    assert.equal(f.map.viewport.style.cursor, 'grabbing');
    f.map.listeners.get('pointerup')({ pixel: [2, 3], pointerType: 'mouse' });
    f.flush();
    assert.equal(f.map.viewport.style.cursor, 'grab');
    assert.equal(f.map.queries.length, 0);
    const next = f.init();
    next.map.hits = [f.hit(false, next.layer)];
    next.map.handlers.get('pointermove')({ pixel: [1, 2] }); f.flush();
    assert.equal(next.map.viewport.style.cursor, 'pointer');
});

test('helper layers and noninteractive features do not give pointer and detection stops at first clickable hit', () => {
    const f = load(); const outlines = new f.Layer(); outlines.set('id', 'filter-areas');
    const overlay = new f.Layer(); overlay.set('id', 'locations'); overlay.set('sosNonInteractive', true);
    f.map.hits = [f.hit(false, outlines), f.hit(false, overlay), f.hit(true)];
    f.move(); f.flush();
    assert.equal(f.map.viewport.style.cursor, 'grab');
    assert.equal(f.map.visits, 1, 'unrelated/helper layers are excluded before inspecting their features');
    f.map.hits.push(f.hit(), ...Array.from({ length: 1000 }, () => f.hit()));
    f.map.visits = 0;
    f.move(); f.flush();
    assert.equal(f.map.viewport.style.cursor, 'pointer');
    assert.equal(f.map.visits, 2, 'skip the noninteractive feature and stop at the first clickable one');
});

test('dragging shows grabbing, cancels queued hit detection, and release restores the hover cursor', () => {
    const f = load(); f.map.hits = [f.hit()];
    f.move(); f.move([3, 4], true); f.flush();
    assert.equal(f.map.viewport.style.cursor, 'grabbing');
    assert.equal(f.map.queries.length, 0);
    f.move([4, 5], true);
    assert.equal(f.frames.size, 0);
    assert.deepEqual(f.map.writes, ['grab', 'grabbing'], 'continued dragging does not rewrite the cursor');
    f.map.listeners.get('pointerup')({ pixel: [4, 5], pointerType: 'mouse' });
    assert.equal(f.map.viewport.style.cursor, 'grab');
    f.flush();
    assert.equal(f.map.viewport.style.cursor, 'pointer');
    assert.equal(f.map.queries.length, 1);
});

test('leaving or canceling clears pending hover work and touch movement does not query features', () => {
    const f = load(); f.map.hits = [f.hit()];
    for (const name of ['pointerleave', 'pointercancel']) {
        f.move(); f.map.listeners.get(name)(); f.flush();
        assert.equal(f.map.viewport.style.cursor, 'grab');
    }
    f.move([1, 2], false, 'touch'); f.move([1, 2], true, 'touch'); f.flush();
    assert.equal(f.map.queries.length, 0);
});

test('destroy and reinitialization cancel hover work and remove both map and viewport listeners', () => {
    const f = load(); f.move();
    f.api.destroyMap(); f.flush();
    assert.equal(f.map.handlers.size, 0);
    assert.equal(f.map.listeners.size, 0);
    assert.equal(f.map.viewport.style.cursor, '');
    assert.equal(f.map.target, null);
    assert.equal(f.map.queries.length, 0);
    const next = f.init();
    assert.equal(next.map.viewport.style.cursor, 'grab');
    assert.equal(next.map.listeners.size, 3);
});
