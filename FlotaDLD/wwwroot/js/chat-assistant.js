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
        // Indica si el panel de chat está abierto o cerrado en la pantalla.
        this.isOpen = false;
        // El historial local para ir guardando la cháchara de la sesión.
        this.messages = [];
        
        // Inicializa la interfaz y deja listos los escuchadores de eventos.
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
     * Crea dinámicamente los elementos HTML del chat en el DOM para evitar saturar el HTML principal.
     */
    createChatWidget() {
        const fragment = document.createDocumentFragment();

        // 1. Botón flotante para abrir el chat (el circulito con el emoji).
        const toggleBtn = document.createElement('button');
        toggleBtn.className = 'chat-toggle-btn';
        toggleBtn.id = 'chatToggleBtn';
        toggleBtn.innerHTML = '💬';
        toggleBtn.title = 'Abrir asistente de chat';
        fragment.appendChild(toggleBtn);

        // 2. Ventana de chat (oculta inicialmente, se activa al hacer clic en el botón de arriba).
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

        // Chanta todo el fragmento construido al final de la página.
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

        // Abre o cierra el chat al hacer clic en el círculo flotante.
        toggleBtn.addEventListener('click', () => this.toggleChat());
        
        // Cierra el chat al presionar la "X" de la cabecera.
        closeBtn.addEventListener('click', () => this.closeChat());
        
        // Envía la pregunta al hacer clic en la flechita.
        sendBtn.addEventListener('click', () => this.sendMessage());
        
        // Si el usuario presiona la tecla 'Enter', manda la pregunta altiro sin tener que hacer clic.
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.sendMessage();
        });
    }

    /**
     * Cambia el estado de visibilidad del chat (abierto/cerrado).
     */
    toggleChat() {
        if (this.isOpen) {
            this.closeChat();
        } else {
            this.openChat();
        }
    }

    /**
     * Muestra la ventana de chat, oculta el botón flotante y le hace foco al input para escribir de inmediato.
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
     * Oculta la ventana de chat y vuelve a mostrar la burbuja flotante.
     */
    closeChat() {
        const widget = document.getElementById('chatWidget');
        const toggleBtn = document.getElementById('chatToggleBtn');
        widget.style.display = 'none';
        toggleBtn.classList.remove('hidden');
        this.isOpen = false;
    }

    /**
     * Muestra el saludo inicial con un retraso de 500ms pa' que parezca que el bot está pensando 
     * y no se vea tan robótico ni soso al cargar la página.
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
     * Rescata el texto escrito, lo muestra en la burbuja del usuario, 
     * activa el spinner de carga ("escribiendo...") y le pregunta al servidor por fetch.
     */
    sendMessage() {
        const input = document.getElementById('chatInput');
        const text = input.value.trim();

        // Si el compadre no escribió nada, no hacemos nada pa' no mandar datos vacíos.
        if (!text) return;

        // Añade el mensaje del usuario en el chat.
        this.addMessage({
            text: text,
            type: 'user'
        });

        // Limpia el campo de texto pa' escribir la siguiente duda.
        input.value = '';
        
        // Muestra los puntitos suspensivos de que el asistente está procesando.
        this.showTypingIndicator();
        
        // Llama al servidor enviándole la consulta.
        this.fetchResponse(text);
    }

    /**
     * Dibuja y añade una burbuja de mensaje en la pantalla del chat.
     * @param {Object} message - Objeto del mensaje con formato { text: string, type: 'user'|'assistant' }
     */
    addMessage(message) {
        this.messages.push(message);

        const messagesContainer = document.getElementById('chatMessages');
        
        // Contenedor del mensaje.
        const messageDiv = document.createElement('div');
        messageDiv.className = `chat-message ${message.type}`;

        // Burbuja de texto.
        const bubble = document.createElement('div');
        bubble.className = 'chat-bubble';
        
        const textSpan = document.createElement('span');
        textSpan.textContent = message.text;
        bubble.appendChild(textSpan);

        // Si el mensaje es de la IA, le ponemos un botón de parlante "🔊" para leerlo en voz alta.
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
            
            // Evento para reproducir el Text-to-Speech al hacer clic.
            speakBtn.addEventListener('click', (e) => {
                e.stopPropagation(); // Evitamos que el evento flote a otros elementos.
                this.speak(message.text);
            });
            bubble.appendChild(speakBtn);
        }

        messageDiv.appendChild(bubble);
        messagesContainer.appendChild(messageDiv);

        // Scroll automático hacia abajo pa' mantener siempre el último mensaje a la vista del usuario.
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    /**
     * Muestra la animación de carga con tres puntitos suspensivos.
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
     * Saca de pantalla la animación de carga.
     */
    removeTypingIndicator() {
        const typingDiv = document.getElementById('typingIndicator');
        if (typingDiv) typingDiv.remove();
    }

    /**
     * Envía una solicitud AJAX tipo POST al backend en C# para obtener la respuesta.
     * @param {string} userMessage - El mensaje ingresado por el usuario.
     */
    async fetchResponse(userMessage) {
        try {
            // Obtiene el token CSRF (antiforgery) obligatorio para que ASP.NET no nos bloquee la petición POST.
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

            // Petición POST al endpoint del backend (/Menu?handler=Chat).
            const response = await fetch('/Menu?handler=Chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ message: userMessage })
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.error || 'Error en la solicitud');
            }

            // Quitamos el cargador y pintamos la respuesta que nos mandó el servidor.
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
     * Recupera el token CSRF del DOM para validaciones de seguridad.
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
     * Solicita la síntesis de voz (Text-to-Speech) al servidor.
     * Quita los emojis, descarga el archivo binario del audio y lo reproduce al tiro.
     * @param {string} text - El texto que se va a leer en voz alta.
     */
    async speak(text) {
        try {
            // Limpiamos los emojis del texto para que el sintetizador no ande balbuceando cosas raras.
            let cleanText = text.replace(/[🚗🚗🚙🚙✈️✈️🚗🚙✈️❓📋👋🤔😊😔📢🛎️🔔📣🔔]/g, '');
            
            // Petición GET al handler "Speak" pasando el texto codificado por URL.
            const response = await fetch(`/Menu?handler=Speak&text=${encodeURIComponent(cleanText)}`);
            if (!response.ok) {
                throw new Error('Error al generar voz');
            }
            // Obtiene la respuesta de audio en formato binario.
            const blob = await response.blob();
            // Crea una URL temporal en memoria para poder reproducirla.
            const audioUrl = URL.createObjectURL(blob);
            const audio = new Audio(audioUrl);
            
            // ¡Póngale play!
            audio.play();
        } catch (error) {
            console.error('Error en Text-to-Speech:', error);
            alert('No se pudo reproducir el audio.');
        }
    }
}

// Inicializa el ayudante apenas el DOM del sitio web esté completamente cargado.
document.addEventListener('DOMContentLoaded', () => {
    new ChatAssistant();
});
