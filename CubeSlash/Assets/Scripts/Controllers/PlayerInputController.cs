using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInputController : MonoBehaviour, IInitializable
{
    public static PlayerInputController Instance;
    public enum JoystickType { XBOX, PLAYSTATION }
    public enum JoystickButtonType { SOUTH, EAST, WEST, NORTH }
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
        return (JoystickButtonType)idx;
    }

    public bool GetJoystickButtonDown(JoystickButtonType type)
    {
        var button = buttons_ability[(int)type];
        return Input.GetButtonDown(button);
    }

    public bool GetAnyJoystickButtonDown()
    {
        var types = System.Enum.GetValues(typeof(JoystickButtonType)).Cast<JoystickButtonType>().ToArray();
        return types.Any(type => GetJoystickButtonDown(type));
    }
}