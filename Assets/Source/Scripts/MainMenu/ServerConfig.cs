using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "PrisMulti/ServerConfig", order = 0)]
    public class ServerConfig : ScriptableObject
    {
        [SerializeField] private string domain = "http://localhost";
        [SerializeField] private int port = 5000;

        public string Domain => $"{domain}:{port}";
        public string EndpointMatchmaking => $"{Domain}/matchmaking";
        public string EndpointGetServerList => $"{EndpointMatchmaking}/getserverlist";
        public string EndpointCreateRoom => $"{EndpointMatchmaking}/createpublicgame";
    }
}