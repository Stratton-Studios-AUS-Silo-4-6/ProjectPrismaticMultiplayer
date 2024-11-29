using MultiFPS.Gameplay;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    [CreateAssetMenu(fileName = "GunProgressionData", menuName = "PrisMulti/GunProgressionData", order = 0)]
    public class GunProgressionConfig : ScriptableObject
    {
        [SerializeField] private Item[] progression;

        public Item GetItem(int index)
        {
            return progression[index];
        }

        public int GetMax()
        {
            return progression.Length;
        }
    }
}