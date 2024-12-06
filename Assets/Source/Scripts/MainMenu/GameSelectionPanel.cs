using System;
using DNServerList;
using Mirror.BouncyCastle.Tls;
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
        private int? gamemodeIndex;

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
            gamemodeIndex = index;
            
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

            var request = new FindMatchRequest
            {
                mapID = index,
                gamemodeID = gamemodeIndex.Value,
                gameDuration = gameSettings.GameDurations.Length - 1,
                maxPlayers = selectedMap.MaxPlayersPresets.Length -1,
                spawnBots = 1, // todo: no bots
                serverName = $"{selectedMap.Name}.{selectedGamemode.Value.ToString()}", // todo: multiple instances of same room
            };
            
            request.Log();
            
            matchmaker.FindMatch(request);
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
    
    public struct FindMatchRequest
    {
        /// <summary>
        /// The index of the map respective to its location in <see cref="GameSettingsSO.Maps"/>.
        /// </summary>
        public int mapID;
        
        /// <summary>
        /// The index of the <see cref="Gamemodes"/> respective to the current map.
        /// </summary>
        public int gamemodeID;
        
        /// <summary>
        /// The index to use in <see cref="GameSettingsSO.GameDurations"/>, which is an integer array
        /// containing the preset durations in minutes for all maps and gamemodes.
        /// </summary>
        /// <remarks>
        /// Not the actual game duration itself.
        /// </remarks>
        public int gameDuration;
        
        /// <summary>
        /// The index to use in <see cref="MapRepresenter.MaxPlayersPresets"/>, which contains the max players amount.
        /// </summary>
        /// <remarks>
        /// Not the actual game duration itself.
        /// </remarks>
        public int maxPlayers;
        public int spawnBots;
        public string serverName;

        public void Log()
        {
            var log = $"<color=#00ffff>{serverName}</color>";
            log += $"\n\t {nameof(mapID)}: {mapID}";
            log += $"\n\t {nameof(gamemodeID)}: {gamemodeID}";
            log += $"\n\t {nameof(gameDuration)}: {gameDuration}";
            log += $"\n\t {nameof(maxPlayers)}: {maxPlayers}";
            log += $"\n\t {nameof(spawnBots)}: {spawnBots}";
            Debug.Log(log);
        }
    }
}