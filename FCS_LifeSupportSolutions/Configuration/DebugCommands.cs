using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Configuration
{
    internal class DebugCommands
    {
        private static Survival _survival;

        
        [ConsoleCommand("sethealth")]
        public static string SetHealthCommand(int value)
        {
            Player.main.liveMixin.health = value;
            return $"Parameters: {value}";
        }

        [ConsoleCommand("setfood")]
        public static string SetFoodValueCommand(float value)
        {
            if (_survival == null)
            {
                _survival = Player.mainObject.GetComponent<Survival>();
            }
            
            _survival.food = Mathf.Clamp( value, 0, 100f);
            return $"Parameters: {value}";
        }

        [ConsoleCommand("setwater")]
        public static string SetWaterValueCommand(float value)
        {
            if (_survival == null)
            {
                _survival = Player.mainObject.GetComponent<Survival>();
            }

            _survival.water = Mathf.Clamp(value, 0, 100f);
            return $"Parameters: {value}";
        }

        [ConsoleCommand("setvitals")]
        public static string SetVitalsValueCommand(float value)
        {
            if (_survival == null)
            {
                _survival = Player.mainObject.GetComponent<Survival>();
            }
            Player.main.liveMixin.health = Mathf.Clamp(value, 0, 100f);
            _survival.water = Mathf.Clamp(value, 0, 100f);
            _survival.food = Mathf.Clamp(value, 0, 100f);
            return $"Parameters: {value}";
        }
    }
}
