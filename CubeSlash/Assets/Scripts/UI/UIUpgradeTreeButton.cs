using UnityEngine;
using UnityEngine.UI;

public class UIUpgradeTreeButton : MonoBehaviour
{
    [SerializeField] private UIIconButton btn_icon;
    [SerializeField] private Image img_unlocked;

    public SelectableMenuItem Button { get { return btn_icon.Button; } }

    public void SetUpgrade(UpgradeInfo info)
    {
        img_unlocked.enabled = info.isUnlocked;
        btn_icon.Icon = info.upgrade.icon;
    }
}