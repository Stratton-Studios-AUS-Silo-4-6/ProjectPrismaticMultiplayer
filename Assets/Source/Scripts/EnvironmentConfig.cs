using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    /// <summary>
    /// ScriptableObject intended to contain configurations for environments with different scopes.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(EnvironmentConfig), menuName = "PrisMulti/EnvironmentConfig", order = 0)]
    public class EnvironmentConfig : ScriptableObject
    {
        [SerializeField] private HostingEnvironment hostingEnvironment;

        public HostingEnvironment HostingEnvironment => hostingEnvironment;
    }

    /// <summary>
    /// Environment of the instance that hosts the server.
    /// </summary>
    public enum HostingEnvironment
    {
        Local,
        Development,
        Production
    }
}