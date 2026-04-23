using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Creates a full-screen red flash overlay when the player takes damage.
/// Attach this to any GameObject in the scene. The overlay will be created
/// automatically on the main Canvas.
/// </summary>
public class DamageFlashOverlay : MonoBehaviour
{
    public static DamageFlashOverlay Instance { get; private set; }

    [Header("Flash Settings")]
    [Tooltip("The color of the flash overlay when damage is taken")]
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.4f);
    
    [Tooltip("How long the flash lasts in seconds")]
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private Image _flashImage;
    private Canvas _canvas;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Find the canvas
        _canvas = FindFirstObjectByType<Canvas>();
        if (_canvas == null)
        {
            if (debugMode) Debug.LogWarning("[DamageFlashOverlay] No Canvas found in scene. Will create one when needed.");
            return;
        }

        CreateOverlay();
    }

    private void CreateOverlay()
    {
        if (_canvas == null)
        {
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null)
            {
                if (debugMode) Debug.LogWarning("[DamageFlashOverlay] Still no Canvas found.");
                return;
            }
        }

        // Create the flash overlay image as a child of the canvas
        GameObject flashObj = new GameObject("DamageFlashOverlay_Image");
        flashObj.transform.SetParent(_canvas.transform, false);
        
        _flashImage = flashObj.AddComponent<Image>();
        
        // Set up the RectTransform to cover the entire screen
        RectTransform rt = flashObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.SetAsLastSibling(); // Ensure it's on top of all other UI elements
        
        // Configure the image
        _flashImage.color = Color.clear;
        _flashImage.raycastTarget = false; // Don't block UI interactions
        
        // Set a simple white sprite (Unity's built-in)
        _flashImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Default.psd");
        
        if (debugMode) Debug.Log("[DamageFlashOverlay] Overlay created on canvas: " + _canvas.name);
    }

    /// <summary>
    /// Trigger the damage flash effect.
    /// Call this from PlayerController.TakeDamage() or any damage source.
    /// </summary>
    public void TriggerFlash()
    {
        // Ensure overlay exists
        if (_flashImage == null)
        {
            CreateOverlay();
            if (_flashImage == null)
            {
                if (debugMode) Debug.LogWarning("[DamageFlashOverlay] Cannot create overlay!");
                return;
            }
        }
        
        // Cancel any ongoing flash
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);
        
        // Start new flash
        _flashCoroutine = StartCoroutine(FlashRoutine());
        
        if (debugMode) Debug.Log("[DamageFlashOverlay] Flash triggered! Color: " + flashColor + ", Duration: " + flashDuration);
    }

    private IEnumerator FlashRoutine()
    {
        // Set to full flash color
        _flashImage.color = flashColor;
        
        float elapsed = 0f;
        
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / flashDuration;
            
            // Linear fade out
            Color c = flashColor;
            c.a = flashColor.a * (1f - t);
            _flashImage.color = c;
            
            yield return null;
        }
        
        // Ensure fully transparent at end
        _flashImage.color = Color.clear;
    }

    /// <summary>
    /// Trigger a flash with a custom color and duration.
    /// </summary>
    public void TriggerFlash(Color customColor, float duration = -1f)
    {
        if (duration > 0f)
            flashDuration = duration;
        
        Color originalColor = flashColor;
        flashColor = customColor;
        TriggerFlash();
        flashColor = originalColor;
    }

    // Helper method for PlayerController to call easily
    public static void Flash()
    {
        if (Instance != null)
        {
            Instance.TriggerFlash();
        }
        else
        {
            // Try to find one in the scene
            DamageFlashOverlay overlay = FindFirstObjectByType<DamageFlashOverlay>();
            if (overlay != null)
            {
                overlay.TriggerFlash();
            }
            else if (Instance == null)
            {
                Debug.LogWarning("[DamageFlashOverlay] No instance found. Add DamageFlashOverlay component to a GameObject in the scene.");
            }
        }
    }
}