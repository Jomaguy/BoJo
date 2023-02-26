open = false;

$("#speechBubbleImage").click(function () {
    if (open == false)
    {
        $('.transform').toggleClass('openChatbox');
        document.getElementById("messageBox").style.display = 'block';
        document.getElementById("sendButton").style.display = 'block';
        document.getElementById("closeButton").style.display = 'block';
        document.getElementById("speechBubbleImage").style.display = 'none';
        open = true;
    }
});

$("#closeButton").click(function () {
    if (open == true) {
        $('.transform').toggleClass('openChatbox');
        document.getElementById("messageBox").style.display = 'none';
        document.getElementById("sendButton").style.display = 'none';
        document.getElementById("closeButton").style.display = 'none';
        document.getElementById("speechBubbleImage").style.display = 'block';
        open = false;
    }
});