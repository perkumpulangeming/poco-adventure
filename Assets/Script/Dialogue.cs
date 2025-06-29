using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    //Fields
    //Window
    public GameObject window;
    //Indicator
    public GameObject indicator;
    //Text component
    public TMP_Text dialogueText;
    //Dialogues list
    public List<string> dialogues;
    //Writing speed
    public float writingSpeed;
    
    [Header("Window Sizing")]
    [SerializeField] private float minWindowHeight = 100f;
    [SerializeField] private float maxWindowHeight = 300f;
    [SerializeField] private float windowPadding = 20f;
    [SerializeField] private float resizeAnimationSpeed = 2f;
    
    //Index on dialogue
    private int index;
    //Character index
    private int charIndex;
    //Started boolean
    private bool started;
    //Wait for next boolean
    private bool waitForNext;
    
    // Window resizing components
    private RectTransform windowRectTransform;
    private Vector2 originalWindowSize;
    private Coroutine resizeCoroutine;

    private void Awake()
    {
        // Get window RectTransform component
        windowRectTransform = window.GetComponent<RectTransform>();
        if (windowRectTransform != null)
        {
            originalWindowSize = windowRectTransform.sizeDelta;
        }
        
        ToggleIndicator(false);
        ToggleWindow(false);
    }

    private void ToggleWindow(bool show)
    {
        window.SetActive(show);
    }
    
    public void ToggleIndicator(bool show)
    {
        indicator.SetActive(show);
    }

    //Start Dialogue
    public void StartDialogue()
    {
        if (started)
            return;

        //Boolean to indicate that we have started
        started = true;
        //Show the window
        ToggleWindow(true);
        //hide the indicator
        ToggleIndicator(false);
        //Start with first dialogue
        GetDialogue(0);
    }

    private void GetDialogue(int i)
    {
        //start index at zero
        index = i;
        //Reset the character index
        charIndex = 0;
        //clear the dialogue component text
        dialogueText.text = string.Empty;
        
        // Calculate and animate to new window size based on full dialogue text
        if (windowRectTransform != null)
        {
            ResizeWindowForText(dialogues[index]);
        }
        
        //Start writing
        StartCoroutine(Writing());
    }

    private void ResizeWindowForText(string fullText)
    {
        if (windowRectTransform == null) return;
        
        // Simple calculation based on text length
        int textLength = fullText.Length;
        
        // Base height calculation - more characters = taller window
        float calculatedHeight = minWindowHeight;
        
        if (textLength > 50)
            calculatedHeight = minWindowHeight + 50f;
        if (textLength > 100)
            calculatedHeight = minWindowHeight + 100f;
        if (textLength > 150)
            calculatedHeight = minWindowHeight + 150f;
        
        // Clamp to max height
        calculatedHeight = Mathf.Clamp(calculatedHeight, minWindowHeight, maxWindowHeight);
        
        // Debug info 
        Debug.Log($"Dialog Text Length: {textLength} characters");
        Debug.Log($"Window Height: {calculatedHeight}px (was {windowRectTransform.sizeDelta.y}px)");
        
        // Set new size immediately for testing
        Vector2 newSize = new Vector2(originalWindowSize.x, calculatedHeight);
        windowRectTransform.sizeDelta = newSize;
        
        // Optional: Add smooth animation
        if (resizeCoroutine != null)
        {
            StopCoroutine(resizeCoroutine);
        }
        resizeCoroutine = StartCoroutine(AnimateWindowResize(newSize));
    }
    
    private IEnumerator AnimateWindowResize(Vector2 targetSize)
    {
        Vector2 startSize = windowRectTransform.sizeDelta;
        float elapsedTime = 0f;
        float animationDuration = 1f / resizeAnimationSpeed;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Smooth animation curve
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            windowRectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, smoothProgress);
            
            yield return null;
        }
        
        // Ensure final size is exactly the target
        windowRectTransform.sizeDelta = targetSize;
        resizeCoroutine = null;
    }

    //End Dialogue
    public void EndDialogue()
    {
        //Stared is disabled
        started = false;
        //Disable wait for next as well
        waitForNext = false;
        //Stop all Ienumerators
        StopAllCoroutines();
        
        // Reset window size to original
        if (windowRectTransform != null && resizeCoroutine == null)
        {
            resizeCoroutine = StartCoroutine(AnimateWindowResize(originalWindowSize));
        }
        
        //Hide the window
        ToggleWindow(false);        
    }
    
    //Writing logic
    IEnumerator Writing()
    {
        yield return new WaitForSeconds(writingSpeed);

        string currentDialogue = dialogues[index];
        //Write the character
        dialogueText.text += currentDialogue[charIndex];
        //increase the character index 
        charIndex++;
        //Make sure you have reached the end of the sentence
        if(charIndex < currentDialogue.Length)
        {
            //Wait x seconds 
            yield return new WaitForSeconds(writingSpeed);
            //Restart the same process
            StartCoroutine(Writing());
        }
        else
        {
            //End this sentence and wait for the next one
            waitForNext = true;
        }        
    }

    private void Update()
    {
        if (!started)
            return;

        if(waitForNext && Input.GetKeyDown(KeyCode.F))
        {
            waitForNext = false;
            index++;

            //Check if we are in the scope fo dialogues List
            if(index < dialogues.Count)
            {
                //If so fetch the next dialogue
                GetDialogue(index);
            }
            else
            {
                //If not end the dialogue process
                ToggleIndicator(true);
                EndDialogue();
            }            
        }
    }
}
