using System;
using System.Collections;
using System.Linq;
using System.Text;
using DNServerList;
using MultiFPS;
using MultiFPS.ServerList;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace StrattonStudioGames.PrisMulti
{
    public class Matchmaker : MonoBehaviour
    {
        [SerializeField] private GameSettingsSO gameSettings;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private TextMeshProUGUI label;

        private void Start()
        {
            loadingIndicator.SetActive(false);
        }
        
        public async Awaitable<bool> TryFindMatch(FindMatchRequest requestData)
        {
            
            using var request = UnityWebRequest.Get("http://localhost:5000/matchmaking/getserverlist");
            
            loadingIndicator.SetActive(true);
            label.enabled = false;
            await request.SendWebRequest();
            label.enabled = true;
            loadingIndicator.SetActive(false);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Could not connect to lobby. Request result: {request.result.ToString()}");
                loadingIndicator.SetActive(false);
                return false;
            }
            
            var allLobbies = JsonUtility.FromJson<Lobbies>(request.downloadHandler.text);

            if (TryGetValidLobby(allLobbies, requestData, out var validLobby))
            {   
                var port = Convert.ToUInt16(validLobby.accessPort);
                Join(allLobbies.address, port);
            }
            else // no lobbies up, make one
            {
                CreateRoom(request);
            }

            return true;
        }

        private async void CreateRoom(object findMatchRequest)
        {
            var form = new CreateGameContract
            {
                metadata = JsonUtility.ToJson(findMatchRequest),
                isPrivate = false,
            };

            var formJson = JsonUtility.ToJson(form);
            var jsonBytes  = Encoding.UTF8.GetBytes(formJson);
            
            using var request = UnityWebRequest.PostWwwForm("http://localhost:5000/matchmaking/createpublicgame", "POST");
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.SetRequestHeader("Content-Type", "application/json");
            
            loadingIndicator.SetActive(true);
            label.enabled = false;
            await request.SendWebRequest();
            loadingIndicator.SetActive(false);
            label.enabled = true;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Could not connect to lobby. Request result: {request.result.ToString()}");
            }
            else if (request.responseCode != 202)
            {
                Debug.LogError($"Could not connect to lobby. Response code: {request.result.ToString()}");
            }
            else
            {
                var data = request.downloadHandler.text;
                var connectInfo = JsonUtility.FromJson<PlayerConnectToRoomRequest>(data);
                Join(connectInfo.address, connectInfo.port);
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
                loadingIndicator.SetActive(false);
                label.enabled = true;
            }
            
            loadingIndicator.SetActive(true);
            label.enabled = false;
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