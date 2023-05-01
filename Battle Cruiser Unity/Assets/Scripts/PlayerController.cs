using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Inputs _inputs;

    [field: SerializeField]
    public float Speed { get; private set; } = 5;
    [field: SerializeField]
    public Vector2 Velocity { get; private set; }


    void Awake()
    {
        _inputs = new Inputs();
        _inputs.Player.Movement.performed += HandleStartMovement;
        _inputs.Player.Movement.canceled += HandleStopMovement;
    }

    private void HandleStopMovement(InputAction.CallbackContext obj)
    {
        Velocity = Vector2.zero;
    }

    private void HandleStartMovement(InputAction.CallbackContext ctx)
    {
        Vector2 raw = ctx.ReadValue<Vector2>();
        Velocity = raw;
    }

    void OnEnable()
    {
        _inputs.Enable();
    }

    void OnDisable()
    {
        _inputs.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 position = transform.position;
        position += Velocity * Speed * Time.deltaTime;
        transform.position = position;
    }
}
