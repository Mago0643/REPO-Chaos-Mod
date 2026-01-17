using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChaosMod
{
    internal class OptionSetting : MonoBehaviour, IPointerClickHandler
    {
        internal OptionsMenuController controller;
        internal TextMeshProUGUI categoryTxt;
        internal RectTransform Arrow;
        internal Transform child;
        internal int objectCount = 0;
        internal bool isClosed = false;

        void Start()
        {
            Close();
        }

        public void Open()
        {
            targetAngle = 0f;
            isClosed = false;
            child.gameObject.SetActive(true);

            // each option size is 25. has 10 distance
            var shit = (RectTransform)child;
            shit.sizeDelta = new Vector2(shit.sizeDelta.x, 35f + (25f * objectCount) + (10f * objectCount));

            // update self size, to update vertical layout.
            var fuck = (RectTransform)transform;
            fuck.sizeDelta = new Vector2(fuck.sizeDelta.x, 50f + shit.sizeDelta.y - 25f);

            // force update
            UpdateLayout();
        }

        void UpdateLayout()
        {
            controller.layout.enabled = false;
            controller.layout.enabled = true;
        }

        float targetAngle = 0f;
        void Update()
        {
            Arrow.localEulerAngles = Vector3.forward * Mathf.Lerp(targetAngle, Arrow.localEulerAngles.z, Mathf.Exp(-Mathf.PI * 5f * Time.deltaTime));
        }

        public void OnPointerClick(PointerEventData data)
        {
            if (data.pointerPressRaycast.gameObject.name != "BG") return;
            if (isClosed)
                Open();
            else
                Close();
        }

        public void Close()
        {
            targetAngle = 90f;
            isClosed = true;
            child.gameObject.SetActive(false);
            var fuck = (RectTransform)transform;
            fuck.sizeDelta = new Vector2(fuck.sizeDelta.x, 50f);
            UpdateLayout();
        }
    }
}
