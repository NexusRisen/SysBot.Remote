using Discord.WebSocket;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Discord;
using System.Threading;
using SysbotMacro;
using SysBot.Base;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.IO;


namespace SysbotMacro.Discord
{
    internal class MessageHandler
    {
        private const int DEFAULT_SWITCH_PORT = 6000;
        private const int PING_TIMEOUT_MS = 2000;
        private const string COMMAND_PREFIX = "!";
        
        private readonly List<Dictionary<string, object>> _ipDict;
        private readonly List<Dictionary<string, object>> _macroDict;

        public MessageHandler(List<Dictionary<string, object>> ipDict, List<Dictionary<string, object>> macroDict)
        {
            _ipDict = ipDict;
            _macroDict = macroDict;
        }

        public byte[] PixelPeek()
        {
            return SwitchCommand.PixelPeek();
        }
        public async Task HandleMessage(SocketMessage message, Func<string, Task> sendResponse)
        {
            // Split the message into words
            var parts = message.Content.Split(' ');
            var firstPart = parts.FirstOrDefault();


            // Check if the first word starts with the command prefix
            if (firstPart != null && firstPart.StartsWith(COMMAND_PREFIX))
            {
                var command = firstPart.Substring(1); // Remove prefix

                // COMMANDS //

                if (command == "ping")
                {
                    await sendResponse("Pong!");
                }
                // command: to message in the channel with all macros and switches
                else if (command == "data")
                {
                    if (_macroDict == null || !_macroDict.Any() || _ipDict == null || !_ipDict.Any())
                    {
                        await sendResponse("One or both of the dictionaries are either null or empty.");
                        return;
                    }

                    var embed = new EmbedBuilder
                    {
                        Title = "Macro and Switch IP Data",
                        Color = global::Discord.Color.Blue
                    };

                    string macroField = "";
                    foreach (var dict in _macroDict)
                    {
                        foreach (var kvp in dict)
                        {
                            macroField += $"{kvp.Key}: **{kvp.Value}**, ";
                        }
                        macroField = macroField.TrimEnd(',', ' ') + "\n";
                    }
                    embed.AddField("Macros", macroField);

                    string ipField = "";
                    foreach (var dict in _ipDict)
                    {
                        foreach (var kvp in dict)
                        {
                            if (kvp.Key == "IsChecked") continue;
                            ipField += $"{kvp.Key}: **{kvp.Value}**, ";
                        }
                        ipField = ipField.TrimEnd(',', ' ') + "\n";
                    }
                    embed.AddField("Switch IPs", ipField);

                    await ((ISocketMessageChannel)message.Channel).SendMessageAsync(embed: embed.Build());
                }
                // command: user created macro commands
                else if (_macroDict.Any(dict => dict.ContainsKey("Name") && dict["Name"].ToString().Equals(command, StringComparison.OrdinalIgnoreCase)))
                {

                    // Key is empty maybe manual deletion of the key in the JSON?
                    var switchKey = parts.Length > 1 ? parts[1] : string.Empty;
                    if (string.IsNullOrEmpty(switchKey))
                    {
                        await sendResponse("Missing Switch Key for Macro");
                        return;
                    }

                    var macroDictEntry = _macroDict.First(dict => dict.ContainsKey("Name") && dict["Name"].ToString().Equals(command, StringComparison.OrdinalIgnoreCase));
                    var macro = macroDictEntry["Macro"].ToString();

                    var switchIpDict = _ipDict.FirstOrDefault(dict => dict.ContainsKey("SwitchName") && dict["SwitchName"].ToString().Equals(switchKey, StringComparison.OrdinalIgnoreCase));
                    var switchIp = switchIpDict != null ? switchIpDict["IPAddress"].ToString() : null;

                    // Handle dumb crap like JSON edits to bypass check in form1
                    if (switchIp == null || !IPAddress.TryParse(switchIp, out _))
                    {
                        await sendResponse($"No valid Switch IP found for {switchKey}");
                        return;
                    }

                    using (var pingSender = new Ping())
                    {
                        // Ping the IP address
                        PingReply reply = await pingSender.SendPingAsync(switchIp, PING_TIMEOUT_MS);

                        // Check the status and if down, exit to prevent hanging
                        if (reply.Status != IPStatus.Success)
                        {
                            await sendResponse($"Switch IP {switchIp} is not reachable");
                            return;
                        }
                    }

                    // Execute
                    await sendResponse($"Executing macro {command} on Switch {switchKey}");
                    var config = new SwitchConnectionConfig
                    {
                        IP = switchIp,
                        Port = DEFAULT_SWITCH_PORT,
                        Protocol = SwitchProtocol.WiFi
                    };

                    var bot = new Bot(config);
                    try
                    {
                        bot.Connect();
                        var cancellationToken = new CancellationToken();
                        await bot.ExecuteCommands(macro, () => false, cancellationToken);
                        await sendResponse($"Executed macro {command} on Switch {switchKey}");
                    }
                    catch (Exception ex)
                    {
                        await sendResponse($"Error executing macro: {ex.Message}");
                    }
                    finally
                    {
                        try { bot.Disconnect(); } catch { }
                    }
                }
                // command: to check the status of all switches
                else if (command == "status")
                {

                    StringBuilder embedDescription = new StringBuilder();
                    foreach (var switchDict in _ipDict)
                    {
                        // Extract the name and the IP.
                        var switchName = switchDict.ContainsKey("SwitchName") ? switchDict["SwitchName"].ToString() : "Unknown";
                        var ipAddress = switchDict.ContainsKey("IPAddress") ? switchDict["IPAddress"].ToString() : "Unknown";

                        // Initialize a new instance of the Ping class.
                        using (var pingSender = new Ping())
                        {
                            try
                            {
                                PingReply reply = await pingSender.SendPingAsync(ipAddress, PING_TIMEOUT_MS);
                                string status = reply.Status == IPStatus.Success ? "Up" : "Down";
                                embedDescription.AppendLine($"Name: {switchName}, IP: {ipAddress}, Status: {status}");
                            }
                            catch (Exception ex)
                            {
                                embedDescription.AppendLine($"Name: {switchName}, IP: {ipAddress}, Status: Error - {ex.Message}");
                            }
                        }
                    }

                    // Create embed message with Discord.Net's EmbedBuilder.
                    var embed = new EmbedBuilder
                    {
                        Title = "Switch Status Check",
                        Description = embedDescription.ToString(),
                        Color = global::Discord.Color.Green  // You can change the color based on your preferences.
                    };

                    // Send the embed.
                    await ((ISocketMessageChannel)message.Channel).SendMessageAsync(embed: embed.Build());
                }
                //command: use sysbot.base and pull screen capture from switch. command is peek switchname
                else if (command == "peek")
                {
                    // Retrieve the name of the Switch
                    var switchKey = parts.Length > 1 ? parts[1] : string.Empty;
                    if (string.IsNullOrEmpty(switchKey))
                    {
                        await sendResponse("Missing Switch Key for peek");
                        return;
                    }

                    // Find the IP associated with the Switch name
                    var switchIpDict = _ipDict.FirstOrDefault(dict => dict.ContainsKey("SwitchName") && dict["SwitchName"].ToString().Equals(switchKey, StringComparison.OrdinalIgnoreCase));
                    var switchIp = switchIpDict != null ? switchIpDict["IPAddress"].ToString() : null;

                    // Validate the IP address
                    if (switchIp == null || !IPAddress.TryParse(switchIp, out _))
                    {
                        await sendResponse($"No valid Switch IP found for {switchKey}");
                        return;
                    }

                    // Connect to the Switch
                    var config = new SwitchConnectionConfig
                    {
                        IP = switchIp,
                        Port = DEFAULT_SWITCH_PORT,
                        Protocol = SwitchProtocol.WiFi
                    };
                    var bot = new Bot(config);
                    try
                    {
                        bot.Connect();
                        byte[]? imageBytes = bot.PixelPeek();
                        
                        if (imageBytes == null || imageBytes.Length == 0)
                        {
                            await sendResponse("Failed to capture the screen - no data received.");
                            return;
                        }

                        string hexString = Encoding.UTF8.GetString(imageBytes);

                        // Convert the hexadecimal string back to a byte array
                        byte[] actualImageBytes = Enumerable.Range(0, hexString.Length)
                                             .Where(x => x % 2 == 0)
                                             .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                                             .ToArray();

                        // Send the image as an attachment in Discord
                        if (actualImageBytes.Length > 0)
                        {
                            using (var stream = new MemoryStream(actualImageBytes))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                var embed = new EmbedBuilder();
                                embed.ImageUrl = "attachment://screenshot.jpg";
                                await ((ISocketMessageChannel)message.Channel).SendFileAsync(stream, "screenshot.jpg", $"Here's the screenshot of {switchKey}", embed: embed.Build());
                            }
                        }
                        else
                        {
                            await sendResponse("Failed to process screen capture data.");
                        }
                    }
                    catch (Exception ex)
                    {
                        await sendResponse($"Error capturing screenshot: {ex.Message}");
                    }
                    finally
                    {
                        try { bot.Disconnect(); } catch { }
                    }




                }


            }
        }
    }
}
