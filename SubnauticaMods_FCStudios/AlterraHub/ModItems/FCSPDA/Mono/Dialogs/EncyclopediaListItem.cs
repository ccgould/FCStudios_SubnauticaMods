using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCSCommon.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs
{
    internal class EncyclopediaListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TMP_Text _label;
        private Button _button;
        private EncyclopediaMainTabController _controller;
        private EncyclopediaTabController _encyclopediaPage;
        private EncyclopediaEntryData _entryData;

        private void Awake()
        {
            _label = gameObject.GetComponentInChildren<TMP_Text>();
            _button = gameObject.GetComponent<Button>();

            _button.onClick.AddListener(() =>
            {
                if(FCSPDAController.Main.Screen.GetCurrentPage() == PDAPages.Encyclopedia)
                {
                    QuickLogger.Debug($"Passing Data: {_entryData.Path}");
                    _encyclopediaPage.SetData(_entryData);
                }
                else
                {
                    FCSPDAController.Main.Screen.GoToPage(PDAPages.Encyclopedia, _label.text);
                }
            });

        }

        internal void Initialize(string text, EncyclopediaEntryData item = null)
        {
            if(_label != null)
            {
                _label.text = text;
            }
            _controller  = EncyclopediaMainTabController.Instance;
            _encyclopediaPage = EncyclopediaTabController.Instance;
            _entryData = item;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //TODO decouple
            if (_controller == null) return;
            _controller.HoverTriggered(_label.text);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_controller == null) return;
            _controller.Clear();
        }
    }
}