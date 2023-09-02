using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Flawliz.GenericOptions
{
    public class ButtonControl : SelectableControl
    {
        [SerializeField] private Image _img_normal, _img_selected, _img_submitted, _img_disabled;

        private State _state;

        private enum State
        {
            Normal, Submitted, Selected, Disabled
        }

        private void SetState(State state)
        {
            _state = state;
            _img_normal.enabled = state == State.Normal;
            _img_selected.enabled = state == State.Selected;
            _img_submitted.enabled = state == State.Submitted;
            _img_disabled.enabled = state == State.Disabled;
        }

        public virtual void Submit()
        {

        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            Submit();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            SetState(State.Submitted);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            SetState(State.Normal);
        }

        public override void OnInteractable(bool interatable)
        {
            base.OnInteractable(interatable);
            SetState(interactable ? State.Normal : State.Disabled);
        }
    }
}