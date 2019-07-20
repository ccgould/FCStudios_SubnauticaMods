using FCSCommon.Objects;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSCommon.Models.Components
{
    [RequireComponent(typeof(WeldablePoint))]
    public class HealthController : MonoBehaviour
    {
        public LiveMixin LiveMixin { get; set; }

        /// <summary>
        /// This is the first thing to set before using this controller.
        /// If no live mixing data in supplied it will use the default live mixing data in <see cref="CustomLiveMixinData.Get"/>
        /// </summary>
        /// <param name="liveMixinData"></param>
        public void Startup(LiveMixin liveMixin, LiveMixinData liveMixinData)
        {
            if (liveMixin == null)
            {
                QuickLogger.Error($"{typeof(HealthController)}|| LiveMixing cannot be null!");
                return;
            }

            LiveMixin = liveMixin;

            if (liveMixinData == null)
            {
                QuickLogger.Error($"LiveMixing Data  is null!");
                QuickLogger.Info($"Creating Data");
                LiveMixin.data = CustomLiveMixinData.Get();
                QuickLogger.Info($"Created Data");
            }
            else
            {
                LiveMixin.data = liveMixinData;
            }
        }

        public float GetHealth()
        {
            return LiveMixin.health;
        }

        public void SetHealth(float health)
        {
            LiveMixin.health = Mathf.Clamp(health, 0, LiveMixin.maxHealth);
        }

        public void ApplyDamage(float amount)
        {
            LiveMixin.health = Mathf.Clamp(LiveMixin.health - amount, 0, LiveMixin.maxHealth);
        }

        public void ApplyHealth(float amount)
        {
            if (amount > 0)
            {
                LiveMixin.health = Mathf.Clamp(LiveMixin.health + amount, 0, LiveMixin.maxHealth);
            }
        }

        public void Kill()
        {
            LiveMixin.health = 0;

        }

        public void FullHealth()
        {
            LiveMixin.health = LiveMixin.maxHealth;
        }

        public bool IsFullHealth()
        {
            return LiveMixin.IsFullHealth();
        }
    }
}
