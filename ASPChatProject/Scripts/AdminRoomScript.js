function Load() {
    document.getElementById("lcs").click();
    document.getElementById("chatButton").click();
}


function OnSuccessAdd(res) {
    $("#addResult").text(res);
    document.getElementById("lcs").click();
}

function OnFailureAdd(res) {
    $("#addResult").text(res.responseText);
}

onload = Load