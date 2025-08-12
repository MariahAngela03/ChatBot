
class Chatbot {
    constructor() {
        this.apiEndpoint = 'https://localhost:7xxx/api/chat'; // Your C# API endpoint
        this.messageInput = document.getElementById('messageInput');
        this.sendButton = document.getElementById('sendButton');
        this.chatMessages = document.getElementById('chatMessages');
        this.typingIndicator = document.getElementById('typingIndicator');
        this.charCount = document.getElementById('charCount');
        
        this.initializeEventListeners();
    }
    
    initializeEventListeners() {
        // Send button click
        this.sendButton.addEventListener('click', () => this.sendMessage());
        
        // Enter key press
        this.messageInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        });
        
        // Character counter
        this.messageInput.addEventListener('input', () => {
            this.charCount.textContent = this.messageInput.value.length;
        });
        
        // Auto-focus input
        this.messageInput.focus();
    }
    
    async sendMessage() {
        const message = this.messageInput.value.trim();
        if (!message) return;
        
        // Clear input and disable button
        this.messageInput.value = '';
        this.charCount.textContent = '0';
        this.toggleSendButton(false);
        
        // Remove welcome message if it exists
        const welcomeMsg = document.querySelector('.welcome-message');
        if (welcomeMsg) {
            welcomeMsg.remove();
        }
        
        // Add user message
        this.addMessage(message, 'user');
        
        // Show typing indicator
        this.showTypingIndicator();
        
        try {
            // Send to backend API
            const response = await this.callAPI(message);
            
            // Hide typing indicator
            this.hideTypingIndicator();
            
            // Add bot response
            this.addMessage(response, 'bot');
        } catch (error) {
            console.error('Error:', error);
            this.hideTypingIndicator();
            this.addMessage('Sorry, I encountered an error. Please try again.', 'bot');
        }
        
        // Re-enable send button and focus input
        this.toggleSendButton(true);
        this.messageInput.focus();
    }
    
    async callAPI(message) {
        try {
            const response = await fetch(this.apiEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message: message })
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            return data.response || 'No response received.';
        } catch (error) {
            // Fallback for demo purposes - replace with actual API call
            console.warn('API not available, using demo responses');
            return this.getDemoResponse(message);
        }
    }
    
    getDemoResponse(message) {
        // Demo responses for testing without backend
        const responses = [
            "That's an interesting question! I'd be happy to help you with that.",
            "I understand what you're asking about. Let me provide some insight on that topic.",
            "Thanks for sharing that with me. Here's my thoughts on what you mentioned.",
            "That's a great point! I can definitely assist you with that kind of request.",
            "I appreciate you asking about that. Let me give you a helpful response."
        ];
        
        // Simple keyword-based responses
        const lowerMessage = message.toLowerCase();
        if (lowerMessage.includes('joke')) {
            return "Why don't programmers like nature? It has too many bugs! 😄";
        } else if (lowerMessage.includes('help') || lowerMessage.includes('can you')) {
            return "I can help you with various tasks like answering questions, providing information, having conversations, and more. What specifically would you like assistance with?";
        } else if (lowerMessage.includes('interesting')) {
            return "Here's something fascinating: Octopuses have three hearts and blue blood! Two hearts pump blood to the gills, while the third pumps blood to the rest of the body.";
        }
        
        return responses[Math.floor(Math.random() * responses.length)];
    }
    
    addMessage(text, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}`;
        
        const timestamp = new Date().toLocaleTimeString([], { 
            hour: '2-digit', 
            minute: '2-digit' 
        });
        
        messageDiv.innerHTML = `
            <div class="message-bubble">
                ${this.formatMessage(text)}
                <small class="timestamp">${timestamp}</small>
            </div>
        `;
        
        this.chatMessages.appendChild(messageDiv);
        this.scrollToBottom();
    }
    
    formatMessage(text) {
        // Simple formatting - you can extend this
        return text.replace(/\n/g, '<br>');
    }
    
    showTypingIndicator() {
        this.typingIndicator.style.display = 'block';
        this.scrollToBottom();
    }
    
    hideTypingIndicator() {
        this.typingIndicator.style.display = 'none';
    }
    
    toggleSendButton(enabled) {
        this.sendButton.disabled = !enabled;
        this.sendButton.innerHTML = enabled ? 
            '<i class="fas fa-paper-plane"></i>' : 
            '<i class="fas fa-spinner fa-spin"></i>';
    }
    
    scrollToBottom() {
        setTimeout(() => {
            this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
        }, 100);
    }
}

// Quick message function for demo buttons
function sendQuickMessage(message) {
    const messageInput = document.getElementById('messageInput');
    messageInput.value = message;
    chatbot.sendMessage();
}

// Initialize chatbot when page loads
let chatbot;
document.addEventListener('DOMContentLoaded', () => {
    chatbot = new Chatbot();
});