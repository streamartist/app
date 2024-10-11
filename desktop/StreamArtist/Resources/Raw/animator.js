let chatQueue = [];
let fireElements = [];

function setup() {
    const fireUber = document.getElementById("fire-uber");
    const parentContainer = fireUber.parentElement;
    const containerWidth = parentContainer.offsetWidth;

    // Create 20 copies of the fire-uber element
    for (let i = 0; i < 10; i++) {
        const clone = fireUber.cloneNode(true);
        clone.id = `fire-uber-${i}`;
        clone.style.display = "none";
        clone.style.position = "absolute";
        clone.style.left = `${(i / 10) * 100}%`;
        clone.style.bottom = "0px";
        parentContainer.appendChild(clone);
        fireElements.push(clone);
    }

    // Hide the original fire-uber element
    fireUber.style.display = "none";

    if (window.location.protocol === 'file:') {
        // addTestChat();
    }
}

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
        const chat = chatQueue[0];
        const screen = document.getElementById("screen");

        // Calculate zoom based on chat.Size
        const zoom = chat.Size;

        // Show the number of fire elements based on chat.Num
        const numToShow = Math.min(chat.Num, fireElements.length);
        
        // Start from the right-most element
        for (let i = fireElements.length - 1; i >= 0; i--) {
            const element = fireElements[i];
            if (i >= fireElements.length - numToShow) {
                element.style.display = "block";
                element.style.transform = `scale(${zoom})`;
                setFirePosition(element, chat);
            } else {
                element.style.display = "none";
            }
        }

        // Display the message
        document.getElementById("message").style.display = "block";
        document.getElementById("message").innerHTML = `${chat.Name} donated ${chat.DisplayAmount}!`;
    }
}

function setFirePosition(FireUber, Chat) {
    // const FireUber = document.getElementById(id);
    const Screen = document.getElementById("screen");

    // Position the fire-uber element
    FireUber.style.position = "absolute";
    FireUber.style.bottom = "0px";
    // FireUber.style.left = "80%";

    // Ensure the screen div has a relative position for proper positioning
    Screen.style.position = "relative";

    

    // Compute how much of fire-container is hidden in the overflow
    const FireContainerRect = FireUber.getBoundingClientRect();//FireContainer.getBoundingClientRect();
    const ParentContainer = Screen;
    const ParentContainerRect = ParentContainer.getBoundingClientRect();


    // Scaling causes some issues with positioning.
    const parentBottom = ParentContainerRect.top + ParentContainerRect.height;
    const fireBottom = FireContainerRect.top + FireContainerRect.height;
    const hiddenHeight = Math.max(0, fireBottom - parentBottom);

    FireUber.style.bottom = `${hiddenHeight}px`;
}

function processChats() {
    const CurrentTime = Date.now();
    while (chatQueue.length > 0 && CurrentTime >= chatQueue[0].ExpirationTime) {
        chatQueue.shift();
        hideElements();
    }
    if (chatQueue.length > 0) {
        displayNextChat();
        chatQueue.shift();
    }
}

function hideElements() {
    document.getElementById("message").style.display = "none";
    
    fireElements.forEach(element => {
        element.style.display = "none";
    });
}

function addTestChat() {
    const TestChat = {
        "Amount": 10,
        "DisplayAmount": "$10",
        "Size": 1.1,
        "Num": 10,
        "TTL": 20,
        "Name": "Joel",
        "Message": null,
        "ExpirationTime": Date.now() + 2000 * 1000
    };
    chatQueue.push(TestChat);
    console.log("Test chat added:", TestChat);
    // displayNextChat();
    processChats();
}

// Set an interval to call fetchData every 5 seconds
setInterval(fetchData, 5000);

// Set an interval to process chats every 1 second
setInterval(processChats, 1000);



// Call setup function when the page loads
window.onload = setup;
