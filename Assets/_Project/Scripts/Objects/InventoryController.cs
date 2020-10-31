using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public PlayerController playerController;
    public Inventory inventory;
    public Inventory savedinventory;
    public List<Interactable> interactables = new List<Interactable>();
    // Start is called before the first frame update
    void Awake()
    {
        inventory = Instantiate(savedinventory);
    }

    public void Pickup(in PickupObject pickup)
    {
        switch (pickup.Type)
        {
            case PickupObject.TYPE.BULLET:
                inventory.Bullets += (int)pickup.Quantity;
                inventory.Bullets = Mathf.Clamp(inventory.Bullets, 0, inventory.BulletsMax);
                break;
            case PickupObject.TYPE.TEXTILE:
                inventory.Textiles += (int)pickup.Quantity;
                inventory.Textiles = Mathf.Clamp(inventory.Textiles, 0, inventory.TextilesMax);
                break;
            case PickupObject.TYPE.BOTTLE:
                inventory.Bottles += (int)pickup.Quantity;
                inventory.Bottles = Mathf.Clamp(inventory.Bottles, 0, inventory.BottlesMax);
                break;
            case PickupObject.TYPE.WOOD:
                inventory.Bottles += (int)pickup.Quantity;
                inventory.Bottles = Mathf.Clamp(inventory.Bottles, 0, inventory.BottlesMax);
                break;
            case PickupObject.TYPE.BANDAIDS:
                inventory.Bandaids += (int)pickup.Quantity;
                inventory.Bandaids = Mathf.Clamp(inventory.Bandaids, 0, inventory.BandaidsMax);
                break;
            case PickupObject.TYPE.CAMP:
                inventory.Camps += (int)pickup.Quantity;
                inventory.Camps = Mathf.Clamp(inventory.Camps, 0, inventory.CampsMax);
                break;
            case PickupObject.TYPE.WATER:
                inventory.Water += pickup.Quantity;
                inventory.Water = Mathf.Clamp(inventory.Water, 0, inventory.WaterMax);
                break;
            case PickupObject.TYPE.ALCOHOL:
                inventory.Alcohol += pickup.Quantity;
                inventory.Alcohol = Mathf.Clamp(inventory.Alcohol, 0, inventory.AlcoholMax);
                break;
            case PickupObject.TYPE.OIL:
                inventory.Oil += pickup.Quantity;
                inventory.Oil = Mathf.Clamp(inventory.Oil, 0, inventory.OilMax);
                break;
            default:
                break;
        }
    }
    public bool Craft(PickupObject.TYPE type)
    {
        switch (type)
        {
            case PickupObject.TYPE.BANDAIDS:
                if (inventory.Alcohol > 0.5f && inventory.Bottles > 1 && inventory.Textiles > 1)
                {
                    inventory.Alcohol -= .5f;
                    inventory.Bottles--;
                    inventory.Textiles--;
                    inventory.Bandaids++;
                    inventory.Bandaids = Mathf.Clamp(inventory.Bandaids, 0, inventory.BandaidsMax);
                }
                break;
            case PickupObject.TYPE.CAMP:
                if (inventory.Oil > 0.5f && inventory.Woods > 2)
                {
                    inventory.Oil -= .5f;
                    inventory.Woods-=2;
                    inventory.Camps++;
                    inventory.Camps = Mathf.Clamp(inventory.Camps, 0, inventory.CampsMax);
                }
                break;
        }
        return false;
    }
    public void FillLantern()
    {
        float difference = inventory.lampGas - inventory.lampGasMax;
        inventory.Oil += difference;
        inventory.lampGas = 1;
        if (inventory.Oil < 0)
            inventory.lampGas += inventory.Oil;
        inventory.Oil = Mathf.Clamp(inventory.Oil, 0, inventory.OilMax);
        inventory.lampGas = Mathf.Clamp01(inventory.lampGas);

    }
}
