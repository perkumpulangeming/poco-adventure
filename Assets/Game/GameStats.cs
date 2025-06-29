using UnityEngine;

namespace Game
{
    public static class GameStats
    {
        private const string GEMS_KEY = "CollectedGems";
        private const string ENEMIES_KEY = "KilledEnemies";
        private const string WIN_KEY = "IsWin";

        public static uint CollectedGems
        {
            get => (uint)PlayerPrefs.GetInt(GEMS_KEY, 0);
            set => PlayerPrefs.SetInt(GEMS_KEY, (int)value);
        }

        public static uint KilledEnemies
        {
            get => (uint)PlayerPrefs.GetInt(ENEMIES_KEY, 0);
            set => PlayerPrefs.SetInt(ENEMIES_KEY, (int)value);
        }

        public static bool IsWin
        {
            get => PlayerPrefs.GetInt(WIN_KEY, 0) == 1;
            set => PlayerPrefs.SetInt(WIN_KEY, value ? 1 : 0);
        }

        /// <summary>
        /// Add gems to the player's collection
        /// </summary>
        /// <param name="amount">Amount of gems to add</param>
        public static void AddGems(uint amount)
        {
            CollectedGems += amount;
            SaveData();
        }

        /// <summary>
        /// Save all game stats to PlayerPrefs
        /// </summary>
        public static void SaveData()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Reset only level-specific stats (kills, win status)
        /// Keep gems persistent across levels
        /// </summary>
        public static void ResetLevelStats()
        {
            KilledEnemies = 0;
            IsWin = false;
            SaveData();
        }

        /// <summary>
        /// Reset all stats including gems (for new game)
        /// </summary>
        public static void ResetAllStats()
        {
            CollectedGems = 0;
            KilledEnemies = 0;
            IsWin = false;
            SaveData();
        }

        /// <summary>
        /// Show current stats for debugging
        /// </summary>
        public static void ShowStats()
        {
            Debug.Log($"💎 Current Stats 💎\n" +
                     $"Gems: {CollectedGems}\n" +
                     $"Enemies Killed: {KilledEnemies}\n" +
                     $"Level Won: {IsWin}");
        }
    }
}