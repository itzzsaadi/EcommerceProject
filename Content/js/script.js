// Function to format description into bullet points
function displayProductDescription(description) {
    let bulletList = "<ul>";
    description.split("\n").forEach(line => {
        if (line.trim() !== "") {
            bulletList += `<li>${line.trim()}</li>`;
        }
    });
    bulletList += "</ul>";

    // Insert into the product description div
    document.getElementById("productDescription").innerHTML = bulletList;
}
