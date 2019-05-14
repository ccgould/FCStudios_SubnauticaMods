using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FCSCommon.Objects
{
    public static class Colors
    {
        public static Color SystemOnColor { get; } = new Color(0.08235294f, 1f, 1f);
        public static Color SystemOffColor { get; } = new Color(1f, 0.07843138f, 0.07843138f);
        public static Color SystemChargeColor { get; } = new Color(0.8627452f, 0.6784314f, 0.3098039f);
    }
}
