using Lego.ChaosMod.Backend;
using System.Collections.Generic;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class IncreaseValuableWorthEvent: ChaosEvent
{
    internal static float[] mults = [];
    internal static int selectedIdx = 0;

    public IncreaseValuableWorthEvent(float[] mults): base("귀중품 가격", "evt_increase_valuable_desc")
    {
        isOnce = true;
        options.oncePerLevel = true;
        IncreaseValuableWorthEvent.mults = mults;
        ID = 16;
    }

    public override string GetName()
    {
        return Language.GetText("evt_increase_valuable").Replace("%f", mults[selectedIdx].ToString());
    }

    public override void Start()
    {
        base.Start();

        List<ValuableObject> items = ValuableDirector.instance.valuableList;
        if (items == null || items.Count == 0)
        {
            if (ChaosMod.IsDebug)
                Logging.Warn("Did not found any valuables. no increasing cash for you.");
            return;
        }

        selectedIdx = Random.Range(0, mults.Length);

        foreach (var item in items)
        {
            if (item == null)
            {
                if (ChaosMod.IsDebug)
                    Logging.Warn("Did not found one item.");
                continue;
            }

            float ogMoney = item.dollarValueCurrent;
            ogMoney *= mults[selectedIdx];

            item.dollarValueCurrent = Mathf.RoundToInt(ogMoney);
            item.dollarValueOriginal = Mathf.Max(ogMoney, item.dollarValueOriginal);
        }
    }
}