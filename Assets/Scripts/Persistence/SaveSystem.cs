using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{

    // Main function to save player progress
    public static void savePlayerProgress(bool onboardingCleared) {
        // Set up
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + getPlayerProgressPath();
        FileStream stream = new FileStream(path, FileMode.Create);

        // Save
        PlayerProgressSaveData saveData = new PlayerProgressSaveData(onboardingCleared);
        formatter.Serialize(stream, saveData);
        stream.Close();
    }


    // Main function to load player progress: can return null if player never created a file yet
    public static PlayerProgressSaveData loadPlayerProgress() {
        // Set up
        string path = Application.persistentDataPath + getPlayerProgressPath();

        // If file exists, just return that file
        if (File.Exists(path)) {
            // Set up
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            PlayerProgressSaveData saveData = formatter.Deserialize(stream) as PlayerProgressSaveData;
            stream.Close();

            return saveData;

        // Else return null
        } else {
            return null;
        }
    }


    // Helper function to get the save data path
    private static string getPlayerProgressPath() {
        return "/PlayerProgress.bin";
    }

}
