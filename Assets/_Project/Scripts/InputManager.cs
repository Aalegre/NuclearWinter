using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public class Action
    {
        public InputAction.CallbackContext ctx;
    }
    [System.Serializable]
    public class Axis : Action
    {
        public Vector2 axis;
    }
    [System.Serializable]
    public class Button : Action
    {
        public bool pressed;
        public bool waspressed;
        public bool ButtonDown() { return !waspressed && pressed; }
        public bool ButtonPressed() { return pressed; }
        public bool ButtonUp() { return waspressed && !pressed; }
    }


    static public InputManager Instance;

    public Axis Move;
    public Button Fire;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        Fire.ctx = context;
        Fire.waspressed = Fire.pressed;
        Fire.pressed = context.performed;
        if (!context.performed)
            StartCoroutine(ButtonCancelled(Fire));
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move.ctx = context;
        Move.axis = context.ReadValue<Vector2>();
    }

    IEnumerator ButtonCancelled(Button btn)
    {
        yield return null;
        btn.waspressed = false;
    }
}
