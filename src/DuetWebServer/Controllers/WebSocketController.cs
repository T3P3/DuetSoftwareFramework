﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using DuetAPI.Connection;
using DuetAPIClient;
using DuetAPIClient.Exceptions;
using Microsoft.Extensions.Logging;

namespace DuetWebServer.Controllers
{
    /// <summary>
    /// This class takes care of WebSocket-based communication.
    /// At the moment it is only used to provide continuous machine model updates.
    /// </summary>
    public static class WebSocketController
    {
        private static readonly byte[] PONG = Encoding.UTF8.GetBytes("PONG\n");

        /// <summary>
        /// Deal with a newly opened WebSocket.
        /// A client may receive one of the WS codes: (1001) Endpoint unavailable (1003) Invalid command (1011) Internal error
        /// </summary>
        /// <param name="webSocket">WebSocket connection</param>
        /// <param name="socketPath">Path to the UNIX socket</param>
        /// <param name="logger">Logger instance</param>
        /// <returns>Asynchronous task</returns>
        public static async Task Process(WebSocket webSocket, string socketPath, ILogger logger)
        {
            using (SubscribeConnection connection = new SubscribeConnection())
            {
                // 1. Authentication
                // TODO

                // 2. Connect to DCS
                try
                {
                    await connection.Connect(SubscriptionMode.Patch, socketPath, Program.CancelSource.Token);
                }
                catch (AggregateException ae) when (ae.InnerException is IncompatibleVersionException)
                {
                    logger.LogError($"[{nameof(WebSocketController)}] Incompatible DCS version");
                    await CloseConnection(webSocket, WebSocketCloseStatus.InternalServerError, "Incompatible DCS version");
                    return;
                }
                catch (Exception)
                {
                    logger.LogError($"[{nameof(WebSocketController)}] DCS is unavailable");
                    await CloseConnection(webSocket, WebSocketCloseStatus.EndpointUnavailable, "DCS is unavailable");
                    return;
                }

                // 3. Keep the client up-to-date
                try
                {
                    // 3a. Fetch full model copy and send it over initially
                    string json = await connection.GetSerializedMachineModel();
                    await webSocket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, Program.CancelSource.Token);

                    // 3b. Keep sending updates to the client and wait for "OK" after each update
                    do
                    {
                        // 3c. Wait for response from the client
                        byte[] receivedBytes = new byte[8];
                        await webSocket.ReceiveAsync(receivedBytes, Program.CancelSource.Token);
                        string receivedData = Encoding.UTF8.GetString(receivedBytes);

                        // 3d. Deal with PING requests
                        if (receivedData.Equals("PING\n", StringComparison.InvariantCultureIgnoreCase))
                        {
                            await webSocket.SendAsync(PONG, WebSocketMessageType.Text, true, Program.CancelSource.Token);
                            continue;
                        }

                        // 3e. Check if the client has acknowledged the received data
                        if (!receivedData.Equals("OK\n", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Terminate the connection if anything else than "OK" is received
                            await CloseConnection(webSocket, WebSocketCloseStatus.InvalidMessageType, "Invalid command");
                            break;
                        }

                        // 3f. Check for another update and send it to the client
                        json = await connection.GetSerializedMachineModel();
                        await webSocket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, Program.CancelSource.Token);
                    } while (webSocket.State == WebSocketState.Open);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "WebSocket terminated with an exception");
                    await CloseConnection(webSocket, WebSocketCloseStatus.InternalServerError, e.Message);
                }
            }
        }

        private static async Task CloseConnection(WebSocket webSocket, WebSocketCloseStatus status, string message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(status, message, Program.CancelSource.Token);
            }
        }
    }
}
