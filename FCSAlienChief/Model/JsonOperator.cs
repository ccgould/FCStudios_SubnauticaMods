using Oculus.Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FCSAlienChief
{
    /// <summary>
    /// Class to handle Json Operations
    /// </summary>
    public static class JsonOperator
    {
        /// <summary>
        /// Creates C# objects from a json file using Json.net
        /// </summary>
        /// <returns></returns>
        public static JsonObject CreateJsonObject()
        {
            Log.Info("Creating Json Object");

            // Find the location of the dll
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string jsonFile = Path.Combine(assemblyFolder, "config.json");

            // Read the config file
            Log.Info($"Config Path: {jsonFile}");
            var json = File.ReadAllText(jsonFile);
            // Deserialize the config file intoa JsonObject
            var jObj = JsonConvert.DeserializeObject<JsonObject>(json);

            Log.Info("Created Json Object");
            Log.Info("------------------------------------------------------------------");
            return jObj;
        }

        /// <summary>
        /// Representation of an json object
        /// </summary>
        public class JsonObject
        {
            /// <summary>
            /// Boolean to enable debuging mode
            /// </summary>
            public bool Debugging { get; set; }

            /// <summary>
            /// An <see cref="ObjItem"/> that stores information about the food category
            /// </summary>
            public ObjItem Foods { get; set; }

            /// <summary>
            /// An <see cref="ObjItem"/> that stores information about the drinks category 
            /// </summary>
            public ObjItem Drinks { get; set; }


            /// <summary>
            /// An <see cref="ObjItem"/> that stores information about the condiments category
            /// </summary>
            public ObjItem Condiments { get; set; }

            /// <summary>
            /// An <see cref="ObjItem"/> that stores information about the resources category
            /// </summary>
            public ObjItem Resources { get; set; }
        }
        /// <summary>
        /// An object containg the inventory of the category
        /// </summary>
        public class ObjItem
        {
            /// <summary>
            /// A boolean that states if the item is a FCSAlienChief tool
            /// </summary>
            public bool isTool { get; set; }

            /// <summary>
            /// A list of <see cref="Inventory"/> that stores the different food items in that category
            /// </summary>
            public List<Inventory> Inventory { get; set; }
        }
        /// <summary>
        ///  An object containing the structur of an Inventory
        /// </summary>
        public class Inventory
        {
            /// <summary>
            /// A list of <see cref="LinkedItems"/> that will be returned after crafting
            /// </summary>
            public List<string> LinkedItems { get; set; }

            /// <summary>
            /// The amount of items returned of that item
            /// </summary>
            public int CraftingAmount { get; set; }

            /// <summary>
            /// Declares if thatt item is enabled and viewabl in the game
            /// </summary>
            public bool Enabled { get; set; }

            /// <summary>
            /// The NameID of the item used as a unique identifier
            /// <example>"peeperBurger1"</example>
            /// </summary>
            public string ItemName { get; set; }

            /// <summary>
            /// The special name of the item 
            /// <example>"Peeper Burger"</example>
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Icon of the item for use in the parent category
            /// </summary>
            public string Icon { get; set; }

            /// <summary>
            /// The description of the item with mouse is hovered over the item
            /// </summary>
            public string ToolTip { get; set; }

            /// <summary>
            /// A list of <see cref="Ingredients"/> that stores the items need to craft this item
            /// </summary>
            public List<Ingredient> Ingredients { get; set; }

            /// <summary>
            /// A object storing the size of the item in the inventory.
            /// <example>"X:1 Y:1" makes a one slot item</example>
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            /// A object that stores the Food and Water values of the item
            /// </summary>
            public Values Values { get; set; }
        }

        /// <summary>
        /// An object definning the values object
        /// </summary>
        public class Values
        {
            /// <summary>
            /// The food value of the item
            /// </summary>
            public int Food { get; set; }

            /// <summary>
            /// The water value of the item
            /// </summary>
            public int Water { get; set; }
        }

        /// <summary>
        /// An object definning the size object
        /// </summary>
        public class Size
        {
            /// <summary>
            /// The amount of cubes taken in the X axis in the inventory
            /// </summary>
            public int X { get; set; }

            /// <summary>
            /// The amount of cubes taken in the YX axis in the inventory
            /// </summary>
            public int Y { get; set; }
        }
        /// <summary>
        /// An object definning the ingredient object
        /// </summary>
        public class Ingredient
        {
            /// <summary>
            /// The amount of the item need
            /// </summary>
            public int Amount { get; set; }

            /// <summary>
            /// The item need for the formula
            /// </summary>
            public string Item { get; set; }
        }
    }
}
