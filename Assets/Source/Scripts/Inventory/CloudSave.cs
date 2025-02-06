using System.IO;
using System.Threading.Tasks;
using Beamable;
using Beamable.Api.CloudSaving;
using UnityEngine;

namespace StrattonStudioGames.PrisMulti
{
    public class CloudSave : MonoSingleton<CloudSave>
    {
        private CloudSavingService service;

        #region Public methods
        
        public async Task Refresh()
        {
            var context = await BeamContext.Default.Instance;
            service = context.Api.CloudSavingService;
            await service.Refresh();
        }

        public async Task<T> LoadData<T>(string fileName)
        {
            await Refresh();
            return LoadDataInternal<T>(fileName);
        }

        public void SaveData<T>(string fileName, T saveData)
        {
            SaveDataInternal(fileName, saveData);
        }

        #endregion

        #region Unity hooks

        private async void Start()
        {
            await Refresh();
            service.Init();
        }

        #endregion

        private string GetCloudPath(string fileName)
        {
            var localCloudDataFullPath = service.LocalCloudDataFullPath;
            
            // Required format
            return $"{localCloudDataFullPath}{Path.DirectorySeparatorChar}{fileName}";
        }

        private T LoadDataInternal<T>(string fileName)
        {
            var filePath = GetCloudPath(fileName);
            
            if (!Directory.Exists(service.LocalCloudDataFullPath))
            {
                Directory.CreateDirectory(service.LocalCloudDataFullPath);
            }
            
            T data = default;

            if (!File.Exists(filePath))
            {
                return data;
            }
            
            var json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<T>(json);

            return data;
        }
        
        private void SaveDataInternal<T>(string fileName, T data)
        {
            var json = JsonUtility.ToJson(data);
            
            if (!Directory.Exists(GetCloudPath(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GetCloudPath(fileName)));
            }

            // Once the data is written to disk, the service will
            // automatically upload the contents to the cloud
            File.WriteAllText(GetCloudPath(fileName), json);
        }
    }
}