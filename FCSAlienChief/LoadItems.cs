using FCSAlienChief.Items;
using FCSAlienChief.Model;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;

namespace FCSAlienChief
{
    /// <summary>
    /// A class that loads items into the game
    /// </summary>
    public class LoadItems
    {
        /// <summary>
        /// Current name of the inventory item
        /// </summary>
        private static string _name;

        /// <summary>
        /// A list of ingredients
        /// </summary>
        public static readonly List<Ingredient> Ingredientss = new List<Ingredient>();

        /// <summary>
        /// A List of <see cref="IAlienChiefItem"/>
        /// </summary>
        public static readonly List<IAlienChiefItem> ObjItems = new List<IAlienChiefItem>();

        /// <summary>
        /// Execute to start the creation process to load the items into the game
        /// </summary>
        public static void Patch()
        {
            // Create Json Objects from Config file in mod folder
            var jsonObject = JsonOperator.CreateJsonObject();

            foreach (var curFood in jsonObject.Foods.Inventory)
            {
                // make a variable for the ref;
                var foods = curFood;

                if (!foods.Enabled)
                {
                    return;
                }

                // Lets make sure that the object has a name and the Size is correct
                VerifyObjectData(ref foods);

                // Make a new FoodItem
                var foodPrefab = new FoodItem(
                    foods.ItemName,
                    foods.Name,
                    foods.ToolTip,
                    foods.Icon,
                    foods.Values.Food,
                    foods.Values.Water,
                    foods.CraftingAmount,
                    Ingredientss
                );

                foodPrefab.RegisterItem();

                // Add item to list
                ObjItems.Add(foodPrefab);
                
                Ingredientss.Clear();
                //LinkedItems.Clear();
            }

            CustomFabricator customFabricator = new CustomFabricator(ObjItems, "AlienChiefFabricator");
            customFabricator.RegisterAlienChiefFabricator();
        }


        #region Item Verification Methods
        /// <summary>
        /// Verifies the current Object in the list and replaces data if incorrect
        /// </summary>
        /// <example>
        /// An example of the Varification
        /// Amount = 0 Item = Peeper => Amount = 1 Item Peeper
        /// </example>
        /// <param name="inventory">The inventory of the current category of items type of <see cref="JsonOperator.Inventory"/></param>
        private static void VerifyObjectData(ref JsonOperator.Inventory inventory)
        {
            #region Reference variables
            var inventoryName = inventory.Name;
            var sizeX = inventory.Size.X;
            var sizeY = inventory.Size.Y;
            #endregion

            // Check the name of the object and fix if empty
            CheckName(ref inventoryName);

            // Set Global Name
            _name = inventoryName;

            //Check Item Size
            CheckItemSize(ref sizeX, ref sizeY);
            inventory.Size.X = sizeX;
            inventory.Size.Y = sizeY;

            // Check Ingredients
            foreach (var ingredient in inventory.Ingredients)
            {

                #region Reference variables
                var ingredientItem = ingredient.Item;
                var ingredientAmount = ingredient.Amount;
                #endregion

                // Check the current ingredient from the list.
                CheckIngredients(ref ingredientItem, ref ingredientAmount);

                // if the ingredient item has changed replace it else don't
                if (ingredient.Item != ingredientItem) { ingredient.Item = ingredientItem; }

                // if the ingredient amount has changed replace it else don't
                if (ingredient.Amount != ingredientAmount) { ingredient.Amount = ingredientAmount; }
            }
        }

        /// <summary>
        /// Checks the Size X/Y of the current <see cref="JsonOperator.Inventory"/>  object and corrects any errors
        /// </summary>
        /// <param name="x"> The X value</param>
        /// <param name="y">The Y value</param>
        private static void CheckItemSize(ref int x, ref int y)
        {
            if (x < 1 || x > 8)
            {
                x = 1;
                Log.Warning(_name, $"X Size can't be less than 1 or greater than 8 X value is current set to {y}");
                Log.Info(_name, "X was set to 1");
            }
            if (y < 1 || y > 8)
            {

                Log.Warning(_name, $"Y Size can't be less than 1 or greater than 8 Y value is current set to {y}");
                Log.Info(_name, "Y was set to 1");
            }
        }

        /// <summary>
        /// Checks the name of the current <see cref="JsonOperator.Inventory"/> item. 
        /// </summary>
        /// <param name="name">The name parameter of the current object</param>
        private static void CheckName(ref string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Name can't be blank";
                Log.Warning(_name, "Name can't be blank");
                Log.Info(_name, "Name was set to 'Name can't be blank'");
            }
        }

        /// <summary>
        /// Checks the Ingredients of the current <see cref="JsonOperator.Inventory"/> item
        /// </summary>
        /// <param name="item">The current item TechType parameter</param>
        /// <param name="amount">The current item amount parameter</param>
        public static void CheckIngredients(ref string item, ref int amount)
        {
            //If the current ingredient doesn't have the values Amount = 0 and Item = "None" 
            if ((amount != 0) && (item != "None"))
            {
                // Make sure the value isnt 0 and if it is set it to 1
                if (amount < 1)
                {
                    amount = 1;
                    Log.Warning(_name, $"Ingredient {item} amount must not be less than 1");
                    Log.Info(_name, $"Ingredient {item} amount was set to 1");
                }

                // Check if the TechType exist
                if (!Enum.IsDefined(typeof(TechType), item))
                {

                    //If the item is an custom item try to find the TechType if not found report an error
                    Log.Info($"Trying to get custom TechType: {item}");
                    if (TechTypeHandler.TryGetModdedTechType(item, out TechType customTechType))
                    {
                        Ingredientss.Add(new Ingredient(customTechType, amount));
                    }
                    else
                    {
                        item = $"Ingredient {item} invalid (Old value: {item})";
                        Log.Warning(_name, $"Ingredient{item} must be a TechType");
                        Log.Info(_name, $"Ingredient{item} was set to a dummy value and disabled");
                        amount = 0;
                    }

                }
                else
                {
                    // if the Item isnt custom just add it TODO remove if not needed
                    Ingredientss.Add(new Ingredient((TechType)Enum.Parse(typeof(TechType), item), amount));
                }
            }
            else
            {
                Log.Debug(_name, $"Ingredient {item} is disabled. Ignoring...");
            }
        }
        #endregion
    }
}
