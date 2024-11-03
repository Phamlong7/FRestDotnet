function toggleChatbox() {
    const chatbox = $('#chatbox');
    chatbox.is(':visible') ? chatbox.hide() : chatbox.show();
}

async function sendMessage() {
    const message = $('#chat-input').val();
    if (!message) return;

    $('#chatbox-body').append(`<div class="message user">User: ${message}</div>`);
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
        let assistantResponseDiv = $('<div class="message assistant"></div>');
        $('#chatbox-body').append(assistantResponseDiv);

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            const chunk = decoder.decode(value, { stream: true });
            const lines = chunk.split('\n');

            for (const line of lines) {
                if (line.startsWith('data:')) {
                    const content = line.slice(6).trim();
                    if (content === '[DONE]') continue;
                    assistantResponseDiv.append(content + ' ');
                }
            }

            $('#chatbox-body').scrollTop($('#chatbox-body')[0].scrollHeight);
        }
    } catch (error) {
        console.error('Error:', error);
        $('#chatbox-body').append(`<div class="message error">Error: ${error.message}</div>`);
    }
}

$(document).ready(function () {
    $('#send-btn').click(sendMessage);

    $('#chat-input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            sendMessage();
        }
    });
});
