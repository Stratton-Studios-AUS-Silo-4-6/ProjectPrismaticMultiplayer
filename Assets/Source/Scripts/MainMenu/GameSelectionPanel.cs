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
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private GameSettingsSO gameSettings;
        [SerializeField] private TextMeshProUGUI mapNameLabel;
        [SerializeField] private TextMeshProUGUI gamemodeLabel;

        private MapRepresenter selectedMap;
        private Gamemodes? selectedGamemode;

        #region MonoBehaviour events

        private void Start()
        {
            ValidateRequest();
            mapPreview.color = Color.clear;
            gamemodeLabel.text = string.Empty;
            mapNameLabel.text = string.Empty;
        }

        private void OnEnable()
        {
            findMatchButton.onClick.AddListener(FindMatch);
        }

        private void OnDisable()
        {
            findMatchButton.onClick.RemoveListener(FindMatch);
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
            var isValid = selectedMap != null && selectedGamemode != null;
            findMatchButton.interactable = isValid;
        }
    }
}