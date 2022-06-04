using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemController : Singleton
{
    public static EventSystemController Instance { get { return Instance<EventSystemController>(); } }
    public EventSystem EventSystem { get { return EventSystem.current; } }

    private GameObject prev_selected_object;

    private void Update()
    {
        if(EventSystem.currentSelectedGameObject == null)
        {
            if (prev_selected_object != null)
            {
                EventSystem.SetSelectedGameObject(prev_selected_object);
            }
        }
        else
        {
            prev_selected_object = EventSystem.currentSelectedGameObject;
        }
    }
}