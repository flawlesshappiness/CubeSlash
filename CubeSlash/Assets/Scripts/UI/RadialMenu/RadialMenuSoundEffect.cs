using UnityEngine;

public class RadialMenuSoundEffect : MonoBehaviour
{
    public SoundEffectType sfx_submit;
    public SoundEffectType sfx_select;

    private FMODEventInstance inst_select_cur;

    private void Start()
    {
        var radial = GetComponent<RadialMenu>();
        radial.OnSubmitBegin += Submit;
        radial.OnSelect += Select;
    }

    private void Select(RadialMenuElement element)
    {
        if(element != null)
        {
            inst_select_cur = SoundController.Instance.Play(sfx_select);
        }
    }

    private void Submit(RadialMenuElement element)
    {
        if(element != null)
        {
            if(inst_select_cur != null)
            {
                inst_select_cur.Stop();
            }

            SoundController.Instance.Play(sfx_submit);
        }
    }
}