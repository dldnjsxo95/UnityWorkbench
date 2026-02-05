using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Interface for all quests.
    /// </summary>
    public interface IQuest
    {
        string QuestId { get; }
        string Title { get; }
        string Description { get; }
        QuestType QuestType { get; }
        int RequiredLevel { get; }
        bool IsRepeatable { get; }
    }

    /// <summary>
    /// Quest type categories.
    /// </summary>
    public enum QuestType
    {
        Main,           // Main story quests
        Side,           // Optional side quests
        Daily,          // Daily repeatable quests
        Weekly,         // Weekly repeatable quests
        Event,          // Time-limited event quests
        Tutorial        // Tutorial/introduction quests
    }

    /// <summary>
    /// Quest status.
    /// </summary>
    public enum QuestStatus
    {
        Unavailable,    // Requirements not met
        Available,      // Can be accepted
        InProgress,     // Currently active
        Completed,      // All objectives done
        TurnedIn,       // Rewards claimed
        Failed          // Quest failed
    }

    /// <summary>
    /// Objective types.
    /// </summary>
    public enum ObjectiveType
    {
        Kill,           // Kill X enemies
        Collect,        // Collect X items
        Talk,           // Talk to NPC
        Explore,        // Visit location
        Escort,         // Escort NPC
        Deliver,        // Deliver item to NPC
        Interact,       // Interact with object
        Custom          // Custom objective
    }
}
