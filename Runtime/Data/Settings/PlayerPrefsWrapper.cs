using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Type-safe wrapper for PlayerPrefs with default values.
    /// </summary>
    public static class PlayerPrefsWrapper
    {
        private const string PREFIX = "lwt_";

        private static string GetKey(string key) => PREFIX + key;

        // ===== Int =====
        public static int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(GetKey(key), defaultValue);
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(GetKey(key), value);
        }

        // ===== Float =====
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(GetKey(key), defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(GetKey(key), value);
        }

        // ===== String =====
        public static string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(GetKey(key), defaultValue);
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(GetKey(key), value);
        }

        // ===== Bool =====
        public static bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(GetKey(key), defaultValue ? 1 : 0) == 1;
        }

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(GetKey(key), value ? 1 : 0);
        }

        // ===== Vector3 =====
        public static Vector3 GetVector3(string key, Vector3 defaultValue = default)
        {
            float x = GetFloat(key + "_x", defaultValue.x);
            float y = GetFloat(key + "_y", defaultValue.y);
            float z = GetFloat(key + "_z", defaultValue.z);
            return new Vector3(x, y, z);
        }

        public static void SetVector3(string key, Vector3 value)
        {
            SetFloat(key + "_x", value.x);
            SetFloat(key + "_y", value.y);
            SetFloat(key + "_z", value.z);
        }

        // ===== Color =====
        public static Color GetColor(string key, Color defaultValue = default)
        {
            float r = GetFloat(key + "_r", defaultValue.r);
            float g = GetFloat(key + "_g", defaultValue.g);
            float b = GetFloat(key + "_b", defaultValue.b);
            float a = GetFloat(key + "_a", defaultValue.a);
            return new Color(r, g, b, a);
        }

        public static void SetColor(string key, Color value)
        {
            SetFloat(key + "_r", value.r);
            SetFloat(key + "_g", value.g);
            SetFloat(key + "_b", value.b);
            SetFloat(key + "_a", value.a);
        }

        // ===== Enum =====
        public static T GetEnum<T>(string key, T defaultValue = default) where T : System.Enum
        {
            int value = GetInt(key, System.Convert.ToInt32(defaultValue));
            return (T)System.Enum.ToObject(typeof(T), value);
        }

        public static void SetEnum<T>(string key, T value) where T : System.Enum
        {
            SetInt(key, System.Convert.ToInt32(value));
        }

        // ===== DateTime =====
        public static System.DateTime GetDateTime(string key, System.DateTime defaultValue = default)
        {
            string str = GetString(key, "");
            if (string.IsNullOrEmpty(str)) return defaultValue;

            if (System.DateTime.TryParse(str, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public static void SetDateTime(string key, System.DateTime value)
        {
            SetString(key, value.ToString("o"));
        }

        // ===== Utility =====
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(GetKey(key));
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(GetKey(key));
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
