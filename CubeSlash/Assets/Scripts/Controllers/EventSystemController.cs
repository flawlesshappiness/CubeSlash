using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemController : Singleton
{
    public static EventSystemController Instance { get { return Instance<EventSystemController>(); } }
    public EventSystem EventSystem { get { return EventSystem.current; } }

    private GameObject default_selected_object;

    private void Update()
    {
        if(EventSystem.currentSelectedGameObject == null || !EventSystem.currentSelectedGameObject.activeInHierarchy)
        {
            if (default_selected_object != null)
            {
                EventSystem.SetSelectedGameObject(default_selected_object);
            }
        }
    }

    public void SetDefaultSelection(GameObject g)
    {
        default_selected_object = g;
    }
}