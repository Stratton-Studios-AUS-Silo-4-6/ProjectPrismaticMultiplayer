using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using StreamReader = System.IO.StreamReader;

namespace StrattonStudioGames.PrisMulti.Editor
{
    public static class Builder
    {
        /// <summary>
        /// Build the application for a windows server.
        /// Creates a new folder in the given directory (named after the application version)
        /// in which we output the build.
        /// </summary>
        [MenuItem("Build/Windows Server")]
        public static void BuildWindowsServer()
        {
            // get current build options
            var buildPlayerOptions = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
            
            // get the selected file path for the build output
            var rawFilePath = new FileInfo(buildPlayerOptions.locationPathName);
            
            // get the selected folder path for the build output
            var directory = rawFilePath.Directory;
            
            // mutate the selected file path to use the version as its output folder within the parent folder of the original file path
            var finalFilePath = new FileInfo(Path.Combine(directory.FullName, Application.version, rawFilePath.Name));
            
            buildPlayerOptions.locationPathName = finalFilePath.FullName;
            
            // build the player, and store its resulting report
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                ConfigureServer(directory, finalFilePath);
                Debug.Log("Build succeeded");
            }
            else
            {
                Debug.LogError("Build failed");
            }
        }

        /// <summary>
        /// Configure the config file for the DNServerList_WindowsServer
        /// </summary>
        /// <remarks>
        /// Ensure that we have the DNServerList_WindowsServer already initialized somewhere before building with this.
        /// <see href="https://strattonstudios1.atlassian.net/wiki/spaces/Prismatic/pages/293928962/Dev+Onboarding#Setup"/>
        /// </remarks>
        /// <param name="serverConfigDirectory">
        /// The folder path where we can find the server config file.
        /// </param>
        /// <param name="unityBuildFilePath">
        /// The file path where the unity application was built.
        /// </param>
        private static void ConfigureServer(DirectoryInfo serverConfigDirectory, FileInfo unityBuildFilePath)
        {
            var sourcePath = Path.Combine(serverConfigDirectory.FullName, "config.txt");
            var tempPath = Path.Combine(serverConfigDirectory.FullName, "temp.txt");
            File.Copy(sourcePath, tempPath);
            
            using (var reader = new StreamReader(tempPath))
            {
                using (var writer = new StreamWriter(sourcePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("ServerGameBuildExecutablePath"))
                        {
                            writer.WriteLine($"ServerGameBuildExecutablePath: {Path.Combine(unityBuildFilePath.Directory.Name, unityBuildFilePath.Name)}");
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }

            File.Delete(tempPath);
        }
    }
}