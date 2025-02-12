using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace StrattonStudioGames.PrisMulti.Editor
{
    public static class SceneUtility
    {
        [MenuItem("Scenes/MainMenu/Matchmaking")]
        private static void ToMatchmaking()
        {
            EditorSceneManager.OpenScene("Assets/Source/MainMenu/Matchmaking.unity");
        }
        
        [MenuItem("Scenes/MainMenu/MainMenu")]
        private static void ToMainMenu()
        {
            EditorSceneManager.OpenScene("Assets/Source/MainMenu/MainMenu.unity");
        }

        [MenuItem("Scenes/Gameplay/Range")]
        private static void ToRange()
        {
            EditorSceneManager.OpenScene("Assets/MultiFPS/Levels/map_range.unity");
        }

        [MenuItem("Scenes/Gameplay/CircleArena")]
        private static void ToCircleArena()
        {
            EditorSceneManager.OpenScene("Assets/MultiFPS/Levels/map_circleArena.unity");
        }

        [MenuItem("Scenes/Gameplay/Construction")]
        private static void ToConstruction()
        {
            EditorSceneManager.OpenScene("Assets/MultiFPS/Levels/map_construction.unity");
        }

        [MenuItem("Scenes/Gameplay/BombBuilding")]
        private static void ToBombBuilding()
        {
            EditorSceneManager.OpenScene("Assets/MultiFPS/Levels/map_bombBuilding.unity");
        }

        [MenuItem("Scenes/Gameplay/Arena")]
        private static void ToArena()
        {
            EditorSceneManager.OpenScene("Assets/MultiFPS/Levels/map_arena.unity");
        }
        
        [MenuItem("Scenes/MainMenu/AccountManager")]
        private static void ToAccountManager()
        {
            EditorSceneManager.OpenScene("Assets/Samples/Beamable/2.0.2/AccountManagement/Sample_AccountManager.unity");
        }
        
        [MenuItem("Scenes/MainMenu/InventoryManager")]
        private static void ToInventoryManager()
        {
            EditorSceneManager.OpenScene("Assets/Samples/Beamable/2.0.2/InventoryManagement/Sample_InventoryManager.unity");
        }
        
        [MenuItem("Scenes/MainMenu/Loadout")]
        private static void ToLoadout()
        {
            EditorSceneManager.OpenScene("Assets/Source/MainMenu/Loadout.unity");
        }
        
        [MenuItem("Scenes/MainMenu/LoadoutEquip")]
        private static void ToLoadoutEquip()
        {
            EditorSceneManager.OpenScene("Assets/Source/MainMenu/LoadoutEquip.unity");
        }
        
        [MenuItem("Scenes/MainMenu/Login")]
        private static void ToLogin()
        {
            EditorSceneManager.OpenScene("Assets/Source/MainMenu/Login.unity");
        }
    }
}