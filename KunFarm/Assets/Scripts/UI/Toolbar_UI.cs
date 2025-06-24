using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar_UI : MonoBehaviour
{
    [SerializeField] private List<Slot_UI> toolbarSlots = new List<Slot_UI>();

    private Slot_UI selectedSlot;

    private void Start()
    {
        SelectSlot(0);
    }

    private void Update()
    {
        CheckAlphaNumericKeys();
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
            Debug.Log("Selected Slot: " + selectedSlot.name);
        }
    }

    private void CheckAlphaNumericKeys()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(6);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(7);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(8);
    }
}
