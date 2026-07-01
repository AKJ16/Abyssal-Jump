using UnityEngine;
using System.IO;
using System;

public class SaveSystem : MonoBehaviour
{
    // Attributes

    private static string path = Application.persistentDataPath + "/savedata.txt";
    private static string encryptionKey = "AbyssalJump123!";

    // Methods

    public static void SaveBestTime(int score)
    {
        string data = score.ToString();
        string encryptedData = EncryptDecrypt(data);

#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString("BestTimeData", encryptedData);
        PlayerPrefs.Save();
#else
        File.WriteAllText(path, encryptedData);
#endif
    }

    public static int LoadBestTime()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (PlayerPrefs.HasKey("BestTimeData"))
        {
            try
            {
                string encryptedData = PlayerPrefs.GetString("BestTimeData");
                string decryptedData = EncryptDecrypt(encryptedData);
                return int.Parse(decryptedData);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        return 0;
#else
        if (File.Exists(path))
        {
            try
            {
                string encryptedData = File.ReadAllText(path);
                string decryptedData = EncryptDecrypt(encryptedData);
                return int.Parse(decryptedData);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        return 0;
#endif
    }

    private static string EncryptDecrypt(string text)
    {
        string result = "";
        for (int i = 0; i < text.Length; i++)
        {
            result += (char)(text[i] ^ encryptionKey[i % encryptionKey.Length]);
        }
        return result;
    }
}