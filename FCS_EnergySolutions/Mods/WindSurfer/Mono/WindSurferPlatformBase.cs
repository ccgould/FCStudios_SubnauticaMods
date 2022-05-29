using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Configuration;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal abstract class WindSurferPlatformBase : FcsDevice,IFCSSave<SaveData>
    {
        public override void Initialize()
        {
            //if (IsInitialized) return;

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }
            
            CreateLadders();
            //IsInitialized = true;

        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
#if DEBUG
            QuickLogger.Debug($"Changing Alterra Solar Cluster color to {ColorList.GetName(color)}", true);
#endif

            return _colorManager.ChangeColor(template);
        }

        private void CreateLadders()
        {
            var t01 = GameObjectHelpers.FindGameObject(gameObject, "T01").EnsureComponent<LadderController>();
            t01.Set(GameObjectHelpers.FindGameObject(gameObject, "T01_Top"));

            var t02 = GameObjectHelpers.FindGameObject(gameObject, "T02").EnsureComponent<LadderController>();
            t02.Set(GameObjectHelpers.FindGameObject(gameObject, "T02_Top"));

            var t03 = GameObjectHelpers.FindGameObject(gameObject, "T03").EnsureComponent<LadderController>();
            t03.Set(GameObjectHelpers.FindGameObject(gameObject, "T03_Top"));

            var t04 = GameObjectHelpers.FindGameObject(gameObject, "T04").EnsureComponent<LadderController>();
            t04.Set(GameObjectHelpers.FindGameObject(gameObject, "T04_Top"));
        }

        public virtual WindSurferPowerController PowerController { get; set; }
        public abstract PlatformController PlatformController { get; }


        public virtual void TryMoveToPosition(){}
        public virtual void PoleState(bool value){}
        public abstract void Save(SaveData newSaveData, ProtobufSerializer serializer = null);

        public virtual void LoadFromSave(){}
    }
}