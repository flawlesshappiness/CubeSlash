using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour, IInitializable
{
    public static PlayerInputController Instance;
    public enum JoystickType { XBOX, PLAYSTATION }
    public enum JoystickButtonType { NORTH, EAST, SOUTH, WEST }
    public enum MouseButtonType { LMB, RMB, MMB }

    private string[] buttons_ability = new string[] { "Fire1", "Fire2", "Fire3", "Jump" };
    public int CountAbilityButtons { get { return buttons_ability.Length; } }

    public void Initialize()
    {
        Instance = this;
    }

    private void Update()
    {
        InputAbilityUpdate();
    }

    private void InputAbilityUpdate()
    {
        for (int i = 0; i < buttons_ability.Length; i++)
        {
            var button = buttons_ability[i];
            if (Input.GetButtonDown(button))
            {
                Player.Instance.PressAbility(i);
            }

            if (Input.GetButtonUp(button))
            {
                Player.Instance.ReleaseAbility(i);
            }
        }
    }

    public JoystickButtonType GetJoystickButtonType(int idx)
    {
        return idx switch
        {
            0 => JoystickButtonType.SOUTH,
            1 => JoystickButtonType.EAST,
            2 => JoystickButtonType.WEST,
            3 => JoystickButtonType.NORTH,
            _ => JoystickButtonType.NORTH
        };
    }
}