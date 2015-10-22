function Load() {
    Refresh(); 
}

function Refresh() {
    document.getElementById("chatButton").click();
    SetUpdateTime();
    setTimeout("Refresh();", 1000);
}

function SetUpdateTime() {
    Scroll();
    var dt = new Date();
    var time = dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();
    $("#updateTime").text("Обновлено в " + time);
    

}
function Scroll() {
    var win = $('#Messages');
    var height = win[0].scrollHeight;
    win.scrollTop(height);
}

function OnSuccessChat(res) {
    document.getElementById("chatButton").click();
    SetUpdateTime();
}

onload = Load