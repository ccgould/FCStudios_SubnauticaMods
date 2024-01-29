using FCS_AlterraHub.Models;
using FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Core.Components.uGUIComponents;
internal class uGUI_ErrorListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text errorText;
    private DeviceErrorModel errorSO;

    private void Awake()
    {
        //errorText = gameObject.GetComponent<TMP_Text>();
        //InvokeRepeating(nameof(CheckIfResolved), 1, 1);
    }

    public void SetMessage(DeviceErrorModel errorModel)
    {
        errorSO = errorModel;

        gameObject.SetActive(true);

        if(errorModel is not null )
        {
            errorText.text = Language.main.Get(errorModel.errorMessage);
        }
    }

    private void CheckIfResolved()
    {
        var result = errorSO.Func?.Invoke() ?? false;

        if (result)
        {
            Destroy(gameObject);
        }
    }
}
