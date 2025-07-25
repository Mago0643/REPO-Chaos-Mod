using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace ChaosMod
{
    public class Commands: MonoBehaviour
    {
        TMP_InputField input;

        void Start()
        {
            if (!ChaosMod.IsDebug)
            {
                Destroy(this);
                return;
            }
        }

        void Update()
        {

        }
    }
}
