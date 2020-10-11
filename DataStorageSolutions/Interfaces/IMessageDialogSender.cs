using System;
using FCSTechFabricator.Enums;

namespace DataStorageSolutions.Interfaces
{
    internal interface IMessageDialogSender
    {
        void ShowMessageBox(string message,Action<FCSDialogResult> callback = null, FCSMessageBox mode = FCSMessageBox.OK);
    }
}
