using UnityEngine;

public class EyeGroup : MonoBehaviourExtended
{
    public EyeSprite[] Eyes { get { return GetComponentsOnce<EyeSprite>(ComponentSearchType.CHILDREN); } }

    public void SetEyesOpen(bool open)
    {
        foreach(var eye in Eyes)
        {
            eye.SetOpen(open);
        }
    }
}