using FCSCommon.Objects;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCSCommon.Controllers
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class HealthController : MonoBehaviour
    {
        public LiveMixin LiveMixin { get; set; }
        public Action OnDamaged { get; set; }
        public Action OnRepaired { get; set; }

        /// <summary>
        /// This is the first thing to set before using this controller.
        /// If no live mixing data in supplied it will use the default live mixing data in <see cref="CustomLiveMixinData.Get"/>
        /// </summary>
        /// <param name="liveMixinData"></param>
        public void Initialize(LiveMixin liveMixin, LiveMixinData liveMixinData = null)
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

        public void HealthChecks() // In and InvokeRepeating
        {
            try
            {
                if (GetHealth() >= 1f && IsDamagedFlag)
                {
                    IsDamagedFlag = false;
                    OnRepaired?.Invoke();
                }

                if (GetHealth() <= 0f && !IsDamagedFlag)
                {
                    IsDamagedFlag = true;
                    OnDamaged?.Invoke();
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
        }

        public bool IsDamagedFlag { get; set; }

        public float GetHealth()
        {
            return LiveMixin.health;
        }

        public void SetHealth(float health)
        {
            LiveMixin.health = Mathf.Clamp(health, 0f, LiveMixin.maxHealth);
            QuickLogger.Debug($"Health set to {LiveMixin.health }", true);
        }

        public void ApplyDamage(float amount)
        {
            if (GetHealth() <= 0) return;
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
