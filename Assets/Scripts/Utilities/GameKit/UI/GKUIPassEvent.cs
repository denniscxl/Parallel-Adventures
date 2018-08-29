using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GKUI
{
    public class GKUIPassEvent : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerDownHandler);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerUpHandler);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.submitHandler);
            PassEvent(eventData, ExecuteEvents.pointerClickHandler);
        }

        public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            GameObject current = data.pointerCurrentRaycast.gameObject;
            for (int i = 0; i < results.Count; i++)
            {
                if (current != results[i].gameObject)
                {
                    ExecuteEvents.Execute(results[i].gameObject, data, function);
                    // If you only want to affect the nearest object, please add break;
                }
            }
        }
    }
}