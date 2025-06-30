namespace Game
{
    public static class SceneNames
    {
        // Main Scenes
        public const string MainMenu = "MainMenu";
        public const string Credits = "Credits";
        public const string ExitMenu = "ExitMenu";
        public const string ShopMenu = "ShopMenu";

        // Level Scenes
        public const string Level_01 = "Level_01";
        public const string Level_02 = "Level_02";
        public const string Level_03 = "Level_03";
        public const string Level_Bonus = "Level_Bonus";

        // Template (biasanya tidak digunakan untuk gameplay)
        public const string URP2DSceneTemplate = "URP2DSceneTemplate";
    }

    public enum GameScene
    {
        MainMenu,
        Credits,
        ExitMenu,
        ShopMenu,
        Level_01,
        Level_02,
        Level_03,
        Level_Bonus
    }
}
