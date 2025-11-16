(function () {
    'use strict';

    const ASSISTANT_ID = '{{ASSISTANT_ID}}';
    const API_BASE_URL = '{{API_BASE_URL}}';

    // Create and inject styles
    const styles = `
        .rag-chat-widget {
            position: fixed !important;
            bottom: 20px !important;
            right: 20px !important;
            width: 350px !important;
            height: 500px !important;
            border-radius: 10px !important;
            background: white !important;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important;
            display: flex !important;
            flex-direction: column !important;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif !important;
            z-index: 10000 !important;
        }
        
        /* Full screen on mobile and tablet */
        @media screen and (max-width: 1024px) {
            .rag-chat-widget {
                top: 0 !important;
                left: 0 !important;
                right: 0 !important;
                bottom: 0 !important;
                width: 100% !important;
                height: 100% !important;
                border-radius: 0 !important;
                max-width: 100% !important;
                max-height: 100% !important;
            }
        }
        
        .rag-chat-header {
            background: #0053a5 !important;
            color: white !important;
            padding: 15px !important;
            border-radius: 10px 10px 0 0 !important;
            font-weight: 600 !important;
            display: flex !important;
            justify-content: space-between !important;
            align-items: center !important;
        }
        
        @media screen and (max-width: 1024px) {
            .rag-chat-header {
                border-radius: 0 !important;
            }
        }
        
        .rag-chat-close {
            background: none !important;
            border: none !important;
            color: white !important;
            font-size: 20px !important;
            cursor: pointer !important;
            padding: 0 !important;
            width: 24px !important;
            height: 24px !important;
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
        }
        
        .rag-chat-messages {
            flex: 1 !important;
            overflow-y: auto !important;
            padding: 15px !important;
            display: flex !important;
            flex-direction: column !important;
            gap: 10px !important;
        }
        
        .rag-chat-message {
            max-width: 80% !important;
            padding: 10px 14px !important;
            border-radius: 18px !important;
            word-wrap: break-word !important;
            line-height: 1.4 !important;
            font-size: 14px !important;
            white-space: pre-wrap !important;
        }
        
        .rag-chat-message.user {
            background: #0071df !important;
            color: white !important;
            align-self: flex-end !important;
            border-bottom-right-radius: 4px !important;
        }
        
        .rag-chat-message.assistant {
            background: #f1f3f4 !important;
            color: #333 !important;
            align-self: flex-start !important;
            border-bottom-left-radius: 4px !important;
        }
        
        .rag-chat-message.error {
            background: #fee !important;
            color: #c33 !important;
            align-self: flex-start !important;
            border-bottom-left-radius: 4px !important;
        }
        
        .rag-chat-input-container {
            padding: 15px !important;
            border-top: 1px solid #eee !important;
            display: flex !important;
            gap: 10px !important;
        }
        
        .rag-chat-input {
            flex: 1 !important;
            padding: 10px !important;
            border: 1px solid #ddd !important;
            border-radius: 20px !important;
            outline: none !important;
            font-size: 14px !important;
            font-family: inherit !important;
            color: black !important;
        }
        
        .rag-chat-input:focus {
            border-color: #0071df !important;
        }
        
        .rag-chat-send {
            background: #0071df !important;
            color: white !important;
            border: none !important;
            border-radius: 20px !important;
            padding: 10px 20px !important;
            cursor: pointer !important;
            font-weight: 600 !important;
            font-size: 14px !important;
            transition: background 0.2s !important;
        }
        
        .rag-chat-send:hover:not(:disabled) {
            background: #0053a5 !important;
        }
        
        .rag-chat-send:disabled {
            background: #ccc !important;
            cursor: not-allowed !important;
        }
        
        .rag-chat-toggle {
            position: fixed !important;
            bottom: 25px !important;
            right: 25px !important;
            width: 50px !important;
            height: 50px !important;
            border-radius: 50% !important;
            background: #0071df !important;
            color: white !important;
            border: none !important;
            cursor: pointer !important;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important;
            font-size: 24px !important;
            display: flex;
            align-items: center !important;
            justify-content: center !important;
            z-index: 10000 !important;
        }

        .rag-chat-toggle:hover {
            opacity: 0.7 !important;
        }
        
        .rag-chat-loading {
            display: inline-block !important;
            width: 8px !important;
            height: 8px !important;
            border-radius: 50% !important;
            background: #0071df !important;
            animation: rag-pulse 1.4s infinite ease-in-out both !important;
        }
        
        .rag-chat-loading:nth-child(1) {
            animation-delay: -0.32s !important;
        }
        
        .rag-chat-loading:nth-child(2) {
            animation-delay: -0.16s !important;
        }
        
        @keyframes rag-pulse {
            0%, 80%, 100% {
                opacity: 0.3 !important;
            }
            40% {
                opacity: 1 !important;
            }
        }
        
        .rag-chat-widget.hidden {
            display: none !important;
        }
    `;

    const styleSheet = document.createElement('style');
    styleSheet.textContent = styles;
    document.head.appendChild(styleSheet);

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
        container.innerHTML = widgetHTML;
        document.body.appendChild(container);

        const widget = document.getElementById('ragChatWidget');
        const toggle = document.getElementById('ragChatToggle');
        const closeBtn = document.getElementById('ragChatClose');
        const input = document.getElementById('ragChatInput');
        const sendBtn = document.getElementById('ragChatSend');
        const messagesContainer = document.getElementById('ragChatMessages');

        let isOpen = false;

        function toggleChat() {
            isOpen = !isOpen;
            widget.classList.toggle('hidden', !isOpen);
            toggle.style.display = isOpen ? 'none' : 'flex';
            if (isOpen) {
                input.focus();
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
