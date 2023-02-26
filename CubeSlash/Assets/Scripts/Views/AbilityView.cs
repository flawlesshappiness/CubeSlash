using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class AbilityView : View
{
    [SerializeField] private SelectableMenuItem btn_continue;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private UIInputLayout layout_input;
    [SerializeField] private UIAbilitySlot template_slot_unlocked;
    [SerializeField] private UIAbilitySlotMove slot_move;

    private List<UIAbilitySlot> slots_unlocked = new List<UIAbilitySlot>();
    private List<UIAbilitySlot> slots = new List<UIAbilitySlot>();
    public List<UIAbilityEquipment> equipments = new List<UIAbilityEquipment>();

    private AbilityStatView view_stats;
    private UIAbilitySlot selected_slot;

    public event System.Action OnContinue;

    private void Start()
    {
        // Buttons
        btn_continue.onSubmit += ClickContinue;
        btn_continue.onSelect += OnSelectContinue;

        // Slots
        InitializeSlots();

        // Selection
        EventSystem.current.SetSelectedGameObject(slots_unlocked[0].Button.gameObject);
    }

    private void OnEnable()
    {
        GameController.Instance.PauseLock.AddLock(nameof(AbilityView));

        var input = PlayerInput.Controls.Player;
        input.Menu.performed += PressStart;

        PlayerInput.OnDeviceChanged += UpdateEquipmentOrder;
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(AbilityView));

        var input = PlayerInput.Controls.Player;
        input.Menu.performed -= PressStart;

        PlayerInput.OnDeviceChanged -= UpdateEquipmentOrder;
    }

    #region SLOTS
    private void InitializeSlots()
    {
        // Move
        slot_move.SetAbility(null);

        // Unlocked slots
        template_slot_unlocked.gameObject.SetActive(false);
        foreach(var ability in AbilityController.Instance.GetGainedAbilities())
        {
            InitializeAbilitySlot(ability);
        }

        // Equipment slots
        foreach(var equipment in equipments)
        {
            InitializeEquipmentSlot(equipment);
        }
        UpdateEquipment();
        UpdateEquipmentOrder(PlayerInput.CurrentDevice);
    }

    private void InitializeAbilitySlot(Ability ability)
    {
        var equipped = AbilityController.Instance.IsAbilityEquipped(ability.Info.type);
        var modifier = AbilityController.Instance.IsModifier(ability.Info.type);
        var slot = Instantiate(template_slot_unlocked, template_slot_unlocked.transform.parent);
        slot.gameObject.SetActive(true);
        slot.SetAbility((equipped || modifier) ? null : ability);
        slot.SetWrong(false);

        slots.Add(slot);
        slots_unlocked.Add(slot);

        slot.Button.onSubmit += () => ClickSlot(slot);
        slot.Button.onSelect += () => SelectSlot(slot, false);
        slot.Button.onSelect += () => SelectInventorySlot(slot);
    }

    private void InitializeEquipmentSlot(UIAbilityEquipment equipment)
    {
        var ability = AbilityController.Instance.GetEquippedAbility(equipment.type_button);
        equipment.gameObject.SetActive(true);
        equipment.Initialize(ability);

        slots.Add(equipment.Slot);

        equipment.Slot.Button.onSubmit += () => ClickSlot(equipment.Slot);
        equipment.Slot.Button.onSelect += () => SelectSlot(equipment.Slot, false);
        equipment.Slot.Button.onSelect += () => SelectEquipmentSlot(equipment.Slot);

        foreach (var slot in equipment.ModifierSlots)
        {
            slots.Add(slot);
            slot.Button.onSubmit += () => ClickSlot(slot);
            slot.Button.onSelect += () => SelectSlot(slot, true);
        }
    }

    private void UpdateEquipment()
    {
        foreach (var equipment in equipments)
        {
            var main_filled = equipment.Slot.Ability != null;
            var other_filled = equipment.ModifierSlots.Any(m => m.Ability != null);
            foreach (var modifier in equipment.ModifierSlots)
            {
                var filled = modifier.Ability != null;
                modifier.gameObject.SetActive(filled || (main_filled && IsMovingAbility()));
                modifier.SetWrong(filled && !main_filled);
            }

            equipment.Slot.SetWrong(!main_filled && other_filled);
        }

        var any_wrong = slots.Any(slot => slot.IsWrong);
        var any_filled = equipments.Any(e => e.Slot.Ability != null);
        btn_continue.interactable = !any_wrong && any_filled && !IsMovingAbility();
    }

    private void UpdateEquipmentOrder(PlayerInput.DeviceType type)
    {
        if(type == PlayerInput.DeviceType.KEYBOARD)
        {
            // WASD
            SetEquipmentOrder(PlayerInput.ButtonType.NORTH, 0);
            SetEquipmentOrder(PlayerInput.ButtonType.WEST, 1);
            SetEquipmentOrder(PlayerInput.ButtonType.SOUTH, 2);
            SetEquipmentOrder(PlayerInput.ButtonType.EAST, 3);
        }
        else
        {
            // ABXY
            SetEquipmentOrder(PlayerInput.ButtonType.SOUTH, 0);
            SetEquipmentOrder(PlayerInput.ButtonType.EAST, 1);
            SetEquipmentOrder(PlayerInput.ButtonType.WEST, 2);
            SetEquipmentOrder(PlayerInput.ButtonType.NORTH, 3);
        }

        UIAbilityEquipment GetEquipment(PlayerInput.ButtonType button_type)
        {
            return equipments.FirstOrDefault(e => e.type_button == button_type);
        }

        void SetEquipmentOrder(PlayerInput.ButtonType button_type, int order)
        {
            var e = GetEquipment(button_type);
            e.transform.SetSiblingIndex(order);
        }
    }

    private void ClickSlot(UIAbilitySlot slot)
    {
        var temp = slot.Ability;
        slot.SetAbility(slot_move.Ability);
        slot_move.SetAbility(temp);

        UpdateEquipment();

        SoundController.Instance.Play(SoundEffectType.sfx_ui_submit);
    }

    private void SelectSlot(UIAbilitySlot slot, bool is_modifier)
    {
        slot_move.MoveToSlot(slot);

        if(slot.Ability != null)
        {
            if (is_modifier)
            {
                var parent_slot = slot.GetComponentInParent<UIAbilityEquipment>();
                DisplayModifier(parent_slot.Slot, slot.Ability);
            }
            else
            {
                DisplayAbility(slot.Ability);
            }
        }
        else if(IsMovingAbility())
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_move);
            if (is_modifier)
            {
                var parent_slot = slot.GetComponentInParent<UIAbilityEquipment>();
                DisplayModifier(parent_slot.Slot, slot_move.Ability);
            }
            else
            {
                DisplayAbility(slot_move.Ability);
            }
        }
        else
        {
            DisplayAbility(null);
        }
    }

    private void SelectEquipmentSlot(UIAbilitySlot slot)
    {
        selected_slot = slot;
    }

    private void SelectInventorySlot(UIAbilitySlot slot)
    {
        selected_slot = null;
    }

    private bool IsMovingAbility() => slot_move.Ability != null;

    private void UpdatePlayer()
    {
        // Unequip abilities
        AbilityController.Instance.UnequipAllAbilities();

        // Equip abilities
        foreach(var equipment in equipments)
        {
            if (equipment.Slot.Ability == null) continue;

            var ability = equipment.Slot.Ability;
            AbilityController.Instance.EquipAbility(ability, equipment.type_button);

            for (int i = 0; i < equipment.ModifierSlots.Count; i++)
            {
                var slot = equipment.ModifierSlots[i];
                if (slot.Ability == null) continue;
                AbilityController.Instance.AddModifier(ability.Info.type, slot.Ability.Info.type);
            }
        }
    }
    #endregion
    #region BUTTONS
    private void ClickContinue()
    {
        UpdatePlayer();
        Close(0);
        OnContinue?.Invoke();
    }

    private void OnSelectContinue()
    {
        selected_slot = null;
    }
    #endregion
    #region DISPLAY
    private void DisplayNoAbility()
    {
        var any_wrong = slots.Any(slot => slot.IsWrong);
        var any_filled = equipments.Any(e => e.Slot.Ability != null);

        var db_color = ColorDatabase.Load();

        if (any_wrong)
        {
            tmp_desc.text = "Some slots are " + "invalid".Color(db_color.text_wrong.GetColor()) + ".";
        }
        else if (!any_filled)
        {
            tmp_desc.text = "Equip at least 1 ability to continue.";
        }
        else
        {
            tmp_desc.text = "";
        }
        DisplayInputNoAbility();
    }

    private void DisplayAbility(Ability a)
    {
        if(a != null)
        {
            tmp_desc.text = a.Info.desc_ability;
            DisplayInputAbility();
        }
        else
        {
            DisplayNoAbility();
        }
    }
    private void DisplayModifier(UIAbilitySlot equipment_slot, Ability modifier_ability)
    {
        if(modifier_ability == null)
        {
            DisplayNoAbility();
        }
        else if(equipment_slot.Ability != null)
        {
            var modifier = equipment_slot.Ability.Info.modifiers.GetModifier(modifier_ability.Info.type);
            var info = UpgradeController.Instance.GetUpgradeInfo(modifier.id);
            string s = "";
            for (int i = 0; i < info.upgrade.stats.Count; i++)
            {
                var stat = info.upgrade.stats[i];
                s += i == 0 ? "" : "\n";
                s += stat.GetDisplayString();
            }
            tmp_desc.text = s;
            DisplayInputAbility();
        }
    }
    #endregion
    #region INPUT
    private void DisplayInputAbility()
    {
        ClearInputDisplay();
        layout_input.AddInput(PlayerInput.UIButtonType.SOUTH, "Grab/Place");
    }

    private void DisplayInputNoAbility()
    {
        ClearInputDisplay();
    }

    private void ClearInputDisplay()
    {
        layout_input.Clear();
    }

    private void PressStart(InputAction.CallbackContext context)
    {
        if (btn_continue.interactable)
        {
            EventSystem.current.SetSelectedGameObject(btn_continue.gameObject);
        }
    }

    private void OnNorthPressed(InputAction.CallbackContext context)
    {
        if(selected_slot != null && selected_slot.Ability != null)
        {
            Interactable = false;
            SelectableMenuItem.RemoveSelection();

            UpdatePlayer();
            PlayerValueController.Instance.UpdateValues();

            view_stats = ViewController.Instance.ShowView<AbilityStatView>(0, "AbilityStatView");
            view_stats.SetAbility(selected_slot.Ability);
        }
    }

    private void OnNorthReleased(InputAction.CallbackContext context)
    {
        if(view_stats != null)
        {
            view_stats.Close(0);
            view_stats = null;

            Interactable = true;
            EventSystem.current.SetSelectedGameObject(selected_slot.Button.gameObject);
        }
    }
    #endregion
}