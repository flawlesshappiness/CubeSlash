using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Flawliz.GenericOptions
{
    public class SelectableControl : Selectable, IPointerClickHandler, ISubmitHandler, IPointerEnterHandler
    {
        [SerializeField] private CanvasGroup _cvg;

        public bool select_on_hover = true;

        public event System.Action OnSubmitEvent;

        public void SetInteractable(bool interactable)
        {
            this.interactable = interactable;
            _cvg.interactable = interactable;
            _cvg.blocksRaycasts = interactable;
            OnInteractable(interactable);
        }

        public virtual void OnInteractable(bool interactable)
        {
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnSubmit(eventData);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            OnSubmitEvent?.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (select_on_hover)
            {
                Select();
            }
        }
    }
}