using UnityEngine;

[RequireComponent(typeof(ButtonExtended))]
public class FMODButtonEvent : MonoBehaviour
{
    [SerializeField] private FMODEventReference eventOnClick;
    [SerializeField] private FMODEventReference eventOnSelect;

    private float timestamp_next;

    private ButtonExtended btn;

    private void OnEnable()
    {
        btn = GetComponent<ButtonExtended>();
        btn.onClick.AddListener(() => Play(eventOnClick));
        btn.OnSelectedChanged += s =>
        {
            if (s)
            {
                Play(eventOnSelect);
            }
        };
    }

    private void Play(FMODEventReference reference)
    {
        if (string.IsNullOrEmpty(reference.reference.Path))
        {

        }
        else
        {
            reference.Play();
        }
    }
}