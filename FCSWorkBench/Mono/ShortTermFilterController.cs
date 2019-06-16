using FCSCommon.Enums;
using FCSCommon.Utilities;

namespace FCSTechWorkBench.Mono
{
    public class ShortTermFilterController : Filter
    {
        public override FilterTypes FilterType { get; set; } = FilterTypes.ShortTermFilter;

        public override float GetMaxTime()
        {
            return MaxTime;
        }

        public override void StartTimer()
        {
            RunTimer = true;
            QuickLogger.Debug($"RunTimer was Set to {RunTimer}", true);
        }

        public override void StopTimer()
        {
            RunTimer = false;
            QuickLogger.Debug($"Runtimer was Set to {RunTimer}", true);
        }

        public override void Initialize()
        {
            SetMaxTime();
        }

        public override string GetRemainingTime()
        {
            return RemainingTime;
        }

    }
}
