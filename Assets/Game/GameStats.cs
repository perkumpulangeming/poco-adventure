using UnityEngine;

namespace Game
{
    public struct GameStats
    {
        private const string GEMS_SAVE_KEY = "CollectedGems";
        
        private static uint _collectedGems;
        
        public static uint CollectedGems 
        { 
            get 
            {
                // Load from PlayerPrefs if not already loaded
                if (_collectedGems == 0 && PlayerPrefs.HasKey(GEMS_SAVE_KEY))
                {
                    _collectedGems = (uint)PlayerPrefs.GetInt(GEMS_SAVE_KEY, 0);
                }
                return _collectedGems;
            }
            set 
            {
                _collectedGems = value;
                // Save to PlayerPrefs immediately when changed
                PlayerPrefs.SetInt(GEMS_SAVE_KEY, (int)_collectedGems);
                PlayerPrefs.Save();
            }
        }
        
        public static uint KilledEnemies { get; set; }
        public static bool IsWin { get; set; }
        
        /// <summary>
        /// Reset only the session-specific stats (kills, win status) but keep persistent gems
        /// </summary>
        public static void ResetSessionStats()
        {
            KilledEnemies = 0;
            IsWin = false;
        }
        
        /// <summary>
        /// Reset all stats including persistent gems (use carefully, e.g., for new game)
        /// </summary>
        public static void ResetAllStats()
        {
            CollectedGems = 0;
            KilledEnemies = 0;
            IsWin = false;
        }
    }
}