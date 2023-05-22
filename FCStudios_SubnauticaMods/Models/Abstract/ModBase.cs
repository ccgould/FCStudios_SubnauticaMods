using Nautilus.Assets;
using Nautilus.Crafting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract;
public abstract class ModBase
{

    public GameObject Prefab { get; set; }
    protected Action OnStartRegister;
    protected Action OnFinishRegister;

    /// <summary>
    /// Information for registering the prefab.
    /// </summary>
    public PrefabInfo PrefabInfo { get; set; }

    /// <summary>
    /// The ClassID of this object, sourced from the PrefabInfo property.
    /// </summary>
    public string ClassID
    {
        get
        {
            return PrefabInfo.ClassID;
        }
    }

    public string FriendlyName { get; set; }

    /// <summary>
    /// The TechType of this creature, sourced from the PrefabInfo property.
    /// </summary>
    public TechType TechType
    {
        get
        {
            return PrefabInfo.TechType;
        }

    }

    public virtual RecipeData GetRecipe()
    {
        return new RecipeData
        {
            craftAmount = 1,
            Ingredients = new List<CraftData.Ingredient>
            {
                new CraftData.Ingredient(TechType.Titanium)
            }
        };
    }

    public ModBase(string friendlyName)
    {
        FriendlyName = friendlyName;
    }
}
