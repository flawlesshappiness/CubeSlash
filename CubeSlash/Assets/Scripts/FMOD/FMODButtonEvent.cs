using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ButtonExtended))]
public class FMODButtonEvent : MonoBehaviour
{
    [SerializeField] private FMODEventReference eventOnClick;
    [SerializeField] private FMODEventReference eventOnSelect;

    public static Button PreviousSelected { get; set; }

    private ButtonExtended btn;

    private void OnEnable()
    {
        btn = GetComponent<ButtonExtended>();
        btn.onClick.AddListener(() => Play(eventOnClick));
        btn.OnSelectedChanged += s =>
        {
            if (s)
            {
                if(PreviousSelected != null)
                {
                    Play(eventOnSelect);
                }

                PreviousSelected = btn;
            }
        };
    }

    private void Play(FMODEventReference reference)
    {
        if (string.IsNullOrEmpty(reference.Path))
        {

        }
        else
        {
            reference.Play();
        }
    }
}