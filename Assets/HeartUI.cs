using UnityEngine;
using UnityEngine.UI;
using Game.Characters.Components;

public class HeartUI : MonoBehaviour
{
    public Health playerHealth;
    public Image[] hearts;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    void Update()
    {
        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < playerHealth.currentHealth)
                hearts[i].sprite = fullHeartSprite;
            else
                hearts[i].sprite = emptyHeartSprite;
        }
    }
}
