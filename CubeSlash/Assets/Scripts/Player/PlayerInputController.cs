using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : Singleton
{
    public static PlayerInputController Instance { get { return Instance<PlayerInputController>(); } }

    protected override void Initialize()
    {
        base.Initialize();
        var controls = PlayerInput.Controls;
    }
}