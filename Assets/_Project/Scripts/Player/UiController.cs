using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiController : MonoBehaviour
{
    public PlayerController playerController;
    public CanvasGroup Fade;
    public CanvasGroup HUD;
    public TextMeshProUGUI TimeHours;
    public TextMeshProUGUI TimeMinutes;
    public TextMeshProUGUI Temperature;
    public Gradient TemperatureMinus;
    public Gradient TemperaturePlus;
    public Slider Thirst;
    public Slider Hunger;
    public Slider[] Life;
    int LastLifeDivisions;
    private void Awake()
    {
        StartCoroutine(FadeOut(Fade, 0, 2));
        StartCoroutine(FadeIn(HUD, 4, 1));
        LastLifeDivisions = playerController.LifeDivisions;
        UpdateLifeDivisions();
    }

    private void Update()
    {
        GlobalStats();
        LocalStats();
    }
    public void LocalStats()
    {
        if(LastLifeDivisions != playerController.LifeDivisions)
        {
            LastLifeDivisions = playerController.LifeDivisions;
            UpdateLifeDivisions();
        }
        float life = playerController.Life;
        for (ushort i = 0; i < playerController.LifeDivisions; i++)
        {
            Life[i].value = life;
            life -= 1;
        }
        Thirst.value = playerController.Thirst;
        Hunger.value = playerController.Hunger;
    }
    void UpdateLifeDivisions()
    {
        foreach (var item in Life)
        {
            item.gameObject.SetActive(false);
        }
        for (ushort i = 0; i < playerController.LifeDivisions; i++)
        {
            Life[i].gameObject.SetActive(true);
        }
    }
    public void GlobalStats()
    {
        TimeHours.text = GameManager.Instance.atmosphericsController.hms.x.ToString("00");
        TimeMinutes.text = GameManager.Instance.atmosphericsController.hms.y.ToString("00");
        Temperature.text = playerController.Temperature.ToString("0.0");
        if (playerController.TemperatureDamage == 0)
        {
            Temperature.color = Color.white;
        }
        else
        {
            if (playerController.TemperatureDamage < 0)
            {
                Temperature.color = TemperatureMinus.Evaluate(Mathf.Abs(playerController.TemperatureDamage));
            }
            else
            {
                Temperature.color = TemperaturePlus.Evaluate(Mathf.Abs(playerController.TemperatureDamage));
            }
        }
    }
    public IEnumerator FadeOut(CanvasGroup group, float fadeTime = 2, float waitTime = 0)
    {
        float count = waitTime;
        group.alpha = 1;
        while (count > 0)
        {
            count -= Time.deltaTime;
            yield return null;
        }
        count = 0;
        while (count < 1)
        {
            count += Time.deltaTime / fadeTime;
            group.alpha = Mathf.Lerp(1, 0, count);
            yield return null;
        }
        group.alpha = 0;
    }
    public IEnumerator FadeIn(CanvasGroup group, float fadeTime = 2, float waitTime = 0)
    {
        float count = waitTime;
        group.alpha = 0;
        while(count > 0)
        {
            count -= Time.deltaTime;
            yield return null;
        }
        count = 0;
        while (count < 1)
        {
            count += Time.deltaTime / fadeTime;
            group.alpha = Mathf.Lerp(0, 1, count);
            yield return null;
        }
        group.alpha = 1;
    }
}
