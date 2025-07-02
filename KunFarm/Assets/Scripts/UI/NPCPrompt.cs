using UnityEngine;
using TMPro;

public class NPCPrompt : MonoBehaviour
{
    [Header("UI References")]
    public Canvas promptCanvas;
    public TMP_Text promptText;
    
    [Header("Settings")]
    public string message = "Press E";
    public bool alwaysFaceCamera = true;
    
    private Transform target;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (promptText != null)
            promptText.text = message;
            
        Hide();
    }
    
    
    public void SetTarget(Transform npcTransform)
    {
        target = npcTransform;
    }
    
    public void Show()
    {
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(false);
    }
    

} 