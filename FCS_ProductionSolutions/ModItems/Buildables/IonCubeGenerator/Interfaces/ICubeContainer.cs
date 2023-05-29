using System.Collections;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Interfaces;

/// <summary>
/// Defines the rules of a CubeContainer
/// </summary>
internal interface ICubeContainer
{
    bool IsFull { get; }
    int NumberOfCubes { get; set; }

    void AttemptToOpenStorage(FCSPDAController fcspda);
    IEnumerator OpenStorage();
}
