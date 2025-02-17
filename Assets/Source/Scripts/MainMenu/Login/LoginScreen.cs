using System.Collections.Generic;
using Beamable;
using Thirdweb;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb.Unity;
using TMPro;
using UnityEngine.SceneManagement;

namespace StrattonStudioGames.PrisMulti
{
    public class LoginScreen : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button googleButton;
        [SerializeField] private Button facebookButton;
        [SerializeField] private Button loginAsGuest;
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI stateLabel;
        
        private ulong ActiveChainId = 421614;
        private BeamContext _beamContext;

        private async void Start()
        {
            googleButton.interactable = false;
            facebookButton.interactable = false;
            loginButton.interactable = false;
            usernameField.interactable = false;
            passwordField.interactable = false;
            stateLabel.text = "Initializing Beamable...";
            
            _beamContext = BeamContext.Default;
            await _beamContext.OnReady;
            await _beamContext.Accounts.OnReady;
            
            loginButton.interactable = true;
            usernameField.interactable = true;
            passwordField.interactable = true;
            stateLabel.text = string.Empty;
        }

        private void OnEnable()
        {
            loginButton.onClick.AddListener(LoginEmail);
            loginAsGuest.onClick.AddListener(LoginGuest);
            exitButton.onClick.AddListener(Application.Quit);
        }

        private void OnDisable()
        {
            loginButton.onClick.RemoveListener(LoginEmail);
            loginAsGuest.onClick.RemoveListener(LoginGuest);
            exitButton.onClick.RemoveListener(Application.Quit);
        }

        private async void LoginEmail()
        {
            stateLabel.text = $"Logging in via email...";
            loginButton.interactable = false;
            loginAsGuest.interactable = false;
            usernameField.interactable = false;
            passwordField.interactable = false;
            
            var operation = await _beamContext.Accounts.RecoverAccountWithEmail(usernameField.text, passwordField.text);
            
            if (operation.isSuccess)
            {
                Debug.Log($"Found existing account, playerId=[{operation.account.GamerTag}]");
                await operation.SwitchToAccount();
                SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            }
            else
            {
                loginButton.interactable = true;
                loginAsGuest.interactable = true;
                usernameField.interactable = true;
                passwordField.interactable = true;
                stateLabel.text = $"Failed to recovery account via email, reason=[{operation.error}]";
            }
        }

        private void LoginGuest()
        {
            SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        }

        private async void InitThirdWeb()
        {
            ThirdwebManager.Instance.Initialize();
            
            // create wallet options
            var options = new WalletOptions(provider: WalletProvider.PrivateKeyWallet, chainId: ActiveChainId);
            
            // create wallet
            stateLabel.text = "Connecting wallet...";
            var wallet = await ThirdwebManager.Instance.ConnectWallet(options);

            stateLabel.text = "Upgrading smart wallet...";
            // upgrade to smart wallet
            var smartWallet = await ThirdwebManager.Instance.UpgradeToSmartWallet(personalWallet: wallet, chainId: ActiveChainId, smartWalletOptions: new SmartWalletOptions(sponsorGas: true));
            
            stateLabel.text = "Generating private key...";
            var randomWallet = await PrivateKeyWallet.Generate(ThirdwebManager.Instance.Client);
            
            stateLabel.text = "Generating random wallet address...";
            var randomWalletAddress = await randomWallet.GetAddress();
            var timeTomorrow = Utils.GetUnixTimeStampNow() + 60 * 60 * 24;
            
            stateLabel.text = "Creating session key...";
            
            // get session key
            var sessionKey = await smartWallet.CreateSessionKey(
                signerAddress: randomWalletAddress,
                approvedTargets: new List<string> { Constants.ADDRESS_ZERO },
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: timeTomorrow.ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: timeTomorrow.ToString()
            );
            
            Debug.Log(sessionKey.ToString());
        }
    }
}