using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantumTeleporter.Managers
{
    internal class QTPingManager
    {
        private PingInstance _ping;

        internal void Initialize(PingInstance ping)
        {
            _ping = ping;
        }

        internal void SetName(string name)
        {
            _ping.SetLabel(name);
        }

        internal void ChangeColor(int index = 0)
        {
            _ping.SetColor(index);
        }

        internal void TogglePing(bool visible)
        {
            _ping.SetVisible(visible);
        }
    }
}
