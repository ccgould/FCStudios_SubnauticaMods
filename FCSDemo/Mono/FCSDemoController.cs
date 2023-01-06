using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
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

                if (Main.Configuration.ControlEmissionStrength)
                {
                    MaterialHelpers.ChangeEmissionStrength(AlterraHub.TBaseEmission, gameObject, Main.Configuration.EmissionStrength);
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
#if SUBNAUTICA
            main.SetInteractTextRaw($"Item PrefabID: {GetPrefabID()}","");
#else
            main.SetTextRaw(HandReticle.TextType.Hand,$"Item PrefabID: {GetPrefabID()}");
#endif
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
