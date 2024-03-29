﻿using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Linq;

namespace Discord_ServiceManager
{
    class Program
    {
        private readonly DiscordSocketClient _client;
        string mention;
        readonly Config myConf;

        // Discord.Net heavily utilizes TAP for async, so we create
        // an asynchronous context from the beginning.
        static void Main(string[] args)
        {
            string arg;
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated)
            {
                throw new UnauthorizedAccessException("This programs require admin permissions ! Managing services is an admin privilege");
            }
            foreach (string s in args)
            {
                if (s == "--help" || s == "-h")
                {
                    Console.WriteLine("Usage: Discord-ServiceManager [config]");
                    Console.WriteLine("If not provided, the bot will search for \"config.toml\"");
                    Console.WriteLine("The config file is a TOML file, with 1 namespace (\"config\"), containing 2 values, \"token\" and \"command\"");
                    return;
                }
            }
            arg = (args.Length == 0 ? "config.toml" : args[0]);

            new Program(arg).MainAsync().GetAwaiter().GetResult();
        }

        public Program(string fname)
        {
            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            _client = new DiscordSocketClient();

            myConf = ConfigGetter.GetConfigFromTOML(fname);
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task MainAsync()
        {
            //Console.WriteLine(string.Format("Token is {0}"), myConf.token);
            // Tokens should be considered secret data, and never hard-coded.
            await _client.LoginAsync(TokenType.Bot, myConf.token);
            await _client.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            if (myConf.command == "@")
            {
                mention = null;
            }
            else
            {
                mention = myConf.command;
            }
            Console.WriteLine($"Mention is ${mention}");
            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (mention == null && message.MentionedUsers.Select(u => u.Id).Contains(_client.CurrentUser.Id))
            {
                MessageProcess.Process(message, myConf);
                return;
            } else if (message.Content.StartsWith(mention))
            {
                MessageProcess.Process(message, myConf);
                return;
            }
        }
    }
}
