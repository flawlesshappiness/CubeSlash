using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BodySelectView : View
{
    [SerializeField] private Image img_ability;
    [SerializeField] private TMP_Text tmp_ability;
    [SerializeField] private GameObject template_health_point, template_armor_point, template_spd, template_acc;
    [SerializeField] private Transform pivot_arrow_left, pivot_arrow_right, pivot_ability, pivot_stats;
    [SerializeField] private UIInputLayout controls_confirm, controls_back;
    [SerializeField] private PlayerBodySettingsDatabase db_player_body_settings;
    [SerializeField] private CanvasGroup cvg_controls;
    [SerializeField] private FMODEventReference sfx_confirm_charge;
    [SerializeField] private FMODEventReference sfx_confirm;
    [SerializeField] private FMODEventReference sfx_back;
    [SerializeField] private FMODEventReference sfx_change_body;

    private List<GameObject> health_points = new List<GameObject>();
    private List<GameObject> spd_points = new List<GameObject>();
    private List<GameObject> acc_points = new List<GameObject>();

    private int idx_body_selected = -1;
    private bool transitioning;

    private Coroutine cr_confirm;

    private void Start()
    {
        template_health_point.SetActive(false);
        template_armor_point.SetActive(false);
        template_spd.SetActive(false);
        template_acc.SetActive(false);

        controls_confirm.AddInput(PlayerInput.UIButtonType.SOUTH, "Hold to confirm");
        controls_back.AddInput(PlayerInput.UIButtonType.EAST, "Back");

        Player.Instance.gameObject.SetActive(true);
        Player.Instance.MovementLock.AddLock(nameof(BodySelectView));
        Player.Instance.AbilityLock.AddLock(nameof(BodySelectView));

        ResetCamera();

        SetBody(0);

        StartCoroutine(AppearCr());
    }

    IEnumerator AppearCr()
    {
        cvg_controls.alpha = 0;
        pivot_stats.localScale = Vector3.zero;
        pivot_ability.localScale = Vector3.zero;
        AnimatePivotScaleShow(pivot_ability, true, 0);
        AnimatePivotScaleShow(pivot_stats, true, 0.2f);
        yield return new WaitForSeconds(0.5f);
        yield return LerpEnumerator.Value(1f, f => cvg_controls.alpha = Mathf.Lerp(0f, 1f, f));
    }

    private IEnumerator HideCr()
    {
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;

        var cr1 = AnimatePivotScaleShow(pivot_ability, false, 0);
        var cr2 = AnimatePivotScaleShow(pivot_stats, false, 0.2f);
        var alpha_start = cvg_controls.alpha;
        yield return LerpEnumerator.Value(0.5f, f => cvg_controls.alpha = Mathf.Lerp(alpha_start, 0f, f));
        yield return cr1;
        yield return cr2;
    }

    private IEnumerator TransitionToMainMenu()
    {
        transitioning = true;
        yield return HideCr();
        ViewController.Instance.ShowView<StartView>(0.5f);
        Close(0);
    }

    private IEnumerator TransitionToGame()
    {
        transitioning = true;
        yield return HideCr();
        GameController.Instance.StartGame();
        Close(0);
    }

    private void OnEnable()
    {
        PlayerInput.Controls.UI.Navigate.started += Navigate;

        PlayerInput.Controls.Player.South.started += ConfirmHeld;
        PlayerInput.Controls.Player.South.canceled += ConfirmReleased;

        PlayerInput.Controls.Player.East.started += PressBack;

        GameStateController.Instance.SetGameState(GameStateType.MENU);
    }

    private void OnDisable()
    {
        PlayerInput.Controls.UI.Navigate.started -= Navigate;

        PlayerInput.Controls.Player.South.started -= ConfirmHeld;
        PlayerInput.Controls.Player.South.canceled -= ConfirmReleased;

        PlayerInput.Controls.Player.East.started -= PressBack;

        Player.Instance.MovementLock.RemoveLock(nameof(BodySelectView));
        Player.Instance.AbilityLock.RemoveLock(nameof(BodySelectView));

        sfx_confirm_charge.Stop();
    }

    private void Navigate(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        if(value.x.Abs() > 0.1f)
        {
            var right = value.x > 0.1f;
            if (right)
            {
                NavigateRight();
            }
            else
            {
                NavigateLeft();
            }

            sfx_change_body.Play();
        }
    }

    private void NavigateRight()
    {
        if(SetBody(idx_body_selected + 1))
        {
            AnimateArrowRight();
        }
    }

    private void NavigateLeft()
    {
        if(SetBody(idx_body_selected - 1))
        {
            AnimateArrowLeft();
        }
    }

    private bool SetBody(int idx)
    {
        var max = db_player_body_settings.settings.Count - 1;
        idx = Mathf.Clamp(idx, 0, max);
        if (idx == idx_body_selected) return false;
        idx_body_selected = idx;
        var settings = db_player_body_settings.settings[idx_body_selected];
        SetBody(settings);
        return true;
    }

    private void SetBody(PlayerBodySettings settings)
    {
        // Stats
        ClearHealthPoints();
        ClearSpeedPoints();
        ClearAccelerationPoints();

        AddHealthPoints(settings.health);
        AddArmorPoints(settings.armor);

        var velocity_points = (int)(settings.linear_velocity / 0.5f);
        AddSpeedPoints(velocity_points);

        var acc_points = (int)(settings.linear_acceleration / 2f);
        AddAccelerationPoints(acc_points);

        // Body
        Player.Instance.SetPlayerBody(settings);

        // Ability
        var ability = AbilityController.Instance.GetAbility(settings.ability_type);
        SetAbility(ability);
    }

    private void SetAbility(Ability ability)
    {
        img_ability.sprite = ability.Info.sprite_icon;
        tmp_ability.text = ability.Info.desc_ability;
    }

    private void ClearHealthPoints()
    {
        health_points.ForEach(point => Destroy(point));
        health_points.Clear();
    }

    private void AddHealthPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_health_point, template_health_point.transform.parent);
            inst.gameObject.SetActive(true);
            health_points.Add(inst);
        }
    }

    private void AddArmorPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_armor_point, template_armor_point.transform.parent);
            inst.gameObject.SetActive(true);
            health_points.Add(inst);
        }
    }

    private void ClearSpeedPoints()
    {
        spd_points.ForEach(point => Destroy(point));
        spd_points.Clear();
    }

    private void ClearAccelerationPoints()
    {
        acc_points.ForEach(point => Destroy(point));
        acc_points.Clear();
    }

    private void AddSpeedPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_spd, template_spd.transform.parent);
            inst.gameObject.SetActive(true);
            spd_points.Add(inst);
        }
    }
    private void AddAccelerationPoints(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Instantiate(template_acc, template_acc.transform.parent);
            inst.gameObject.SetActive(true);
            acc_points.Add(inst);
        }
    }

    private void ConfirmHeld(InputAction.CallbackContext context)
    {
        if (cr_confirm != null) return;
        cr_confirm = StartCoroutine(Cr());
        IEnumerator Cr()
        {
            sfx_confirm_charge.Play();

            CameraController.Instance.AnimateSize(2f, 5f, EasingCurves.EaseOutQuad);
            yield return new WaitForSeconds(2f);

            sfx_confirm_charge.Stop();
            sfx_confirm.Play();

            StartCoroutine(TransitionToGame());
        }
    }

    private void ConfirmReleased(InputAction.CallbackContext context)
    {
        if (cr_confirm == null) return;
        StopCoroutine(cr_confirm);
        cr_confirm = null;
        ResetCamera();

        sfx_confirm_charge.Stop();
    }

    private void PressBack(InputAction.CallbackContext context)
    {
        if (transitioning) return;

        Player.Instance.gameObject.SetActive(false);

        ConfirmReleased(context);

        sfx_back.Play();

        StartCoroutine(TransitionToMainMenu());
    }

    private void ResetCamera()
    {
        CameraController.Instance.AnimateSize(1f, 6f, EasingCurves.EaseInOutQuad);
    }

    private void AnimateArrowLeft()
    {
        var start = new Vector3(-25f, 0);
        var end = Vector3.zero;
        Lerp.LocalPosition(pivot_arrow_left, 0.5f, start, end)
            .Curve(EasingCurves.EaseOutBack);
    }

    private void AnimateArrowRight()
    {
        var start = new Vector3(25f, 0);
        var end = Vector3.zero;
        Lerp.LocalPosition(pivot_arrow_right, 0.5f, start, end)
            .Curve(EasingCurves.EaseOutBack);
    }

    private Coroutine AnimatePivotScaleShow(Transform pivot, bool show, float delay)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            var start = pivot.transform.localScale;
            var end = show ? Vector3.one : Vector3.zero;
            var curve = show ? EasingCurves.EaseOutBack : EasingCurves.EaseInBack;
            var duration = show ? 1f : 0.5f;
            yield return LerpEnumerator.LocalScale(pivot, duration, start, end).Curve(curve);
        }
    }
}