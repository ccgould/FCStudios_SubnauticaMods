using FCS_AlterraHub.Models.Structs;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Models;

[Serializable]
public class ColorTemplate
{
    public Color PrimaryColor { get; set; } = Color.white;
    public Color SecondaryColor { get; set; } = Color.white;
    public Color EmissionColor { get; set; } = Color.white;
    public string TemplateName { get; set; } = "New Template";
}

public struct ColorTemplateSave
{
    public Vec4 PrimaryColor { get; set; }
    public Vec4 SecondaryColor { get; set; }
    public Vec4 EmissionColor { get; set; }
}
