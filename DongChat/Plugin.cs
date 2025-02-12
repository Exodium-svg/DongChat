using Common.Network.ServerNet;
using Common.Utils;

namespace Common
{
    public abstract class Plugin
    {
        public int Version { get; private set; } = -1;
        public string Name { get; private set; } = "Bug author for name";
        public string Description { get; private set; } = "Bug author for description";
        public CVars CVars { get; private set; }
        public Server Server { get; init; }
        public Plugin(Server server)
        {
            Server = server;
            CVars = new CVars();
        }

        public abstract void Init();
        public virtual void OnUpdateCVars(CVars cVars) => CVars = cVars;
    }
}
