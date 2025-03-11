using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
internal class DDHolographSlot : MonoBehaviour

    //TODO Replace the GetComponentfromparent
{
    public Transform Target { get; set; }
    //internal DeepDrillerOperatorController DeepDrillerOperatorController
    //{
    //    get
    //    {
    //        if (_windsurferController == null)
    //        {
    //            _windsurferController = GetComponentInParent<DeepDrillerOperatorController>();
    //        }

    //        return _windsurferController;
    //    }
    //}

    //internal DDPlatformController PlatformController
    //{
    //    get
    //    {
    //        if (_platformController == null)
    //        {
    //            _platformController = GetComponentInParent<DDPlatformController>();
    //        }

    //        return _platformController;
    //    }
    //}

    //internal uGUI_DDHoloGraphControl HoloGraphControl
    //{
    //    get
    //    {
    //        if (_holoGraphControl == null)
    //        {
    //            _holoGraphControl = GetComponentInParent<uGUI_DDHoloGraphControl>();
    //        }

    //        return _holoGraphControl;
    //    }
    //}

    [SerializeField] private int _id;
    private uGUI_DDHeavyHomePage _holographHomePage;
    private DeepDrillerOperatorController _operatorController;
    private DDPlatformController _platformController;
    private Button _button;
    [SerializeField] private uGUI_DDHoloGraphControl _holoGraphControl;
    private bool _initialized;

    internal Vector2Int Direction
    {
        get
        {
            switch (_id)
            {
                case 1:
                    return Vector2Int.up;
                case 2:
                    return Vector2Int.left;
                case 3:
                    return Vector2Int.down;
                case 4:
                    return Vector2Int.right;
                default:
                    return Vector2Int.zero;
            }
        }
    }

    private void Start()
    {
        _button = gameObject.GetComponent<Button>();

        _button.onClick.AddListener((() =>
        {
            StartCoroutine(TryAddPlatform());
        }));

        InvokeRepeating(nameof(UpdateSlot), .1f, .1f);

        _initialized = true;
    }


    internal void Set(uGUI_DDHeavyHomePage holographHomePage,DeepDrillerOperatorController deepDrillerOperatorController, DDPlatformController PlatformController)
    {
        _holographHomePage = holographHomePage;
        _operatorController = deepDrillerOperatorController;
        _platformController = PlatformController;
    }

    internal DDPlatformController GetPlatformController()
    {
        return _platformController;
    }

    private IEnumerator TryAddPlatform()
    {
        var result = new TaskResult<bool>();
        yield return _holographHomePage.AddPlatform(this, GetPosition() + Direction, result);

        if (result.Get())
        {
            _operatorController.RefreshMST();
           //TODO Move this function in ther UI DeepDrillerOperatorController.RefreshHoloGrams();
        }
    }

    private void UpdateSlot()
    {
        try
        {
            _button.interactable = _operatorController.Grid.ElementAt(GetPosition() + Direction) == null;

        }
        catch (Exception ex)
        {
            QuickLogger.DebugError(ex.Message);
        }
    }


    private Vector2Int GetPosition()
    {
        return _operatorController.Grid.Position(_holoGraphControl);
    }

    internal int GetSlotID()
    {
        return _id;
    }
}
