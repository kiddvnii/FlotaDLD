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
        // Estado del asistente: prendido (true) o apagado (false).
        this.enabled = false;
        // Guarda el último elemento que leyó pa' no andar repitiendo la misma lesera a cada rato.
        this.lastElement = null;
        // Temporizador para controlar el retraso (evita que el bot hable si mueves el mouse muy rápido).
        this.speechTimeout = null;
        
        // Inicializa el asistente de accesibilidad al tiro.
        this.init();
    }

    /**
     * Inicialización del asistente.
     * Carga el estado guardado del almacenamiento local (localStorage) y configura la interfaz.
     */
    init() {
        // Lee el estado guardado en el navegador. Si no existe, por defecto parte apagado ('false').
        this.enabled = localStorage.getItem('autoVoiceEnabled') === 'true';

        // Si el DOM aún se está cargando, espera al evento DOMContentLoaded.
        // Si ya está listo, configura la interfaz de inmediato.
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.setupUI());
        } else {
            this.setupUI();
        }

        // Configura los escuchadores de eventos globales para cachar el movimiento del mouse.
        this.setupEventListeners();
    }

    /**
     * Configura y vincula los elementos del Widget de Accesibilidad en el DOM.
     * Maneja la activación del asistente y la visualización de la barra de herramientas.
     */
    setupUI() {
        const toggle = document.getElementById('autoVoiceToggle');
        const widgetContainer = document.getElementById('accessibilityWidget');
        const toggleBtn = document.getElementById('accessibilityToggleBtn');
        const closeBtn = document.getElementById('accessibilityCloseBtn');

        // Configuración del interruptor (checkbox) de lectura de voz.
        if (toggle) {
            // Sincroniza el checkbox de la pantalla con el estado guardado.
            toggle.checked = this.enabled;
            
            // Escucha cuando el usuario cambia el interruptor.
            toggle.addEventListener('change', (e) => {
                this.enabled = e.target.checked;
                // Guarda la preferencia en el navegador pa' que se acuerde la próxima vez.
                localStorage.setItem('autoVoiceEnabled', this.enabled);

                if (!this.enabled) {
                    // Si se apaga, silencia al robot inmediatamente para que no siga hinchando.
                    window.speechSynthesis.cancel();
                } else {
                    // Si se activa, saluda con voz de robot para avisar que ya está funcionando.
                    this.speak("Lectura automática al pasar el mouse activada");
                }
            });
        }

        // Abre o cierra el panel de accesibilidad al hacer clic en el botón flotante.
        if (toggleBtn && widgetContainer) {
            toggleBtn.addEventListener('click', () => {
                widgetContainer.classList.toggle('active');
            });
        }

        // Cierra el menú al presionar la "X".
        if (closeBtn && widgetContainer) {
            closeBtn.addEventListener('click', () => {
                widgetContainer.classList.remove('active');
            });
        }

        // Si el usuario hace clic en cualquier parte fuera de la ventana de accesibilidad, la cerramos pa' que no estorbe.
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
        // Evento 'mouseover': se activa cuando el puntero del mouse entra en cualquier elemento.
        document.addEventListener('mouseover', (e) => {
            // Si la lectura automática está apagada, no hacemos ni una cuestión y salimos.
            if (!this.enabled) return;

            // Busca el elemento importante más cercano donde esté el mouse (enlaces, botones, títulos, celdas de tablas, etc.).
            const target = e.target.closest('a, button, label, input, select, textarea, h1, h2, h3, h4, h5, h6, p, th, td, .menu-card, .chat-toggle-btn');
            
            // Si el mouse está flotando en un espacio vacío, resetea la última referencia y sale.
            if (!target) {
                this.lastElement = null;
                return;
            }

            // Si el mouse se mueve dentro del mismo elemento, no leemos otra vez pa' que el robot no tartamudee.
            if (target === this.lastElement) return;
            this.lastElement = target;

            // Cancelamos el temporizador anterior (antirrebote / debounce).
            // Esto evita leer palabras cortadas si el usuario mueve el mouse rápido por la pantalla.
            if (this.speechTimeout) {
                clearTimeout(this.speechTimeout);
            }

            // Espera 150 milisegundos con el cursor quieto sobre el elemento antes de empezar a leer.
            this.speechTimeout = setTimeout(() => {
                this.readElement(target);
            }, 150);
        });

        // Evento 'mouseout': se activa cuando el mouse sale de un elemento.
        document.addEventListener('mouseout', (e) => {
            if (!this.enabled) return;
            
            const target = e.target.closest('a, button, label, input, select, textarea, h1, h2, h3, h4, h5, h6, p, th, td, .menu-card, .chat-toggle-btn');
            // Si salimos del elemento que estábamos leyendo, limpiamos la referencia pa' poder volver a leerlo después si el mouse entra de nuevo.
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

        // 1. Prioriza leer etiquetas de accesibilidad como 'aria-label' o el 'title' descriptivo.
        if (element.getAttribute('aria-label')) {
            textToRead = element.getAttribute('aria-label');
        } else if (element.getAttribute('title')) {
            textToRead = element.getAttribute('title');
        } 
        // 2. Si es un campo de texto, combo o caja de texto de formulario, busca su label asociado.
        else if (tagName === 'input' || tagName === 'textarea' || tagName === 'select') {
            let labelText = "";
            
            // Busca la etiqueta <label> con el atributo 'for' asociado al ID de este control.
            if (element.id) {
                const label = document.querySelector(`label[for="${element.id}"]`);
                if (label) {
                    labelText = label.textContent.trim();
                }
            }
            
            // Si no encontró nada, revisa si el campo está metido dentro de un <label> contenedor.
            if (!labelText) {
                const parentLabel = element.closest('label');
                if (parentLabel) {
                    labelText = parentLabel.textContent.trim();
                }
            }

            // Consigue la descripción amigable del tipo de campo (ej. "Campo de contraseña").
            const typeText = this.getInputTypeDescription(element);
            const placeholder = element.getAttribute('placeholder') || "";

            // Junta todo de forma ordenada: Label + Placeholder + Tipo de control.
            if (labelText) {
                textToRead = `${labelText}. ${typeText}`;
            } else if (placeholder) {
                textToRead = `${placeholder}. ${typeText}`;
            } else {
                textToRead = `${typeText}`;
            }
        } 
        // 3. Si es un botón, le avisa al usuario que es un botón antes de leer el texto.
        else if (tagName === 'button' || element.classList.contains('btn')) {
            const btnText = element.textContent.trim();
            textToRead = `Botón: ${btnText}`;
        }
        // 4. Si es un enlace, avisa adónde lleva leyendo su descripción.
        else if (tagName === 'a') {
            const linkText = element.textContent.trim();
            textToRead = `Enlace a: ${linkText}`;
        }
        // 5. Si es un título (h1, h2, etc.), avisa que es un título para dar contexto estructural.
        else if (tagName.startsWith('h')) {
            textToRead = `Título: ${element.textContent.trim()}`;
        }
        // 6. Para cualquier otro elemento de texto plano, lo lee tal cual está.
        else {
            textToRead = element.textContent.trim();
        }

        // Limpiamos los emojis y caracteres raros antes de mandar el texto al sintetizador.
        textToRead = this.cleanText(textToRead);

        // Si tenemos un texto limpio y válido, ¡póngale play!
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
     * Limpia el texto de espacios múltiples, emojis y caracteres raros.
     * Evita que la voz intente deletrear caracteres gráficos extraños.
     * @param {string} text - Texto original a limpiar.
     * @returns {string} Texto limpio listo para ser leído.
     */
    cleanText(text) {
        if (!text) return "";
        return text
            .replace(/\s+/g, ' ') // Colapsa múltiples espacios seguidos en uno solo.
            .replace(/[💬🔊🔇♿🤖🚗🚙✈️❓📋👋🤔😊😔📢🛎️🔔📣🔔➤×]/g, '') // Quita los emojis de raíz pa' que no interfieran en la lectura.
            .trim();
    }

    /**
     * Reproduce un texto en voz alta utilizando la API de síntesis de voz nativa del navegador.
     * @param {string} text - El texto a reproducir.
     */
    speak(text) {
        // Si el navegador no apoya la síntesis de voz, salimos sin hacer dramas.
        if (!window.speechSynthesis) return;

        // Cancela cualquier lectura que esté sonando en ese mismo instante pa' que no se junten las voces.
        window.speechSynthesis.cancel();

        // Crea la instancia de la frase a pronunciar.
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'es-ES'; // Idioma configurado en español.
        
        // Busca si el sistema tiene una voz nativa en español para no hablar con acento de gringo intentando español.
        const voices = window.speechSynthesis.getVoices();
        const spanishVoice = voices.find(voice => voice.lang.startsWith('es'));
        if (spanishVoice) {
            utterance.voice = spanishVoice;
        }

        // Manda a hablar al sintetizador.
        window.speechSynthesis.speak(utterance);
    }
}

// Instancia única del asistente de voz de accesibilidad.
const accessibilityAssistantInstance = new AccessibilityVoiceAssistant();

// Torpedo para navegadores Chrome/Safari:
// Carga las voces del sistema en segundo plano de manera asíncrona para tenerlas listas de inmediato.
if (window.speechSynthesis) {
    if (window.speechSynthesis.onvoiceschanged !== undefined) {
        window.speechSynthesis.onvoiceschanged = () => {
            window.speechSynthesis.getVoices();
        };
    }
}
