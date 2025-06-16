using UnityEngine;

namespace Game
{
    public struct Global
    {
        public struct Layers
        {
            public static readonly int PlayerID = LayerMask.NameToLayer("Player");
            public static readonly int LadderID = LayerMask.NameToLayer("Ladder");
            public static readonly int GroundID = LayerMask.GetMask("Ground"); // TODO: Replace old layers to this.
        }

        public struct Levels
        {
            public const int MainMenu = 0;
            public const int Main = 1;
        }
    }
}