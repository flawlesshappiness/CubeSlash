using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInputButton : MonoBehaviour
{
    [SerializeField] private Image img_outline;
    [SerializeField] private Image img_fill;
    [SerializeField] private Image img_symbol;
    [SerializeField] private TMP_Text tmp_text;
    
    [System.Serializable]
    public class ButtonMap
    {
        public Sprite sprite;
        public Sprite sprite_symbol;
        public Color symbol = Color.white;
        public Color outline = Color.white;
        public Color fill = Color.white;
        public string text;
    }

    [System.Serializable]
    public class JoystickMap
    {
        public ButtonMap north;
        public ButtonMap east;
        public ButtonMap south;
        public ButtonMap west;
    }

    [System.Serializable]
    public class MouseMap
    {
        public ButtonMap lmb;
        public ButtonMap rmb;
        public ButtonMap mmb;
    }

    public JoystickMap map_xbox;
    public JoystickMap map_playstation;
    public MouseMap map_mouse;
    public ButtonMap map_keyboard;

    public void SetJoystickButton(PlayerInputController.JoystickType joystick, PlayerInputController.JoystickButtonType button)
    {
        var map_joystick = joystick == PlayerInputController.JoystickType.XBOX ? map_xbox : map_playstation;
        var map_button =
            button == PlayerInputController.JoystickButtonType.NORTH ? map_joystick.north :
            button == PlayerInputController.JoystickButtonType.EAST ? map_joystick.east :
            button == PlayerInputController.JoystickButtonType.SOUTH ? map_joystick.south :
            map_joystick.west;
        SetButtonMap(map_button);
    }

    public void SetKeyboardButton(char c)
    {
        var map = map_keyboard;
        map.text = c.ToString();
        SetButtonMap(map);
    }

    public void SetMouseButton(PlayerInputController.MouseButtonType type)
    {
        var map = map_mouse;
        var map_button =
            type == PlayerInputController.MouseButtonType.LMB ? map.lmb :
            type == PlayerInputController.MouseButtonType.RMB ? map.rmb :
            map.mmb;
        SetButtonMap(map_button);

    }

    private void SetButtonMap(ButtonMap map)
    {
        img_outline.sprite = map.sprite;
        img_fill.sprite = map.sprite;
        img_symbol.sprite = map.sprite_symbol;
        img_symbol.gameObject.SetActive(map.sprite_symbol != null);
        img_outline.color = map.outline;
        img_symbol.color = map.symbol;
        img_fill.color = map.fill;
        tmp_text.text = map.text;
        tmp_text.color = map.symbol;
    }
}
