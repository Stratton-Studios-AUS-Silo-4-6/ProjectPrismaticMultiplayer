using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    [CreateAssetMenu(menuName = "PrisMulti/Cosmetic/Gun")]
    public class GunCosmetic : Cosmetic
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;

        public void Apply(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            skinnedMeshRenderer.sharedMesh = mesh;
            skinnedMeshRenderer.material = material;
        }
    }
}