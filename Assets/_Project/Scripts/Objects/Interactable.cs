using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public InventoryController inventoryController;
    public PickupObject pickupObject;
    public bool destroyOnInteract = true;
    public UnityEvent OnInteract;
    public virtual void Interact()
    {
        OnInteract.Invoke();
    }
    private void Awake()
    {
        pickupObject = GetComponent<PickupObject>();
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
