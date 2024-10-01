let chatQueue = [];

function fetchData() {
        
    // Check if the page is loaded as a local file
    if (window.location.protocol === 'file:') {
        return;
    }
    
    console.log('Fetching data...');
    fetch('/update')
        .then(response => response.json())
        .then(data => {
            console.log(data);
            const Chats = data;
            if (Chats.length > 0) {
                Chats.forEach(Chat => {
                    Chat.ExpirationTime = Date.now() + Chat.TTL * 1000;
                    chatQueue.push(Chat);
                });
                displayNextChat();
            }
        })
        .catch(error => console.error('Error fetching data:', error));
}

function displayNextChat() {
    if (chatQueue.length > 0) {
        const Chat = chatQueue[0];
        const FireUber = document.getElementById("fire-uber");
        const Screen = document.getElementById("screen");

        // Position the fire-uber element at the bottom of the screen
        FireUber.style.position = "absolute";
        FireUber.style.bottom = "0";
        FireUber.style.left = "50%";
        FireUber.style.transform = "translateX(-50%)";

        // Ensure the screen div has a relative position for proper positioning
        Screen.style.position = "relative";

        document.getElementById("animation").style.display = "block";
        document.getElementById("message").style.display = "block";
        document.getElementById("message").innerHTML = `${Chat.Name} donated ${Chat.Amount}!`;
    }
}

function processChats() {
    const CurrentTime = Date.now();
    while (chatQueue.length > 0 && CurrentTime >= chatQueue[0].ExpirationTime) {
        chatQueue.shift();
        hideElements();
    }
    if (chatQueue.length > 0) {
        displayNextChat();
    }
}

function hideElements() {
    document.getElementById("animation").style.display = "none";
    document.getElementById("message").style.display = "none";
}

function addTestChat() {
    const TestChat = {
        "Amount": 10,
        "DisplayAmount": "$10",
        "Size": 2,
        "Num": 2,
        "TTL": 20,
        "Name": "Joel",
        "Message": null,
        "ExpirationTime": Date.now() + 2000 * 1000
    };
    chatQueue.push(TestChat);
    console.log("Test chat added:", TestChat);
    displayNextChat();
}

// Set an interval to call fetchData every 5 seconds
setInterval(fetchData, 5000);

// Set an interval to process chats every 1 second
setInterval(processChats, 1000);