using Beamable;
using TMPro;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Component that integrates Beamable Inventory system to display currencies in UI objects.
    /// </summary>
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] labels;

        #region Unity hooks

        private void Start()
        {
            Clear();
            Refresh();
        }

        #endregion

        private void Clear()
        {
            foreach (var label in labels)
            {
                label.text = string.Empty;
            }
        }

        private async void Refresh()
        {
            var context = BeamContext.Default;

            await context.OnReady;
            await context.Inventory.Refresh();

            var currencies = context.Inventory.Currencies;

            for (var i = 0; i < currencies.Count; i++)
            {
                var currency = context.Inventory.Currencies[i];
                var text = currency.CurrencyId switch
                {
                    "currency.gems" => $"gems: {currency.Amount}",
                    "currency.coins" => $"coins: {currency.Amount}",
                    _ => string.Empty,
                };

                labels[i].text = text;
            }
        }
    }
}