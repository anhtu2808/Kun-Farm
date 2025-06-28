using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar_UI : MonoBehaviour
{
    [SerializeField] private List<Slot_UI> toolbarSlots = new List<Slot_UI>();

    private Slot_UI selectedSlot;
    private ToolManager toolManager;

    private void Start()
    {
        // Find ToolManager
        toolManager = FindObjectOfType<ToolManager>();
        
        // Setup click callbacks for all slots
        for (int i = 0; i < toolbarSlots.Count; i++)
        {
            toolbarSlots[i].SetClickCallback(OnSlotClicked);
        }
        
        SelectSlot(0);
    }

    private void Update()
    {
        // Only handle keys if no ToolManager is present
        if (FindObjectOfType<ToolManager>() == null)
        {
            CheckAlphaNumericKeys();
        }
    }

    public void SelectSlot(int index)
    {
        if(toolbarSlots.Count == 9)
        {
            if(selectedSlot != null)
            {
                selectedSlot.SetHighlight(false);
            }
            selectedSlot = toolbarSlots[index];
            selectedSlot.SetHighlight(true);
            
            // Notify ToolManager if available (don't update UI to avoid infinite loop)
            if (toolManager != null)
            {
                toolManager.SelectTool(index, false);
            }
        }
    }
    
    private void OnSlotClicked(int index)
    {
        SelectSlot(index);
    }

    private void CheckAlphaNumericKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SelectSlot(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SelectSlot(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SelectSlot(2); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SelectSlot(3); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SelectSlot(4); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { SelectSlot(5); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { SelectSlot(6); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { SelectSlot(7); }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { SelectSlot(8); }
    }

    public List<Slot_UI> GetToolbarSlots()
    {
        return toolbarSlots;
    }
}
