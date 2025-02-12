using Common;
using Common.Logging;
using Common.Network.ServerNet;
using System.Reflection;

namespace DongServer.Plugins
{
    public class PluginManager
    {
        public List<Plugin> plugins = new List<Plugin>();

        public PluginManager(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                if (!filePath.EndsWith(".dll"))
                {
                    Terminal.Warning($"Skipped a file ${filePath} in plugins folder, all files have to end with .dll");
                    continue;
                }

                //Do error checking or do we want to crash?
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(filePath);
                Assembly pluginAssembly = Assembly.Load(assemblyName);

                Type? pluginType = pluginAssembly.GetTypes().Where(type => type == typeof(Plugin)).FirstOrDefault();

                if(pluginType is null)
                {
                    Terminal.Error($"Skipping plugin, missing plugin class for entry point: {assemblyName.Name} version: {assemblyName.Version}");
                    continue;
                }

                Plugin? plugin = Activator.CreateInstance(pluginType, Server.Instance) as Plugin;

                if(plugin == null)
                {
                    Terminal.Error($"Failed to load {assemblyName.Name} version: {assemblyName.Version}");
                    continue;
                }

                plugin.Init();

                plugins.Add(plugin);
            }
        }
    }
}
