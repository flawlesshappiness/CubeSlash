using FMOD;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BodypartEditController : Singleton
{
    public static BodypartEditController Instance { get { return Instance<BodypartEditController>(); } }

    public PlayerBody Body { get { return Player.Instance.PlayerBody; } }

    private Bodypart _selected_part;

    private const float MOVE_SPEED = 0.5f;
    private const float DEAD_ZONE = 0.25f;

    public Bodypart CreatePart(BodypartInfo info)
    {
        var part = Body.CreateBodypart(info.type);

        var data = new BodypartSavaData
        {
            type = info.type,
            position = part.Position
        };

        part.SaveData = data;
        part.CounterPart.SaveData = data;
        Save.PlayerBody.bodyparts.Add(data);
        return part;
    }

    public void RemovePart(Bodypart part)
    {
        Body.RemoveBodypart(part);
        Save.PlayerBody.bodyparts.Remove(part.SaveData);
    }

    public void BeginMovingPart(Bodypart bodypart, System.Action OnSubmit, System.Action OnCancel)
    {
        _selected_part = bodypart;
        _selected_part.SetSelected(true);
        _selected_part.CounterPart.SetSelected(true);

        var start_position = _selected_part.Position;
        var cr = StartCoroutine(UpdateCr());

        PlayerInput.Controls.UI.Submit.started += Submit;
        PlayerInput.Controls.UI.Cancel.started += Cancel;

        IEnumerator UpdateCr()
        {
            while (true)
            {
                var dir = PlayerInput.Controls.UI.Navigate.ReadValue<Vector2>();
                if (dir.y.Abs() < DEAD_ZONE)
                {
                    yield return null;
                    continue;
                }

                var sign = Mathf.Sign(dir.y);
                var t_max = 1f - DEAD_ZONE;
                var t = Mathf.Abs(dir.y) - DEAD_ZONE;
                var t_y = t / t_max * sign;

                var y = _selected_part.Position;
                var delta = t_y * MOVE_SPEED * Time.unscaledDeltaTime;
                var y_next = Mathf.Clamp01(y + delta);
                _selected_part.SetPosition(y_next);

                yield return null;
            }
        }

        void Submit(InputAction.CallbackContext context)
        {
            End();
            OnSubmit?.Invoke();
        }

        void Cancel(InputAction.CallbackContext context)
        {
            End();
            _selected_part.SetPosition(start_position);
            OnCancel?.Invoke();
        }

        void End()
        {
            _selected_part.SetSelected(false);
            _selected_part.CounterPart.SetSelected(false);

            PlayerInput.Controls.UI.Submit.started -= Submit;
            PlayerInput.Controls.UI.Cancel.started -= Cancel;
            StopCoroutine(cr);

            var part = _selected_part;
            var data = Save.PlayerBody;
        }
    }

    public void BeginSelectingPart(Bodypart selected, System.Action<Bodypart> OnSelect, System.Action OnCancel)
    {
        var parts = Body.Bodyparts
            .Where(part => part.BoneSide == Bodypart.Side.Left)
            .Where(part => !part.Info.is_ability_part)
            .OrderBy(part => part.Position)
            .ToList();
        
        if(parts.Count == 0)
        {
            OnCancel?.Invoke();
            return;
        }

        var i_selected = selected != null ? parts.IndexOf(selected) : 0;
        selected = parts[i_selected];
        SelectPart(selected);
        var cr = StartCoroutine(UpdateCr());

        PlayerInput.Controls.UI.Submit.started += Submit;
        PlayerInput.Controls.UI.Cancel.started += Cancel;

        IEnumerator UpdateCr()
        {
            while (true)
            {
                var dir = PlayerInput.Controls.UI.Navigate.ReadValue<Vector2>();
                if(dir.y.Abs() > DEAD_ZONE)
                {
                    var i = (int)Mathf.Sign(dir.y);
                    i_selected = Mathf.Clamp(i_selected + i, 0, parts.Count - 1);
                    SelectPart(parts[i_selected]);
                    yield return new WaitForSecondsRealtime(0.2f);
                }
                else
                {
                    yield return null;
                }
            }
        }

        void SelectPart(Bodypart part)
        {
            if(selected != null)
            {
                selected.SetHover(false);
                selected.CounterPart.SetHover(false);
            }

            selected = part;

            if(selected != null)
            {
                selected.SetHover(true);
                selected.CounterPart.SetHover(true);
            }
        }

        void Submit(InputAction.CallbackContext context)
        {
            End();
            OnSelect?.Invoke(selected);
        }

        void Cancel(InputAction.CallbackContext context)
        {
            End();
            OnCancel?.Invoke();
        }

        void End()
        {
            selected.SetHover(false);
            selected.CounterPart.SetHover(false);

            PlayerInput.Controls.UI.Submit.started -= Submit;
            PlayerInput.Controls.UI.Cancel.started -= Cancel;
            StopCoroutine(cr);
        }
    }

    public void BeginRemovingPart(Bodypart part, System.Action<Bodypart> OnRemove)
    {
        var idx = Body.Bodyparts.IndexOf(part);
        RemovePart(part);

        var parts = Body.Bodyparts;
        part = parts.Count > 0 ? parts[Mathf.Clamp(idx, 0, parts.Count - 1)] : null;
        OnRemove(part);
    }
}