using UnityEngine;

public class RadialMenuSoundEffect : MonoBehaviour
{
    public SoundEffectType sfx_submit;

    private void Start()
    {
        var radial = GetComponent<RadialMenu>();
        radial.OnSubmitBegin += _ => SoundController.Instance.Play(sfx_submit);
    }
}