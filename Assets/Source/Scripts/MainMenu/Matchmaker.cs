using System;
using System.Collections;
using System.Linq;
using DNServerList;
using MultiFPS;
using MultiFPS.ServerList;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class Matchmaker : MonoBehaviour
    {
        [SerializeField] private WebRequestManager webRequestManager;
        [SerializeField] private GameSettingsSO gameSettings;
        
        public void FindMatch(FindMatchRequest request)
        {
            webRequestManager.Get("/getserverlist", OnSuccess, OnError);

            void OnError(string data, int code)
            {
                Debug.LogError("Could not get server list");
            }

            void OnSuccess(string data, int code)
            {
                var allLobbies = JsonUtility.FromJson<Lobbies>(data);

                if (TryGetValidLobby(allLobbies, request, out var validLobby))
                {   
                    var port = Convert.ToUInt16(validLobby.accessPort);
                    Join(allLobbies.address, port);
                }
                else // no lobbies up, make one
                {
                    CreateRoom(request);
                }
            }
        }

        private void CreateRoom(object findMatchRequest)
        {
            var form = new CreateGameContract
            {
                metadata = JsonUtility.ToJson(findMatchRequest),
                isPrivate = false,
            };

            var formJson = JsonUtility.ToJson(form);

            webRequestManager.PostJson("/createpublicgame", formJson, OnSuccess, OnError);

            void OnSuccess(string data, int code)
            {
                if (code == 202)
                {
                    PlayerConnectToRoomRequest connectInfo = JsonUtility.FromJson<PlayerConnectToRoomRequest>(data);
                    Join(connectInfo.address, connectInfo.port);
                }
                else
                {
                    Debug.LogError("Could not connect to Lobby.");
                }
            }

            void OnError(string data, int code)
            {
                Debug.LogError("Could not create Lobby.");
            }
        }

        private void Join(string address, ushort port)
        {
            IEnumerator Connect()
            {
                yield return null;

                var networkManager = DNNetworkManager.Instance;
                networkManager.networkAddress = address;
                networkManager.Action_SetNetworkManagerPort(port);
                networkManager.StartClient();
            }
            
            StartCoroutine(Connect());
        }

        private bool TryGetValidLobby(Lobbies lobbies, FindMatchRequest request, out LobbyData validLobby)
        {
            var mapData = gameSettings.Maps[request.mapID];
            var maxPlayers = mapData.MaxPlayersPresets[request.maxPlayers];
            
            var validLobbies = from entry in lobbies.lobbies
                let lobbyData = JsonUtility.FromJson<ExampleLobbyProperties>(entry.metadata)
                where lobbyData.MapID == request.mapID
                where lobbyData.GamemodeID == request.gamemodeID
                where lobbyData.CurrentPlayers < maxPlayers
                select entry;

            if (validLobbies.Any())
            {
                validLobby = validLobbies.First();
                return true;
            }
            else
            {
                validLobby = null;
                return false;
            }
        }
    }
}