using Nett;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;

namespace Discord_ServiceManager
{

    class Service
    {
        public ServiceController controller { get; set; }
        public string service_name { get; set; }
        public string display_name { get; set; }
        public List<ulong> whitelist { get; set; }

        public Service(string service_name, string display_name, ulong[] whitelist)
        {
            this.service_name = service_name;
            this.display_name = display_name;
            controller = new ServiceController(service_name);
            this.whitelist = whitelist.ToList();
        }
    }

    class Config
    {
        public string token { get; set; }
        public string command { get; set; }
        public List<Service> services { get; set; }
        public Config()
        {
        }
    }
    static class ConfigGetter
    {
        public static Config GetConfigFromTOML(string file)
        {
            Config configuration = new Config();
            TomlTable config_file = Toml.ReadFile(file);
            List<Service> services = new List<Service>();

            configuration.token = config_file.Get<TomlTable>("config").Get<string>("token");
            configuration.command = config_file.Get<TomlTable>("config").Get<string>("command");
            foreach (var service in config_file.Get<TomlTable>("services"))
            {
                TomlTable subtable = (TomlTable)service.Value;
                string service_name = subtable.Get<string>("service_name");
                string display_name = subtable.Get<string>("display_name");
                ulong[] whitelist = subtable.Get<ulong[]>("whitelist");
                services.Add(new Service(service_name, display_name, whitelist));
            }
            configuration.services = services;
            return configuration;
        }
    }
}
