// WebLLM integration for SpinnerNet
// Based on working prototype from Examples/02-WebLLM-Client-Side.md

/**
 * SpinnerNetWebLLM - Client-side LLM for privacy-first AI processing
 * Uses working patterns from the Examples directory
 */
class SpinnerNetWebLLM {
    constructor() {
        this.engine = null;
        this.isInitialized = false;
        this.isLoading = false;
        // Using small, fast model from prebuilt list (~1GB, loads in 30-60 seconds)
        this.modelId = "Llama-3.2-1B-Instruct-q4f32_1-MLC";
        this.dotNetHelper = null;
        this.initializationPromise = null;
    }

    /**
     * Initialize WebLLM engine with WebGPU support
     */
    async initialize(dotNetHelper) {
        if (this.initializationPromise) {
            return this.initializationPromise;
        }

        this.initializationPromise = this._performInitialization(dotNetHelper);
        return this.initializationPromise;
    }

    async _performInitialization(dotNetHelper) {
        try {
            this.dotNetHelper = dotNetHelper;
            this.isLoading = true;
            
            await this._notifyStatus("Loading WebLLM library...");
            console.log("🚀 Initializing WebLLM with model:", this.modelId);
            
            // Check WebGPU support (non-blocking)
            if (!navigator.gpu) {
                console.warn("⚠️ WebGPU not supported, falling back to CPU");
                await this._notifyStatus("WebGPU not available, using CPU...");
            } else {
                console.log("✅ WebGPU support detected");
                await this._notifyStatus("WebGPU detected, initializing...");
            }
            
            // Import WebLLM using the working pattern from examples
            const { CreateMLCEngine } = await import("https://esm.run/@mlc-ai/web-llm");
            
            await this._notifyStatus("Creating engine with working model...");
            console.log("📥 Loading model... This may take a few minutes on first run.");
            
            await this._notifyStatus("Loading model from prebuilt list...");
            
            // Create engine with prebuilt model (no custom appConfig needed)
            this.engine = await CreateMLCEngine(
                this.modelId,
                { 
                    initProgressCallback: (progress) => {
                        console.log(`Loading progress: ${Math.round(progress.progress * 100)}%`);
                        this._notifyProgress(Math.round(progress.progress * 100));
                    }
                }
            );
            
            this.isLoading = false;
            this.isInitialized = true;
            
            await this._notifyStatus("WebLLM engine ready!");
            console.log("✅ WebLLM initialized successfully");
            
            // Test the engine
            await this._testEngine();
            
            return true;

        } catch (error) {
            this.isLoading = false;
            this.isInitialized = false;
            const errorMsg = `WebLLM initialization failed: ${error.message}`;
            await this._notifyError(errorMsg);
            console.error("❌ Failed to initialize WebLLM:", error);
            return false;
        }
    }

    /**
     * Test engine functionality
     */
    async _testEngine() {
        try {
            const testResponse = await this.engine.chat.completions.create({
                messages: [{ role: "user", content: "Hello" }],
                temperature: 0.7,
                max_tokens: 10
            });
            
            console.log("🧪 Engine test successful:", testResponse.choices[0].message.content);
        } catch (error) {
            console.error("❌ Engine test failed:", error);
        }
    }

    async generateResponse(prompt, options = {}) {
        if (!this.isInitialized || !this.engine) {
            throw new Error("WebLLM engine not ready");
        }

        try {
            // Age-adaptive system prompts based on SpinnerNet's persona patterns
            const systemPrompt = this._getSystemPrompt(options.userAge || 18);
            
            const startTime = performance.now();
            
            const completion = await this.engine.chat.completions.create({
                messages: [
                    { role: "system", content: systemPrompt },
                    { role: "user", content: prompt }
                ],
                temperature: options.temperature || 0.7,
                max_tokens: options.maxTokens || 200,
                top_p: options.topP || 0.9,
                frequency_penalty: 0.1,
                presence_penalty: 0.1
            });

            const endTime = performance.now();
            const latency = endTime - startTime;
            
            console.log(`🚀 Response generated in ${latency.toFixed(0)}ms`);

            return completion.choices[0].message.content;

        } catch (error) {
            await this._notifyError(`Response generation failed: ${error.message}`);
            throw error;
        }
    }

    _getSystemPrompt(userAge) {
        // Age-adaptive system prompts following SpinnerNet's persona patterns
        const basePrompt = "You are a helpful AI assistant for SpinnerNet, designed to help users create personalized AI personas. ";
        
        if (userAge < 13) {
            return basePrompt + "Use simple, friendly language appropriate for children. Focus on creativity, learning, and fun activities. Always maintain a positive, encouraging tone.";
        } else if (userAge < 18) {
            return basePrompt + "Use engaging language appropriate for teenagers. Consider their interests in technology, social connections, and future planning. Be supportive and understanding.";
        } else if (userAge < 65) {
            return basePrompt + "Use professional, clear language appropriate for adults. Focus on practical solutions, productivity, and goal achievement. Be direct and informative.";
        } else {
            return basePrompt + "Use respectful, patient language appropriate for seniors. Focus on wisdom sharing, life experience, and thoughtful reflection. Be warm and considerate.";
        }
    }

    async _notifyStatus(message) {
        try {
            if (this.dotNetHelper) {
                await this.dotNetHelper.invokeMethodAsync('OnWebLLMStatusUpdate', message);
            }
        } catch (error) {
            console.error("Failed to notify status:", error);
        }
    }

    async _notifyProgress(progress) {
        try {
            if (this.dotNetHelper) {
                const progressObj = {
                    progress: progress / 100.0,
                    text: `Loading: ${progress}%`,
                    timeElapsed: 0
                };
                await this.dotNetHelper.invokeMethodAsync('OnWebLLMProgress', progressObj);
            }
        } catch (error) {
            console.error("Failed to notify progress:", error);
        }
    }

    async _notifyError(error) {
        try {
            if (this.dotNetHelper) {
                await this.dotNetHelper.invokeMethodAsync('OnWebLLMError', error);
            }
        } catch (error) {
            console.error("Failed to notify error:", error);
        }
    }

    isEngineReady() {
        return this.isInitialized && this.engine !== null;
    }

    dispose() {
        if (this.engine) {
            this.engine = null;
        }
        this.isInitialized = false;
        this.isLoading = false;
        this.dotNetHelper = null;
    }
}

// Global instance
let webllmInstance = null;

// Create spinnerNetWebLLM namespace for .NET interop using working patterns
window.spinnerNetWebLLM = {
    initialize: async function(dotNetHelper, sessionId) {
        try {
            if (!webllmInstance) {
                webllmInstance = new SpinnerNetWebLLM();
            }
            return await webllmInstance.initialize(dotNetHelper);
        } catch (error) {
            console.error("SpinnerNet WebLLM initialization failed:", error);
            return false;
        }
    },

    generatePersonaResponse: async function(prompt, options) {
        try {
            if (!webllmInstance) {
                throw new Error("WebLLM not initialized");
            }
            return await webllmInstance.generateResponse(prompt, options);
        } catch (error) {
            console.error("SpinnerNet WebLLM generation failed:", error);
            throw error;
        }
    },

    getPerformanceMetrics: function() {
        if (!webllmInstance) {
            return {
                isInitialized: false,
                isLoading: false,
                currentStatus: "Not initialized",
                lastError: "WebLLM instance not created"
            };
        }
        
        return {
            isInitialized: webllmInstance.isInitialized,
            isLoading: webllmInstance.isLoading,
            currentStatus: webllmInstance.isInitialized ? "Ready" : "Not ready",
            lastError: null
        };
    },

    dispose: function() {
        if (webllmInstance) {
            webllmInstance.dispose();
            webllmInstance = null;
        }
    }
};

// Also expose as window.webLLM for compatibility with examples
window.webLLM = {
    async init() {
        if (!webllmInstance) {
            webllmInstance = new SpinnerNetWebLLM();
        }
        return await webllmInstance.initialize(null);
    },
    
    async generateInterviewResponse(userMessage, responseCount, totalResponses) {
        if (!webllmInstance) {
            throw new Error("WebLLM not initialized");
        }
        return await webllmInstance.generateResponse(userMessage, {});
    },
    
    getStatus() {
        return webllmInstance ? {
            isInitialized: webllmInstance.isInitialized,
            isLoading: webllmInstance.isLoading,
            hasWebGPU: !!navigator.gpu
        } : { isInitialized: false, isLoading: false, hasWebGPU: false };
    },
    
    isReady() {
        return webllmInstance ? webllmInstance.isEngineReady() : false;
    }
};

console.log("🎯 WebLLM integration loaded (compatible with working examples)");