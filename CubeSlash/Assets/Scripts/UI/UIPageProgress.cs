using System.Collections.Generic;
using UnityEngine;

public class UIPageProgress : TemplateItemList<UIPageProgressDot>
{
    private UIPageProgressDot current;

    public override void CreateItems(int count)
    {
        ClearItems();
        base.CreateItems(count);
    }

    public void SetIndex(int i)
    {
        i = Mathf.Clamp(i, 0, Items.Count - 1);

        if(current != null)
        {
            current.AnimateHide();
            current = null;
        }

        current = Items[i];

        if(current != null)
        {
            current.AnimateShow();
        }
    }

    public override void ClearItems()
    {
        base.ClearItems();
        current = null;
    }
}