using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCS_HomeSolutions.Mods.JukeBox.Mono
{
    internal class JukeBoxInstance
    {
        private string _file;
        private float _positionDirty;
        private static List<JukeBoxInstance> All = new List<JukeBoxInstance>();
        public string File
        {
            get => _file;
            set
            {
                _positionDirty = -0.5f;
                if (_file != value)
                {
                    _file = value;
                   // JukeBox.TrackInfo info = JukeBox.GetInfo(_file);
                }
                _file = value;
            }
        }

        public float Volume { get; set; }
        public Repeat Repeat { get; set; }
        public bool Shuffle { get; set; }

        public static void NotifyInfo(string id, TrackInfo info)
        {
            for (int i = 0; i < JukeBoxInstance.All.Count; i++)
            {
                var jukeBoxInstance = All[i];
                //if (jukeBoxInstance.file == id)
                //{
                //    jukeBoxInstance.SetLabel(info.Label);
                //    jukeBoxInstance.SetLabel(info.Length);
                //}
            }
        }

        public void OnRelease()
        {
            
        }

        public void OnControl()
        {

        }
    }
}
