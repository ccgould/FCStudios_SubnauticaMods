using UnityEngine;

namespace FCS_AlterraHub.Model
{
    public class CustomLiveMixinData
    {
        private static LiveMixinData _instance;

        public static LiveMixinData Get()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = ScriptableObject.CreateInstance<LiveMixinData>();
            _instance.broadcastKillOnDeath = false;
            _instance.canResurrect = true;
            _instance.damageEffect = null;
            _instance.deathEffect = null;
            _instance.destroyOnDeath = false;
            _instance.electricalDamageEffect = null;
#if SUBNAUTICA_STABLE
            _instance.invincibleInCreative = false;
#elif BELOWZERO
            _instance.sendKillOnDeath = false;
            _instance.repairRequiresMaterials = false;
#endif
            
            _instance.knifeable = false;
            _instance.loopEffectBelowPercent = 0;
            _instance.loopingDamageEffect = null;
            _instance.maxHealth = 100;
            _instance.minDamageForSound = 0;
            _instance.passDamageDataOnDeath = false;
            _instance.weldable = true;

            return _instance;
        }

#if SUBNAUTICA
        internal class UniqueLiveMixinData
        {
            internal LiveMixinData Create(int maxHealth, bool weldable, bool knifeable, bool canResurrect, bool destroyOnDeath = false, bool passDamageDataOnDeath = false,
                bool invincibleInCreative = false, bool explodeOnDestroy = false, bool broadcastKillOnDeath = false, int minDamageForSound = 0,
                int loopEffectBelowPercent = 0, GameObject damageEffect = null, GameObject deathEffect = null, GameObject electricalDamageEffect = null,
                GameObject loopingDamageEffect = null)
            {
                var data = ScriptableObject.CreateInstance<LiveMixinData>();
                data.broadcastKillOnDeath = broadcastKillOnDeath;
                data.canResurrect = canResurrect;
                data.damageEffect = damageEffect;
                data.deathEffect = deathEffect;
                data.destroyOnDeath = destroyOnDeath;
                data.electricalDamageEffect = electricalDamageEffect;
                data.invincibleInCreative = invincibleInCreative;
                data.knifeable = knifeable;
                data.loopEffectBelowPercent = loopEffectBelowPercent;
                data.loopingDamageEffect = loopingDamageEffect;
                data.maxHealth = maxHealth;
                data.minDamageForSound = minDamageForSound;
                data.passDamageDataOnDeath = passDamageDataOnDeath;
                data.weldable = weldable;

                return data;
            }
        }
#elif BELOWZERO
        internal class UniqueLiveMixinData
        {
            internal LiveMixinData Create(int maxHealth, bool weldable, bool repairRequiresMaterials, bool knifeable, bool canResurrect, bool destroyOnDeath = false, bool passDamageDataOnDeath = false,
                bool invincibleInCreative = false, bool sendKillOnDeath = false, bool broadcastKillOnDeath = false, int minDamageForSound = 0,
                int loopEffectBelowPercent = 0, GameObject damageEffect = null, GameObject deathEffect = null, GameObject electricalDamageEffect = null,
                GameObject loopingDamageEffect = null)
            {
                var data = ScriptableObject.CreateInstance<LiveMixinData>();
                data.broadcastKillOnDeath = broadcastKillOnDeath;
                data.canResurrect = canResurrect;
                data.damageEffect = damageEffect;
                data.deathEffect = deathEffect;
                data.destroyOnDeath = destroyOnDeath;
                data.electricalDamageEffect = electricalDamageEffect;
                data.sendKillOnDeath = sendKillOnDeath;
                //data.invincibleInCreative = invincibleInCreative;
                data.knifeable = knifeable;
                data.repairRequiresMaterials = repairRequiresMaterials;
                data.loopEffectBelowPercent = loopEffectBelowPercent;
                data.loopingDamageEffect = loopingDamageEffect;
                data.maxHealth = maxHealth;
                data.minDamageForSound = minDamageForSound;
                data.passDamageDataOnDeath = passDamageDataOnDeath;
                data.weldable = weldable;

                return data;
            }
        }

#endif
    }
}
