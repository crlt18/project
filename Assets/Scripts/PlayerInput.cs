using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public InputActionAsset controls;   //the actions asset

    private InputAction clickAction;    //the click action

    private void OnEnable()
    {
        controls.FindActionMap("Player").Enable();  //the player action map within the 'controls' asset     
    }
    private void OnDisable()
    {
        controls.FindActionMap("Player").Disable();  //the player action map within the 'controls' asset     
    }

    private void Awake()
    {
        clickAction = controls.FindActionMap("Player").FindAction("Click");
    }


}

