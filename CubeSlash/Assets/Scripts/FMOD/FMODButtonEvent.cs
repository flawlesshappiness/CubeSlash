using UnityEngine;

[RequireComponent(typeof(ButtonExtended))]
public class FMODButtonEvent : MonoBehaviour
{
    [SerializeField] private FMODEventReference eventOnClick;
    [SerializeField] private FMODEventReference eventOnSelect;

    private ButtonExtended btn;

    private void OnEnable()
    {
        btn = GetComponent<ButtonExtended>();
        btn.onClick.AddListener(() => eventOnClick.Play());
        btn.OnSelectedChanged += s =>
        {
            if (s)
            {
                eventOnSelect.Play();
            }
        };
    }
}