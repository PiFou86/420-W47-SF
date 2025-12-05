class ChatApp {
    constructor() {
        // DOM Elements
        this.microphoneSelect = document.getElementById('microphone-select');
        this.toggleButton = document.getElementById('toggle-listening');
        this.statusElement = document.getElementById('status');
        this.statusText = this.statusElement.querySelector('.status-text');
        this.messagesContainer = document.getElementById('messages');
        this.messageCountElement = document.getElementById('message-count');
        this.durationElement = document.getElementById('duration');
        this.playbackSpeedSlider = document.getElementById('playback-speed');
        this.speedValueDisplay = document.getElementById('speed-value');
        this.textInput = document.getElementById('text-input');
        this.sendTextButton = document.getElementById('send-text-button');
        this.toggleAudioButton = document.getElementById('toggle-audio');
        this.audioIcon = document.getElementById('audio-icon');
        this.audioStatusText = document.getElementById('audio-status-text');

        // State
        this.isListening = false;
        this.audioEnabled = true; // Audio playback enabled by default
        this.ws = null;
        this.messageCount = 0;
        this.startTime = null;
        this.durationInterval = null;
        this.selectedMicrophone = null;
        this.audioContext = null;
        this.mediaStream = null;
        this.audioWorkletNode = null;
        this.currentTranscript = { user: '', assistant: '' };
        this.audioQueue = [];
        this.isPlayingAudio = false;
        this.currentResponseId = null;
        this.playbackSpeed = 1.0;
        this.minBufferChunks = 8; // Nombre de chunks à accumuler avant de commencer (augmenté pour meilleure qualité)
        this.isBuffering = false;
        this.currentAudioSource = null; // Track current playing audio source for interruption

        // Initialize
        this.init();
    }

    async init() {
        await this.loadMicrophones();
        this.setupEventListeners();
        this.checkSoundTouchAvailability();
    }

    checkSoundTouchAvailability() {
        if (typeof window.SoundTouch !== 'undefined') {
            console.log('✅ SoundTouch chargé - Time-stretching disponible');
        } else {
            console.warn('⚠️ SoundTouch non disponible - Utilisation de playbackRate (change la hauteur)');
        }
    }

    async loadMicrophones() {
        try {
            // Request microphone permission
            await navigator.mediaDevices.getUserMedia({ audio: true });

            // Get all audio input devices
            const devices = await navigator.mediaDevices.enumerateDevices();
            const microphones = devices.filter(device => device.kind === 'audioinput');

            // Clear and populate microphone select
            this.microphoneSelect.innerHTML = '<option value="">Sélectionnez un microphone...</option>';
            
            microphones.forEach((mic, index) => {
                const option = document.createElement('option');
                option.value = mic.deviceId;
                option.textContent = mic.label || `Microphone ${index + 1}`;
                this.microphoneSelect.appendChild(option);
            });

            if (microphones.length > 0) {
                this.updateStatus('Prêt - Sélectionnez un microphone');
            } else {
                this.updateStatus('Aucun microphone détecté');
            }
        } catch (error) {
            console.error('Erreur lors du chargement des microphones:', error);
            this.updateStatus('Erreur: Accès au microphone refusé');
            this.addSystemMessage('❌ Impossible d\'accéder au microphone. Veuillez autoriser l\'accès dans les paramètres de votre navigateur.');
        }
    }

    setupEventListeners() {
        this.microphoneSelect.addEventListener('change', (e) => {
            this.selectedMicrophone = e.target.value;
            this.toggleButton.disabled = !this.selectedMicrophone;
            if (this.selectedMicrophone) {
                this.updateStatus('Prêt', 'ready');
            }
        });

        this.toggleButton.addEventListener('click', () => {
            this.toggleListening();
        });

        this.playbackSpeedSlider.addEventListener('input', (e) => {
            this.playbackSpeed = parseFloat(e.target.value);
            this.speedValueDisplay.textContent = this.playbackSpeed.toFixed(1) + 'x';
        });

        // Text input events
        this.sendTextButton.addEventListener('click', () => {
            this.sendTextMessage();
        });

        this.textInput.addEventListener('keydown', (e) => {
            // Send on Enter (without Shift)
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendTextMessage();
            }
        });

        // Audio toggle button
        this.toggleAudioButton.addEventListener('click', () => {
            this.toggleAudioPlayback();
        });
    }

    async connectWebSocket() {
        return new Promise((resolve, reject) => {
            const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
            const wsUrl = `${protocol}//${window.location.host}/ws/realtime`;

            this.ws = new WebSocket(wsUrl);

            this.ws.onopen = () => {
                console.log('WebSocket connecté');
                this.updateStatus('Connexion au serveur...');
            };

            this.ws.onmessage = async (event) => {
                try {
                    const message = JSON.parse(event.data);
                    await this.handleServerMessage(message);
                } catch (error) {
                    console.error('Erreur lors du traitement du message:', error);
                }
            };

            this.ws.onerror = (error) => {
                console.error('Erreur WebSocket:', error);
                this.addSystemMessage('❌ Erreur de connexion au serveur');
                reject(error);
            };

            this.ws.onclose = () => {
                console.log('WebSocket déconnecté');
                if (this.isListening) {
                    this.stopListening();
                }
                this.ws = null;
            };

            // Wait for ready message
            const readyHandler = (event) => {
                const message = JSON.parse(event.data);
                if (message.type === 'ready') {
                    this.ws.removeEventListener('message', readyHandler);
                    resolve();
                }
            };
            this.ws.addEventListener('message', readyHandler);

            // Timeout after 10 seconds
            setTimeout(() => reject(new Error('Connection timeout')), 10000);
        });
    }

    async handleServerMessage(message) {
        switch (message.type) {
            case 'ready':
                this.updateStatus('Connecté - Prêt à écouter', 'ready');
                this.addSystemMessage('✅ Connecté à OpenAI. Vous pouvez commencer à parler.');
                break;

            case 'status':
                // If user starts speaking while audio is playing, stop it
                if (message.status === 'User speaking...' && this.isPlayingAudio) {
                    console.log('[Interruption] ⚠️ Utilisateur parle - Arrêt audio (via status)');
                    this.stopAudioPlayback();
                }
                this.updateStatus(message.status, message.status.includes('Ready') ? 'ready' : 'listening');
                break;

            case 'audio':
                // Queue audio for playback (only if audio is enabled)
                if (message.audio) {
                    if (!this.audioEnabled) {
                        console.log('[Audio] Audio désactivé, chunk ignoré');
                        break;
                    }
                    
                    this.audioQueue.push(message.audio);
                    
                    // Si on n'est pas en train de jouer et qu'on a assez de buffer, commencer
                    if (!this.isPlayingAudio && !this.isBuffering) {
                        if (this.audioQueue.length >= this.minBufferChunks) {
                            console.log(`[Buffer] ${this.audioQueue.length} chunks accumulés, début de la lecture`);
                            this.playAudioQueue();
                        } else {
                            this.isBuffering = true;
                            console.log(`[Buffer] Accumulation... (${this.audioQueue.length}/${this.minBufferChunks})`);
                        }
                    } else if (this.isBuffering && this.audioQueue.length >= this.minBufferChunks) {
                        console.log(`[Buffer] Buffer plein (${this.audioQueue.length} chunks), démarrage lecture`);
                        this.isBuffering = false;
                        this.playAudioQueue();
                    }
                }
                break;

            case 'transcript':
                this.handleTranscript(message.role, message.transcript);
                break;

            case 'error':
                this.addSystemMessage(`❌ Erreur: ${message.error}`);
                this.updateStatus('Erreur');
                break;

            default:
                console.log('Type de message inconnu:', message.type);
        }
    }

    handleTranscript(role, transcript) {
        // Check for special system commands
        if (role === 'system' && transcript === '__CANCEL_AUDIO__') {
            console.log('[Interruption] ⚠️ COMMANDE D\'ANNULATION REÇUE');
            this.stopAudioPlayback();
            return;
        }
        
        if (role === 'user') {
            // User transcript arrives complete from the server
            // Note: It may arrive AFTER the assistant response has started
            if (transcript && transcript.trim()) {
                this.addUserMessageBeforeLastAssistant(transcript);
            }
        } else if (role === 'assistant') {
            // Assistant transcript arrives as deltas
            this.updateOrAddAssistantMessage(transcript);
        } else if (role === 'system' && transcript === '__RESPONSE_DONE__') {
            // Mark current response as complete
            this.finalizeCurrentAssistantMessage();
            // Reset buffering for next response
            this.isBuffering = false;
        }
    }

    updateOrAddAssistantMessage(delta) {
        // Find the last assistant message that is still streaming
        const messages = this.messagesContainer.querySelectorAll('.message.ai');
        let lastStreamingMessage = null;
        
        // Find the last message that is still streaming
        for (let i = messages.length - 1; i >= 0; i--) {
            if (messages[i].dataset.streaming === 'true') {
                lastStreamingMessage = messages[i];
                break;
            }
        }
        
        if (lastStreamingMessage) {
            // Update existing streaming message
            const textElement = lastStreamingMessage.querySelector('.message-text');
            textElement.textContent += delta;
            this.scrollToBottom();
        } else {
            // Create new assistant message
            const message = this.createMessageElement('ai', '🤖', 'IA', delta);
            message.dataset.streaming = 'true';
            message.dataset.responseId = Date.now(); // Unique ID for this response
            this.messagesContainer.appendChild(message);
            this.scrollToBottom();
            this.updateMessageCount();
        }
    }

    finalizeCurrentAssistantMessage() {
        // Mark the current streaming message as complete
        const messages = this.messagesContainer.querySelectorAll('.message.ai');
        for (let i = messages.length - 1; i >= 0; i--) {
            if (messages[i].dataset.streaming === 'true') {
                messages[i].dataset.streaming = 'false';
                break;
            }
        }
    }

    stopAudioPlayback() {
        console.log('[Interruption] ━━━ DÉBUT ARRÊT AUDIO ━━━');
        console.log('[Interruption] État avant: isPlaying =', this.isPlayingAudio, ', queue =', this.audioQueue.length, ', source =', !!this.currentAudioSource);
        
        // CRITICAL: Set this FIRST to prevent onended from continuing
        this.isPlayingAudio = false;
        this.isBuffering = false;
        
        // Stop currently playing audio source
        if (this.currentAudioSource) {
            try {
                console.log('[Interruption] Arrêt de la source audio...');
                this.currentAudioSource.stop();
                this.currentAudioSource.disconnect();
                console.log('[Interruption] ✅ Source audio arrêtée');
            } catch (e) {
                // Source may already be stopped
                console.log('[Interruption] ⚠️ Source déjà arrêtée:', e.message);
            }
            this.currentAudioSource = null;
        } else {
            console.log('[Interruption] Pas de source audio active');
        }
        
        // Clear the audio queue
        const queueSize = this.audioQueue.length;
        this.audioQueue = [];
        console.log(`[Interruption] ✅ ${queueSize} chunks supprimés de la queue`);
        
        // Finalize any streaming assistant message
        this.finalizeCurrentAssistantMessage();
        
        console.log('[Interruption] ━━━ FIN ARRÊT AUDIO ━━━');
    }

    async playAudioQueue() {
        console.log(`[PlayQueue] Appelé - Queue: ${this.audioQueue.length}, isPlaying: ${this.isPlayingAudio}, audioEnabled: ${this.audioEnabled}`);
        
        // Don't play if audio is disabled
        if (!this.audioEnabled) {
            console.log('[PlayQueue] Audio désactivé, arrêt');
            this.isPlayingAudio = false;
            this.audioQueue = [];
            return;
        }
        
        if (this.audioQueue.length === 0) {
            this.isPlayingAudio = false;
            console.log('[PlayQueue] Queue vide, arrêt');
            return;
        }

        this.isPlayingAudio = true;
        const base64Audio = this.audioQueue.shift();
        console.log(`[PlayQueue] Traitement chunk - Reste: ${this.audioQueue.length}`);

        try {
            // Decode base64 to raw PCM16 data
            const binaryString = atob(base64Audio);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }

            // Convert to Int16Array (PCM16)
            const pcm16 = new Int16Array(bytes.buffer);

            // Create audio context for playback if not exists
            if (!this.playbackContext) {
                console.log('[PlayQueue] Création AudioContext');
                this.playbackContext = new (window.AudioContext || window.webkitAudioContext)({
                    sampleRate: 24000
                });
                console.log('[PlayQueue] AudioContext state:', this.playbackContext.state);
            }
            
            // Resume context if suspended (Safari requirement)
            if (this.playbackContext.state === 'suspended') {
                console.log('[PlayQueue] AudioContext suspendu, tentative de reprise...');
                await this.playbackContext.resume();
                console.log('[PlayQueue] AudioContext state après resume:', this.playbackContext.state);
            }

            let processedSamples;

            // Apply time-stretching if speed is not 1.0
            if (this.playbackSpeed !== 1.0 && typeof SoundTouch !== 'undefined') {
                processedSamples = this.applyTimeStretching(pcm16, 24000, this.playbackSpeed);
            } else if (this.playbackSpeed !== 1.0) {
                console.warn('[Audio] SoundTouch non disponible, utilisation de playbackRate classique');
                // Fallback to simple conversion and we'll use playbackRate
                processedSamples = new Float32Array(pcm16.length);
                for (let i = 0; i < pcm16.length; i++) {
                    processedSamples[i] = pcm16[i] / 32768.0;
                }
            } else {
                // No time-stretching needed, just convert to Float32
                processedSamples = new Float32Array(pcm16.length);
                for (let i = 0; i < pcm16.length; i++) {
                    processedSamples[i] = pcm16[i] / 32768.0;
                }
            }

            // Create audio buffer
            const audioBuffer = this.playbackContext.createBuffer(1, processedSamples.length, 24000);
            audioBuffer.getChannelData(0).set(processedSamples);

            // Play the audio
            const source = this.playbackContext.createBufferSource();
            source.buffer = audioBuffer;
            
            // Store reference for potential interruption
            this.currentAudioSource = source;
            
            // If SoundTouch isn't available, use playbackRate as fallback
            if (this.playbackSpeed !== 1.0 && typeof SoundTouch === 'undefined') {
                source.playbackRate.value = this.playbackSpeed;
                console.log(`[Audio] Utilisation de playbackRate: ${this.playbackSpeed}x (fallback)`);
            }
            
            source.connect(this.playbackContext.destination);
            
            source.onended = () => {
                console.log('[PlayQueue] Chunk terminé');
                this.currentAudioSource = null; // Clear reference
                
                // CRITICAL CHECK: Only continue if not interrupted
                if (this.isPlayingAudio) {
                    console.log('[PlayQueue] ➡️ Passage au chunk suivant');
                    this.playAudioQueue(); // Play next in queue
                } else {
                    console.log('[PlayQueue] ⛔ INTERROMPU - Arrêt de la queue');
                }
            };

            console.log('[PlayQueue] Démarrage lecture...');
            source.start();
            console.log('[PlayQueue] ✅ Lecture démarrée');
        } catch (error) {
            console.error('[PlayQueue] ❌ ERREUR:', error);
            console.error('[PlayQueue] Stack:', error.stack);
            this.playAudioQueue(); // Continue with next audio
        }
    }

    applyTimeStretching(pcm16Samples, sampleRate, speed) {
        try {
            // Convert Int16 to Float32 for SoundTouch (interleaved stereo format)
            const float32Input = new Float32Array(pcm16Samples.length * 2); // Stereo
            for (let i = 0; i < pcm16Samples.length; i++) {
                const sample = pcm16Samples[i] / 32768.0;
                float32Input[i * 2] = sample;     // Left channel
                float32Input[i * 2 + 1] = sample; // Right channel (same for mono)
            }
            
            // Create SoundTouch instance
            const soundTouch = new window.SoundTouch();
            soundTouch.tempo = speed; // Change tempo (speed) without affecting pitch
            soundTouch.pitch = 1.0;   // Keep original pitch
            
            // Feed all input samples to SoundTouch
            soundTouch.inputBuffer.putSamples(float32Input, 0, pcm16Samples.length);
            
            // Process the samples
            soundTouch.process();
            
            // Extract all processed samples
            const outputFrames = soundTouch.outputBuffer.frameCount;
            
            if (outputFrames === 0) {
                throw new Error('No output frames');
            }
            
            const stereoOutput = new Float32Array(outputFrames * 2);
            soundTouch.outputBuffer.receiveSamples(stereoOutput, outputFrames);
            
            // Convert back to mono Float32
            const monoOutput = new Float32Array(outputFrames);
            for (let i = 0; i < outputFrames; i++) {
                monoOutput[i] = stereoOutput[i * 2]; // Take left channel
            }
            
            return monoOutput;
            
        } catch (error) {
            console.error('[TimeStretch] Erreur:', error);
            
            // Fallback: simple conversion without time-stretching
            const result = new Float32Array(pcm16Samples.length);
            for (let i = 0; i < pcm16Samples.length; i++) {
                result[i] = pcm16Samples[i] / 32768.0;
            }
            return result;
        }
    }

    async toggleListening() {
        if (this.isListening) {
            await this.stopListening();
        } else {
            await this.startListening();
        }
    }

    async startListening() {
        try {
            // Clear welcome message if present
            const welcomeMessage = this.messagesContainer.querySelector('.welcome-message');
            if (welcomeMessage) {
                welcomeMessage.remove();
            }

            // Connect to WebSocket server
            this.updateStatus('Connexion...');
            await this.connectWebSocket();

            // Set up audio capture
            await this.setupAudioCapture();

            this.isListening = true;
            this.startTime = Date.now();
            this.startDurationCounter();

            // Update UI
            this.toggleButton.classList.add('listening');
            this.toggleButton.querySelector('.btn-text').textContent = 'Arrêter l\'écoute';
            this.toggleButton.querySelector('.btn-icon').textContent = '⏹️';
            this.updateStatus('En écoute...', 'listening');

        } catch (error) {
            console.error('Erreur lors du démarrage:', error);
            this.addSystemMessage('❌ Impossible de démarrer l\'écoute: ' + error.message);
            await this.stopListening();
        }
    }

    async setupAudioCapture() {
        try {
            // Get microphone stream
            this.mediaStream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    deviceId: this.selectedMicrophone ? { exact: this.selectedMicrophone } : undefined,
                    sampleRate: 24000,
                    channelCount: 1,
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                }
            });

            // Create audio context
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)({
                sampleRate: 24000
            });

            const source = this.audioContext.createMediaStreamSource(this.mediaStream);

            // Create script processor for audio processing
            const bufferSize = 4096;
            const processor = this.audioContext.createScriptProcessor(bufferSize, 1, 1);

            processor.onaudioprocess = (e) => {
                if (!this.isListening || !this.ws || this.ws.readyState !== WebSocket.OPEN) {
                    return;
                }

                const inputData = e.inputBuffer.getChannelData(0);
                
                // Convert Float32 to Int16 (PCM16)
                const pcm16 = new Int16Array(inputData.length);
                for (let i = 0; i < inputData.length; i++) {
                    const s = Math.max(-1, Math.min(1, inputData[i]));
                    pcm16[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
                }

                // Convert to base64
                const base64 = this.arrayBufferToBase64(pcm16.buffer);

                // Send to server
                this.ws.send(JSON.stringify({
                    type: 'audio',
                    audio: base64
                }));
            };

            source.connect(processor);
            processor.connect(this.audioContext.destination);

            this.audioWorkletNode = processor;

        } catch (error) {
            console.error('Erreur lors de la configuration audio:', error);
            throw error;
        }
    }

    arrayBufferToBase64(buffer) {
        let binary = '';
        const bytes = new Uint8Array(buffer);
        const len = bytes.byteLength;
        for (let i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return btoa(binary);
    }

    async stopListening() {
        this.isListening = false;
        this.stopDurationCounter();

        // Stop audio capture
        if (this.audioWorkletNode) {
            this.audioWorkletNode.disconnect();
            this.audioWorkletNode = null;
        }

        if (this.audioContext) {
            await this.audioContext.close();
            this.audioContext = null;
        }

        if (this.mediaStream) {
            this.mediaStream.getTracks().forEach(track => track.stop());
            this.mediaStream = null;
        }

        // Close WebSocket
        if (this.ws && this.ws.readyState === WebSocket.OPEN) {
            this.ws.close();
        }

        // Update UI
        this.toggleButton.classList.remove('listening');
        this.toggleButton.querySelector('.btn-text').textContent = 'Démarrer l\'écoute';
        this.toggleButton.querySelector('.btn-icon').textContent = '🎤';
        this.updateStatus('Prêt', 'ready');

        this.addSystemMessage('⏸️ Écoute arrêtée.');
    }

    startDurationCounter() {
        this.durationInterval = setInterval(() => {
            const elapsed = Math.floor((Date.now() - this.startTime) / 1000);
            const minutes = Math.floor(elapsed / 60).toString().padStart(2, '0');
            const seconds = (elapsed % 60).toString().padStart(2, '0');
            this.durationElement.textContent = `${minutes}:${seconds}`;
        }, 1000);
    }

    stopDurationCounter() {
        if (this.durationInterval) {
            clearInterval(this.durationInterval);
            this.durationInterval = null;
        }
    }

    updateStatus(text, status = '') {
        this.statusText.textContent = text;
        this.statusElement.className = `status ${status}`;
    }

    addUserMessage(text) {
        const message = this.createMessageElement('user', '👤', 'Vous', text);
        this.messagesContainer.appendChild(message);
        this.scrollToBottom();
        this.updateMessageCount();
    }

    addUserMessageBeforeLastAssistant(text) {
        // Find the last assistant message that is currently streaming
        const assistantMessages = this.messagesContainer.querySelectorAll('.message.ai');
        let targetAssistantMessage = null;
        
        // Find the last streaming assistant message
        for (let i = assistantMessages.length - 1; i >= 0; i--) {
            if (assistantMessages[i].dataset.streaming === 'true') {
                targetAssistantMessage = assistantMessages[i];
                break;
            }
        }
        
        // If no streaming message found, check if we already have a user message for this response
        if (!targetAssistantMessage && assistantMessages.length > 0) {
            const lastAssistant = assistantMessages[assistantMessages.length - 1];
            const previousElement = lastAssistant.previousElementSibling;
            
            // If there's already a user message before the last assistant, don't add another
            if (previousElement && previousElement.classList.contains('user')) {
                console.log('[Transcript] User message already exists before assistant response');
                return;
            }
            
            targetAssistantMessage = lastAssistant;
        }
        
        const message = this.createMessageElement('user', '👤', 'Vous', text);
        
        if (targetAssistantMessage) {
            // Insert before the target assistant message
            this.messagesContainer.insertBefore(message, targetAssistantMessage);
        } else {
            // No assistant message yet, just append
            this.messagesContainer.appendChild(message);
        }
        
        this.scrollToBottom();
        this.updateMessageCount();
    }

    addSystemMessage(text) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'message system text-center py-2';
        messageDiv.innerHTML = `
            <div class="inline-block px-4 py-2 bg-blue-50 text-blue-700 text-sm rounded-md border border-blue-200">
                ${text}
            </div>
        `;
        this.messagesContainer.appendChild(messageDiv);
        this.scrollToBottom();
    }

    createMessageElement(type, avatar, sender, text) {
        const messageDiv = document.createElement('div');
        // Keep type class (user/ai) for JavaScript selectors + Tailwind classes
        messageDiv.className = `message ${type} flex gap-3 ${type === 'user' ? 'flex-row-reverse' : ''}`;
        
        const time = new Date().toLocaleTimeString('fr-FR', { 
            hour: '2-digit', 
            minute: '2-digit' 
        });

        const isUser = type === 'user';
        const bgClass = isUser ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-900';
        const timeClass = isUser ? 'text-blue-200' : 'text-gray-500';

        messageDiv.innerHTML = `
            <div class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-lg ${isUser ? 'bg-blue-100' : 'bg-gray-200'}">
                ${avatar}
            </div>
            <div class="max-w-[70%]">
                <div class="flex items-center gap-2 mb-1 ${isUser ? 'flex-row-reverse' : ''}">
                    <span class="text-xs font-semibold text-gray-700">${sender}</span>
                    <span class="text-xs ${timeClass}">${time}</span>
                </div>
                <div class="${bgClass} px-4 py-2 rounded-lg text-sm leading-relaxed message-text">
                    ${this.escapeHtml(text)}
                </div>
            </div>
        `;

        return messageDiv;
    }

    updateMessageCount() {
        this.messageCount++;
        const plural = this.messageCount > 1 ? 's' : '';
        this.messageCountElement.textContent = `${this.messageCount} message${plural}`;
    }

    scrollToBottom() {
        this.messagesContainer.scrollTop = this.messagesContainer.scrollHeight;
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    async sendTextMessage() {
        const text = this.textInput.value.trim();
        
        if (!text) {
            return;
        }

        // Check if we need to connect first
        if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
            try {
                // Clear welcome message if present
                const welcomeMessage = this.messagesContainer.querySelector('.welcome-message');
                if (welcomeMessage) {
                    welcomeMessage.remove();
                }

                this.updateStatus('Connexion...');
                await this.connectWebSocket();
                this.updateStatus('Connecté', 'ready');
            } catch (error) {
                console.error('Erreur lors de la connexion:', error);
                this.addSystemMessage('❌ Impossible de se connecter au serveur');
                return;
            }
        }

        // Add user message to UI
        this.addUserMessage(text);

        // Clear the input
        this.textInput.value = '';

        // Send to server
        try {
            this.ws.send(JSON.stringify({
                type: 'text',
                text: text
            }));
            
            this.updateStatus('Envoi du message...', 'listening');
        } catch (error) {
            console.error('Erreur lors de l\'envoi du message:', error);
            this.addSystemMessage('❌ Erreur lors de l\'envoi du message');
        }
    }

    toggleAudioPlayback() {
        this.audioEnabled = !this.audioEnabled;
        
        if (this.audioEnabled) {
            // Audio enabled
            this.audioIcon.textContent = '🔊';
            this.audioStatusText.textContent = 'Audio activé';
            this.toggleAudioButton.classList.remove('bg-gray-600', 'hover:bg-gray-700');
            this.toggleAudioButton.classList.add('bg-green-600', 'hover:bg-green-700');
            console.log('[Audio] ✅ Lecture audio activée');
        } else {
            // Audio disabled - stop current playback and clear queue
            this.audioIcon.textContent = '🔇';
            this.audioStatusText.textContent = 'Audio désactivé';
            this.toggleAudioButton.classList.remove('bg-green-600', 'hover:bg-green-700');
            this.toggleAudioButton.classList.add('bg-gray-600', 'hover:bg-gray-700');
            
            console.log('[Audio] 🔇 Lecture audio désactivée');
            
            // Stop any currently playing audio
            if (this.isPlayingAudio) {
                this.stopAudioPlayback();
            }
        }
    }
}

// Initialize the app when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new ChatApp();
});
