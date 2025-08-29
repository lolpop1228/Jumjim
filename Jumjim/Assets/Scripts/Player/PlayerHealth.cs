using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health & Armor")]
    public int maxHealth = 100;
    public int currentHealth;

    public int maxArmor = 100;
    public int currentArmor;
    public float fallThreshold = -50f;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText; // Optional: assign this via tag or in inspector

    [Header("Hurt Screen")]
    public Image hurtScreen;
    public float hurtFadeDuration = 0.5f;
    public CameraShake cameraShake;
    public float shakeMagnitude = 0.3f;
    public float shakeDuration = 0.15f;
    private Color originalColor;

    void Start()
    {
        GameObject healthUI = GameObject.FindWithTag("HealthUI");
        if (healthUI != null)
        {
            healthText = healthUI.GetComponent<TextMeshProUGUI>();
        }

        GameObject armorUI = GameObject.FindWithTag("ArmorUI");
        if (armorUI != null)
        {
            armorText = armorUI.GetComponent<TextMeshProUGUI>();
        }

        if (hurtScreen != null)
        {
            originalColor = new Color(0.4f, 0f, 0f, 0.03f); // darker red, low alpha
            SetAlpha(0f);
        }

        currentHealth = maxHealth;
        currentArmor = maxArmor;
        UpdateUI();
    }

    private void Update()
    {
        if (transform.position.y <= fallThreshold)
        {
            ReloadScene();
        }
    }

    public void TakeDamage(int amount)
    {
        int damageLeft = amount;

        // Subtract from armor first
        if (currentArmor > 0)
        {
            int absorbed = Mathf.Min(currentArmor, damageLeft);
            currentArmor -= absorbed;
            damageLeft -= absorbed;
        }

        // Then subtract remaining damage from health
        if (damageLeft > 0)
        {
            currentHealth -= damageLeft;
        }

        Debug.Log($"Health: {currentHealth}, Armor: {currentArmor}");
        UpdateUI();
        ShowHurtScreen();

        if (cameraShake != null)
        {
            cameraShake.Shake(shakeMagnitude, shakeDuration);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died.");
        ReloadScene();
    }

    public void HealPlayer(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void AddArmor(int amount)
    {
        currentArmor = Mathf.Min(currentArmor + amount, maxArmor);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"Health: {currentHealth}";

        if (armorText != null)
            armorText.text = $"Armor: {currentArmor}";
    }

    void ShowHurtScreen()
    {
        if (hurtScreen != null)
            StartCoroutine(FadeHurtScreen());
    }

    System.Collections.IEnumerator FadeHurtScreen()
    {
        SetAlpha(1f); // Full red flash

        float elapsed = 0f;
        while (elapsed < hurtFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / hurtFadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f); // Ensure it's invisible at the end
    }

    void SetAlpha(float alpha)
    {
        if (hurtScreen != null)
        {
            Color c = originalColor;
            c.a = alpha;
            hurtScreen.color = c;
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene("DeathScene");
    }
}
