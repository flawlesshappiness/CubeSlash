using UnityEngine;

public class TelegraphSprite : MonoBehaviourExtended
{
    public SpriteRenderer SpriteRenderer { get { return GetComponentOnce<SpriteRenderer>(ComponentSearchType.CHILDREN); } }
}