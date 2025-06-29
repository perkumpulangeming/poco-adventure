using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEncounterManager : MonoBehaviour
{
    [Header("Boss Encounter Setup")]
    [SerializeField] private DialogueBoss dialogueBossScript;
    [SerializeField] private BossBattleSoundManager soundManager;
    [SerializeField] private GameObject boss1;
    [SerializeField] private GameObject boss2;
    
    [Header("Encounter Settings")]
    [SerializeField] private bool autoStartOnTrigger = true;
    [SerializeField] private float encounterDistance = 8f;
    [SerializeField] private Transform playerTransform;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;
    
    private bool encounterStarted = false;
    private bool battleStarted = false;

    private void Start()
    {
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        // Auto-setup sound manager references
        if (soundManager != null)
        {
            // You can access private fields through reflection if needed,
            // or just assign manually in inspector
        }
        
        if (showDebugMessages)
            Debug.Log("🎮 Boss Encounter Manager Ready!");
    }

    private void Update()
    {
        if (!encounterStarted && autoStartOnTrigger && playerTransform != null)
        {
            CheckPlayerDistance();
        }
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance <= encounterDistance)
        {
            StartBossEncounter();
        }
    }

    // Main method to start the whole boss encounter sequence
    public void StartBossEncounter()
    {
        if (encounterStarted) return;
        
        encounterStarted = true;
        
        if (showDebugMessages)
            Debug.Log("🎬 Starting Boss Encounter Sequence!");
        
        // Start cinematic dialogue
        if (dialogueBossScript != null)
        {
            dialogueBossScript.StartCinematicBossDialogue();
        }
        else
        {
            // If no dialogue, go straight to battle
            StartBattleDirectly();
        }
    }

    // Method to start battle without dialogue
    public void StartBattleDirectly()
    {
        if (battleStarted) return;
        
        battleStarted = true;
        
        if (showDebugMessages)
            Debug.Log("⚔️ Starting Boss Battle Directly!");
        
        if (soundManager != null)
        {
            soundManager.StartEpicBattle();
        }
    }

    // Method to be called from DialogueBoss when dialogue ends
    public void OnDialogueEnded()
    {
        if (showDebugMessages)
            Debug.Log("🎬 Boss Dialogue Ended, Starting Battle!");
            
        StartBattleDirectly();
    }

    // Method to check if encounter is complete
    public bool IsEncounterComplete()
    {
        if (soundManager != null)
        {
            return !soundManager.IsBattleActive();
        }
        
        // Fallback: check if bosses are defeated
        bool boss1Defeated = boss1 == null || !boss1.activeInHierarchy;
        bool boss2Defeated = boss2 == null || !boss2.activeInHierarchy;
        
        return boss1Defeated && boss2Defeated;
    }

    // Public methods for manual control
    public void ManualStartEncounter()
    {
        StartBossEncounter();
    }

    public void ManualStartBattle()
    {
        StartBattleDirectly();
    }

    public void StopEncounter()
    {
        if (soundManager != null)
        {
            soundManager.StopBattle();
        }
        
        encounterStarted = false;
        battleStarted = false;
        
        if (showDebugMessages)
            Debug.Log("🛑 Boss Encounter Stopped!");
    }

    public void ResetEncounter()
    {
        encounterStarted = false;
        battleStarted = false;
        
        if (soundManager != null)
        {
            soundManager.ResetBattle();
        }
        
        if (dialogueBossScript != null)
        {
            dialogueBossScript.SetFirstEncounter(true);
        }
        
        if (showDebugMessages)
            Debug.Log("🔄 Boss Encounter Reset!");
    }

    // Visualize encounter distance in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, encounterDistance);
        
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            Gizmos.color = distance <= encounterDistance ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
} 