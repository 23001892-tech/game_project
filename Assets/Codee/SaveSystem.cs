using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");

    public static GameSaveData currentData = new GameSaveData();

    // Hàm LƯU game thành file JSON
    public static void SaveGame()
    {
        string json = JsonUtility.ToJson(currentData, true); 
        File.WriteAllText(savePath, json);
        Debug.Log("Đã lưu game bằng JSON vào: " + savePath);
    }

    // Hàm TẢI game từ file JSON lên RAM
    public static bool LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            currentData = JsonUtility.FromJson<GameSaveData>(json);
            return true; 
        }
        
        return false; // Không tìm thấy file save (Game mới hoàn toàn)
    }

    // Hàm XÓA dữ liệu (Dùng khi bấm New Game hoặc muốn xóa bộ nhớ test map)
    public static void ClearSaveData()
    {
        currentData = new GameSaveData(); 
        currentData.currentHealth = 100; // Giá trị mặc định khi bắt đầu game mới
        currentData.currentMana = 50; // Giá trị mặc định khi bắt đầu game mới
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Đã xóa file JSON, sẵn sàng cho game mới!");
        }
    }
}