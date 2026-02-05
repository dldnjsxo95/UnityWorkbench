using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// JSON-based save system with optional encryption.
    /// Saves to Application.persistentDataPath.
    /// </summary>
    public class JsonSaveSystem : ISaveSystem
    {
        private readonly string _basePath;
        private readonly string _extension;
        private readonly bool _useEncryption;
        private readonly string _encryptionKey;
        private readonly bool _prettyPrint;

        public JsonSaveSystem(
            string subFolder = "Saves",
            string extension = ".json",
            bool useEncryption = false,
            string encryptionKey = null,
            bool prettyPrint = true)
        {
            _basePath = Path.Combine(Application.persistentDataPath, subFolder);
            _extension = extension;
            _useEncryption = useEncryption;
            _encryptionKey = encryptionKey ?? Application.productName;
            _prettyPrint = prettyPrint;

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        private string GetFilePath(string key) => Path.Combine(_basePath, key + _extension);

        public void Save<T>(string key, T data) where T : class
        {
            try
            {
                string json = JsonUtility.ToJson(data, _prettyPrint);

                if (_useEncryption)
                {
                    json = Encrypt(json);
                }

                string path = GetFilePath(key);
                File.WriteAllText(path, json, Encoding.UTF8);

                Debug.Log($"[SaveSystem] Saved: {key}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed for {key}: {e.Message}");
            }
        }

        public T Load<T>(string key) where T : class
        {
            try
            {
                string path = GetFilePath(key);

                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[SaveSystem] File not found: {key}");
                    return null;
                }

                string json = File.ReadAllText(path, Encoding.UTF8);

                if (_useEncryption)
                {
                    json = Decrypt(json);
                }

                T data = JsonUtility.FromJson<T>(json);
                Debug.Log($"[SaveSystem] Loaded: {key}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed for {key}: {e.Message}");
                return null;
            }
        }

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
                Debug.Log($"[SaveSystem] Deleted: {key}");
            }
        }

        public void DeleteAll()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, true);
                Directory.CreateDirectory(_basePath);
                Debug.Log("[SaveSystem] All saves deleted");
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

        // Simple XOR encryption (for basic obfuscation, not secure)
        private string Encrypt(string text)
        {
            var result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                result.Append((char)(text[i] ^ _encryptionKey[i % _encryptionKey.Length]));
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
        }

        private string Decrypt(string encoded)
        {
            string text = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                result.Append((char)(text[i] ^ _encryptionKey[i % _encryptionKey.Length]));
            }
            return result.ToString();
        }

        /// <summary>
        /// Get all save file names.
        /// </summary>
        public string[] GetAllSaveKeys()
        {
            if (!Directory.Exists(_basePath)) return Array.Empty<string>();

            var files = Directory.GetFiles(_basePath, "*" + _extension);
            var keys = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                keys[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return keys;
        }

        /// <summary>
        /// Create a backup of a save file.
        /// </summary>
        public void CreateBackup(string key)
        {
            string source = GetFilePath(key);
            if (File.Exists(source))
            {
                string backup = GetFilePath(key + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                File.Copy(source, backup);
                Debug.Log($"[SaveSystem] Backup created: {key}");
            }
        }
    }
}
