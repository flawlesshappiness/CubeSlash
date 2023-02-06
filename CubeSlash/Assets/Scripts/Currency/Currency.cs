using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Currency
{
    public static int Coins { get { return CurrencyController.Instance.GetAmount(CurrencyType.Coins); } }
    public static int DNA { get { return CurrencyController.Instance.GetAmount(CurrencyType.DNA); } }
}