using System.Collections.Generic;
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
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI stateLabel;
            
        private ulong ActiveChainId = 421614;

        private void OnEnable()
        {
            button.onClick.AddListener(Login);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(Login);
        }

        private async void Login()
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
            
            stateLabel.text = "Logging in...";
            await SceneManager.LoadSceneAsync("Matchmaking", LoadSceneMode.Single);
        }
    }
}