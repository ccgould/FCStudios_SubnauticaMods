using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.MiniMedBay.Mono;
internal class MiniMedBayMedKitDispenser : HandTarget, IHandTarget
{
    [SerializeField] private MiniMedBayController _mono;
    [SerializeField] private MiniMedBayContainer medBayStorageContainer;

    public void OnHandHover(GUIHand hand)
    {
        HandReticle main = HandReticle.main;

        if (_mono == null || !_mono.IsInitialized || !_mono.IsConstructed || medBayStorageContainer == null) return;


        if (medBayStorageContainer.IsContainerFull)
        {
            main.SetIcon(HandReticle.IconType.Hand, 1f);
            main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("MedicalCabinet_PickupMedKit"));
        }
        else
        {
            if (medBayStorageContainer.GetIsEmpty() == false)
            {
                main.SetIcon(HandReticle.IconType.Hand, 1f);
                main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("MedicalCabinet_PickupMedKit"));
                main.SetTextRaw(HandReticle.TextType.HandSubscript,
                    $"Progress: {Mathf.FloorToInt(medBayStorageContainer.Progress * 100)}%");
            }
            else
            {
                main.SetProgress(medBayStorageContainer.Progress);
                main.SetIcon(HandReticle.IconType.Progress);
            }
        }
    }

    public void OnHandClick(GUIHand hand)
    {
        if (_mono != null)
        {
            medBayStorageContainer.RemoveSingleKit();
        }
    }
}
