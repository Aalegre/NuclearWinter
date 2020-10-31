using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public InventoryController inventoryController;
    public virtual void Interact()
    {

    }
    protected virtual void Start()
    {
        if (!inventoryController)
            inventoryController = FindObjectOfType<InventoryController>();
        inventoryController.interactables.Add(this);
    }
    protected virtual void OnDestroy()
    {
        if (inventoryController)
            inventoryController.interactables.Remove(this);
    }
}
