using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header ("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header ("Audio Clips")]
    public AudioClip EnemyHit;
    public AudioClip EnemyDeath;
    public AudioClip PlayerHit;
    public AudioClip PlayerDeath;
    public AudioClip PlayerMove;
    public AudioClip BossAttack;

    [Header ("Background Music")]
    public AudioClip BackgroundMusic;

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
        if (BackgroundMusic != null)
        {
            PlayBGM(BackgroundMusic);
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
        if (clip == null && sfxSource == null)
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
}
