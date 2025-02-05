using TMPro;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// UI component, handles displaying data for type of item we're trying to set cosmetics for.
    /// </summary>
    public class LoadoutEquipPage : MonoSingleton<LoadoutEquipPage>
    {
        [SerializeField] private TextMeshProUGUI headerLabel;
        [SerializeField] private LoadoutEquipList loadoutEquipList;

        #region Public methods

        public void Init(GunItemData gunItemData)
        {
            headerLabel.text = gunItemData.ItemName;
            loadoutEquipList.Init(gunItemData);
        }

        #endregion
    }
}