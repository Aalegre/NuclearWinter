using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CameraController cameraController;
    public UiController uiController;
    public InventoryController inventoryController;
    public bool useCamera = true;
    public PlayerCharacterController playerCharacterController;
    public AtmosphericsController atmosphericsController;
    public float walkSpeed = 2;
    public float runSpeed = .5f;
    public Vector3 AimDirection;
    public Vector3 AimPosition;
    public float AimRadius = 10;
    public float AimHeight = 1.5f;
    [Range(0,.5f)]
    public float AimMinRadius = .1f;
    public Vector2 MoveAxis;
    [Header("Stats")]
    [Range(0, 10)] public float Life = 1;
    [Range(1, 10)] public ushort LifeDivisions = 3;
    [Range(0, .1f)] public float LifeGainSpeed = 0.02f;
    [Range(0, 1)] public float Hunger = 1;
    [Range(0, .01f)] public float HungerLostSpeed = 0.0005f;
    public float HungerSaturation = 1;
    [Range(0, 1)] public float Thirst = 1;
    [Range(0, .01f)] public float ThirstLostSpeed = 0.003f;
    public float ThirstSaturation = 1;
    public float Temperature = 10;
    float LastTemperature = 10;
    [Range(0, 10)] public float TemperatureRise = 4;
    [Range(0, 10)] public float TemperatureFall = 1;
    public float TemperatureMaxBase = 36;
    public float TemperatureMaxScale = 0.1f;
    public float TemperatureMinBase = -5;
    public float TemperatureMinScale = 0.25f;
    public float TemperatureDamage = 0;
    public float TemperatureDamageLifeScale = 1;
    [Range(0, 5)] public float ShootDelay = 0.5f;
    float ShootTimer = 0;
    [Range(0, .1f)] public float TorchDecay = 0.05f;
    private void Awake()
    {
        GameManager.Instance.playerController = this;
    }
    private void Start()
    {
        LastTemperature = GameManager.Instance.atmosphericsController.GetTemperature(playerCharacterController.transform.position);
        Temperature = LastTemperature;
    }

    // Fixed update is called in sync with physics
    private void Update()
    {
        Inputs();
        UpdateTemperature();
        UpdateStats();
    }

    void Inputs()
    {
        if (InputManager.Instance.Pause.ButtonDown())
        {
            GameManager.Instance.Paused = !GameManager.Instance.Paused;
            if (GameManager.Instance.Paused)
            {
                InputManager.Instance.ChangeMap(InputManager.MAP.UI);
            }
            else
            {
                InputManager.Instance.ChangeMap(InputManager.MAP.Player);
            }
        }
        // read inputs
        if (MoveAxis.magnitude > .5f)
        {
            MoveAxis = Vector2.Lerp(MoveAxis, InputManager.Instance.Move.axis, Time.deltaTime * runSpeed);
        }
        else
        {
            MoveAxis = Vector2.Lerp(MoveAxis, InputManager.Instance.Move.axis, Time.deltaTime * walkSpeed);
        }

        Vector3 move = Vector3.zero;

        // calculate move direction to pass to character
        if (cameraController && useCamera)
        {
            // calculate camera relative direction to move:
            move = MoveAxis.y * Vector3.Scale(cameraController.cam.transform.forward, new Vector3(1, 0, 1)).normalized + MoveAxis.x * cameraController.cam.transform.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            move = MoveAxis.y * Vector3.forward + MoveAxis.x * Vector3.right;
        }

        // pass all parameters to the character control script
        playerCharacterController.Move(move, false, false);
        if (InputManager.Instance.Look.axis.magnitude > AimMinRadius)
        {
            Vector2 aim = InputManager.Instance.Look.axis;
            if (InputManager.Instance.currentController != InputManager.SCHEME.KEYBOARD)
            {
                aim.Normalize();
            }
            AimDirection = new Vector3(aim.x, 0, aim.y) * AimRadius;
            AimDirection.y = AimHeight;
        }
        AimPosition = playerCharacterController.root.position + AimDirection;
        if (playerCharacterController.Gun)
        {
            ShootTimer += Time.deltaTime;
            if (playerCharacterController.ShowGun)
            {
                if (InputManager.Instance.Fire.ButtonDown() && ShootTimer >= ShootDelay && inventoryController.inventory.Bullets > 0)
                {
                    playerCharacterController.Shoot();
                    ShootTimer = 0;
                    inventoryController.inventory.Bullets--;
                }
            }
        }
        else
        {
            ShootTimer = ShootDelay;
        }
        if (InputManager.Instance.Weapon.ButtonDown())
        {
            playerCharacterController.Gun = !playerCharacterController.Gun;
        }
        if (InputManager.Instance.Torch.ButtonDown())
        {
            playerCharacterController.Torch = !playerCharacterController.Torch;
        }
    }

    void UpdateTemperature()
    {
        LastTemperature = GameManager.Instance.atmosphericsController.GetTemperature(playerCharacterController.transform.position);
        if(Temperature > LastTemperature)
        {
            Temperature = Mathf.Lerp(Temperature, LastTemperature, Time.deltaTime * TemperatureFall);
        }
        else
        {
            Temperature = Mathf.Lerp(Temperature, LastTemperature, Time.deltaTime * TemperatureRise);
        }
        if(Temperature < TemperatureMinBase)
        {
            TemperatureDamage = (Temperature - TemperatureMinBase) * TemperatureMinScale;
        }
        else if(Temperature > TemperatureMaxBase)
        {
            TemperatureDamage = (Temperature - TemperatureMaxBase) * TemperatureMaxScale;
        }
        else
        {
            TemperatureDamage = 0;
        }
    }


    void UpdateStats()
    {
        if(TemperatureDamage != 0)
        {
            float modifier = Mathf.Abs(TemperatureDamage) + 1;
            if(TemperatureDamage < 0)
            {
                HungerSaturation -= Time.deltaTime * HungerLostSpeed * modifier;
                ThirstSaturation -= Time.deltaTime * ThirstLostSpeed;
                if(TemperatureDamage < -1)
                {
                    Life -= Mathf.LerpUnclamped(0, Time.deltaTime * TemperatureDamageLifeScale, Mathf.Max(Mathf.Abs(TemperatureDamage + 1), 0));
                }
                //Debug.Log(modifier + " : " + HungerLostSpeed * modifier);
            }
            else
            {
                HungerSaturation -= Time.deltaTime * HungerLostSpeed;
                ThirstSaturation -= Time.deltaTime * ThirstLostSpeed * modifier;
                //Debug.Log(modifier + " : " + ThirstLostSpeed * modifier);
            }
        }
        else
        {
            HungerSaturation -= Time.deltaTime * HungerLostSpeed;
            ThirstSaturation -= Time.deltaTime * ThirstLostSpeed;
        }
        if (HungerSaturation < -1)
            HungerSaturation = -1;
        if (ThirstSaturation < -1)
            ThirstSaturation = -1;
        Hunger = HungerSaturation + 1;
        Thirst = ThirstSaturation + 1;
        Hunger = Mathf.Clamp01(Hunger);
        Thirst = Mathf.Clamp01(Thirst);
        if (Hunger > 0 && Thirst > 0)
        {
            float div = Life % 1f;
            div = Life - div;
            div += 0.999f;
            Life += Time.deltaTime * LifeGainSpeed;
            if (Life > div)
                Life = div;
        }
        Life = Mathf.Clamp(Life, 0, LifeDivisions);
        if (playerCharacterController.Torch)
        {
            inventoryController.inventory.lampGas -= Time.deltaTime * TorchDecay;
            if(inventoryController.inventory.lampGas <= 0)
            {
                playerCharacterController.TorchLighted = false;
            }
            else
            {
                playerCharacterController.TorchLighted = true;
            }
        }
    }
}
