using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossShooterBody : BossBody
{
    [Header("SHOOTER")]
    [SerializeField] private EyeGroup eyes_single;
    [SerializeField] private EyeGroup eyes_arc;
    [SerializeField] private EyeGroup eyes_circle;
    [SerializeField] private Transform t_front;

    private EyeGroup eyes_current;
    private List<Transform> circle_transforms;

    private void Start()
    {
        SetEyesSingle();
    }

    public void SetEyesSingle()
    {
        this.StartCoroutineWithID(EyesCr(eyes_single), "eyes_" + GetInstanceID());
    }

    public void SetEyesArc()
    {
        this.StartCoroutineWithID(EyesCr(eyes_arc), "eyes_" + GetInstanceID());
    }

    public void SetEyesCircle()
    {
        this.StartCoroutineWithID(EyesCr(eyes_circle), "eyes_" + GetInstanceID());
    }

    IEnumerator EyesCr(EyeGroup eyes)
    {
        if(eyes_current != null)
        {
            eyes_current.SetEyesOpen(false);
            yield return new WaitForSeconds(0.5f);
        }

        eyes_current = eyes;
        eyes_current.SetEyesOpen(true);
    }

    public Vector3 GetFrontPosition() => t_front.position;

    public List<Transform> GetCircleTransforms()
    {
        if(circle_transforms == null)
        {
            circle_transforms = eyes_circle.Eyes.Select(e => e.transform).ToList();
        }
        return circle_transforms;
    }
}