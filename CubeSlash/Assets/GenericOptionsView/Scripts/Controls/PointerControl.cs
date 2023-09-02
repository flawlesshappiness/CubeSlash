using UnityEngine;
using UnityEngine.EventSystems;

namespace Flawliz.GenericOptions
{
    public class PointerControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerMoveHandler
    {
        public event System.Action<PointerEventData> OnPointerDownEvent, OnPointerUpEvent, OnPointerEnterEvent, OnPointerExitEvent, OnPointerClickEvent, OnPointerMoveEvent;

        public void OnPointerClick(PointerEventData eventData) => OnPointerClickEvent?.Invoke(eventData);

        public void OnPointerDown(PointerEventData eventData) => OnPointerDownEvent?.Invoke(eventData);

        public void OnPointerEnter(PointerEventData eventData) => OnPointerEnterEvent?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData) => OnPointerExitEvent?.Invoke(eventData);

        public void OnPointerMove(PointerEventData eventData) => OnPointerMoveEvent?.Invoke(eventData);

        public void OnPointerUp(PointerEventData eventData) => OnPointerUpEvent?.Invoke(eventData);
    }
}