using TMPro;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class LoadoutEquipEntry : MonoBehaviour, IListViewEntry<Cosmetic>
    {
        [SerializeField] private TextMeshProUGUI label;

        public Cosmetic Data { get; private set; }

        public void OnAdd(Cosmetic data)
        {
            Data = data;
            label.text = data.Id;
        }

        public void OnRemove()
        {
        }
    }
}