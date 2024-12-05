using System;
using System.Collections;
using DNServerList;
using MultiFPS;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class Matchmaker : MonoBehaviour
    {
        [SerializeField] private WebRequestManager webRequestManager;

        public void ConnectToDomain(object gameProperties)
        {
            webRequestManager.Get("/ping", OnDomainSuccess, OnDomainFail);
            
            void OnDomainFail(string downloadhandler, int responsecode)
            {
                Debug.LogError("Could not find domain.");
            }
            
            void OnDomainSuccess(string downloadhandler, int responsecode)
            {
                if (responsecode != 200)
                {
                    Debug.LogError("Could not connect to domain.");
                }
                else
                {
                    CreateRoom(gameProperties);
                }
            }
        }
        
        struct CreateGameContract
        {
            public string metadata;
            public bool isPrivate;
        }

        private void CreateRoom(object gameProperties)
        {
            var form = new CreateGameContract
            {
                metadata = JsonUtility.ToJson(gameProperties),
                isPrivate = false,
            };

            var formJson = JsonUtility.ToJson(form);

            webRequestManager.PostJson("/createpublicgame", formJson, OnSuccess, OnError);

            void OnSuccess(string data, int code)
            {

                if (code == 202)
                {
                    PlayerConnectToRoomRequest connectInfo = JsonUtility.FromJson<PlayerConnectToRoomRequest>(data);
                    ConnectToLobby(connectInfo.address, connectInfo.port);
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


        private void ConnectToLobby(string address, ushort port)
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
    }
}