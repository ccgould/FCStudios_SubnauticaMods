using System;
using System.Collections;

namespace FCS_AlterraHub.Enumerators
{
    public interface IUiEffect
    {
        IEnumerator Execute();
        event Action<IUiEffect> OnComplete;
    }
}
