using FCSCommon.Helpers;
using System.Collections;

namespace FCS_DeepDriller.Mono
{
    internal class FCSDeepDrillerFocusController : AIDisplay
    {
        private FCSDeepDrillerController _mono;

        public void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;
        }

        public override void ClearPage()
        {
            throw new System.NotImplementedException();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            throw new System.NotImplementedException();
        }

        public override void ItemModified<T>(T item)
        {
            throw new System.NotImplementedException();
        }

        public override bool FindAllComponents()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator PowerOff()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator ShutDown()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator CompleteSetup()
        {
            throw new System.NotImplementedException();
        }

        public override void DrawPage(int page)
        {
            throw new System.NotImplementedException();
        }
    }
}
