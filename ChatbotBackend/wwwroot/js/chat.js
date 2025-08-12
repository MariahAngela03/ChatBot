const form = document.getElementById('chat-form');
const input = document.getElementById('message-input');
const messages = document.getElementById('messages');

function appendMessage(text, from = 'bot'){
  const row = document.createElement('div');
  row.className = 'message-row';
  const bubble = document.createElement('div');
  bubble.className = 'message-bubble ' + (from === 'user' ? 'message-user' : 'message-bot');
  bubble.innerText = text;
  row.appendChild(bubble);
  messages.appendChild(row);
  messages.parentElement.scrollTop = messages.parentElement.scrollHeight;
}

async function sendToServer(message){
  try{
    const res = await fetch('http://localhost:5181/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message })
    });
    if(!res.ok) throw new Error('Network response not ok');
    const data = await res.json();
    return data.reply;
  }catch(err){
    console.error(err);
    return 'Error: could not reach server.';
  }
}

form.addEventListener('submit', async (e) =>{
  e.preventDefault();
  const text = input.value.trim();
  if(!text) return;
  appendMessage(text, 'user');
  input.value = '';
  appendMessage('…', 'bot'); // temporary

  const reply = await sendToServer(text);

  // remove the temporary last bot message (the ellipsis)
  const last = messages.querySelectorAll('.message-row');
  if(last.length) last[last.length -1].remove();

  appendMessage(reply, 'bot');
});

// welcome message
appendMessage('Hi! I\'m running locally. Say hi!', 'bot');