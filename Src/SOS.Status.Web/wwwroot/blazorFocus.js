// wwwroot/blazorFocus.js
window.blazorFocus = {
    set: (element) => {
        console.log('blazorFocus.set har anropats:', element);
        element.focus();
    }
};
//window.blazorFocus = {    
//    set: (element) => { element.focus(); }
//};
