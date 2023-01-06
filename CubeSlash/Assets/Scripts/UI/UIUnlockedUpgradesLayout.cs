using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIUnlockedUpgradesLayout : MonoBehaviour
{
    [SerializeField] private UIIconButton temp_btn;

    public System.Action<Upgrade> OnUpgradeLevelSelected;

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
            btn.Button.OnSelectedChanged += s => OnSelected(s, u.upgrade);
        });

        void OnSelected(bool selected, Upgrade upgrade)
        {
            if (selected)
            {
                OnUpgradeLevelSelected?.Invoke(upgrade);
            }
        }
    }
}