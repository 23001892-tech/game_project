using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    // --- DỮ LIỆU PLAYER ---
    public string lastSavedScene;
    public float playerX;
    public float playerY;
    public int currentHealth;
    public int maxHealth;
    public int currentMana;
    public int maxMana;

    // --- DỮ LIỆU THẾ GIỚI (Mở rương, Cốt truyện) ---
    // Dùng Dictionary hoặc List để lưu ID của những rương đã mở
    public List<string> openedChestIDs = new List<string>();
    
    // Tiến trình cốt truyện (Ví dụ: đang ở Quest số mấy)
    public int currentQuestIndex;
    public List<string> completedStoryTriggers = new List<string>();
    public List<string> defeatedEnemyIDs = new List<string>();
}
