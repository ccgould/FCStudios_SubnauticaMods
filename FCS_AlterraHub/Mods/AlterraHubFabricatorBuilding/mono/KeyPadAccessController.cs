﻿using FCS_AlterraHub.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    public class KeyPadAccessController : MonoBehaviour
    {
        public string _accessCode;
        public GameObject keypadUI;
        public GameObject unlockIcon;
        public GameObject buttonHolder;
        public Text numberField;
        public GameObject root;
        public FMOD_CustomEmitter keyInputSound;
        public FMOD_CustomEmitter acceptedSound;
        public FMOD_CustomEmitter rejectedSound;
        public bool unlocked;
        private bool tempDisable;
        private DoorController _door;
        private int _id;

        internal void Initialize(string accessCode, DoorController door, int id)
        {
            _id = id;
            _door = door;

            keyInputSound = FModHelpers.CreateCustomEmitter(gameObject, "input_number", "event:/env/input_number");
            acceptedSound = FModHelpers.CreateCustomEmitter(gameObject, "keypad_correct", "event:/env/keypad_correct");
            rejectedSound = FModHelpers.CreateCustomEmitter(gameObject, "keypad_wrong", "event:/env/keypad_wrong");

            keypadUI = GameObjectHelpers.FindGameObject(gameObject, "DialPadBG");
            unlockIcon = GameObjectHelpers.FindGameObject(gameObject, "LockIcon");
            buttonHolder = GameObjectHelpers.FindGameObject(gameObject, "DialPad");
            numberField = GameObjectHelpers.FindGameObject(gameObject, "DisplayText").GetComponent<Text>();
            var backBTN = GameObjectHelpers.FindGameObject(gameObject, "BackButton").GetComponent<Button>();
            backBTN.onClick.AddListener(BackspaceButtonPress);

            _accessCode = accessCode;

            keypadUI.SetActive(false);
            unlockIcon.SetActive(false);
            
            int num = 1;
            foreach (Transform trans in buttonHolder.transform)
            {
                var btn = trans.gameObject.EnsureComponent<Button>();

                var keypadDoorConsoleButton = trans.gameObject.EnsureComponent<KeypadDoorConsoleButton>();
                keypadDoorConsoleButton.index = num;
                btn.onClick.AddListener(keypadDoorConsoleButton.OnNumberButtonPress);
                trans.GetChild(0).GetComponent<Text>().text = num.ToString();
                num++;
            }
        }

        public void NumberButtonPress(int index)
        {
            if (numberField.text.Length <= 3)
            {
                if (keyInputSound)
                {
                    keyInputSound.Play();
                }
                numberField.text = numberField.text + index;
            }
            if (numberField.text.Length == 4)
            {
                if (numberField.text.Equals(_accessCode))
                {
                    Unlock();
                    if (acceptedSound)
                    {
                        acceptedSound.Play();
                        return;
                    }
                }
                else
                {
                    tempDisable = true;
                    numberField.color = Color.red;
                    Invoke("ResetNumberField", 2f);
                    if (rejectedSound)
                    {
                        rejectedSound.Play();
                    }
                }
            }
        }

        internal void Unlock()
        {
            unlocked = true;
            tempDisable = true;
            numberField.color = Color.green;
            Invoke("AcceptNumberField", 2f);
        }

        private void AcceptNumberField()
        {
            _door.UnlockDoor();
            UnlockDoor();
        }

        public void UnlockDoor()
        {
            keypadUI.SetActive(false);
            unlockIcon.SetActive(true);
        }

        public void LockDoor()
        {
            keypadUI.SetActive(true);
            unlockIcon.SetActive(false);
        }

        private void ResetNumberField()
        {
            numberField.color = Color.white;
            numberField.text = "";
            tempDisable = false;
        }

        public void BackspaceButtonPress()
        {
            if (tempDisable)
            {
                return;
            }
            if (numberField.text.Length > 0)
            {
                numberField.text = numberField.text.Remove(numberField.text.Length - 1);
            }
        }

        public void TurnOn()
        {
            keypadUI.SetActive(true);
        }

        public void ForceOpen()
        {
            _door.Open();
        }

        public bool IsUnlocked()
        {
            return unlocked;
        }

        public void Lock()
        {
            unlocked = false;
            ResetNumberField();
            LockDoor();
            _door.LockDoor();
            _door.Close();
        }
    }
}
