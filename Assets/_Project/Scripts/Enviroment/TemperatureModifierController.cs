using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureModifierController : MonoBehaviour
{
    public AtmosphericsController atmosphericsController;
    public bool active = false;
    bool activelast = false;
    public float Temperature = 1;
    public float TemperatureMax;
    [Range(1, 50)] public float TemperatureRadius = 1;
    [Range(1, 50)] public float TemperatureDecayActive = 10;
    [Range(1, 10)] public float TemperatureDecayNotActive = 1;
    public float TemperatureInner;
    public AnimationCurve TemperatureDistribution = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Header("Visuals")]
    public ParticleSystem activeParticleSystem;
    ParticleSystem.EmissionModule activeParticleSystemEmission;
    public GameObject[] activeObjects;
    public GameObject[] notactiveObjects;
    void Start()
    {
        if (!atmosphericsController)
            atmosphericsController = GameManager.Instance.atmosphericsController;
        atmosphericsController.modifiers.Add(this);
        if (activeParticleSystem)
            activeParticleSystemEmission = activeParticleSystem.emission;
        activelast = active;
        UpdateVisuals();
    }
    private void Update()
    {
        if (active != activelast)
        {
            activelast = active;
            UpdateVisuals();
        }
        if (active)
        {
            TemperatureInner = Mathf.Lerp(TemperatureInner, Temperature, Time.deltaTime * TemperatureDecayActive);
        }
        else
        {
            TemperatureInner = Mathf.Lerp(TemperatureInner, 0, Time.deltaTime * TemperatureDecayNotActive);
        }
    }

    public void Toggle()
    {
        active = !active;
    }

    public float GetTemperature(Vector3 pos)
    {
        Vector3 dir = (transform.position - pos);
        dir.y = 0;
        float distance = dir.sqrMagnitude;
        if (distance > TemperatureRadius * TemperatureRadius)
            return 0;
        distance = Mathf.Sqrt(distance);
        distance /= TemperatureRadius;
        float temp = TemperatureDistribution.Evaluate(distance) * TemperatureInner;
        if(TemperatureMax != 0)
        {
            if(TemperatureMax > 0)
            {
                temp = Mathf.Min(temp, TemperatureMax);
            }
            else
            {
                temp = Mathf.Max(temp, TemperatureMax);
            }
        }
        return temp;
    }
    public void UpdateVisuals()
    {
        if (active)
        {
            if (activeParticleSystem) activeParticleSystemEmission.enabled = true;
            foreach (var item in activeObjects)
            {
                item.SetActive(true);
            }
            foreach (var item in notactiveObjects)
            {
                item.SetActive(false);
            }
        }
        else
        {
            if (activeParticleSystem) activeParticleSystemEmission.enabled = false;
            foreach (var item in activeObjects)
            {
                item.SetActive(false);
            }
            foreach (var item in notactiveObjects)
            {
                item.SetActive(true);
            }
        }
    }
    private void OnDestroy()
    {
        if (atmosphericsController)
            atmosphericsController.modifiers.Remove(this);
    }
}
