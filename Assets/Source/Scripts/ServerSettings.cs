using System;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public static class ServerSettings
    {
        private static ServerConfig serverConfig;
        private static EnvironmentConfig environmentConfig;

        public static ServerConfig Config => serverConfig;
        public static EnvironmentConfig Environment => environmentConfig;
        
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            environmentConfig = Resources.Load<EnvironmentConfig>("EnvironmentConfig");
            
            serverConfig = environmentConfig.HostingEnvironment switch
            {
                HostingEnvironment.Local => Resources.Load<ServerConfig>("LocalServer"),
                HostingEnvironment.Development => Resources.Load<ServerConfig>("DevServer"),
                HostingEnvironment.Production => Resources.Load<ServerConfig>("ProdServer"),
                _ => Resources.Load<ServerConfig>("LocalServer"),
            };
        }
    }
}