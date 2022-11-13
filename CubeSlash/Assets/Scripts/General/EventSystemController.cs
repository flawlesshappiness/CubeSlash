using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemController : Singleton
{
    public static EventSystemController Instance { get { return Instance<EventSystemController>(); } }
    public EventSystem EventSystem { get { return EventSystem.current; } }

    private GameObject default_selection;
    private GameObject prev_selection;

    private void Update()
    {
        var current = EventSystem.currentSelectedGameObject;
        if (CanSelect(current))
        {
            prev_selection = current;
        }
        else if (CanSelect(prev_selection))
        {
            Select(prev_selection);
        }
        else if (CanSelect(default_selection))
        {
            Select(default_selection);
        }
    }

    public void SetDefaultSelection(GameObject g)
    {
        default_selection = g;
    }

    private bool CanSelect(GameObject g)
    {
        return g != null && g.activeInHierarchy;
    }

    private void Select(GameObject g)
    {
        EventSystem.SetSelectedGameObject(g);
    }
}