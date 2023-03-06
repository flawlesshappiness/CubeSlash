using System.Collections.Generic;
using UnityEngine;

public class TemplateItemList<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] private T template_item;

    public List<T> Items { get; private set; } = new List<T>();

    protected virtual void Start()
    {
        template_item.gameObject.SetActive(false);
    }

    public virtual void CreateItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var dot = CreateItem();
        }
    }

    public virtual T CreateItem()
    {
        var inst = Instantiate(template_item, template_item.transform.parent);
        inst.gameObject.SetActive(true);
        Items.Add(inst);
        return inst;
    }

    public virtual void ClearItems()
    {
        Items.ForEach(item => Destroy(item.gameObject));
        Items.Clear();
    }
}