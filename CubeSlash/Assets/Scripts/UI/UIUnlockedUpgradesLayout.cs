using System.Collections.Generic;
using UnityEngine;

public class UIUnlockedUpgradesLayout : MonoBehaviour
{
    [SerializeField] private UIIconButton temp_btn;

    public System.Action<UIIconButton, Upgrade> OnUpgradeLevelSelected;

    private List<UIIconButton> btns = new List<UIIconButton>();

    private void Start()
    {
        temp_btn.gameObject.SetActive(false);

        btns.ForEach(b => Destroy(b.gameObject));
        btns.Clear();

        UpgradeController.Instance.GetUnlockedUpgrades().ForEach(u =>
        {
            var btn = Instantiate(temp_btn, temp_btn.transform.parent);
            btn.gameObject.SetActive(true);
            btn.Icon = u.upgrade.icon;
            btn.Button.onSelect += () => OnSelect(btn, u.upgrade);
        });

        void OnSelect(UIIconButton btn, Upgrade upgrade)
        {
            OnUpgradeLevelSelected?.Invoke(btn, upgrade);
        }
    }
}