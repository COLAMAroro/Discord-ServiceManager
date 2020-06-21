using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Discord_ServiceManager
{
    static class ServiceManager
    {
        private static readonly string[] commands { 
            "status",
        };
        public static async void ProcessMessage(SocketMessage message, Config config)
        {
            List<string> args = message.Content.Split(' ').ToList();
            if (args.Count != 3)
            {
                await message.Channel.SendMessageAsync("Unknown pattern");
            }

        }
    }
}
