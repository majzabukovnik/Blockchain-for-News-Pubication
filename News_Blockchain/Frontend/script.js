/*var feesRange = document.getElementById("fees");
var feeOutput = document.getElementById("fee-output");

feesRange.addEventListener("input", function() {   
    feeOutput.textContent = feesRange.value;
});
*/
var feesRange = document.getElementById("fees");
var feeOutput = document.getElementById("fee-output");

feesRange.addEventListener("input", function() {   
    feeOutput.textContent = feesRange.value + " satoshi/byte";
});