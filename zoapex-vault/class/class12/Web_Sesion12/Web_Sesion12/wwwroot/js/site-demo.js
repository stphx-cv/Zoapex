// Script general del sitio. Se enlaza una sola vez desde _Layout.cshtml,
// por lo que sus funciones quedan disponibles en cualquier pagina.

// Da formato de moneda en soles a un numero
function formatearSoles(monto) {
    return "S/ " + Number(monto).toFixed(2);
}

// Convierte una cadena a numero, aceptando que venga vacia
function aNumero(texto) {
    var n = parseFloat(texto);
    return isNaN(n) ? 0 : n;
}
