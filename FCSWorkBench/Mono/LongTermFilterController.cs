using FCSCommon.Enums;
using FCSCommon.Utilities;

namespace FCSTechWorkBench.Mono
{
    public class LongTermFilterController : Filter
    {
        public override FilterTypes FilterType { get; set; } = FilterTypes.LongTermFilter;

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
