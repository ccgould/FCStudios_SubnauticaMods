﻿using System;

namespace FCS_AlterraHub.Core.Helpers;
public static class TimeConverters
{
    public static string SecondsToHMS(float seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);

        return $"{t.Hours:D2}h:{t.Minutes:D2}m:{t.Seconds:D2}s";
    }

    public static string SecondsToMS(float seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return $"{t.Minutes:D2}m:{t.Seconds:D2}s";
    }
}
