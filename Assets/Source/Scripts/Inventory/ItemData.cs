using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    [CreateAssetMenu(menuName = "PrisMulti/ItemData")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string itemName;
        [SerializeField] private Cosmetic[] cosmetics;

        public string ItemId => itemId;
        public string ItemName => itemName;
        public Cosmetic[] Cosmetics => cosmetics;
    }
}