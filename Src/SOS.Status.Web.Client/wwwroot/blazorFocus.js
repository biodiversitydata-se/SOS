// wwwroot/blazorFocus.js
window.blazorFocus = {
    set: (element) => {
        console.log('blazorFocus.set har anropats:', element);
        element.focus();
    },    
    showMessage: (message) => {
        console.log('blazorFocus.showMessage har anropats:', message);
        alert(message); // Visar ett meddelande i webbläsaren
    }
};

//window.blazorFocus = {    
//    set: (element) => { element.focus(); }
//};
