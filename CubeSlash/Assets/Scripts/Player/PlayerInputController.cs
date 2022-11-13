using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : Singleton
{
    public static PlayerInputController Instance { get { return Instance<PlayerInputController>(); } }

    protected override void Initialize()
    {
        base.Initialize();
        var controls = PlayerInput.Controls;
    }
}