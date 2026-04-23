using UnityEngine;

public class AudioDirector : MonoBehaviour
{
    public static AudioDirector Instance;

    [Header("References")]
    public GridManager gridManager;
    public BirdManager birdManager;
    public EndGameManager endGameManager;
    

    [Header("Music / Ambience")]
    public AudioSource calmMusicSource;
    public AudioSource birdLoopSource;
    public AudioSource rumbleSource;

    [Header("Filters")]
    public AudioLowPassFilter musicLowPass;

    [Header("Low Pass Settings")]
    public float lowPassStart = 22000f; // clear
    public float lowPassEnd = 800f;     // very muffled

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioSource uiSource;
    public AudioSource tileSource;

    [Header("Clips")]
    public AudioClip calmMusicClip;
    public AudioClip birdLoopClip;
    public AudioClip rumbleClip;
    public AudioClip tileDugClip;
    public AudioClip attackSirenClip;

    public AudioClip playerDigClip;
    public AudioClip gunShotClip;
    public AudioClip gunEquipClip;
    public AudioClip tranqShotClip;

    public AudioClip animalKillClip;
    public AudioClip animalFallClip;

    public AudioClip uiSuccessClip;
    public AudioClip uiFailClip;
    public AudioClip drillClip;
    public AudioClip jcbClip;

    [Header("Mix")]
    public float calmStartVolume = 0.75f;
    public float calmEndVolume = 0.0f;

    public float birdStartVolume = 0.35f;
    public float birdEndVolume = 0.0f;

    public float workerToolStartVolume = 0.18f;
    public float workerToolEndVolume = 1.0f;

    [Header("Depth Tuning")]

    // Music fade
    public float musicFadeStartDepth = 10f;
    public float musicFadeEndDepth = 40f;

    // Tool volume increase
    public float toolFadeStartDepth = 10f;
    public float toolFadeEndDepth = 40f;

    public float fadeSpeed = 1.6f;

    private bool ending;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (calmMusicSource != null && calmMusicClip != null)
        {
            calmMusicSource.clip = calmMusicClip;
            calmMusicSource.loop = true;
            calmMusicSource.Play();
        }

        if (birdLoopSource != null && birdLoopClip != null)
        {
            birdLoopSource.clip = birdLoopClip;
            birdLoopSource.loop = true;
            birdLoopSource.Play();
        }

        if (rumbleSource != null)
        {
            rumbleSource.volume = 0f;
        }
    }

    void Update()
    {
        if (!ending && endGameManager != null && endGameManager.IsTriggered)
            BeginEnding();

        UpdateMix();
    }

    void UpdateMix()
    {
        if (ending)
        {
            FadeAllExceptRumble();
            return;
        }

        float currentDepth = gridManager != null ? gridManager.GetMaxDepthReached() : 0f;

        // Music fade curve
        float music01 = GetDepth01(currentDepth, musicFadeStartDepth, musicFadeEndDepth);
        if (musicLowPass != null)
        {
            float cutoff = Mathf.Lerp(lowPassStart, lowPassEnd, music01);
            musicLowPass.cutoffFrequency = cutoff;
            musicLowPass.lowpassResonanceQ = Mathf.Lerp(1f, 2.5f, music01);
        }

        // Tool curve (can be different later if you want)
        float tool01 = GetDepth01(currentDepth, toolFadeStartDepth, toolFadeEndDepth);

        float bird01 = 0f;
        if (birdManager != null)
        {
            bird01 = Mathf.Clamp01((float)birdManager.ActiveBirdCount / Mathf.Max(1, birdManager.MaxBirdCount));
        }

        if (calmMusicSource != null)
            calmMusicSource.volume = Mathf.Lerp(calmStartVolume, calmEndVolume, music01);

        if (birdLoopSource != null)
        {
            birdLoopSource.volume = Mathf.Lerp(birdEndVolume, birdStartVolume, bird01);

            if (birdManager != null && birdManager.ActiveBirdCount <= 0 && birdLoopSource.isPlaying)
                birdLoopSource.Stop();
            else if (birdManager != null && birdManager.ActiveBirdCount > 0 && !birdLoopSource.isPlaying && birdLoopClip != null)
                birdLoopSource.Play();
        }

        if (rumbleSource != null)
            rumbleSource.volume = 0f;
    }

    public float GetWorkerToolVolumeMultiplier()
    {
        if (ending) return 0f;

        float currentDepth = gridManager != null ? gridManager.GetMaxDepthReached() : 0f;

        float tool01 = GetDepth01(currentDepth, toolFadeStartDepth, toolFadeEndDepth);

        return Mathf.Lerp(workerToolStartVolume, workerToolEndVolume, tool01);
    }

    public void PlayPlayerDig()
    {
        PlayOneShot(sfxSource, playerDigClip);
    }

    public void PlayTileDug()
    {
        PlayOneShot(tileSource, tileDugClip);
    }

    public void PlayGunShot()
    {
        PlayOneShot(sfxSource, gunShotClip);
    }

    public void PlayTranqShot()
    {
        PlayOneShot(sfxSource, tranqShotClip);
    }

    public void PlayUISuccess()
    {
        PlayOneShot(uiSource, uiSuccessClip);
    }

    public void PlayUIFail()
    {
        PlayOneShot(uiSource, uiFailClip);
    }

    public void PlayPickSelect()
    {
        PlayOneShot(uiSource, playerDigClip);
    }

    public void PlayWeaponSelect()
    {
        PlayOneShot(uiSource, gunEquipClip);
    }

    public void PlayToolUpgrade(Tool tool)
    {
        if(tool.toolName=="Bulldozer"){
            PlayOneShot(uiSource,jcbClip);
        }
        else if(tool.toolName=="Drill"){
            PlayOneShot(uiSource,drillClip);
        }
        else{
            PlayUISuccess();
        }
    }

    public void PlayAttackSiren()
    {
        if (sfxSource == null || attackSirenClip == null) return;
        sfxSource.PlayOneShot(attackSirenClip);
    }

    public void PlayTargetHit(AudioClip clip)
    {
        if (clip == null) return;
        PlayOneShot(sfxSource, clip);
    }

    public void PlayAnimalKill()
    {
        PlayOneShot(sfxSource, animalKillClip);
    }

    public void PlayAnimalFall()
    {
        PlayOneShot(sfxSource, animalFallClip);
    }

    public void BeginEnding()
    {
        ending = true;

        if (rumbleSource != null && rumbleClip != null)
        {
            rumbleSource.clip = rumbleClip;
            if (!rumbleSource.isPlaying)
                rumbleSource.Play();
        }
    }

    void FadeAllExceptRumble()
    {
        if (calmMusicSource != null)
            calmMusicSource.volume = Mathf.MoveTowards(calmMusicSource.volume, 0f, Time.deltaTime * fadeSpeed);

        if (birdLoopSource != null)
            birdLoopSource.volume = Mathf.MoveTowards(birdLoopSource.volume, 0f, Time.deltaTime * fadeSpeed);

        if (sfxSource != null)
            sfxSource.volume = Mathf.MoveTowards(sfxSource.volume, 0f, Time.deltaTime * fadeSpeed);

        if (uiSource != null)
            uiSource.volume = Mathf.MoveTowards(uiSource.volume, 0f, Time.deltaTime * fadeSpeed);

        if (rumbleSource != null)
            rumbleSource.volume = Mathf.MoveTowards(rumbleSource.volume, 1f, Time.deltaTime * fadeSpeed);
    }

    void PlayOneShot(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip);
    }

    float GetDepth01(float currentDepth, float start, float end)
    {
        if (currentDepth <= start) return 0f;
        if (currentDepth >= end) return 1f;

        return (currentDepth - start) / (end - start);
    }
    
}