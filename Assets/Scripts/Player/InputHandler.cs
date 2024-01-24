using UnityEngine;

public class InputHandler : MonoBehaviour
{ 
    // this script "binds" input maps to actions in a centralised location

    public static InputHandler instance { get; private set; }
    public PlayerInput input { get; private set; }
    public bool active { get; private set; }
    void Awake()
    {
        instance = this;
        input = new();
        input.Player.Enable();
    }
    void Update()
    {
        if (!active) { return; }

        // grapple button held
        if (input.Player.Grapple.IsPressed()) 
        { 
            Grapple.instance.GrappleHeld();
            if (input.Player.CancelGrapple.IsPressed()) { Grapple.instance.GrappleCancel(); }
        }

        // grapple button released
        if (input.Player.Grapple.WasReleasedThisFrame()) { Grapple.instance.GrappleReleased(); } 

        // mouse vector
        Player.instance.mouseRotation = input.Player.Look.ReadValue<Vector2>(); 

        //if (Input.GetKeyDown(KeyCode.R)) { Grapple.instance.GrappleAmmoCheck(); }
	}
    public void SetActive(bool state)
    {
        active = state;
    }
}