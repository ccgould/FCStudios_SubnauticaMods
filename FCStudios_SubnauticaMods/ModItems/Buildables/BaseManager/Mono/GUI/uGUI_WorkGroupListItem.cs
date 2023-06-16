using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI;
internal class uGUI_WorkGroupListItem : MonoBehaviour
{
    private string guid;
    private FCSDevice fcsDevice;
    private List<IWorkUnit> workUnits;

    [SerializeField] private TextMeshProUGUI text;

    [Header("Toggle Options")]
    [SerializeField] private uGUI_Icon uGUIIconOn;
    private TechType techType;

    public void Initialize(TechType techType)
    {
        uGUIIconOn.sprite = SpriteManager.Get(techType);
        text.text = Language.main.Get(techType);
        this.techType = techType;
        gameObject.SetActive(true);
    }

    public void Initialize(FCSDevice fcsDevice)
    {
        this.fcsDevice = fcsDevice;
        uGUIIconOn.sprite = SpriteManager.Get(fcsDevice.GetTechType());
        text.text = fcsDevice.GetDeviceName();
        gameObject.SetActive(true);
    }

    public void Initialize(string guid, List<IWorkUnit> workUnits)
    {
        text.text = guid;
        this.workUnits = workUnits;
        this.guid = guid;
        gameObject.SetActive(true);
    }

    public FCSDevice GetDevice() { return fcsDevice; }

    public void OnEditButtonClicked()
    {

    }

    public void OnDeleteButtonClicked()
    {

    }

    public void OnTogglePower(bool value)
    {

    }

    internal TechType GetTechType()
    {
        return techType;
    }
}
