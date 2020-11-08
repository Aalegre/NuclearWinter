using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public class Action
    {
        public InputAction.CallbackContext ctx;
        public virtual bool Update(InputAction.CallbackContext context)
        {
            ctx = context;
            return context.canceled;
        }
    }
    [System.Serializable]
    public class Axis : Action
    {
        public Vector2 axis;
        public override bool Update(InputAction.CallbackContext context)
        {
            base.Update(context);
            axis = context.ReadValue<Vector2>();
            if (context.canceled)
            {
                axis = Vector2.zero;
            }
            else
            {

            }
            return context.canceled;
        }
    }
    [System.Serializable]
    public class Button : Action
    {
        public bool pressed;
        public bool waspressed;
        public bool ButtonDown() { return !waspressed && pressed; }
        public bool ButtonPressed() { return pressed; }
        public bool ButtonUp() { return waspressed && !pressed; }
        public override bool Update(InputAction.CallbackContext context)
        {
            base.Update(context);
            if (context.started)
            {
                pressed = true;
            }
            if (context.canceled)
            {
                pressed = false;
            }
            else
            {

            }
            return context.canceled;
        }
    }


    static public InputManager Instance;
    public PlayerInput playerInput;
    public enum MAP { Player, UI};
    public enum SCHEME { KEYBOARD, CONTROLLER, PLAYSTATION};
    public string[] SCHEMENAME = { "KeyboardMouse", "Controller", "PlayStation" };
    public SCHEME currentController;
    public Axis Move;
    public Axis Look;
    public Axis Mouse;
    public Button Fire;
    public Button Interact;
    public Button Weapon;
    public Button Torch;
    public Button Pause;
    public Button Cancel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
            return;
        }
        if (!playerInput)
            playerInput = GetComponent<PlayerInput>();
    }
    private void LateUpdate()
    {
        Fire.waspressed = Fire.pressed;
        Interact.waspressed = Interact.pressed;
        Weapon.waspressed = Weapon.pressed;
        Torch.waspressed = Torch.pressed;
    }
    public IEnumerator Feedback_Coroutine(float LowFrequencyPower, float LowFrequencyStart, float LowFrequencyEnd, float HighFrequencyPower, float HighFrequencyStart, float HighFrequencyEnd)
    {
        if (currentController == SCHEME.KEYBOARD)
            yield break;
        float lowWait = -LowFrequencyStart;
        float highWait = -HighFrequencyStart;
        float lowPower = 0;
        float highPower = 0;
        while(lowWait < LowFrequencyEnd || highWait < HighFrequencyEnd)
        {
            lowWait += Time.deltaTime;
            highWait += Time.deltaTime;
            if (lowWait >= 0)
                lowPower = Mathf.Lerp(1, 0, lowWait / LowFrequencyEnd);
            if (highWait >= 0)
                highPower = Mathf.Lerp(1, 0, highWait / HighFrequencyEnd);
            Gamepad.current.SetMotorSpeeds(lowPower, highPower);
            yield return null;
        }
        Gamepad.current.SetMotorSpeeds(0, 0);
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        UpdateScheme();
        Move.Update(context);
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        UpdateScheme();
        if (currentController == SCHEME.KEYBOARD)
        {
            Mouse.ctx = context;
            Mouse.axis = context.ReadValue<Vector2>();
            Look.ctx = context;
            Look.axis = context.ReadValue<Vector2>();
            Look.axis -= GameManager.Instance.resolutionHalfF;
            Look.axis /= GameManager.Instance.resolutionMinF * 0.5f;
            if (Look.axis.magnitude > 1f)
                Look.axis.Normalize();
        }
        else
        {
            Look.Update(context);
        }
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        UpdateScheme();
        if (Fire.Update(context)) 
            StartCoroutine(ButtonCancelled(Fire));
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        UpdateScheme();
        if (Interact.Update(context)) 
            StartCoroutine(ButtonCancelled(Interact));
    }
    public void OnWeapon(InputAction.CallbackContext context)
    {
        UpdateScheme();
        if (Weapon.Update(context)) 
            StartCoroutine(ButtonCancelled(Weapon));
    }
    public void OnTorch(InputAction.CallbackContext context)
    {
        UpdateScheme();
        if (Torch.Update(context)) 
            StartCoroutine(ButtonCancelled(Torch));
    }
    public void OnPause(InputAction.CallbackContext context)
    {
        UpdateScheme();
        if (Pause.Update(context)) 
            StartCoroutine(ButtonCancelled(Pause));
    }
    public void OnCancel(InputAction.CallbackContext context)
    {
        UpdateScheme();
        if (Cancel.Update(context)) 
            StartCoroutine(ButtonCancelled(Cancel));
    }

    public void ChangeMap(MAP map)
    {
        playerInput.SwitchCurrentActionMap(map.ToString());
    }

    public void UpdateScheme()
    {
        try
        {
            string currentScheme = playerInput.currentControlScheme;
            for (int i = 0; i < SCHEMENAME.Length; i++)
            {
                if (currentScheme == SCHEMENAME[i])
                {
                    currentController = (SCHEME)i;
                    return;
                }
            }
        }
        catch { }
    }

    IEnumerator ButtonCancelled(Button btn)
    {
        yield return null;
        btn.waspressed = false;
    }
}
