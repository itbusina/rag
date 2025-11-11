(function() {
    'use strict';
    
    const ASSISTANT_ID = '{{ASSISTANT_ID}}';
    const API_BASE_URL = '{{API_BASE_URL}}';
    
    // Create and inject styles
    const styles = `
        .rag-chat-widget {
            position: fixed;
            bottom: 20px;
            right: 20px;
            width: 350px;
            height: 500px;
            border-radius: 10px;
            background: white;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            display: flex;
            flex-direction: column;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            z-index: 10000;
        }
        
        /* Full screen on mobile and tablet */
        @media screen and (max-width: 1024px) {
            .rag-chat-widget {
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                width: 100%;
                height: 100%;
                border-radius: 0;
                max-width: 100%;
                max-height: 100%;
            }
        }
        
        .rag-chat-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px;
            border-radius: 10px 10px 0 0;
            font-weight: 600;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        @media screen and (max-width: 1024px) {
            .rag-chat-header {
                border-radius: 0;
            }
        }
        
        .rag-chat-close {
            background: none;
            border: none;
            color: white;
            font-size: 20px;
            cursor: pointer;
            padding: 0;
            width: 24px;
            height: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        .rag-chat-messages {
            flex: 1;
            overflow-y: auto;
            padding: 15px;
            display: flex;
            flex-direction: column;
            gap: 10px;
        }
        
        .rag-chat-message {
            max-width: 80%;
            padding: 10px 14px;
            border-radius: 18px;
            word-wrap: break-word;
            line-height: 1.4;
            font-size: 14px;
            white-space: pre-wrap;
        }
        
        .rag-chat-message.user {
            background: #667eea;
            color: white;
            align-self: flex-end;
            border-bottom-right-radius: 4px;
        }
        
        .rag-chat-message.assistant {
            background: #f1f3f4;
            color: #333;
            align-self: flex-start;
            border-bottom-left-radius: 4px;
        }
        
        .rag-chat-message.error {
            background: #fee;
            color: #c33;
            align-self: flex-start;
            border-bottom-left-radius: 4px;
        }
        
        .rag-chat-input-container {
            padding: 15px;
            border-top: 1px solid #eee;
            display: flex;
            gap: 10px;
        }
        
        .rag-chat-input {
            flex: 1;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 20px;
            outline: none;
            font-size: 14px;
            font-family: inherit;
        }
        
        .rag-chat-input:focus {
            border-color: #667eea;
        }
        
        .rag-chat-send {
            background: #667eea;
            color: white;
            border: none;
            border-radius: 20px;
            padding: 10px 20px;
            cursor: pointer;
            font-weight: 600;
            font-size: 14px;
            transition: background 0.2s;
        }
        
        .rag-chat-send:hover:not(:disabled) {
            background: #5568d3;
        }
        
        .rag-chat-send:disabled {
            background: #ccc;
            cursor: not-allowed;
        }
        
        .rag-chat-toggle {
            position: fixed;
            bottom: 20px;
            right: 20px;
            width: 60px;
            height: 60px;
            border-radius: 50%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            cursor: pointer;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            font-size: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
        }

        .rag-chat-toggle:hover {
            opacity: 0.7;
        }
        
        .rag-chat-loading {
            display: inline-block;
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #667eea;
            animation: rag-pulse 1.4s infinite ease-in-out both;
        }
        
        .rag-chat-loading:nth-child(1) {
            animation-delay: -0.32s;
        }
        
        .rag-chat-loading:nth-child(2) {
            animation-delay: -0.16s;
        }
        
        @keyframes rag-pulse {
            0%, 80%, 100% {
                opacity: 0.3;
            }
            40% {
                opacity: 1;
            }
        }
        
        .rag-chat-widget.hidden {
            display: none;
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
        <button class="rag-chat-toggle" id="ragChatToggle"><svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="#ffffff" viewBox="0 0 256 256"><path d="M208,144a15.78,15.78,0,0,1-10.42,14.94L146,178l-19,51.62a15.92,15.92,0,0,1-29.88,0L78,178l-51.62-19a15.92,15.92,0,0,1,0-29.88L78,110l19-51.62a15.92,15.92,0,0,1,29.88,0L146,110l51.62,19A15.78,15.78,0,0,1,208,144ZM152,48h16V64a8,8,0,0,0,16,0V48h16a8,8,0,0,0,0-16H184V16a8,8,0,0,0-16,0V32H152a8,8,0,0,0,0,16Zm88,32h-8V72a8,8,0,0,0-16,0v8h-8a8,8,0,0,0,0,16h8v8a8,8,0,0,0,16,0V96h8a8,8,0,0,0,0-16Z"></path></svg></button>
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
