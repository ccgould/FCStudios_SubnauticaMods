using FCS_AlterraHub.Models.Structs;
using UnityEngine;

namespace FCS_AlterraHub.Models;

public class ColorTemplate
{
    public Color PrimaryColor { get; set; } = Color.white;
    public Color SecondaryColor { get; set; } = Color.white;
    public Color EmissionColor { get; set; } = Color.white;
}

public struct ColorTemplateSave
{
    public Vec4 PrimaryColor { get; set; }
    public Vec4 SecondaryColor { get; set; }
    public Vec4 EmissionColor { get; set; }
}
