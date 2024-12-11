using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public static class ServerSettings
    {
        private static ServerConfig serverConfig;

        public static ServerConfig Config => serverConfig;
        
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            serverConfig = Resources.Load<ServerConfig>("LocalServer");
        }
    }
}