(function () {
    'use strict';

    const ASSISTANT_ID = '{{ASSISTANT_ID}}';
    const API_BASE_URL = '{{API_BASE_URL}}';

    // Create chat widget HTML
    const widgetHTML = `
        <div class="rag-chat-widget hidden" id="ragChatWidget">
            <div class="rag-chat-header">
                <span>Chat Assistant</span>
                <button class="rag-chat-close" id="ragChatClose">×</button>
            </div>
            <div class="rag-chat-messages" id="ragChatMessages">
                <div class="rag-chat-message assistant">Hello! How can I help you today?</div>
            </div>
            <div class="rag-chat-input-container">
                <input 
                    type="text" 
                    class="rag-chat-input" 
                    id="ragChatInput" 
                    placeholder="Type your message..."
                    autocomplete="off"
                />
                <button class="rag-chat-send" id="ragChatSend">Send</button>
            </div>
        </div>
        <div class="rag-chat-toggle" id="ragChatToggle">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="#ffffff" viewBox="0 0 256 256"><path d="M128,24A104,104,0,0,0,36.18,176.88L24.83,210.93a16,16,0,0,0,20.24,20.24l34.05-11.35A104,104,0,1,0,128,24Zm0,192a87.87,87.87,0,0,1-44.06-11.81,8,8,0,0,0-6.54-.67L40,216,52.47,178.6a8,8,0,0,0-.66-6.54A88,88,0,1,1,128,216Z"></path></svg>
        </div>
    `;

    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initWidget);
    } else {
        initWidget();
    }

    function initWidget() {
        const container = document.createElement('div');
        container.className = 'rag-chat-container';
        container.innerHTML = widgetHTML;
        document.body.appendChild(container);

        const widget = document.getElementById('ragChatWidget');
        const toggle = document.getElementById('ragChatToggle');
        const closeBtn = document.getElementById('ragChatClose');
        const input = document.getElementById('ragChatInput');
        const sendBtn = document.getElementById('ragChatSend');
        const messagesContainer = document.getElementById('ragChatMessages');

        let isOpen = false;

        // Check if device is mobile or tablet
        function isMobileOrTablet() {
            return window.matchMedia('(max-width: 1024px)').matches;
        }

        function toggleChat() {
            isOpen = !isOpen;
            widget.classList.toggle('hidden', !isOpen);
            toggle.style.display = isOpen ? 'none' : 'flex';
            
            // On mobile/tablet, hide/show body content
            if (isMobileOrTablet()) {
                if (isOpen) {
                    document.body.classList.add('rag-chat-open');
                } else {
                    document.body.classList.remove('rag-chat-open');
                }
            }
            
            if (isOpen) {
                //input.focus();
            }
        }

        toggle.addEventListener('click', toggleChat);
        closeBtn.addEventListener('click', toggleChat);

        function addMessage(content, type = 'assistant') {
            const messageDiv = document.createElement('div');
            messageDiv.className = `rag-chat-message ${type}`;
            messageDiv.textContent = content;
            messagesContainer.appendChild(messageDiv);
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }

        function addLoadingIndicator() {
            const loadingDiv = document.createElement('div');
            loadingDiv.className = 'rag-chat-message assistant';
            loadingDiv.id = 'ragChatLoading';
            loadingDiv.innerHTML = '<span class="rag-chat-loading"></span> <span class="rag-chat-loading"></span> <span class="rag-chat-loading"></span>';
            messagesContainer.appendChild(loadingDiv);
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }

        function removeLoadingIndicator() {
            const loading = document.getElementById('ragChatLoading');
            if (loading) {
                loading.remove();
            }
        }

        async function sendMessage() {
            const query = input.value.trim();
            if (!query) return;

            addMessage(query, 'user');
            input.value = '';
            input.disabled = true;
            sendBtn.disabled = true;

            addLoadingIndicator();

            try {
                const response = await fetch(`${API_BASE_URL}/assistants/${ASSISTANT_ID}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(query)
                });

                removeLoadingIndicator();

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const data = await response.json();
                addMessage(data.response || 'No response received', 'assistant');
            } catch (error) {
                removeLoadingIndicator();
                addMessage('Sorry, there was an error processing your request. Please try again.', 'error');
                console.error('Chat error:', error);
            } finally {
                input.disabled = false;
                sendBtn.disabled = false;
                input.focus();
            }
        }

        sendBtn.addEventListener('click', sendMessage);
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    }
})();
