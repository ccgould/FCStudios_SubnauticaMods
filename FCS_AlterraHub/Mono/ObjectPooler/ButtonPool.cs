using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_AlterraHub.Mono.ObjectPooler
{
    public class BUTTONPOOL : MonoBehaviour
    {

        GameObject buttonPrefab;
        List<GameObject> buttonPool;
        bool ready = false;

        public void CreateButtonPool(GameObject buttonPrefab, int amount = 400)
        {
            if (ready)
            {
                Debug.LogError("createButtonPool can only be called once");
                return;
            }

            this.buttonPrefab = buttonPrefab;

            //Create 400 Buttons
            buttonPool = new List<GameObject>();
            for (int i = 0; i < amount; i++)
            {
                buttonPool.Add(Instantiate(this.buttonPrefab) as GameObject);
            }
            ready = true;
        }

        //Creates/Enables single Button
        public GameObject Instantiate()
        {
            if (!ready)
            {
                showError();
                return null;
            }

            //Return any Button that is not active
            for (int i = 0; i < buttonPool.Count; i++)
            {
                if (!buttonPool[i].activeSelf)
                {
                    return buttonPool[i];
                }
            }

            //Create new Button if there is none available in the pool. Add it to the list then return it
            GameObject tempButton = Instantiate(this.buttonPrefab) as GameObject;
            buttonPool.Add(tempButton);
            return tempButton;
        }

        //Destroys/Disables single Button
        public void Destroy(GameObject button)
        {
            if (!ready)
            {
                showError();
                return;
            }
            button.SetActive(false);
        }

        //Destroys/Disables all Buttons
        public void DestroyAll()
        {
            if (!ready)
            {
                showError();
                return;
            }
            for (int i = 0; i < buttonPool.Count; i++)
            {
                if (buttonPool[i].activeSelf)
                {
                    buttonPool[i].SetActive(false);
                }
            }
        }

        private void showError()
        {
            Debug.LogError("createButtonPool must be called once before any other function can be called");
        }
    }
}
