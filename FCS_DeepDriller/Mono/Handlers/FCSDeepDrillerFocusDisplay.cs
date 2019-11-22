using FCSCommon.Utilities;
using System;
using System.Collections;
using FCSCommon.Abstract;
using UnityEngine;

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerFocusDisplay : AIDisplay
    {
        private FCSDeepDrillerController _mono;
        private Animator _animator;
        private int _screenState;

        internal void Setup(FCSDeepDrillerController mono)
        {
            QuickLogger.Debug("Loading Display");
            _mono = mono;
            _screenState = Animator.StringToHash("ScreenState");

            if (FindAllComponents())
            {
                StartCoroutine(StartScreen());
            }
        }

        public override void ClearPage()
        {
            throw new NotImplementedException();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            throw new NotImplementedException();
        }

        public override void ItemModified(TechType item, int newAmount = 0)
        {

        }

        public override bool FindAllComponents()
        {
            _animator = GetComponentInParent<Animator>();
            //GetComponentInParent<Animator>();
            _animator.enabled = true;
            if (_animator == null)
            {
                QuickLogger.Error("Cannot find Animator on the focus gameobject");
                return false;
            }

            return true;
        }

        internal IEnumerator StartScreen()
        {
            int i = 1;
            while (!_animator.GetBool(_screenState))
            {
                _animator.SetBool(_screenState, true);
                QuickLogger.Debug($"Attempt to start screen ({i++})");
                yield return null;
            }
        }

        public override IEnumerator PowerOff()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator ShutDown()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator CompleteSetup()
        {
            throw new NotImplementedException();
        }

        public override void DrawPage(int page)
        {
            throw new NotImplementedException();
        }
    }
}
