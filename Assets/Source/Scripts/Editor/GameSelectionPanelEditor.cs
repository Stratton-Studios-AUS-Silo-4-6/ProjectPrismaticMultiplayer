using System.Reflection;
using MultiFPS;
using UnityEditor;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti.Editor
{
    [CustomEditor(typeof(GameSelectionPanel))]
    public class GameSelectionPanelEditor : UnityEditor.Editor 
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Validatee"))
            {
                Validate();
            }
        }

        private void Validate()
        {
            var target = (GameSelectionPanel) serializedObject.targetObject;
            var maps = target.GetComponentsInChildren<GameSelectionMap>();

            foreach (var map in maps)
            {
                var gamemodes = map.GetComponentsInChildren<GameSelectionMode>();
                var mapData = typeof(GameSelectionMap)
                    .GetField("mapData", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(map) as MapRepresenter;

                // var foo = new GameObject();
                // foo.name = mapData.

                var log = $"<color=#00ffff>{mapData.Name} valid: {gamemodes.Length == mapData.AvailableGamemodes.Length}</color>";
                Debug.Log(log);
            }
        }
    }
}