export function set(element) {
    console.log('set har anropats:', element);
    element.focus();
}

export function showMessage(message) {
    console.log('showMessage har anropats:', message);
    alert(message);
}

export function pauseCircuit(thing) {
    Blazor.pauseCircuit();
};

export function resumeCircuit(thing) {
    Blazor.resumeCircuit();
};