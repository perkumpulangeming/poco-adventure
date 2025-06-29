using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBattleSoundManager : MonoBehaviour
{
    [Header("Boss References")]
    [SerializeField] private GameObject boss1;
    [SerializeField] private GameObject boss2;
    
    [Header("Simple Battle Audio")]
    [SerializeField] private AudioClip epicBattleMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private float musicVolume = 0.7f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip battleStartSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private float sfxVolume = 0.8f;
    
    // Components
    private AudioSource musicSource;
    private AudioSource sfxSource;
    
    // Battle state
    private bool isBattleActive = false;
    private bool boss1Defeated = false;
    private bool boss2Defeated = false;

    private void Awake()
    {
        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        // Music source - use existing or create new
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.volume = 0f;
        
        // SFX source - create separate for sound effects
        GameObject sfxObj = new GameObject("SFX_Source");
        sfxObj.transform.SetParent(transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }

    // Main method to start battle - call this from DialogueBoss
    public void StartEpicBattle()
    {
        if (isBattleActive) return;
        
        Debug.Log("🎵 Starting Epic Boss Battle!");
        
        isBattleActive = true;
        
        // Play battle start sound effect
        if (battleStartSound != null)
        {
            sfxSource.PlayOneShot(battleStartSound, sfxVolume);
        }
        
        // Start epic battle music
        StartCoroutine(StartBattleMusic());
        
        // Monitor boss status
        StartCoroutine(MonitorBosses());
    }

    private IEnumerator StartBattleMusic()
    {
        if (epicBattleMusic != null)
        {
            musicSource.clip = epicBattleMusic;
            musicSource.Play();
            
            // Fade in music
            while (musicSource.volume < musicVolume)
            {
                musicSource.volume += Time.deltaTime * 2f; // Fade in speed
                yield return null;
            }
            musicSource.volume = musicVolume;
        }
    }

    private IEnumerator MonitorBosses()
    {
        while (isBattleActive)
        {
            CheckBossStatus();
            
            // Check if both bosses are defeated
            if (boss1Defeated && boss2Defeated)
            {
                OnAllBossesDefeated();
                break;
            }
            
            yield return new WaitForSeconds(1f); // Check every second
        }
    }

    private void CheckBossStatus()
    {
        // Check boss 1
        if (boss1 != null && !boss1Defeated)
        {
            if (!boss1.activeInHierarchy)
            {
                boss1Defeated = true;
                Debug.Log("🎵 Boss 1 Defeated!");
            }
        }
        
        // Check boss 2
        if (boss2 != null && !boss2Defeated)
        {
            if (!boss2.activeInHierarchy)
            {
                boss2Defeated = true;
                Debug.Log("🎵 Boss 2 Defeated!");
            }
        }
    }

    private void OnAllBossesDefeated()
    {
        Debug.Log("🎵 ALL BOSSES DEFEATED! VICTORY!");
        
        isBattleActive = false;
        
        // Play victory sound
        if (victorySound != null)
        {
            sfxSource.PlayOneShot(victorySound, sfxVolume);
        }
        
        // Switch to victory music
        StartCoroutine(PlayVictoryMusic());
    }

    private IEnumerator PlayVictoryMusic()
    {
        // Fade out battle music
        while (musicSource.volume > 0)
        {
            musicSource.volume -= Time.deltaTime * 3f; // Fade out speed
            yield return null;
        }
        
        // Play victory music if available
        if (victoryMusic != null)
        {
            musicSource.clip = victoryMusic;
            musicSource.Play();
            
            // Fade in victory music
            while (musicSource.volume < musicVolume)
            {
                musicSource.volume += Time.deltaTime * 2f;
                yield return null;
            }
            musicSource.volume = musicVolume;
            
            // Let victory music play for 8 seconds then fade out
            yield return new WaitForSeconds(8f);
            
            while (musicSource.volume > 0)
            {
                musicSource.volume -= Time.deltaTime * 1f;
                yield return null;
            }
            musicSource.Stop();
        }
        else
        {
            // Just stop battle music if no victory music
            musicSource.Stop();
        }
    }

    // Public methods for manual control
    public void StopBattle()
    {
        isBattleActive = false;
        StopAllCoroutines();
        
        // Fade out music
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        while (musicSource.volume > 0)
        {
            musicSource.volume -= Time.deltaTime * 2f;
            yield return null;
        }
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public bool IsBattleActive()
    {
        return isBattleActive;
    }

    // Simple test methods
    public void TestStartBattle()
    {
        StartEpicBattle();
    }

    public void TestStopBattle()
    {
        StopBattle();
    }

    // Reset for testing
    public void ResetBattle()
    {
        isBattleActive = false;
        boss1Defeated = false;
        boss2Defeated = false;
        StopAllCoroutines();
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.volume = 0f;
        }
        Debug.Log("🎵 Battle Reset!");
    }
} 