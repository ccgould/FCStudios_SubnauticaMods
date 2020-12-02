using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Objects;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_HomeSolutions.HoverLiftPad.Mono
{
    internal struct LevelData
    {
        private Vector3 _currentVector3Position;
        [JsonProperty] internal string LevelName { get; set; }
        [JsonProperty] internal Vec3 Position { get; set; }
        [JsonProperty] internal bool IsBase { get; set; }

        public Vector3 CurrentPosition()
        {
            if (_currentVector3Position == default)
            {
                _currentVector3Position = Position.ToVector3();
            }

            return _currentVector3Position;
        }
    }
}