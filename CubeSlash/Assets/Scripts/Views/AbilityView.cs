using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;

public class AbilityView : View
{
    [SerializeField] private ButtonExtended btn_continue;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private UIInputLayout layout_input;
    [SerializeField] private UIAbilitySlot template_slot_unlocked;
    [SerializeField] private UIAbilitySlotMove slot_move;

    [Header("AUDIO")]
    [SerializeField] private FMODEventReference event_move_slot;
    [SerializeField] private FMODEventReference event_insert_slot;

    private List<UIAbilitySlot> slots_unlocked = new List<UIAbilitySlot>();
    private List<UIAbilitySlot> slots = new List<UIAbilitySlot>();
    public List<UIAbilityEquipment> equipments = new List<UIAbilityEquipment>();

    private AbilityStatView view_stats;
    private UIAbilitySlot selected_slot;

    public event System.Action OnContinue;

    private void Start()
    {
        // Buttons
        btn_continue.onClick.AddListener(ClickContinue);
        btn_continue.OnSelectedChanged += OnSelectContinue;

        // Cards
        InitializeAbilitySlots();

        // UI
        EventSystemController.Instance.SetDefaultSelection(slots[0].Button.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(slots[0].Button.gameObject);
    }

    private void OnEnable()
    {
        GameController.Instance.PauseLock.AddLock(nameof(AbilityView));

        var input = PlayerInput.Controls.Player;
        input.Menu.performed += PressStart;
        input.North.started += OnNorthPressed;
        input.North.canceled += OnNorthReleased;
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(AbilityView));

        var input = PlayerInput.Controls.Player;
        input.Menu.performed -= PressStart;
        input.North.started -= OnNorthPressed;
        input.North.canceled -= OnNorthReleased;
    }

    #region SLOTS
    private void InitializeAbilitySlots()
    {
        // Move
        slot_move.SetAbility(null);

        // Unlocked slots
        template_slot_unlocked.gameObject.SetActive(false);
        foreach(var ability in AbilityController.Instance.GetUnlockedAbilities())
        {
            var slot = Instantiate(template_slot_unlocked, template_slot_unlocked.transform.parent);
            slot.gameObject.SetActive(true);
            slot.SetAbility(ability.Equipped ? null : ability);
            slot.SetWrong(false);

            slots.Add(slot);
            slots_unlocked.Add(slot);

            slot.Button.onClick.AddListener(() => ClickSlot(slot));
            slot.Button.OnHoverChanged += hovered => HoverSlot(slot, hovered);
            slot.Button.OnSelectedChanged += selected => SelectSlot(slot, selected);
            slot.Button.OnSelectedChanged += selected => SelectInventorySlot(slot, selected);
        }

        // Equipment slots
        foreach(var equipment in equipments)
        {
            InitializeEquipmentSlot(equipment);
        }
        UpdateEquipment();

        EventSystemController.Instance.EventSystem.SetSelectedGameObject(equipments[0].Slot.Button.gameObject);
    }

    private void InitializeEquipmentSlot(UIAbilityEquipment equipment)
    {
        var ability = AbilityController.Instance.GetEquippedAbility(equipment.type_button);
        equipment.gameObject.SetActive(true);
        equipment.Initialize(ability);

        slots.Add(equipment.Slot);

        equipment.Slot.Button.onClick.AddListener(() => ClickSlot(equipment.Slot));
        equipment.Slot.Button.OnHoverChanged += hovered => HoverSlot(equipment.Slot, hovered);
        equipment.Slot.Button.OnSelectedChanged += selected => SelectSlot(equipment.Slot, selected);
        equipment.Slot.Button.OnSelectedChanged += selected => SelectEquipmentSlot(equipment.Slot, selected);

        foreach (var slot in equipment.ModifierSlots)
        {
            slots.Add(slot);
            slot.Button.onClick.AddListener(() => ClickSlot(slot));
            slot.Button.OnSelectedChanged += selected => SelectSlot(slot, selected);
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

    private void ClickSlot(UIAbilitySlot slot)
    {
        var temp = slot.Ability;
        slot.SetAbility(slot_move.Ability);
        slot_move.SetAbility(temp);

        UpdateEquipment();

        event_insert_slot.Play();
    }

    private void HoverSlot(UIAbilitySlot slot, bool hovered)
    {
        if (!hovered) return;
        slot.Button.Select();
    }

    private void SelectSlot(UIAbilitySlot slot, bool selected)
    {
        if (!selected) return;
        slot_move.MoveToSlot(slot);

        if(slot.Ability != null)
        {
            DisplayAbility(slot.Ability);
        }
        else if(IsMovingAbility())
        {
            event_move_slot.Play();
            DisplayAbility(slot_move.Ability);
        }
        else
        {
            DisplayAbility(null);
        }
    }

    private void SelectEquipmentSlot(UIAbilitySlot slot, bool selected)
    {
        if (selected)
        {
            selected_slot = slot;
        }
    }

    private void SelectInventorySlot(UIAbilitySlot slot, bool selected)
    {
        if (selected)
        {
            selected_slot = null;
        }
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

            AbilityController.Instance.EquipAbility(equipment.Slot.Ability, equipment.type_button);

            for (int i = 0; i < equipment.ModifierSlots.Count; i++)
            {
                var slot = equipment.ModifierSlots[i];
                if (slot.Ability == null) continue;
                equipment.Slot.Ability.SetModifier(slot.Ability, i);
            }
        }
    }
    #endregion
    #region BUTTONS
    private void ClickContinue()
    {
        FMODButtonEvent.PreviousSelected = null;
        UpdatePlayer();
        Close(0);
        OnContinue?.Invoke();
    }

    private void OnSelectContinue(bool selected)
    {
        if (selected)
        {
            selected_slot = null;
        }
    }
    #endregion
    #region DISPLAY
    private void DisplayAbility(Ability a)
    {
        if(a != null)
        {
            string s = a.Info.name_ability;
            s += "\n" + a.Info.desc_ability;
            tmp_desc.text = s;
            DisplayInputAbility();
        }
        else
        {
            var any_wrong = slots.Any(slot => slot.IsWrong);
            var any_filled = equipments.Any(e => e.Slot.Ability != null);

            if (any_wrong)
            {
                tmp_desc.text = "Some slots are " + "invalid".Color(ColorPalette.Main.Get(ColorPalette.Type.WRONG)) + ".";
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
    }
    #endregion
    #region INPUT
    private void DisplayInputAbility()
    {
        ClearInputDisplay();
        layout_input.AddInput(PlayerInput.UIButtonType.SOUTH, "Grab/Place");
        layout_input.AddInput(PlayerInput.UIButtonType.NORTH, "Move");
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
            EventSystemController.Instance.EventSystem.SetSelectedGameObject(btn_continue.gameObject);
        }
    }

    private void OnNorthPressed(InputAction.CallbackContext context)
    {
        if(selected_slot != null && selected_slot.Ability != null)
        {
            UpdatePlayer();
            Player.Instance.ReapplyUpgrades();
            Player.Instance.ReapplyAbilities();

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
        }
    }
    #endregion
}