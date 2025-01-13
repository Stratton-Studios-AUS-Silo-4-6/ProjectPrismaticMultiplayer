using Beamable;
using Beamable.Avatars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// Component that integrates Beamable Account system to display currencies in UI objects.
    /// </summary>
    public class ProfileDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI alias;
        [SerializeField] private Image avatar;

        #region Unity hooks

        private void Start()
        {
            Clear();
            Refresh();
        }

        #endregion

        private void Clear()
        {
            alias.text = string.Empty;
            avatar.color = Color.clear;
        }

        private async void Refresh()
        {
            var context = BeamContext.Default;

            await context.OnReady;
            await context.Accounts.Refresh();

            var account = context.Accounts.Current;
            
            alias.text = account.Alias;
            
            var avatarConfig = context.ServiceProvider.GetService<AvatarConfiguration>();
            avatar.color = Color.white;
            avatar.sprite = avatarConfig.Avatars[0].Sprite;
        }
    }
}