using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header ("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;

    [Header ("Audio Clips")]
    public AudioClip EnemyAttack;
    public AudioClip EnemyHit;
    public AudioClip EnemyDeath;
    public AudioClip PlayerAttack;
    public AudioClip PlayerHit;
    public AudioClip PlayerDeath;
    public AudioClip PlayerMove;
    public AudioClip BossAttack;
    public AudioClip BossSkill2;

    [Header ("Background Music")]
    public AudioClip MainMenuMusic;
    public AudioClip audioClip;
    public AudioClip CombatMusic;
    public AudioClip Boss1Theme;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (MainMenuMusic != null)
        {
            PlayBGM(MainMenuMusic);
        }
    }

    // Update is called once per frame
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || musicSource == null)
        {
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying) return;
        
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        
    }

    public void StopBGM()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayMainMenuMusic() => PlayBGM(MainMenuMusic);
    public void PlayCombatMusic() => PlayBGM(CombatMusic);
    public void PlayBoss1Theme() => PlayBGM(Boss1Theme);
    //Voice control
        public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
 
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
 
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {   
        sfxVolume = Mathf.Clamp01(value);

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }
}
