using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public enum MapMusicType
    {
        MainMenu,
        Combat,
        Boss1
    }
    [SerializeField] private MapMusicType currentMapMusic;
    void Start()
    {
        if (AudioManager.Instance != null)
        {
            switch (currentMapMusic)
            {
                case MapMusicType.MainMenu:
                    AudioManager.Instance.PlayBGM(AudioManager.Instance.MainMenuMusic);
                    break;
                case MapMusicType.Combat:
                    AudioManager.Instance.PlayBGM(AudioManager.Instance.CombatMusic);
                    break;
                case MapMusicType.Boss1:
                    AudioManager.Instance.PlayBGM(AudioManager.Instance.Boss1Theme);
                    break;
                default:
                    Debug.LogWarning("Loại nhạc không xác định cho bản đồ.");
                    break;
            }
        }
        else
        {
            return;
        }
    }


}
