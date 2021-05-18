using FCS_AlterraHub.Mono;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Helpers;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal abstract class WindSurferPlatformBase : FcsDevice,IFCSSave<SaveData>
    {
        public override void Initialize()
        {
            if (IsInitialized) return;
            CreateLadders();
            IsInitialized = true;

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
    }
}