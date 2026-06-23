// ==========================================================================================
// ASISTENTE DE CHAT VIRTUAL (chat-assistant.js)
// ==========================================================================================
// Este script crea y gestiona un widget de chat interactivo que flota en la esquina de la pantalla.
// Permite a los usuarios hacer preguntas al asistente de IA de Flota DLD.
// Realiza solicitudes AJAX al servidor mediante Fetch API para enviar el mensaje y recibir la
// respuesta generada, además de soportar la reproducción de audio a través de un endpoint backend.
// ==========================================================================================

class ChatAssistant {
    constructor() {
        // Indica si el panel de chat está abierto o cerrado visualmente.
        this.isOpen = false;
        // Historial local de mensajes enviados y recibidos en la sesión.
        this.messages = [];
        
        // Inicializa la interfaz y eventos del chat.
        this.init();
    }

    /**
     * Inicializa el asistente: construye el widget HTML, vincula eventos y muestra el saludo inicial.
     */
    init() {
        this.createChatWidget();
        this.attachEventListeners();
        this.showInitialGreeting();
    }

    /**
     * Crea dinámicamente los elementos HTML del chat en el DOM para evitar saturar el marcado principal.
     */
    createChatWidget() {
        const fragment = document.createDocumentFragment();

        // 1. Botón flotante para abrir el chat.
        const toggleBtn = document.createElement('button');
        toggleBtn.className = 'chat-toggle-btn';
        toggleBtn.id = 'chatToggleBtn';
        toggleBtn.innerHTML = '💬';
        toggleBtn.title = 'Abrir asistente de chat';
        fragment.appendChild(toggleBtn);

        // 2. Ventana de chat (oculta inicialmente).
        const widget = document.createElement('div');
        widget.className = 'chat-assistant-widget';
        widget.id = 'chatWidget';
        widget.style.display = 'none'; // Se muestra con flex al abrir.
        widget.innerHTML = `
            <div class="chat-header">
                <div>
                    <h3>Asistente Flota DLD</h3>
                    <p>¿Cómo podemos ayudarte?</p>
                </div>
                <button class="chat-close-btn" id="chatCloseBtn" title="Cerrar chat">&times;</button>
            </div>
            <div class="chat-messages" id="chatMessages"></div>
            <div class="chat-input-area">
                <input type="text" class="chat-input" id="chatInput" placeholder="Escribe tu pregunta...">
                <button class="chat-send-btn" id="chatSendBtn" title="Enviar">➤</button>
            </div>
        `;
        fragment.appendChild(widget);

        // Añade todo el fragmento al cuerpo del documento.
        document.body.appendChild(fragment);
    }

    /**
     * Asocia los eventos de clic y teclado a los botones e inputs del chat.
     */
    attachEventListeners() {
        const toggleBtn = document.getElementById('chatToggleBtn');
        const closeBtn = document.getElementById('chatCloseBtn');
        const sendBtn = document.getElementById('chatSendBtn');
        const input = document.getElementById('chatInput');

        // Alterna el estado del chat al pulsar el botón de burbuja flotante.
        toggleBtn.addEventListener('click', () => this.toggleChat());
        
        // Cierra el chat al presionar el botón "X" en la cabecera.
        closeBtn.addEventListener('click', () => this.closeChat());
        
        // Envía el mensaje actual al hacer clic en el botón de enviar.
        sendBtn.addEventListener('click', () => this.sendMessage());
        
        // Permite enviar el mensaje al presionar la tecla 'Enter' en el campo de texto.
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.sendMessage();
        });
    }

    /**
     * Cambia el estado de visibilidad del chat.
     */
    toggleChat() {
        if (this.isOpen) {
            this.closeChat();
        } else {
            this.openChat();
        }
    }

    /**
     * Muestra la ventana de chat, oculta el botón flotante y coloca el foco en el campo de texto.
     */
    openChat() {
        const widget = document.getElementById('chatWidget');
        const toggleBtn = document.getElementById('chatToggleBtn');
        widget.style.display = 'flex';
        toggleBtn.classList.add('hidden');
        this.isOpen = true;
        document.getElementById('chatInput').focus();
    }

    /**
     * Oculta la ventana de chat y vuelve a mostrar el botón flotante.
     */
    closeChat() {
        const widget = document.getElementById('chatWidget');
        const toggleBtn = document.getElementById('chatToggleBtn');
        widget.style.display = 'none';
        toggleBtn.classList.remove('hidden');
        this.isOpen = false;
    }

    /**
     * Muestra el saludo de bienvenida automático con un pequeño retraso de 500ms
     * para emular un comportamiento más natural de la IA al cargar la página.
     */
    showInitialGreeting() {
        setTimeout(() => {
            this.addMessage({
                text: '¡Hola! Soy el asistente de Flota DLD. ¿En qué puedo ayudarte hoy? 👋',
                type: 'assistant'
            });
        }, 500);
    }

    /**
     * Lee el valor de entrada, añade el mensaje del usuario en pantalla,
     * activa el indicador de escritura ("escribiendo...") y solicita la respuesta del servidor.
     */
    sendMessage() {
        const input = document.getElementById('chatInput');
        const text = input.value.trim();

        // Evita el envío de mensajes vacíos.
        if (!text) return;

        // Añade el mensaje al historial visual.
        this.addMessage({
            text: text,
            type: 'user'
        });

        // Limpia el input del chat.
        input.value = '';
        
        // Muestra la animación de carga/espera.
        this.showTypingIndicator();
        
        // Llama a la API para obtener respuesta.
        this.fetchResponse(text);
    }

    /**
     * Construye y añade una burbuja de mensaje en el contenedor de chat.
     * @param {Object} message - Objeto del mensaje con formato { text: string, type: 'user'|'assistant' }
     */
    addMessage(message) {
        this.messages.push(message);

        const messagesContainer = document.getElementById('chatMessages');
        
        // Contenedor principal del mensaje.
        const messageDiv = document.createElement('div');
        messageDiv.className = `chat-message ${message.type}`;

        // Burbuja de diálogo.
        const bubble = document.createElement('div');
        bubble.className = 'chat-bubble';
        
        // Texto del mensaje.
        const textSpan = document.createElement('span');
        textSpan.textContent = message.text;
        bubble.appendChild(textSpan);

        // Si el mensaje es del asistente, agrega un botón de altavoz "🔊" para leerlo en voz alta.
        if (message.type === 'assistant') {
            const speakBtn = document.createElement('button');
            speakBtn.className = 'speak-btn';
            speakBtn.innerHTML = ' 🔊';
            speakBtn.title = 'Escuchar respuesta';
            speakBtn.style.border = 'none';
            speakBtn.style.background = 'none';
            speakBtn.style.marginLeft = '6px';
            speakBtn.style.cursor = 'pointer';
            speakBtn.style.padding = '0';
            speakBtn.style.display = 'inline-block';
            speakBtn.style.verticalAlign = 'middle';
            
            // Evento para sintetizar el texto de la respuesta al hacer clic en el altavoz.
            speakBtn.addEventListener('click', (e) => {
                e.stopPropagation(); // Evita burbujeo no deseado de eventos.
                this.speak(message.text);
            });
            bubble.appendChild(speakBtn);
        }

        messageDiv.appendChild(bubble);
        messagesContainer.appendChild(messageDiv);

        // Hace scroll hacia abajo para asegurar que el nuevo mensaje quede visible.
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    /**
     * Muestra una animación de tres puntos suspensivos simulando que el asistente está escribiendo.
     */
    showTypingIndicator() {
        const messagesContainer = document.getElementById('chatMessages');
        const typingDiv = document.createElement('div');
        typingDiv.className = 'chat-message assistant';
        typingDiv.id = 'typingIndicator';
        typingDiv.innerHTML = `
            <div class="chat-bubble">
                <div class="chat-loading">
                    <span></span>
                    <span></span>
                    <span></span>
                </div>
            </div>
        `;
        messagesContainer.appendChild(typingDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    /**
     * Remueve el indicador visual de escritura de la pantalla.
     */
    removeTypingIndicator() {
        const typingDiv = document.getElementById('typingIndicator');
        if (typingDiv) typingDiv.remove();
    }

    /**
     * Envía una solicitud AJAX al backend (ASP.NET Razor Page) solicitando la respuesta de la IA.
     * @param {string} userMessage - El mensaje escrito por el usuario.
     */
    async fetchResponse(userMessage) {
        try {
            // Obtiene el token CSRF para proteger la solicitud POST contra ataques CSRF.
            const token = this.getAntiforgeryToken();

            if (!token) {
                console.error('Token CSRF no encontrado');
                this.removeTypingIndicator();
                this.addMessage({
                    text: 'Error: Token de seguridad no encontrado. Recarga la página.',
                    type: 'assistant'
                });
                return;
            }

            // Realiza la petición POST al backend (Handler "Chat" en la página Menu).
            const response = await fetch('/Menu?handler=Chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ message: userMessage })
            });

            const data = await response.json();

            // Lanza error si la respuesta HTTP no fue exitosa.
            if (!response.ok) {
                throw new Error(data.error || 'Error en la solicitud');
            }

            // Remueve el indicador de carga e imprime la respuesta de la IA.
            this.removeTypingIndicator();
            this.addMessage({
                text: data.message,
                type: 'assistant'
            });
        } catch (error) {
            console.error('Error al obtener respuesta:', error);
            this.removeTypingIndicator();
            this.addMessage({
                text: 'Lo siento, ocurrió un error: ' + error.message + '. Por favor, intenta de nuevo. 😔',
                type: 'assistant'
            });
        }
    }

    /**
     * Extrae el token de verificación de solicitudes de ASP.NET Core desde el DOM.
     * @returns {string} Token de seguridad.
     */
    getAntiforgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!token) {
            console.warn('Token input no encontrado en la página');
            return '';
        }
        return token.value;
    }

    /**
     * Solicita la síntesis de voz al servidor backend.
     * Envía el texto y reproduce el stream de audio retornado por el servidor.
     * @param {string} text - El texto a leer en voz alta.
     */
    async speak(text) {
        try {
            // Remueve emojis del texto para que no interfieran con la pronunciación del backend.
            let cleanText = text.replace(/[🚗🚗🚙🚙✈️✈️🚗🚙✈️❓📋👋🤔😊😔📢🛎️🔔📣🔔]/g, '');
            
            // Realiza una petición GET al Handler "Speak" de la página Menu enviando el texto.
            const response = await fetch(`/Menu?handler=Speak&text=${encodeURIComponent(cleanText)}`);
            if (!response.ok) {
                throw new Error('Error al generar voz');
            }
            // Obtiene la respuesta en formato binario (Blob, típicamente audio/wav).
            const blob = await response.blob();
            // Crea una URL local temporal para reproducir el archivo de audio.
            const audioUrl = URL.createObjectURL(blob);
            const audio = new Audio(audioUrl);
            
            // Reproduce el audio.
            audio.play();
        } catch (error) {
            console.error('Error en Text-to-Speech:', error);
            alert('No se pudo reproducir el audio.');
        }
    }
}

// Inicializa el chat una vez que el DOM del sitio web esté completamente cargado.
document.addEventListener('DOMContentLoaded', () => {
    new ChatAssistant();
});
