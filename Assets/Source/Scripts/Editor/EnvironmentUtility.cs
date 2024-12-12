using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti.Editor
{
    public static class EnvironmentUtility
    {
        private static EnvironmentConfig EnvironmentConfig => Resources.Load<EnvironmentConfig>(nameof(EnvironmentConfig));
        
        [MenuItem("Environment/Local", false, 0)]
        private static void SetHostingEnvironmentLocal()
        {
            SetHostingEnvironment(HostingEnvironment.Local);
        }
        
        [MenuItem("Environment/Local", true)]
        private static bool ValidateHostingEnvironmentLocal()
        {
            return EnvironmentConfig.HostingEnvironment != HostingEnvironment.Local;

        }

        [MenuItem("Environment/Development", false, 1)]
        private static void SetHostingEnvironmentDevelopment()
        {
            SetHostingEnvironment(HostingEnvironment.Development);
        }
        
        [MenuItem("Environment/Development", true)]
        private static bool ValidateHostingEnvironmentDevelopment()
        {
            return EnvironmentConfig.HostingEnvironment != HostingEnvironment.Development;
        }

        [MenuItem("Environment/Production", false , 2)]
        private static void SetHostingEnvironmentProduction()
        {
            SetHostingEnvironment(HostingEnvironment.Production);
        }
        
        [MenuItem("Environment/Production", true)]
        private static bool ValidateHostingEnvironmentProduction()
        {
            return EnvironmentConfig.HostingEnvironment != HostingEnvironment.Production;
        }
        
        private static void SetHostingEnvironment(HostingEnvironment hostingEnvironment)
        {
            var type = EnvironmentConfig.GetType();
            var field = type.GetField(nameof(hostingEnvironment), BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(EnvironmentConfig, hostingEnvironment);
            EditorUtility.SetDirty(EnvironmentConfig);
        }
    }
}