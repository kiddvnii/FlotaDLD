// ==========================================================================================
// ASISTENTE DE VOZ PARA ACCESIBILIDAD (accessibility.js)
// ==========================================================================================
// Este script controla el asistente de voz y la interfaz de accesibilidad del sitio web.
// Permite que el usuario active la lectura automática de textos al pasar el cursor (mouseover).
// Guarda las preferencias del usuario en el almacenamiento local (localStorage) para que
// persistan en futuras visitas.
// ==========================================================================================

class AccessibilityVoiceAssistant {
    constructor() {
        // Estado del asistente: si está activo (true) o apagado (false).
        this.enabled = false;
        // Almacena el último elemento leído para evitar repetir la lectura innecesariamente.
        this.lastElement = null;
        // Temporizador para el retraso de la lectura (evita leer elementos al mover rápido el mouse).
        this.speechTimeout = null;
        
        // Inicializa el asistente de accesibilidad.
        this.init();
    }

    /**
     * Inicialización del asistente.
     * Carga el estado guardado del almacenamiento local (localStorage) y configura la interfaz.
     */
    init() {
        // Lee el estado guardado en el navegador. Si no existe, por defecto será 'false'.
        this.enabled = localStorage.getItem('autoVoiceEnabled') === 'true';

        // Si el DOM aún se está cargando, espera al evento DOMContentLoaded.
        // Si ya está listo, configura la interfaz de inmediato.
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.setupUI());
        } else {
            this.setupUI();
        }

        // Configura los eventos globales del mouse para la lectura automática.
        this.setupEventListeners();
    }

    /**
     * Configura y vincula los elementos del Widget de Accesibilidad en el DOM.
     * Maneja la activación del asistente y la visualización de la barra de herramientas.
     */
    setupUI() {
        // Obtiene referencias a los elementos visuales del widget.
        const toggle = document.getElementById('autoVoiceToggle');
        const widgetContainer = document.getElementById('accessibilityWidget');
        const toggleBtn = document.getElementById('accessibilityToggleBtn');
        const closeBtn = document.getElementById('accessibilityCloseBtn');

        // Configuración del interruptor (checkbox) del asistente de voz.
        if (toggle) {
            // Sincroniza el checkbox con el estado actual (activo/inactivo).
            toggle.checked = this.enabled;
            
            // Escucha cambios en el interruptor.
            toggle.addEventListener('change', (e) => {
                this.enabled = e.target.checked;
                // Guarda la nueva preferencia en el navegador.
                localStorage.setItem('autoVoiceEnabled', this.enabled);

                if (!this.enabled) {
                    // Si se desactiva, silencia inmediatamente cualquier voz en curso.
                    window.speechSynthesis.cancel();
                } else {
                    // Si se activa, saluda e informa al usuario.
                    this.speak("Lectura automática al pasar el mouse activada");
                }
            });
        }

        // Evento para abrir/cerrar el menú flotante de accesibilidad al hacer clic en el botón flotante.
        if (toggleBtn && widgetContainer) {
            toggleBtn.addEventListener('click', () => {
                widgetContainer.classList.toggle('active');
            });
        }

        // Evento para cerrar el menú flotante al hacer clic en el botón con la "X".
        if (closeBtn && widgetContainer) {
            closeBtn.addEventListener('click', () => {
                widgetContainer.classList.remove('active');
            });
        }

        // Cierra el menú de accesibilidad si el usuario hace clic en cualquier parte fuera del menú.
        document.addEventListener('click', (e) => {
            if (widgetContainer && !widgetContainer.contains(e.target) && toggleBtn && !toggleBtn.contains(e.target)) {
                widgetContainer.classList.remove('active');
            }
        });
    }

    /**
     * Registra los manejadores de eventos en el documento para rastrear el movimiento del mouse.
     */
    setupEventListeners() {
        // Evento 'mouseover': se dispara cuando el mouse entra en un elemento.
        document.addEventListener('mouseover', (e) => {
            // Si el asistente está apagado, no hace nada.
            if (!this.enabled) return;

            // Busca el elemento de interés más cercano (enlaces, botones, párrafos, inputs, etc.).
            const target = e.target.closest('a, button, label, input, select, textarea, h1, h2, h3, h4, h5, h6, p, th, td, .menu-card, .chat-toggle-btn');
            
            // Si el mouse no está sobre un elemento relevante, resetea la última referencia y sale.
            if (!target) {
                this.lastElement = null;
                return;
            }

            // Si el mouse se mueve dentro del mismo elemento, evita repetir la lectura.
            if (target === this.lastElement) return;
            this.lastElement = target;

            // Cancela el temporizador de lectura anterior para aplicar "debounce" (antirrebote).
            // Esto evita leer palabras entrecortadas si el usuario mueve el mouse rápido por la pantalla.
            if (this.speechTimeout) {
                clearTimeout(this.speechTimeout);
            }

            // Espera 150 milisegundos con el cursor quieto antes de leer el contenido.
            this.speechTimeout = setTimeout(() => {
                this.readElement(target);
            }, 150);
        });

        // Evento 'mouseout': se dispara cuando el mouse sale de un elemento.
        document.addEventListener('mouseout', (e) => {
            if (!this.enabled) return;
            
            const target = e.target.closest('a, button, label, input, select, textarea, h1, h2, h3, h4, h5, h6, p, th, td, .menu-card, .chat-toggle-btn');
            // Si salimos del elemento que estábamos leyendo, resetea la referencia del último elemento.
            if (target === this.lastElement) {
                this.lastElement = null;
            }
        });
    }

    /**
     * Analiza el elemento DOM seleccionado y construye una frase descriptiva para leer en voz alta.
     * @param {HTMLElement} element - El elemento que se va a describir y leer.
     */
    readElement(element) {
        let textToRead = "";
        const tagName = element.tagName.toLowerCase();

        // 1. Prioriza leer atributos de accesibilidad como 'aria-label' o el 'title' del elemento.
        if (element.getAttribute('aria-label')) {
            textToRead = element.getAttribute('aria-label');
        } else if (element.getAttribute('title')) {
            textToRead = element.getAttribute('title');
        } 
        // 2. Si es un control de formulario (input, textarea, select), busca su etiqueta asociada.
        else if (tagName === 'input' || tagName === 'textarea' || tagName === 'select') {
            let labelText = "";
            // Busca etiqueta externa con el atributo 'for' asociado al ID de este control.
            if (element.id) {
                const label = document.querySelector(`label[for="${element.id}"]`);
                if (label) {
                    labelText = label.textContent.trim();
                }
            }
            
            // Si no encontró etiqueta por ID, busca si el control está dentro de una etiqueta <label>.
            if (!labelText) {
                const parentLabel = element.closest('label');
                if (parentLabel) {
                    labelText = parentLabel.textContent.trim();
                }
            }

            // Obtiene la descripción textual del tipo de campo (ej. "Campo de contraseña").
            const typeText = this.getInputTypeDescription(element);
            const placeholder = element.getAttribute('placeholder') || "";

            // Arma el texto final combinando la etiqueta, el marcador de posición (placeholder) y el tipo.
            if (labelText) {
                textToRead = `${labelText}. ${typeText}`;
            } else if (placeholder) {
                textToRead = `${placeholder}. ${typeText}`;
            } else {
                textToRead = `${typeText}`;
            }
        } 
        // 3. Si es un botón, avisa que lo es antes de leer su texto.
        else if (tagName === 'button' || element.classList.contains('btn')) {
            const btnText = element.textContent.trim();
            textToRead = `Botón: ${btnText}`;
        }
        // 4. Si es un enlace, indica el destino leyendo su texto descriptivo.
        else if (tagName === 'a') {
            const linkText = element.textContent.trim();
            textToRead = `Enlace a: ${linkText}`;
        }
        // 5. Si es un encabezado (h1, h2, etc.), anuncia que se trata de un título.
        else if (tagName.startsWith('h')) {
            textToRead = `Título: ${element.textContent.trim()}`;
        }
        // 6. Para el resto de los elementos, lee directamente su texto plano.
        else {
            textToRead = element.textContent.trim();
        }

        // Limpia caracteres raros o emojis que puedan interferir con la síntesis de voz.
        textToRead = this.cleanText(textToRead);

        // Si hay un texto válido resultante, lo reproduce en voz alta.
        if (textToRead) {
            this.speak(textToRead);
        }
    }

    /**
     * Traduce el tipo técnico de un control de formulario a una descripción amigable en español.
     * @param {HTMLElement} element - El input, textarea o select.
     * @returns {string} Descripción del tipo de campo.
     */
    getInputTypeDescription(element) {
        const type = element.getAttribute('type') || 'text';
        if (element.tagName.toLowerCase() === 'select') {
            return 'Lista de opciones';
        }
        if (element.tagName.toLowerCase() === 'textarea') {
            return 'Campo de texto multilínea';
        }
        
        switch (type) {
            case 'password': return 'Campo de contraseña';
            case 'number': return 'Campo numérico';
            case 'date': return 'Campo de fecha';
            case 'checkbox': return element.checked ? 'Casilla de verificación seleccionada' : 'Casilla de verificación desmarcada';
            case 'radio': return element.checked ? 'Botón de opción seleccionado' : 'Botón de opción no seleccionado';
            default: return 'Campo de texto';
        }
    }

    /**
     * Limpia el texto de espacios múltiples, emojis y caracteres no pronunciables.
     * @param {string} text - Texto original a limpiar.
     * @returns {string} Texto limpio listo para ser leído.
     */
    cleanText(text) {
        if (!text) return "";
        return text
            .replace(/\s+/g, ' ') // Colapsa múltiples espacios en uno solo.
            .replace(/[💬🔊🔇♿🤖🚗🚙✈️❓📋👋🤔😊😔📢🛎️🔔📣🔔➤×]/g, '') // Remueve los emojis de la interfaz para que el sintetizador no los describa textualmente.
            .trim();
    }

    /**
     * Reproduce un texto en voz alta utilizando la API nativa de Síntesis de Voz del Navegador.
     * @param {string} text - El texto a reproducir.
     */
    speak(text) {
        // Valida que el navegador soporte la síntesis de voz.
        if (!window.speechSynthesis) return;

        // Cancela inmediatamente cualquier lectura que esté sonando en ese momento.
        window.speechSynthesis.cancel();

        // Crea la instancia de la frase a pronunciar.
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'es-ES'; // Configura el idioma en español.
        
        // Intenta obtener y asignar una voz en español del sistema operativo.
        const voices = window.speechSynthesis.getVoices();
        const spanishVoice = voices.find(voice => voice.lang.startsWith('es'));
        if (spanishVoice) {
            utterance.voice = spanishVoice;
        }

        // Ejecuta la reproducción de voz.
        window.speechSynthesis.speak(utterance);
    }
}

// Instancia única del asistente de voz de accesibilidad.
const accessibilityAssistantInstance = new AccessibilityVoiceAssistant();

// Solución alternativa para navegadores Chrome/Safari:
// Carga las voces del sistema en segundo plano de manera asíncrona para que estén listas cuando se soliciten.
if (window.speechSynthesis) {
    if (window.speechSynthesis.onvoiceschanged !== undefined) {
        window.speechSynthesis.onvoiceschanged = () => {
            window.speechSynthesis.getVoices();
        };
    }
}
