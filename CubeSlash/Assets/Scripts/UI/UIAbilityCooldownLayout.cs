using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAbilityCooldownLayout : MonoBehaviour
{
    [SerializeField] private UIAbilityCooldown prefab_cooldown;

    private Dictionary<PlayerInputController.JoystickButtonType, UIAbilityCooldown> cooldowns = new Dictionary<PlayerInputController.JoystickButtonType, UIAbilityCooldown>();

    private void Start()
    {
        // Create ui elements
        for (int i = 0; i < 9; i++)
        {
            bool is_empty = i % 2 == 0;
            if (is_empty)
            {
                var g = new GameObject("Space");
                g.AddComponent<RectTransform>();
                g.transform.parent = prefab_cooldown.transform.parent;
            }
            else
            {
                var inst = Instantiate(prefab_cooldown, prefab_cooldown.transform.parent).GetComponent<UIAbilityCooldown>();
                var type =
                    i == 1 ? PlayerInputController.JoystickButtonType.NORTH :
                    i == 3 ? PlayerInputController.JoystickButtonType.WEST :
                    i == 5 ? PlayerInputController.JoystickButtonType.EAST :
                    PlayerInputController.JoystickButtonType.SOUTH;
                cooldowns.Add(type, inst);
            }
        }
        prefab_cooldown.gameObject.SetActive(false);

        // Attach abilities
        for (int i = 0; i < Player.Instance.AbilitiesEquipped.Length; i++)
        {
            var dir = PlayerInputController.Instance.GetJoystickButtonType(i);
            var cd = cooldowns[dir];
            var ability = Player.Instance.AbilitiesEquipped[i];
            cd.SetAbility(ability);
        }
    }
}
