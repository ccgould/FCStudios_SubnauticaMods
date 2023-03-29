using FCS_AlterraHub.Models.Interfaces;
using FCSCommon.Utilities;
using Newtonsoft.Json;
using SMLHelper.Utility;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services
{
    public class SaveLoadDataService : MonoBehaviour,IDataService
    {
        public static SaveLoadDataService instance;
        private const string KEY = "LFjEdEtOvSiINblqfiM4HvRNRngKtkC9OejYkXLNF7M=";
        private const string IV = "uMwo8t0X3gV5aXeI32DETw==";

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        public bool SaveData<T>(string modName, T Data, bool Encrypted, Action callBack)
        { 
            string path = GetSaveFileDirectory(modName);
            string filePath = Path.Combine(path, "SaveData.json");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                if (File.Exists(filePath))
                {
                    QuickLogger.Debug("Data Exists. Deleting old data file and writing new one!.");
                    File.Delete(filePath);
                }
                else
                {
                    QuickLogger.Debug("Creating new Save File");
                }

                using FileStream stream = File.Create(filePath);

                if(Encrypted)
                {
                    WriteEncryptedData(Data, stream);
                }
                else
                {
                    stream.Close();
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(Data));
                }

                callBack?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error("Unable to save data due to:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }          
        }
        
        internal static string GetSaveFileDirectory(string ModPackID)
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModPackID);
        }

        private void WriteEncryptedData<T>(T data, FileStream stream)
        {
            using Aes aesProvider = Aes.Create();
            aesProvider.Key = Convert.FromBase64String(KEY);
            aesProvider.IV = Convert.FromBase64String(IV);
            using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
            using CryptoStream cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Write);
            var bytes = Encoding.ASCII.GetBytes(CreatejsonData(data));
            cryptoStream.Write(bytes, 0, bytes.Length);
        }

        private static string CreatejsonData<T>(T data)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                //TypeNameHandling = TypeNameHandling.Auto
            };

            return JsonConvert.SerializeObject(data,Formatting.Indented, settings);
        }

        private T ReadEncryptedData<T>(string path)
        {
            byte[] fileBytes = File.ReadAllBytes(path);
            using Aes aesProvider = Aes.Create();
            aesProvider.Key = Convert.FromBase64String(KEY);
            aesProvider.IV = Convert.FromBase64String(IV);
            using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(aesProvider.Key,aesProvider.IV);
            using MemoryStream decryptionStream = new MemoryStream(fileBytes);
            using CryptoStream cryptoStream = new CryptoStream(decryptionStream, cryptoTransform, CryptoStreamMode.Read);
            using StreamReader reader = new StreamReader(cryptoStream);
            string result = reader.ReadToEnd();

            QuickLogger.Info($"Decrypted result (if the following is not legible, probably wrong key or iv): {result}");
            return JsonConvert.DeserializeObject<T>(result);
        }

        public void LoadData<T>(string modName, bool Encrypted,Action<T> dataCallback)
        {
            string path = GetSaveFileDirectory(modName);
            string filePath = Path.Combine(path, "SaveData.json");

            if (!File.Exists(filePath))
            {
                QuickLogger.Warning($"File doesn't exist at path: {filePath}. May be first load.");
                return;
                //throw new FileNotFoundException($"{path} doe not exisit!");
            }

            try
            {
                T data;
                if(Encrypted)
                {
                    data = ReadEncryptedData<T>(filePath);
                }
                else
                {
                    data = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                }
                dataCallback?.Invoke(data);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Failed to load data due to:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                throw e;
            }

        }
    }
}
