using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MadSlime.ShapeFill.Input
{
    [RequireComponent(typeof(Image))]
    public class FillTapButton : MonoBehaviour, IPointerDownHandler
    {
        public event Action Pressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            Pressed?.Invoke();
        }
    }
}
