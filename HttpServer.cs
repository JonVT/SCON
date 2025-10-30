using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace SCON
{
    public class HttpServerManager
    {
        private HttpListener listener;
        private Thread listenerThread;
        private bool isRunning = false;
        private string host;
        private int port;

        public void Initialize(string host, int port)
        {
            this.host = host;
            this.port = port;
            StartListening();
        }

        private void StartListening()
        {
            try
            {
                // Build a list of candidate prefixes to maximize cross-platform compatibility
                var candidates = new List<string>();

                string h = (host ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(h))
                {
                    h = "localhost";
                }

                bool anyHost = h == "*" || h == "+" || h == "0.0.0.0";
                if (anyHost)
                {
                    // Try common wildcard forms first, then explicit binds
                    candidates.Add($"http://*:{port}/");
                    candidates.Add($"http://+:{port}/");
                    candidates.Add($"http://0.0.0.0:{port}/");
                    candidates.Add($"http://127.0.0.1:{port}/");
                }
                else
                {
                    candidates.Add($"http://{h}:{port}/");
                    // Provide a localhost fallback when configured as localhost
                    if (string.Equals(h, "localhost", StringComparison.OrdinalIgnoreCase))
                        candidates.Add($"http://127.0.0.1:{port}/");
                }

                Exception lastError = null;
                string startedPrefix = null;

                foreach (var prefix in candidates)
                {
                    try
                    {
                        listener = new HttpListener();
                        listener.Prefixes.Add(prefix);
                        listener.Start();
                        isRunning = true;
                        startedPrefix = prefix;
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                        try
                        {
                            // Clean up this attempt before trying the next
                            if (listener != null)
                            {
                                listener.Close();
                            }
                        }
                        catch { }
                        listener = null;
                    }
                }

                if (!isRunning)
                {
                    throw new InvalidOperationException($"Failed to start HTTP listener on any prefix. Last error: {lastError?.Message}");
                }

                listenerThread = new Thread(Listen)
                {
                    IsBackground = true
                };
                listenerThread.Start();

                Plugin.Log.LogInfo($"HTTP Listener started on {startedPrefix}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to start HTTP listener: {ex.Message}");
                throw;
            }
        }

        private void Listen()
        {
            while (isRunning && listener != null && listener.IsListening)
            {
                try
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem((_) => HandleRequest(context));
                }
                catch (HttpListenerException ex)
                {
                    if (isRunning)
                    {
                        Plugin.Log.LogError($"Listener error: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Unexpected error: {ex.Message}");
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // Set CORS headers
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

                // Handle OPTIONS preflight request
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                    response.Close();
                    return;
                }

                // Check authentication
                if (!IsAuthenticated(request))
                {
                    string errorResponse = "{\"success\":false,\"message\":\"Unauthorized: Invalid or missing API key\"}";
                    byte[] errorBuffer = Encoding.UTF8.GetBytes(errorResponse);
                    response.StatusCode = 401;
                    response.ContentType = "application/json";
                    response.ContentLength64 = errorBuffer.Length;
                    response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
                    response.Close();
                    Plugin.Log.LogWarning($"Unauthorized access attempt from {request.RemoteEndPoint}");
                    return;
                }

                string responseString = "";
                int statusCode = 200;

                if (request.Url.AbsolutePath == "/command" && request.HttpMethod == "POST")
                {
                    responseString = HandleCommandRequest(request);
                }
                else if (request.Url.AbsolutePath == "/gameinfo" && request.HttpMethod == "GET")
                {
                    responseString = HandleGameInfoRequest();
                }
                else if (request.Url.AbsolutePath == "/health" && request.HttpMethod == "GET")
                {
                    responseString = "{\"status\":\"ok\"}";
                }
                else
                {
                    statusCode = 404;
                    responseString = "{\"success\":false,\"message\":\"Endpoint not found\"}";
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "application/json";
                response.StatusCode = statusCode;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error handling request: {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch { }
            }
        }

        private bool IsAuthenticated(HttpListenerRequest request)
        {
            // Get configured API key
            string configuredApiKey = Plugin.ServerApiKey?.Value?.Trim();
            bool isLocalhost = request.RemoteEndPoint.Address.ToString() == "127.0.0.1" || 
                              request.RemoteEndPoint.Address.ToString() == "::1";

            // If no API key is configured and request is from localhost, allow it
            if (string.IsNullOrEmpty(configuredApiKey) && isLocalhost)
            {
                return true;
            }

            // If API key is configured or request is not from localhost, require authentication
            if (!string.IsNullOrEmpty(configuredApiKey))
            {
                // Check Authorization header
                string authHeader = request.Headers["Authorization"];
                if (string.IsNullOrEmpty(authHeader))
                {
                    return false;
                }

                // Support "Bearer <token>" or just "<token>"
                string providedKey = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader.Substring(7).Trim()
                    : authHeader.Trim();

                return providedKey == configuredApiKey;
            }

            // Non-localhost with no API key configured = deny
            return false;
        }

        private string HandleGameInfoRequest()
        {
            try
            {
                var gameInfo = GameInfoCollector.GetGameInfo();
                return gameInfo;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error getting game info: {ex.Message}");
                return $"{{\"success\":false,\"message\":\"Error: {JsonEscape(ex.Message)}\"}}";
            }
        }

        private string HandleCommandRequest(HttpListenerRequest request)
        {
            try
            {
                string body;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    body = reader.ReadToEnd();
                }

                // Parse JSON manually (simple parsing)
                string command = ExtractCommandFromJson(body);

                if (string.IsNullOrEmpty(command))
                {
                    return "{\"success\":false,\"message\":\"No command provided\"}";
                }

                // Execute command immediately via Unity main thread dispatcher
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    try
                    {
                        CommandExecutor.ExecuteCommand(command);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError($"Failed to execute command '{command}': {ex.Message}");
                    }
                });

                return "{\"success\":true,\"message\":\"Command queued for execution\"}";
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error processing command: {ex.Message}");
                return $"{{\"success\":false,\"message\":\"Error: {JsonEscape(ex.Message)}\"}}";
            }
        }

        private string ExtractCommandFromJson(string json)
        {
            // Simple JSON parsing for {"command":"value"}
            try
            {
                int commandStart = json.IndexOf("\"command\"");
                if (commandStart == -1) return null;

                int valueStart = json.IndexOf(":", commandStart);
                if (valueStart == -1) return null;

                int quoteStart = json.IndexOf("\"", valueStart);
                if (quoteStart == -1) return null;

                int quoteEnd = json.IndexOf("\"", quoteStart + 1);
                if (quoteEnd == -1) return null;

                return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
            }
            catch
            {
                return null;
            }
        }

        private string JsonEscape(string str)
        {
            return str.Replace("\\", "\\\\")
                     .Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t");
        }

        public void Stop()
        {
            isRunning = false;

            if (listener != null && listener.IsListening)
            {
                listener.Stop();
                listener.Close();
            }

            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join(1000);
            }

            Plugin.Log.LogInfo("HTTP server stopped");
        }
    }
}
