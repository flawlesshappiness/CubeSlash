using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICharmPanel : MonoBehaviour
{
    [SerializeField] private LeftRightMenuItem menu;
    [SerializeField] private TMP_Text tmp_name, tmp_desc, tmp_unlock;
    [SerializeField] private UICharm template_charm;
    [SerializeField] private RectTransform pivot_select, content;

    private View parent_view;

    private List<UICharm> charms = new List<UICharm>();
    private int idx_selected;
    private int charms_to_activate;

    private const int MAX_CHARMS = 1;

    private void Start()
    {
        parent_view = GetComponentInParent<View>();

        // Charms
        InitializeCharms();
        charms_to_activate = MAX_CHARMS;

        // Actions
        menu.onMove += OnMove;
        menu.onSelect += OnSelect;
        menu.onDeselect += OnDeselect;
        menu.onSubmit += OnClick;

        // Reset select
        pivot_select.transform.localScale = Vector3.zero;

        // First run
        var first_run = Save.Game.runs_completed <= 0;
        tmp_unlock.enabled = first_run;
        content.gameObject.SetActive(!first_run);
        menu.interactable = !first_run;

        UpdateDisplayText();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return null;
            if(Save.Game.idx_gamesetup_charm > 0)
            {
                ToggleCharm(Save.Game.idx_gamesetup_charm);
            }
            else {
                SetSelectedCharm(0);
            }
            OnDeselect();
        }
    }

    private void InitializeCharms()
    {
        var db = Database.Load<CharmDatabase>();
        foreach(var info in db.collection)
        {
            var charm = Instantiate(template_charm, template_charm.transform.parent);
            charm.gameObject.SetActive(true);
            charms.Add(charm);
            charm.Initialize(info);
        }

        template_charm.gameObject.SetActive(false);
    }

    public List<Charm> GetActivatedCharms()
    {
        return charms.Where(c => c.Activated).Select(c => c.Info).ToList();
    }

    private void OnClick()
    {
        var charm = charms[idx_selected];

        if (!charm.Info.IsUnlocked())
        {
            TryPurchaseCharm(charm);
        }
        else
        {
            ToggleCharm(idx_selected);
        }
    }

    private void TryPurchaseCharm(UICharm charm)
    {
        var selected_button = EventSystem.current.currentSelectedGameObject;
        parent_view.Interactable = false;

        var view = ViewController.Instance.ShowView<ConfirmPurchaseView>(0, nameof(ConfirmPurchaseView));
        view.SetProduct(charm.Info.shop_product);
        view.Title = charm.Info.charm_name;
        view.onConfirm += () => PurchaseCharm(charm);
        view.onConfirm += Close;
        view.onDecline += Close;

        void Close()
        {
            view.Close(0);
            parent_view.Interactable = true;
            EventSystem.current.SetSelectedGameObject(selected_button);
        }
    }

    private void PurchaseCharm(UICharm charm)
    {
        if (InternalShopController.Instance.TryPurchaseProduct(charm.Info.shop_product))
        {
            charm.Initialize(charm.Info);
            ToggleCharm(idx_selected);
        }
    }

    private void OnMove(int dir)
    {
        var idx_prev = idx_selected;
        SetSelectedCharm(idx_selected + dir);

        if(idx_selected != idx_prev)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_move);
        }
    }

    private void SetSelectedCharm(int i)
    {
        idx_selected = Mathf.Clamp(i, 0, charms.Count - 1);
        var charm = charms[idx_selected];
        var unlocked = charm.Info.IsUnlocked();
        tmp_name.text = charm.Info.charm_name;
        tmp_desc.text = charm.Info.charm_description;
        Lerp.Position(pivot_select, 0.1f, charm.transform.position);

        menu.submittable = CanToggle(charm);

        if (!unlocked)
        {
            tmp_name.text += " (Locked)";
        }
        else if (charm.Activated)
        {
            tmp_name.text += " (Activated)";
        }
    }

    private void ToggleCharm(int i)
    {
        i = Mathf.Clamp(i, 0, charms.Count - 1);
        var charm = charms[i];

        if (charm.Activated)
        {
            charms_to_activate++;
            charm.Deactivate();
            SetSelectedCharm(i);
        }
        else if (charms_to_activate > 0 || MAX_CHARMS == 1)
        {
            if (MAX_CHARMS == 1)
            {
                charms.ForEach(c => c.Deactivate());
                charms_to_activate = MAX_CHARMS;
                Save.Game.idx_gamesetup_charm = i;
            }

            charms_to_activate--;
            charm.Activate();
            SetSelectedCharm(i);
        }
    }

    private bool CanToggle(UICharm charm)
    {
        return !charm.Info.IsUnlocked() || (charms_to_activate > 0 || MAX_CHARMS == 1);
    }

    private void OnSelect()
    {
        Lerp.LocalScale(pivot_select, 0.15f, Vector3.one);
        SetSelectedCharm(idx_selected);
    }

    private void OnDeselect()
    {
        Lerp.LocalScale(pivot_select, 0.15f, Vector3.zero);
        UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
        var charm = charms.FirstOrDefault(c => c.Activated);
        if (charm != null)
        {
            var i = charms.IndexOf(charm);
            SetSelectedCharm(i);
        }
        else
        {
            tmp_name.text = "Charms";
            tmp_desc.text = "None selected";
        }
    }
}