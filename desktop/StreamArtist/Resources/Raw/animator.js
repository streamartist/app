let chatQueue = [];
let currentChat = null;
let hidingChat = null;
let fireElements = [];



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
                    
                    chatQueue.push(Chat);
                });
                displayNextChat();
            }
        })
        .catch(error => console.error('Error fetching data:', error));
}

function displayNextChat() {
    if (hidingChat != null || currentChat != null) {
        return;
    }
    console.log("display next chat");
    if (chatQueue.length > 0 && currentChat == null && hidingChat == null) {
        const chat = chatQueue[0];
        chat.ExpirationTime = Date.now() + chat.TTL * 1000;
        console.log(`Chat will expire at ${new Date(chat.ExpirationTime).toLocaleString()}`);
        const screen = document.getElementById("screen");

        const video = document.getElementById("video");
        video.style.display = "block";  

        // Display the message
        document.getElementById("message").style.display = "block";
        document.getElementById("message").innerHTML = `${chat.Name} donated ${chat.DisplayAmount}!`;
        
        currentChat = chat; 
        chatQueue.shift(); 
        animateText();
    }
}

function processChats() {
    console.log("process chats");
    if (hidingChat != null) {
        return;
    }
    const currentTime = Date.now();
    if (currentChat != null  && currentTime >= currentChat.ExpirationTime) {
        
        hideElements();
    }
    if (chatQueue.length > 0) {
        console.log("display next chat");
        displayNextChat();
    }
}

function hideElements() {
    if (hidingChat != null) {
        return;
    }
    console.log("hide");
    clearInterval(pLoop);
    hidingChat = currentChat;
    $('.tlt').textillate('out');   
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
pLoop = setInterval(processChats, 1000);


document.addEventListener("DOMContentLoaded", function(event) {
    // addTestChat();


});

function animateText() {
    $('.tlt').textillate({
        // the default selector to use when detecting multiple texts to animate
        // selector: '.texts',
      
        // enable looping
        loop: false,
      
        // sets the minimum display time for each text before it is replaced
        minDisplayTime: 2000,
      
        // sets the initial delay before starting the animation
        // (note that depending on the in effect you may need to manually apply
        // visibility: hidden to the element before running this plugin)
        initialDelay: 0,
      
        // set whether or not to automatically start animating
        autoStart: true,
      
        // custom set of 'in' effects. This effects whether or not the
        // character is shown/hidden before or after an animation
        inEffects: [],
      
        // custom set of 'out' effects
        outEffects: [ 'hinge' ],
      
        // in animation settings
        in: {
            // set the effect name
          effect: 'fadeInLeftBig',
      
          // set the delay factor applied to each consecutive character
          delayScale: 1.5,
      
          // set the delay between each character
          delay: 50,
      
          // set to true to animate all the characters at the same time
          sync: false,
      
          // randomize the character sequence
          // (note that shuffle doesn't make sense with sync = true)
          shuffle: false,
      
          // reverse the character sequence
          // (note that reverse doesn't make sense with sync = true)
          reverse: false,
      
          // callback that executes once the animation has finished
          callback: function () {}
        },
      
        // out animation settings.
        out: {
          effect: 'flash',
          delayScale: 1.5,
          delay: 50,
          sync: false,
          shuffle: false,
          reverse: false,
          callback: function () {
            document.getElementById("message").style.display = "none";
            document.getElementById("video").style.display="none";
            currentChat = null;
            hidingChat = null;
            pLoop = setInterval(processChats, 1000);
          }
        },
      
        // callback that executes once textillate has finished
        callback: function () {},
      
        // set the type of token to animate (available types: 'char' and 'word')
        type: 'char'
      });
}
