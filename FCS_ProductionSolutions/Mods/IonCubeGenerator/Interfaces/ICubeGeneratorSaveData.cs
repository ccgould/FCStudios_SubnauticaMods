using FCS_ProductionSolutions.Mods.IonCubeGenerator.Enums;

namespace FCS_ProductionSolutions.Mods.IonCubeGenerator.Interfaces
{
    internal interface ICubeGeneratorSaveData
    {
        int NumberOfCubes { get; set; }

        float StartUpProgress { get; set; }
        float GenerationProgress { get; set; }
        float CoolDownProgress { get; set; }

        SpeedModes CurrentSpeedMode { get; set; }
    }
}