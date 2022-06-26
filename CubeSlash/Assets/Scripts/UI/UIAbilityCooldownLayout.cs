using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAbilityCooldownLayout : MonoBehaviour
{
    [SerializeField] private UIAbilityCooldown prefab_cooldown;

    private Dictionary<PlayerInput.ButtonType, UIAbilityCooldown> cooldowns = new Dictionary<PlayerInput.ButtonType, UIAbilityCooldown>();

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
                    i == 1 ? PlayerInput.ButtonType.NORTH :
                    i == 3 ? PlayerInput.ButtonType.WEST :
                    i == 5 ? PlayerInput.ButtonType.EAST :
                    PlayerInput.ButtonType.SOUTH;
                cooldowns.Add(type, inst);
            }
        }
        prefab_cooldown.gameObject.SetActive(false);

        // Attach abilities
        UpdateAbilities();
    }

    private void OnEnable()
    {
        GameController.Instance.OnResume += UpdateAbilities;
    }

    private void OnDisable()
    {
        GameController.Instance.OnResume -= UpdateAbilities;
    }

    private void UpdateAbilities()
    {
        UpdateAbility(PlayerInput.ButtonType.NORTH);
        UpdateAbility(PlayerInput.ButtonType.EAST);
        UpdateAbility(PlayerInput.ButtonType.SOUTH);
        UpdateAbility(PlayerInput.ButtonType.WEST);
    }

    private  void UpdateAbility(PlayerInput.ButtonType type)
    {
        var cd = cooldowns[type];
        var a = Player.Instance.GetEquippedAbility(type);
        cd.SetAbility(a);
    }
}
