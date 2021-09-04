using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Model;
using UnityEngine;

namespace FCSDemo.Mono
{
    internal class FCSDemoController : FcsDevice,IHandTarget
    {
        private bool _runStartUpOnEnable;


        public string Name => gameObject.name;
        public override bool IsInitialized { get; set; }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }
                _runStartUpOnEnable = false;
            }
        }
        
        public override void Initialize()
        {
            QuickLogger.Info("Initializing",true);
            
            if (_colorManager == null)
            {
                QuickLogger.Info($"Creating Color Component", true);
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol, AlterraHub.TBaseEmission);

                if (QPatch.Configuration.ControlEmissionStrength)
                {
                    MaterialHelpers.ChangeEmissionStrength(AlterraHub.TBaseEmission, gameObject, QPatch.Configuration.EmissionStrength);
                }
            }
            QuickLogger.Info("Initialized", true);
        }

        public void Save(SaveData newSaveData)
        {

        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
           reason = String.Empty;
           return true;
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractTextRaw($"Item PrefabID: {GetPrefabID()}","");
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            
            var result = _colorManager.ChangeColor(template);

            var lights = gameObject.GetComponentsInChildren<Light>();
            if (lights != null)
            {
                foreach (Light light in lights)
                {
                    light.color = template.EmissionColor;
                }
            }
            return result;

        }
    }
}
