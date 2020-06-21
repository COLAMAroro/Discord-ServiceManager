using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;

namespace Discord_ServiceManager
{
    static class MessageProcess
    {
        private static async void status(SocketMessage message, Service service)
        {
            ServiceController sc = service.controller;
            string status_str;
            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    status_str = "running";
                    break;
                case ServiceControllerStatus.Stopped:
                    status_str = "stopped";
                    break;
                default:
                    status_str = "intermediate state. Contact server hoster if this stays after a minute";
                    break;
            }
            _ = await message.Channel.SendMessageAsync(string.Format("{0} is {1}", service.display_name, status_str));
            return;
        }

        private static async void stop(SocketMessage message, Service service)
        {
            ServiceController sc = service.controller;
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                _ = await message.Channel.SendMessageAsync(string.Format("{0} is already stopped", service.display_name));
                return;
            }
            sc.Stop();
            while (sc.Status != ServiceControllerStatus.Stopped)
            {
                Thread.Sleep(1000);
                sc.Refresh();
            }
            _ = await message.Channel.SendMessageAsync(string.Format("{0} sucessfully stopped", service.display_name));
        }

        private static async void start(SocketMessage message, Service service)
        {
            ServiceController sc = service.controller;
            if (sc.Status == ServiceControllerStatus.Running)
            {
                _ = await message.Channel.SendMessageAsync(string.Format("{0} is already running", service.display_name));
                return;
            }
            sc.Start();
            int counter = 0;
            while (sc.Status != ServiceControllerStatus.Running)
            {
                if (counter == 10)
                    break;
                Thread.Sleep(1000);
                sc.Refresh();
                counter += 1;
            }
            if (counter == 10 && sc.Status != ServiceControllerStatus.Running)
                _ = await message.Channel.SendMessageAsync(string.Format("{0} did not start sucessfully. Contact server hoster", service.display_name));
            else
                _ = await message.Channel.SendMessageAsync(string.Format("{0} sucessfully started", service.display_name));
        }

        public static async void Process(SocketMessage message, Config conf)
        {
            List<string> args = message.Content.Split(' ').ToList();
            Service service = null;
            if (args.Count != 3)
            {
                await message.Channel.SendMessageAsync("Unknown pattern");
                return;
            }
            foreach (Service s in conf.services)
            {
                if (s.display_name == args[2])
                {
                    service = s;
                    break;
                }
            }
            if (service == null)
            {
                await message.Channel.SendMessageAsync("Unknown pattern");
                return;
            }
            if (!service.whitelist.Contains(message.Author.Id))
            {
                await message.Channel.SendMessageAsync("Forbidden");
                return;
            }
            service.controller.Refresh();
            switch (args[1])
            {
                case "status":
                    status(message, service);
                    break;
                case "stop":
                    stop(message, service);
                    break;
                case "start":
                    start(message, service);
                    break;
                case "restart":
                    stop(message, service);
                    start(message, service);
                    break;
                default:
                    await message.Channel.SendMessageAsync("Unknown pattern");
                    break;
            }
            return;
        }

    }
}
