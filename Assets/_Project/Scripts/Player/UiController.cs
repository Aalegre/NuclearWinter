using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiController : MonoBehaviour
{
    public PlayerController playerController;
    public Canvas canvas;
    [HideInInspector] public RectTransform canvasRect;
    public CanvasGroup Fade;
    [Header("Pause")]
    public bool Paused;
    public CanvasGroup Pause;
    [Header("Hud")]
    public CanvasGroup HUD;
    public TextMeshProUGUI TimeHours;
    public TextMeshProUGUI TimeMinutes;
    public TextMeshProUGUI Temperature;
    public Gradient TemperatureMinus;
    public Gradient TemperaturePlus;
    public Slider Thirst;
    public Slider Hunger;
    public Slider[] Life;
    public Image Torch;
    public GameObject[] Bullets;
    int lastBullets;
    public GameObject[] Prompts_PS;
    public GameObject[] Prompts_XB;
    public GameObject[] Prompts_PC;
    public bool Interact;
    public Vector3 InteractPos;
    public CanvasGroup Interact_CG;
    RectTransform Interact_Rect;
    public float InteractFadeSpeed = 3;
    InputManager.SCHEME lastScheme;
    int LastLifeDivisions;
    private void Awake()
    {
        StartCoroutine(FadeOut(Fade, 0, 2));
        StartCoroutine(FadeIn(HUD, 4, 1));
        LastLifeDivisions = playerController.LifeDivisions;
        UpdateLifeDivisions();
        Interact_CG.alpha = 0;
        Interact_Rect = Interact_CG.GetComponent<RectTransform>();
        if (!canvas)
            canvas = GetComponent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        Pause.blocksRaycasts = false;
        Pause.interactable = false;
        Pause.alpha = 0;
    }

    private void Update()
    {
        if (lastScheme != InputManager.Instance.currentController) PromptsUpdate(InputManager.Instance.currentController);
        GlobalStats();
        LocalStats();
    }
    private void LateUpdate()
    {
        InteractUpdate();
    }

    public void PauseUpdate()
    {
        if (Paused)
        {
            Pause.blocksRaycasts = true;
            Pause.interactable = true;
            Pause.alpha += Time.deltaTime * 10;
        }
        else
        {
            Pause.blocksRaycasts = false;
            Pause.interactable = false;
            Pause.alpha += Time.deltaTime * 10;
        }
    }
    public void PromptsUpdate(InputManager.SCHEME scheme)
    {
        lastScheme = scheme;
        switch (lastScheme)
        {
            case InputManager.SCHEME.KEYBOARD:
                foreach (var item in Prompts_PS) item.SetActive(false);
                foreach (var item in Prompts_XB) item.SetActive(false);
                foreach (var item in Prompts_PC) item.SetActive(true);
                break;
            case InputManager.SCHEME.CONTROLLER:
                foreach (var item in Prompts_PS) item.SetActive(false);
                foreach (var item in Prompts_XB) item.SetActive(true);
                foreach (var item in Prompts_PC) item.SetActive(false);
                break;
            case InputManager.SCHEME.PLAYSTATION:
                foreach (var item in Prompts_PS) item.SetActive(true);
                foreach (var item in Prompts_XB) item.SetActive(false);
                foreach (var item in Prompts_PC) item.SetActive(false);
                break;
        }
    }
    public void InteractUpdate()
    {
        if (Interact)
        {
            Interact_CG.alpha += Time.deltaTime * InteractFadeSpeed;
            Vector2 ViewportPosition = playerController.cameraController.cam.WorldToViewportPoint(InteractPos);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
            Interact_Rect.anchoredPosition = WorldObject_ScreenPosition;
        }
        else
        {
            Interact_CG.alpha -= Time.deltaTime * InteractFadeSpeed;
        }
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
        Torch.fillAmount = playerController.inventoryController.inventory.lampGas / playerController.inventoryController.inventory.lampGasMax;
        if (lastBullets != playerController.inventoryController.inventory.Bullets)
            UpdateBullets();
    }
    void UpdateBullets()
    {
        lastBullets = playerController.inventoryController.inventory.Bullets;
        for (int i = 0; i < Bullets.Length; i++)
        {
            if (i >= lastBullets)
                Bullets[i].SetActive(false);
            else
                Bullets[i].SetActive(true);
        }
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
