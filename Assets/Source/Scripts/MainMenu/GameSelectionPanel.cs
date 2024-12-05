using System;
using DNServerList;
using MultiFPS;
using MultiFPS.Gameplay.Gamemodes;
using MultiFPS.ServerList;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class GameSelectionPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image mapPreview;
        [SerializeField] private Button findMatchButton;
        [SerializeField] private Button playWithBotsButton;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private GameSettingsSO gameSettings;
        [SerializeField] private TextMeshProUGUI mapNameLabel;
        [SerializeField] private TextMeshProUGUI gamemodeLabel;
        [SerializeField] private Matchmaker matchmaker;

        private MapRepresenter selectedMap;
        private Gamemodes? selectedGamemode;

        #region MonoBehaviour events

        private void Start()
        {
            mapPreview.color = Color.clear;
            gamemodeLabel.text = string.Empty;
            mapNameLabel.text = string.Empty;
            ValidateRequest();
        }

        private void OnEnable()
        {
            findMatchButton.onClick.AddListener(FindMatch);
            playWithBotsButton.onClick.AddListener(PlayWithBots);
        }

        private void OnDisable()
        {
            findMatchButton.onClick.RemoveListener(FindMatch);
            playWithBotsButton.onClick.RemoveListener(PlayWithBots);
        }

        #endregion

        public void SelectMap(MapRepresenter mapRepresenter)
        {
            selectedMap = mapRepresenter;
            
            if (mapRepresenter)
            {
                mapPreview.sprite = mapRepresenter.Icon;
                mapPreview.color = Color.white;
                mapNameLabel.text = mapRepresenter.Name;
            }
            else
            {
                mapPreview.sprite = null;
                mapPreview.color = Color.clear;
                mapNameLabel.text = string.Empty;
            }

            ValidateRequest();
        }

        public void SelectMode(int? index)
        {
            if (index.HasValue)
            {
                var gamemode = selectedMap.AvailableGamemodes[index.Value];
                selectedGamemode = gamemode;
                gamemodeLabel.text = gamemode.ToString();
            }
            else
            {
                selectedGamemode = null;
                gamemodeLabel.text = string.Empty;
            }
            
            ValidateRequest();
        }

        public void FindMatch()
        {
            findMatchButton.interactable = false;
            playWithBotsButton.interactable = false;

            var index = Array.FindIndex(gameSettings.Maps, x => x == selectedMap);
            var gamemode = selectedGamemode.Value;

            var request = new
            {
                mapID = index,
                gamemodeID = (int) gamemode,
                gameDuration = gameSettings.GameDurations.Length - 1,
                maxPlayers = selectedMap.MaxPlayersPresets.Length -1,
                spawnBots = 1, // todo: no bots
                serverName = $"{selectedMap.Name}.{gamemode.ToString()}", // todo: multiple instance of same room
            };
            
            matchmaker.ConnectToDomain(request);
        }

        public void PlayWithBots()
        {
            findMatchButton.interactable = false;
            playWithBotsButton.interactable = false;
            
            var networkManager = DNNetworkManager.Instance;
            
            networkManager.onlineScene =  selectedMap.Scene;
            RoomSetup.Properties.P_FillEmptySlotsWithBots = true;
            RoomSetup.Properties.P_Gamemode = selectedGamemode.Value;
            RoomSetup.Properties.P_RespawnCooldown = 6f;
            RoomSetup.Properties.P_GameDuration = gameSettings.GameDurations[^1] * 60;

            var maxPlayers = selectedMap.MaxPlayersPresets[^1];
            RoomSetup.Properties.P_MaxPlayers = maxPlayers; // for gamemode
            networkManager.maxConnections = maxPlayers; // for handling connections
            networkManager.StartHost();
        }

        private void ValidateRequest()
        {
            var isValid = selectedMap && selectedGamemode != null;
            findMatchButton.interactable = isValid;
            playWithBotsButton.interactable = isValid;
        }
    }
}