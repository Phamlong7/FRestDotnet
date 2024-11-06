function toggleChatbox() {
    const chatbox = $('#chatbox');
    chatbox.is(':visible') ? chatbox.hide() : chatbox.show();
}

function addMessageToChatbox(type, message) {
    const messageDiv = `<div class="message ${type}">${type === 'user' ? 'User' : 'Assistant'}: ${message}</div>`;
    $('#chatbox-body').append(messageDiv);
    $('#chatbox-body').scrollTop($('#chatbox-body')[0].scrollHeight);

    // Store the message in localStorage
    let chatHistory = JSON.parse(localStorage.getItem('chatHistory')) || [];
    chatHistory.push({ type, message });
    localStorage.setItem('chatHistory', JSON.stringify(chatHistory));
}

function loadChatHistory() {
    const chatHistory = JSON.parse(localStorage.getItem('chatHistory')) || [];
    chatHistory.forEach(item => {
        const messageDiv = `<div class="message ${item.type}">${item.type === 'user' ? 'User' : 'Assistant'}: ${item.message}</div>`;
        $('#chatbox-body').append(messageDiv);
    });
    $('#chatbox-body').scrollTop($('#chatbox-body')[0].scrollHeight);
}

async function sendMessage() {
    const message = $('#chat-input').val();
    if (!message) return;

    // Add user's message to chatbox and localStorage
    addMessageToChatbox('user', message);
    $('#chat-input').val('');

    try {
        const response = await fetch('/api/chat/stream', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Message: message }),
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');
        let assistantResponse = '';

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            const chunk = decoder.decode(value, { stream: true });
            const lines = chunk.split('\n');

            for (const line of lines) {
                if (line.startsWith('data:')) {
                    const content = line.slice(6).trim();
                    if (content === '[DONE]') continue;
                    assistantResponse += content + ' ';
                }
            }
        }

        // Add assistant's response to chatbox and localStorage
        addMessageToChatbox('assistant', assistantResponse.trim());
    } catch (error) {
        console.error('Error:', error);
        addMessageToChatbox('error', `Error: ${error.message}`);
    }
}

$(document).ready(function () {
    loadChatHistory(); // Load chat history on page load

    $('#send-btn').click(sendMessage);

    $('#chat-input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            sendMessage();
        }
    });
});
