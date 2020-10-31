using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory/Inventory", order = 1)]
public class Inventory : ScriptableObject
{
    public int Bullets;
    public int BulletsMax = 32;
    public int Textiles;
    public int TextilesMax = 16;
    public int Bottles;
    public int BottlesMax = 4;
    public int Woods;
    public int WoodsMax = 4;
    public int Bandaids;
    public int BandaidsMax = 4;
    public int Camps;
    public int CampsMax = 1;
    public float Water;
    public float WaterMax = 5;
    public float Alcohol;
    public float AlcoholMax = 2;
    public float Oil;
    public float OilMax = 2;
    public float lampGas;
    public float lampGasMax = 1;
}
