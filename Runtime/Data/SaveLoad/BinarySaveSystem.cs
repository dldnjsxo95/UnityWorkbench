using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Binary serialization save system.
    /// More compact but requires [Serializable] attribute.
    /// </summary>
    public class BinarySaveSystem : ISaveSystem
    {
        private readonly string _basePath;
        private readonly string _extension;

        public BinarySaveSystem(string subFolder = "Saves", string extension = ".dat")
        {
            _basePath = Path.Combine(Application.persistentDataPath, subFolder);
            _extension = extension;

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        private string GetFilePath(string key) => Path.Combine(_basePath, key + _extension);

#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete
        public void Save<T>(string key, T data) where T : class
        {
            try
            {
                string path = GetFilePath(key);
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                }
                Debug.Log($"[BinarySave] Saved: {key}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[BinarySave] Save failed for {key}: {e.Message}");
            }
        }

        public T Load<T>(string key) where T : class
        {
            try
            {
                string path = GetFilePath(key);

                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[BinarySave] File not found: {key}");
                    return null;
                }

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    T data = (T)formatter.Deserialize(stream);
                    Debug.Log($"[BinarySave] Loaded: {key}");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[BinarySave] Load failed for {key}: {e.Message}");
                return null;
            }
        }
#pragma warning restore SYSLIB0011

        public bool Exists(string key)
        {
            return File.Exists(GetFilePath(key));
        }

        public void Delete(string key)
        {
            string path = GetFilePath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[BinarySave] Deleted: {key}");
            }
        }

        public void DeleteAll()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, true);
                Directory.CreateDirectory(_basePath);
                Debug.Log("[BinarySave] All saves deleted");
            }
        }

        public async Task SaveAsync<T>(string key, T data) where T : class
        {
            await Task.Run(() => Save(key, data));
        }

        public async Task<T> LoadAsync<T>(string key) where T : class
        {
            return await Task.Run(() => Load<T>(key));
        }
    }
}
