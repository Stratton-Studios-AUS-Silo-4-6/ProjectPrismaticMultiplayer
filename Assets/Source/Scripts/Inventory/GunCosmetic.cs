using System.Threading.Tasks;
using Beamable.Common.Content;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StrattonStudioGames.PrisMulti
{
    // [CreateAssetMenu(menuName = "PrisMulti/Cosmetic/Gun")]
    [ContentType("cosmetic")]
    public class GunCosmetic : Cosmetic
    {
        [SerializeField] private AssetReferenceT<Mesh> meshRef;

        [SerializeField] private AssetReferenceT<Material> materialRef;

        public async void Apply(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var meshTask = meshRef.LoadAssetAsync().Task;
            var materialTask = materialRef.LoadAssetAsync().Task;

            await Task.WhenAll(meshTask, materialTask);

            skinnedMeshRenderer.sharedMesh = meshTask.Result;
            skinnedMeshRenderer.material = materialTask.Result;
        }
    }
}