using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public enum TYPE { NULL, BULLET, TEXTILE, BOTTLE, WOOD, BANDAIDS, CAMP, WATER, ALCOHOL, OIL }
    public TYPE Type;
    public float Quantity;
}
