var feesRange = document.getElementById("fees");
var feeOutput = document.getElementById("fee-output");

// Add an input event listener to the range input
feesRange.addEventListener("input", function() {
    // Update the value of the output element with the current value of the range input
    feeOutput.textContent = feesRange.value;
});