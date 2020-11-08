using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UiController : MonoBehaviour
{
    public PlayerController playerController;
    public Canvas canvas;
    [HideInInspector] public RectTransform canvasRect;
    public CanvasGroup Fade;
    [Header("Options")]
    public CanvasGroup Pause;
    public TMP_Dropdown Resolution;
    public TMP_InputField TimeSpeed;
    public Toggle Rain;
    public Slider RainValue;
    public Toggle Fog;
    public Slider FogValue;
    Resolution[] resolutions;
    public TMP_Dropdown Quality;
    [Header("Inventory")]
    public TextMeshProUGUI Bullets;
    public TextMeshProUGUI Textiles;
    public TextMeshProUGUI Bottles;
    public TextMeshProUGUI Woods;
    public TextMeshProUGUI Bandaids;
    public TextMeshProUGUI Camps;
    public TextMeshProUGUI Food;
    public TextMeshProUGUI Water;
    public TextMeshProUGUI Alcohol;
    public TextMeshProUGUI Oil;
    public TextMeshProUGUI Lamp;
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
    public GameObject[] BulletsCounter;
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
        StartCoroutine(FadeOut(Fade, 2, 0));
        StartCoroutine(FadeIn(HUD, 2, 3));
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

        Quality.ClearOptions();
        Quality.AddOptions(QualitySettings.names.ToList());
        Quality.value = QualitySettings.GetQualityLevel();
        Quality.onValueChanged.AddListener(delegate { UpdateQuality(); });
        UpdateQuality();


        Application.targetFrameRate = 300;
        resolutions = Screen.resolutions;
        Resolution.ClearOptions();
        int selected = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if(resolutions[i].height == GameManager.Instance.resolution.y && resolutions[i].width == GameManager.Instance.resolution.x)
                selected = i;
            Resolution.AddOptions(new List<string>() { resolutions[i].width + "x" + resolutions[i].height + " (" + resolutions[i].refreshRate + ")" });
        }
        Resolution.value = selected;
        Resolution.onValueChanged.AddListener(delegate { UpdateResolution(); });
        UpdateResolution();
    }

    private void Update()
    {
        if (lastScheme != InputManager.Instance.currentController) PromptsUpdate(InputManager.Instance.currentController);
        GlobalStats();
        LocalStats();
        PauseUpdate();
    }
    private void LateUpdate()
    {
        InteractUpdate();
    }

    public void PauseUpdate()
    {
        if (GameManager.Instance.Paused)
        {
            Pause.blocksRaycasts = true;
            Pause.interactable = true;
            Pause.alpha += Time.deltaTime * 10;
            if (InputManager.Instance.Cancel.ButtonDown())
            {
                GameManager.Instance.Paused = false;
                InputManager.Instance.ChangeMap(InputManager.MAP.Player);
            }
            OptionsUpdate();
            InventoryUpdate();
        }
        else
        {
            Pause.blocksRaycasts = false;
            Pause.interactable = false;
            Pause.alpha -= Time.deltaTime * 10;
        }
    }
    public void OptionsUpdate()
    {
        int newTime = 60;
        int.TryParse(TimeSpeed.text, out newTime);
        GameManager.Instance.atmosphericsController.TimeSpeed = newTime;
        if (Rain.isOn)
        {
            RainValue.interactable = false;
            GameManager.Instance.atmosphericsController.UpdateRain = true;
            RainValue.value = GameManager.Instance.atmosphericsController.Rain;
        }
        else
        {
            RainValue.interactable = true;
            GameManager.Instance.atmosphericsController.UpdateRain = false;
            GameManager.Instance.atmosphericsController.Rain = RainValue.value;
        }
        if (Fog.isOn)
        {
            FogValue.interactable = false;
            GameManager.Instance.atmosphericsController.UpdateFog = true;
            FogValue.value = GameManager.Instance.atmosphericsController.Fog;
        }
        else
        {
            FogValue.interactable = true;
            GameManager.Instance.atmosphericsController.UpdateFog = false;
            GameManager.Instance.atmosphericsController.Fog = FogValue.value;
        }
    }
    public void InventoryUpdate()
    {
        Bullets.text = playerController.inventoryController.inventory.Bullets + "/" + playerController.inventoryController.inventory.BulletsMax;
        Textiles.text = playerController.inventoryController.inventory.Textiles + "/" + playerController.inventoryController.inventory.TextilesMax;
        Bottles.text = playerController.inventoryController.inventory.Bottles + "/" + playerController.inventoryController.inventory.BottlesMax;
        Woods.text = playerController.inventoryController.inventory.Woods + "/" + playerController.inventoryController.inventory.WoodsMax;
        Bandaids.text = playerController.inventoryController.inventory.Bandaids + "/" + playerController.inventoryController.inventory.BandaidsMax;
        Camps.text = playerController.inventoryController.inventory.Camps + "/" + playerController.inventoryController.inventory.CampsMax;
        Food.text = playerController.inventoryController.inventory.Food.ToString("0.0") + "/" + playerController.inventoryController.inventory.FoodMax.ToString("0");
        Water.text = playerController.inventoryController.inventory.Water.ToString("0.0") + "/" + playerController.inventoryController.inventory.WaterMax.ToString("0");
        Alcohol.text = playerController.inventoryController.inventory.Alcohol.ToString("0.0") + "/" + playerController.inventoryController.inventory.AlcoholMax.ToString("0");
        Oil.text = playerController.inventoryController.inventory.Oil.ToString("0.0") + "/" + playerController.inventoryController.inventory.OilMax.ToString("0");
        Lamp.text = playerController.inventoryController.inventory.lampGas.ToString("0.0") + "/" + playerController.inventoryController.inventory.lampGasMax.ToString("0");
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
        for (int i = 0; i < BulletsCounter.Length; i++)
        {
            if (i >= lastBullets)
                BulletsCounter[i].SetActive(false);
            else
                BulletsCounter[i].SetActive(true);
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


    public void UpdateResolution()
    {
        int selected = Resolution.value;
        Screen.SetResolution(resolutions[selected].width, resolutions[selected].height, true);
        Application.targetFrameRate = resolutions[selected].refreshRate;
        playerController.cameraController.UpdateResolution();
    }
    public void UpdateQuality()
    {
        QualitySettings.SetQualityLevel(Quality.value, true);
        if(QualitySettings.GetQualityLevel() <= 0)
        {
            playerController.cameraController.reflections.gameObject.SetActive(false);
        }
        else
        {
            playerController.cameraController.reflections.gameObject.SetActive(true);
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
