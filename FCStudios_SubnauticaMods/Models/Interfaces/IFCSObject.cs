using UnityEngine;

namespace FCS_AlterraHub.Models.Interfaces;
public interface IFCSObject
{
    TechType GetTechType();
    string GetPrefabID();
    Transform GetTransform();
}
