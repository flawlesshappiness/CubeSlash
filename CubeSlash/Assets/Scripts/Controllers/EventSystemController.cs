using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemController : MonoBehaviour, IInitializable
{
    public static EventSystemController Instance { get; private set; }

    public EventSystem EventSystem { get { return EventSystem.current; } }

    private GameObject prev_selected_object;

    public void Initialize()
    {
        Instance = this;
    }

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