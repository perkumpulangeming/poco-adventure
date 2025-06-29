using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class DialogueBossTrigger : MonoBehaviour
{
    [Header("Boss Dialogue Settings")]
    public DialogueBoss dialogueBossScript;
    
    [Header("Boss Trigger Settings")]
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private float triggerDistance = 10f;
    [SerializeField] private bool autoTriggerOnSight = true;
    [SerializeField] private bool requirePlayerInput = false;
    
    [Header("Boss Detection")]
    [SerializeField] private Transform bossTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject bossIntroEffect;
    [SerializeField] private float introEffectDuration = 2f;
    
    [Header("Audio Effects")]
    [SerializeField] private AudioClip bossEncounterSound;
    [SerializeField] private float encounterSoundVolume = 0.7f;
    
    private bool hasTriggeredBossDialogue = false;
    private bool playerInTriggerZone = false;
    private GameObject playerObject;
    private AudioSource audioSource;
    private Coroutine introEffectCoroutine;

    private void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        // Find boss if not assigned
        if (bossTransform == null)
        {
            // Try to find boss in parent or children
            bossTransform = GetComponentInParent<Transform>();
        }
        
        // Hide intro effect initially
        if (bossIntroEffect != null)
            bossIntroEffect.SetActive(false);
            
        Debug.Log("Boss Dialogue Trigger initialized");
    }

    private void Update()
    {
        // Only check if we haven't triggered yet or if we allow multiple triggers
        if (hasTriggeredBossDialogue && triggerOnlyOnce)
            return;
            
        // Check for boss encounter conditions
        CheckBossEncounter();
        
        // Handle manual trigger input
        if (requirePlayerInput && playerInTriggerZone && Input.GetKeyDown(KeyCode.F))
        {
            TriggerBossDialogue();
        }
    }

    private void CheckBossEncounter()
    {
        if (playerTransform == null || bossTransform == null)
            return;
            
        // Calculate distance between player and boss
        float distanceToPlayer = Vector3.Distance(bossTransform.position, playerTransform.position);
        
        // Check if player is within trigger distance
        if (distanceToPlayer <= triggerDistance)
        {
            // Check line of sight if needed
            if (HasLineOfSight())
            {
                playerInTriggerZone = true;
                
                // Auto trigger if enabled
                if (autoTriggerOnSight && !hasTriggeredBossDialogue)
                {
                    TriggerBossDialogue();
                }
            }
            else
            {
                playerInTriggerZone = false;
            }
        }
        else
        {
            playerInTriggerZone = false;
        }
    }

    private bool HasLineOfSight()
    {
        if (bossTransform == null || playerTransform == null)
            return false;
            
        Vector3 direction = (playerTransform.position - bossTransform.position).normalized;
        float distance = Vector3.Distance(bossTransform.position, playerTransform.position);
        
        // Cast a ray to check for obstacles
        RaycastHit2D hit = Physics2D.Raycast(bossTransform.position, direction, distance, obstacleLayerMask);
        
        // If we hit something that's not the player, line of sight is blocked
        if (hit.collider != null && hit.collider.transform != playerTransform)
        {
            return false;
        }
        
        return true;
    }

    //Detect trigger with player using collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If we triggered the player
        if(collision.gameObject.layer == Global.Layers.PlayerID)
        {
            playerObject = collision.gameObject;
            playerInTriggerZone = true;
            
            Debug.Log("Player entered boss trigger zone");
            
            // Auto trigger if enabled and haven't triggered yet
            if (autoTriggerOnSight && !hasTriggeredBossDialogue)
            {
                TriggerBossDialogue();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //If we lost trigger with the player
        if (collision.gameObject.layer == Global.Layers.PlayerID)
        {
            playerInTriggerZone = false;
            playerObject = null;
            
            Debug.Log("Player exited boss trigger zone");
        }
    }

    private void TriggerBossDialogue()
    {
        // Check if we can trigger
        if (hasTriggeredBossDialogue && triggerOnlyOnce)
            return;
            
        if (dialogueBossScript == null)
        {
            Debug.LogWarning("DialogueBoss script not assigned!");
            return;
        }
        
        // Only trigger if this is first encounter and not already in cinematic
        if ((!dialogueBossScript.IsFirstEncounter() && triggerOnlyOnce) || dialogueBossScript.IsCinematicActive())
            return;
            
        Debug.Log("Triggering Boss Dialogue!");
        
        // Mark as triggered
        hasTriggeredBossDialogue = true;
        
        // Play encounter sound
        PlayBossEncounterSound();
        
        // Show intro effect
        ShowBossIntroEffect();
        
        // Start boss dialogue with slight delay for dramatic effect
        StartCoroutine(StartBossDialogueWithDelay(1f));
    }

    private void PlayBossEncounterSound()
    {
        if (bossEncounterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bossEncounterSound, encounterSoundVolume);
        }
    }

    private void ShowBossIntroEffect()
    {
        if (bossIntroEffect != null)
        {
            bossIntroEffect.SetActive(true);
            
            // Hide effect after duration
            if (introEffectCoroutine != null)
            {
                StopCoroutine(introEffectCoroutine);
            }
            introEffectCoroutine = StartCoroutine(HideIntroEffectAfterDelay());
        }
    }

    private IEnumerator HideIntroEffectAfterDelay()
    {
        yield return new WaitForSeconds(introEffectDuration);
        
        if (bossIntroEffect != null)
        {
            bossIntroEffect.SetActive(false);
        }
        
        introEffectCoroutine = null;
    }

    private IEnumerator StartBossDialogueWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Start the cinematic boss dialogue
        dialogueBossScript.StartCinematicBossDialogue();
    }

    // Public method to manually trigger boss dialogue
    public void ManualTriggerBossDialogue()
    {
        TriggerBossDialogue();
    }
    
    // Reset trigger for testing
    public void ResetBossTrigger()
    {
        hasTriggeredBossDialogue = false;
        if (dialogueBossScript != null)
        {
            dialogueBossScript.SetFirstEncounter(true);
        }
        Debug.Log("Boss trigger reset");
    }
    
    // Check if boss dialogue has been triggered
    public bool HasTriggeredBossDialogue()
    {
        return hasTriggeredBossDialogue;
    }

    // Visualize trigger distance in editor
    private void OnDrawGizmosSelected()
    {
        if (bossTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossTransform.position, triggerDistance);
            
            // Draw line of sight if player is present
            if (playerTransform != null)
            {
                if (HasLineOfSight())
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawLine(bossTransform.position, playerTransform.position);
            }
        }
    }
} 