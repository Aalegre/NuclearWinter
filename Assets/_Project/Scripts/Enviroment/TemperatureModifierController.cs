using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureModifierController : MonoBehaviour
{
    public AtmosphericsController atmosphericsController;
    void Start()
    {
        if (!atmosphericsController)
            atmosphericsController = GameManager.Instance.atmosphericsController;
    }
}
