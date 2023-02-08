using TMPro;
using UnityEngine;

public class UIDifficulty : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp_title, tmp_desc;
    [SerializeField] private UILock uilock;

    public RectTransform RectTransform { get { return _rt ?? GetRectTransform(); } }
    public DifficultyInfo Difficulty { get; private set; }

    private RectTransform _rt;

    public void Initialize(DifficultyInfo info)
    {
        tmp_title.text = info.difficulty_name;
        tmp_desc.text = info.difficulty_description;

        Difficulty = info;
    }

    private RectTransform GetRectTransform()
    {
        if(_rt == null) _rt = GetComponent<RectTransform>();
        return _rt;
    }

    public void SetLocked(bool locked)
    {
        uilock.gameObject.SetActive(locked);
        uilock.Text = "Locked";
        tmp_title.enabled = !locked;
        tmp_desc.enabled = !locked;
    }
}