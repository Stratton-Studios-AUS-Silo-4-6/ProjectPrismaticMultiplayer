using Beamable;
using Beamable.Avatars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class TestBeam : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI gems;
        [SerializeField] private TextMeshProUGUI coins;
        [SerializeField] private TextMeshProUGUI alias;
        private async void Start()
        {
            var context = BeamContext.Default;
            image.color = Color.clear;
            gems.text = string.Empty;
            coins.text = string.Empty;
            alias.text = string.Empty;
            
            Debug.Log("starting context...");
            await context.OnReady;
            
            Debug.Log("refreshing accounts...");
            await context.Accounts.Refresh();
            alias.text = context.Accounts.Current.Alias;

            var avatarConfig = context.ServiceProvider.GetService<AvatarConfiguration>();
            image.sprite = avatarConfig.Avatars[0].Sprite;
            image.color = Color.white;

            foreach (var account in context.Accounts)
            {
                Debug.Log($"account: {account.Alias}-{account.Email}");
            }

            await context.Inventory.Refresh();
            foreach (var currency in context.Inventory.GetCurrencies())
            {
                Debug.Log($"{currency.CurrencyId} : {currency.Amount}");
                
                var label = currency.CurrencyId switch
                {
                    "currency.gems" => gems,
                    "currency.coins" => coins,
                    _ => null,
                };

                var text = currency.CurrencyId switch
                {
                    "currency.gems" => $"gems {currency.Amount}",
                    "currency.coins" => $"coins {currency.Amount}",
                    _ => string.Empty,
                };

                label.text = text;
            }
        }
    }
}