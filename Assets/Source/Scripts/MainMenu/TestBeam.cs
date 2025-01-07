using Beamable;
using Beamable.Avatars;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    public class TestBeam : MonoBehaviour
    {
        [SerializeField] private Image image;
        private async void Start()
        {
            var context = BeamContext.Default;
            image.color = Color.clear;
            
            Debug.Log("starting context...");
            await context.OnReady;
            
            Debug.Log("refreshing accounts...");
            await context.Accounts.Refresh();

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
            }
        }
    }
}