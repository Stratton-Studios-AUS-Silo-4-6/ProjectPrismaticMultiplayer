using TMPro;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class LoadoutEquipEntry : MonoBehaviour, IListViewEntry<Cosmetic>
    {
        [SerializeField] private TextMeshProUGUI label;
        
        public void OnAdd(Cosmetic data)
        {
            label.text = data.CosmeticId;
        }

        public void OnRemove()
        {
        }
    }
}