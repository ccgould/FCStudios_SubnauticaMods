using UnityEngine;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Interfaces
{
    public interface IARSolutionDisplay<T>
    {
        /// <summary>
        /// The Maximum distance for interaction distance from the screen
        /// </summary>
        float MAX_INTERACTION_DISTANCE { get; set; }

        /// <summary>
        /// Resets the Idle timer
        /// </summary>
        void ResetIdleTimer();

        /// <summary>
        /// Handles button clicks on the UI
        /// </summary>
        /// <param name="btnName"></param>
        /// <param name="page"></param>
        void OnButtonClick(string btnName, GameObject page = null);

        T Controller { get; set; }

        void ChangePageBy(int amountToChangePageBy);
    }
}
