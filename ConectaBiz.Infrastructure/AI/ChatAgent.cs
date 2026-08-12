using ConectaBiz.Application.Interfaces;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Infrastructure.AI.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI; // <-- El conector robusto
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.AI
{
    public class ChatAgent : IChatAgent
    {
        private readonly IChatCompletionService _chatService;
        private readonly Kernel _kernel;
        private readonly List<string> _listaModelosAIntentar;

        public ChatAgent(
            IChatCompletionService chatService,
            IConfiguration configuration,
            Kernel kernel,
            EmpresaSkill empresaSkill,
            TicketSkill ticketSkill)
        {
            _chatService = chatService;
            _kernel = kernel;

            // Inyectamos los skills ya construidos con sus servicios DENTRO de la petición actual
            _kernel.Plugins.AddFromObject(empresaSkill);
            _kernel.Plugins.AddFromObject(ticketSkill);

            var activeProvider = configuration["AI:ActiveProvider"] ?? "Google";
            var providerPath = $"AI:Providers:{activeProvider}";

            var modeloPrincipal = configuration[$"{providerPath}:ModelId"];
            var modelosFallback = configuration.GetSection($"{providerPath}:FallbackModels").Get<List<string>>() ?? new List<string>();

            _listaModelosAIntentar = new List<string>();

            if (!string.IsNullOrEmpty(modeloPrincipal))
                _listaModelosAIntentar.Add(modeloPrincipal);

            _listaModelosAIntentar.AddRange(modelosFallback);
        }

        public async Task<ChatResponseDto> PreguntarAsync(string mensaje)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(
                "Eres un asistente corporativo de mesa de ayuda. Responde a las dudas del usuario usando los datos de la base de datos.\n" +
                "IMPORTANTE: Tu respuesta final DEBE SER OBLIGATORIAMENTE un objeto JSON válido con la siguiente estructura:\n" +
                "{\n" +
                "  \"resumen\": \"Texto de tu respuesta en lenguaje natural\",\n" +
                "  \"datosExtra\": [ arreglo con los objetos relevantes o null si no hay ]\n" +
                "}");
            chatHistory.AddUserMessage(mensaje);

            // 1. Creamos una caja para coleccionar los errores como objetos
            var listaErrores = new List<object>();

            foreach (var modeloId in _listaModelosAIntentar)
            {
                try
                {
                    var settings = new OpenAIPromptExecutionSettings
                    {
                        Temperature = 0.7,
                        MaxTokens = 800,
                        ModelId = modeloId,
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    };

                    var respuesta = await _chatService.GetChatMessageContentAsync(chatHistory, settings, _kernel);

                    var metadatos = new Dictionary<string, object>();
                    if (respuesta.Metadata != null)
                    {
                        foreach (var kvp in respuesta.Metadata)
                        {
                            if (kvp.Value != null)
                                metadatos.Add(kvp.Key, kvp.Value);
                        }
                    }

                    string rawContent = respuesta.Content ?? "{}";
                    rawContent = rawContent.Replace("```json", "").Replace("```", "").Trim();

                    object respuestaEstructurada;
                    try
                    {
                        respuestaEstructurada = System.Text.Json.JsonSerializer.Deserialize<object>(rawContent) ?? new { mensaje = "No response" };
                    }
                    catch
                    {
                        // Fallback si la IA se equivocó y no devolvió un JSON válido
                        respuestaEstructurada = new { resumen = rawContent, datosExtra = (object)null };
                    }

                    return new ChatResponseDto
                    {
                        Respuesta = respuestaEstructurada,
                        ModeloUsado = modeloId,
                        MetadatosAdicionales = metadatos
                    };
                }
                catch (Microsoft.SemanticKernel.HttpOperationException httpEx)
                {
                    // Parseamos el string JSON de Google a un objeto real
                    object detalleParseado = null;
                    if (!string.IsNullOrEmpty(httpEx.ResponseContent))
                    {
                        try { detalleParseado = System.Text.Json.JsonSerializer.Deserialize<object>(httpEx.ResponseContent); }
                        catch { detalleParseado = httpEx.ResponseContent; }
                    }

                    listaErrores.Add(new 
                    { 
                        modelo = modeloId, 
                        mensaje = httpEx.Message, 
                        detalleGoogle = detalleParseado 
                    });
                }
                catch (Exception ex)
                {
                    // 2b. Si es cualquier otro error (código C# roto interno)
                    listaErrores.Add(new { modelo = modeloId, mensaje = ex.Message });
                    System.Diagnostics.Debug.WriteLine($"[FALLBACK] Error General: {ex.Message}");
                }
            }

            // 3. En lugar de lanzar una excepción, devolvemos el DTO con la lista de errores
            return new ChatResponseDto
            {
                Respuesta = "Lo siento, todos los servidores de IA están saturados en este momento.",
                ModeloUsado = "Ninguno",
                ErroresFallback = listaErrores
            };
        }



    }
}
