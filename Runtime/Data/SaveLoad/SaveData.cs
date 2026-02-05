using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Base class for saveable data containers.
    /// Inherit from this to create your game's save data structure.
    /// </summary>
    [Serializable]
    public abstract class SaveData
    {
        public string SaveVersion = "1.0";
        public string SaveDate;
        public float PlayTime;

        public SaveData()
        {
            SaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Called before saving. Use for pre-save processing.
        /// </summary>
        public virtual void OnBeforeSave()
        {
            SaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Called after loading. Use for post-load processing.
        /// </summary>
        public virtual void OnAfterLoad() { }

        /// <summary>
        /// Validate save data integrity.
        /// </summary>
        public virtual bool Validate() => true;
    }

    /// <summary>
    /// Example game save data structure.
    /// </summary>
    [Serializable]
    public class GameSaveData : SaveData
    {
        // Player data
        public PlayerSaveData Player = new PlayerSaveData();

        // Game progress
        public int CurrentLevel = 1;
        public int TotalScore = 0;
        public List<string> UnlockedAchievements = new List<string>();
        public List<string> CompletedQuests = new List<string>();

        // Settings reference (usually saved separately)
        public string SettingsKey = "settings";

        public override void OnBeforeSave()
        {
            base.OnBeforeSave();
            // Calculate play time, etc.
        }

        public override bool Validate()
        {
            return Player != null && CurrentLevel >= 1;
        }
    }

    /// <summary>
    /// Player-specific save data.
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public string Name = "Player";
        public int Level = 1;
        public int Experience = 0;

        public float Health = 100f;
        public float MaxHealth = 100f;
        public float Mana = 50f;
        public float MaxMana = 50f;

        public int Gold = 0;
        public Vector3Serializable Position = new Vector3Serializable();
        public string CurrentScene = "MainScene";

        // Inventory (simplified)
        public List<string> InventoryItems = new List<string>();

        // Stats
        public int Strength = 10;
        public int Dexterity = 10;
        public int Intelligence = 10;
    }

    /// <summary>
    /// Serializable Vector3 (Unity's Vector3 doesn't serialize well with JsonUtility).
    /// </summary>
    [Serializable]
    public struct Vector3Serializable
    {
        public float x, y, z;

        public Vector3Serializable(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);

        public static implicit operator Vector3(Vector3Serializable v) => v.ToVector3();
        public static implicit operator Vector3Serializable(Vector3 v) => new Vector3Serializable(v);
    }

    /// <summary>
    /// Serializable Quaternion.
    /// </summary>
    [Serializable]
    public struct QuaternionSerializable
    {
        public float x, y, z, w;

        public QuaternionSerializable(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);

        public static implicit operator Quaternion(QuaternionSerializable q) => q.ToQuaternion();
        public static implicit operator QuaternionSerializable(Quaternion q) => new QuaternionSerializable(q);
    }
}
