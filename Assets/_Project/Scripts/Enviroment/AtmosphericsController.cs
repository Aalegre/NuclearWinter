using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class AtmosphericsController : MonoBehaviour
{
    public bool SyncWithRealtime = true;
    public Vector4 hms;
    public float evaluateTime;
    public System.TimeSpan time;
    public const float MINUTESDIVISOR = 1f / 60f;
    public const float SECONDSDIVISOR = 1f / 60f / 60f;
    public const float MILLISECONDSDIVISOR = 1f / 60f / 60f / 1000f;
    public float TimeSpeed = 60;
    public AnimationCurve TemperatureOverTime = AnimationCurve.EaseInOut(0, 0, 24, 0);
    public float Temperature;
    public bool day;
    public float SunSet = 18;
    public float SunRise = 6;
    [Header("Sun")]
    public Light Sun;
    public HDAdditionalLightData SunData;
    public float SunTimeOffset = 0;
    public AnimationCurve SunRotationOverTime = AnimationCurve.Linear(0, 0, 24, 360);
    public AnimationCurve SunIntensityOverTime = AnimationCurve.Linear(0, 0, 24, 0);
    public float SunIntensityFree = 1;
    public float SunIntensityRain = 0.1f;
    public float SunIntensityFog = 0.1f;
    public AnimationCurve SunColorTemperatureOverTime = AnimationCurve.EaseInOut(0, 6570, 24, 6570);
    public float SunColorTemperatureFree = 1;
    public float SunColorTemperatureRain = 5f;
    public float SunColorTemperatureFog = 2f;
    public AnimationCurve SunTemperatureOverTime = AnimationCurve.EaseInOut(0, 0, 24, 0);
    public float SunDiskFree = 2;
    public float SunDiskRain;
    public float SunDiskFog;
    [Header("Moon")]
    public Light Moon;
    public HDAdditionalLightData MoonData;
    public float MoonTimeOffset = 6;
    public AnimationCurve MoonRotationOverTime = AnimationCurve.Linear(0, 0, 24, 360);
    public AnimationCurve MoonIntensityOverTime = AnimationCurve.Linear(0, 0, 24, 0);
    public float MoonIntensityFree = 1;
    public float MoonIntensityRain = 0.5f;
    public float MoonIntensityFog = 0.25f;
    public AnimationCurve MoonColorTemperatureOverTime = AnimationCurve.EaseInOut(0, 6570, 24, 6570);
    public float MoonColorTemperatureFree = 1;
    public float MoonColorTemperatureRain = .7f;
    public float MoonColorTemperatureFog = .8f;
    public float MoonDiskFree = 2;
    public float MoonDiskRain;
    public float MoonDiskFog;
    [Header("Fog")]
    public DensityVolume densityVolume;
    public Vector3 volumeOffset;
    public Texture3D noise;
    public int noiseSize = 32;
    public float noiseScale = 16;
    public float FogDistanceFree = 100;
    public float FogDistanceRain = 10;
    public float FogDistanceFog = 1;
    [Header("Weather")]
    public bool UpdateRain = true;
    [Range(0, 1)] public float Rain;
    public Vector2 rainNextProbability = new Vector2(1, 3);
    public Vector2 rainProbability = new Vector2(-1, 3);
    float rainNext = -100;
    public float rainFadeSpeed = 1;
    float rainFade = 0;
    public ParticleSystem rainParticles;
    public ParticleSystem.EmissionModule rainParticlesEmission;
    public float rainParticleEmission = 1000;
    [Range(0, 1)] public float Fog;
    public bool UpdateFog = true;
    public Vector2 fogNextProbability = new Vector2(1, 3);
    public Vector2 fogProbability = new Vector2(-1, 3);
    float fogNext = -100;
    public float fogFadeSpeed = 1;
    float fogFade = 0;
    void Awake()
    {
        GameManager.Instance.atmosphericsController = this;
        if (SyncWithRealtime)
        {
            time = System.DateTime.Now.TimeOfDay;
        }
        else
        {
            evaluateTime = hms.x + hms.y * MINUTESDIVISOR + hms.z * SECONDSDIVISOR + hms.w * MILLISECONDSDIVISOR;
            time = System.TimeSpan.FromHours(evaluateTime);
        }
        if (!noise)
            Generate3dNoise();
        rainParticlesEmission = rainParticles.emission;
        UpdateWeather();
        Rain = rainFade;
        Fog = fogFade;
    }

    public void Generate3dNoise()
    {
        noise = new Texture3D(noiseSize, noiseSize, noiseSize, TextureFormat.Alpha8, false);
        noise.wrapMode = TextureWrapMode.Mirror;
        Color[] colors = new Color[noiseSize * noiseSize * noiseSize];
        float inverseResolution = 1.0f / (noiseSize - 1.0f);
        Vector3 random = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
        float minVal = 1;
        float maxVal = 0;
        for (int z = 0; z < noiseSize; z++)
        {
            int zOffset = z * noiseSize * noiseSize;
            for (int y = 0; y < noiseSize; y++)
            {
                int yOffset = y * noiseSize;
                for (int x = 0; x < noiseSize; x++)
                {
                    Vector3 pos = (new Vector3((float)x / (float)noiseSize, (float)y / (float)noiseSize, (float)z / (float)noiseSize) * noiseScale) + random;
                    float gray = Utils.PerlinNoise3d(pos);
                    minVal = Mathf.Min(minVal, gray);
                    maxVal = Mathf.Max(maxVal, gray);
                    colors[x + yOffset + zOffset] = new Color(gray, gray, gray, gray);
                }
            }
        }
        for (int i = 0; i < colors.Length; i++)
        {
            float gray = Utils.Map(colors[i].a, minVal, maxVal, 0, 1);
            Debug.Log(gray + " : " + colors[i].a);
            colors[i] = new Color(gray, gray, gray, gray);
        }
        noise.SetPixels(colors);
        noise.Apply();
        densityVolume.parameters.volumeMask = noise;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
        UpdateWeather();
        UpdateEnviroment();
    }
    void UpdateTime()
    {
        time = time.Add(System.TimeSpan.FromSeconds(Time.deltaTime * TimeSpeed));
        if (time.Hours >= 24)
        {
            time = System.TimeSpan.FromTicks(time.Ticks % new System.TimeSpan(24, 0, 0).Ticks);
        }
        hms.x = time.Hours;
        hms.y = time.Minutes;
        hms.z = time.Seconds;
        hms.w = time.Milliseconds;
        //evaluateTime = hms.x + hms.y * MINUTESDIVISOR + hms.z * SECONDSDIVISOR;
        evaluateTime = hms.x + hms.y * MINUTESDIVISOR + hms.z * SECONDSDIVISOR + hms.w * MILLISECONDSDIVISOR;
        day = evaluateTime > SunRise && evaluateTime < SunSet;
    }

    public void UpdateWeather()
    {
        if (UpdateRain)
        {
            if (rainNext < evaluateTime)
            {
                rainNext = (evaluateTime + Random.Range(rainNextProbability.x, rainNextProbability.y)) % 24f;
                rainFade = Mathf.Clamp(Random.Range(rainProbability.x, rainProbability.y), 0, 1);
            }
            Rain = Mathf.Lerp(Rain, rainFade, Time.deltaTime * rainFadeSpeed);
        }
        rainParticlesEmission.rateOverTime = Mathf.Lerp(0, rainParticleEmission, Rain);

        if (UpdateFog)
        {
            if (fogNext < evaluateTime)
            {
                fogNext = (evaluateTime + Random.Range(fogNextProbability.x, fogNextProbability.y)) % 24f;
                fogFade = Mathf.Clamp(Random.Range(fogProbability.x, fogProbability.y), 0, 1);
            }
            Fog = Mathf.Lerp(Fog, fogFade, Time.deltaTime * fogFadeSpeed);
        }
    }

    public void UpdateTemperature()
    {
        Temperature = TemperatureOverTime.Evaluate(evaluateTime);
    }
    public void UpdateEnviroment()
    {
        float mult = 1;
        Sun.transform.localRotation = Quaternion.Euler(SunRotationOverTime.Evaluate((evaluateTime + SunTimeOffset) % 24f), 0, 0);
        mult = Mathf.Lerp(Mathf.Lerp(SunColorTemperatureFree, SunColorTemperatureRain, Rain), SunColorTemperatureFog, Fog);
        Sun.colorTemperature = SunColorTemperatureOverTime.Evaluate(evaluateTime) * mult;
        mult = Mathf.Lerp(Mathf.Lerp(SunIntensityFree, SunIntensityRain, Rain), SunIntensityFog, Fog);
        SunData.intensity = SunIntensityOverTime.Evaluate(evaluateTime) * mult;
        Moon.transform.localRotation = Quaternion.Euler(MoonRotationOverTime.Evaluate((evaluateTime + MoonTimeOffset) % 24f), 0, 0);
        mult = Mathf.Lerp(Mathf.Lerp(MoonColorTemperatureFree, MoonColorTemperatureRain, Rain), MoonColorTemperatureFog, Fog);
        Moon.colorTemperature = MoonColorTemperatureOverTime.Evaluate(evaluateTime) * mult;
        mult = Mathf.Lerp(Mathf.Lerp(MoonIntensityFree, MoonIntensityRain, Rain), MoonIntensityFog, Fog);
        MoonData.intensity = MoonIntensityOverTime.Evaluate(evaluateTime) * mult;
        if (day)
        {
            Sun.shadows = LightShadows.Soft;
            Moon.shadows = LightShadows.None;
        }
        else
        {
            Sun.shadows = LightShadows.None;
            Moon.shadows = LightShadows.Soft;
        }
        SunData.angularDiameter = Mathf.Lerp(Mathf.Lerp(SunDiskFree, SunDiskRain, Rain), SunDiskFog, Fog);
        MoonData.angularDiameter = Mathf.Lerp(Mathf.Lerp(MoonDiskFree, MoonDiskRain, Rain), MoonDiskFog, Fog);
        try { densityVolume.transform.position = GameManager.Instance.playerCharacterController.transform.position + volumeOffset; } catch { }
        densityVolume.parameters.distanceFadeEnd = Mathf.Lerp(Mathf.Lerp(FogDistanceFree, FogDistanceRain, Rain), FogDistanceFog, Fog);
        try { rainParticles.transform.position = GameManager.Instance.cameraController.cam.transform.position + GameManager.Instance.playerCharacterController.m_Rigidbody.velocity * 2; } catch { }
    }
}
