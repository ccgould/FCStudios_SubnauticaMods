using FCS_AlterraHub.API;
using FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
public class FCSDeviceErrorHandler : MonoBehaviour
{

    [SerializeField] private List<FCSDeviceErrorSO> errorList;

    private void Start()
    {

    }

    public void TriggerError(string errorCode)
    {
        var error = errorList.FirstOrDefault(x => x.errorCode == errorCode);

        if(error is not null)
        {
            
        }
    }
}
