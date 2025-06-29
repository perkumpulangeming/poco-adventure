using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Items.Gem
{
    public sealed class GemController : ItemController
    {
        public delegate void Collect();

        public static event Collect OnItemCollect;

        [SerializeField] private uint rewardAmount;

        protected override void OnPickUp()
        {
            GameStats.AddGems(rewardAmount);
            OnItemCollect?.Invoke();
        }
    }
}