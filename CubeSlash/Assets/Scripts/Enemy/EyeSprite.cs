using UnityEngine;

public class EyeSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private Animator animator;

    public void SetOpen(bool open)
    {
        animator.SetBool("open", open);
    }
}