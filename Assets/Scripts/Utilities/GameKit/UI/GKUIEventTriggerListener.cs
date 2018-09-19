using UnityEngine;
using UnityEngine.EventSystems;
using GKBase;

namespace GKUI
{
    public class GKUIEventTriggerListener : UnityEngine.EventSystems.EventTrigger
    {
        public delegate void VoidDelegate(GameObject go);
        public delegate void DragDelegate(PointerEventData eventData);
        public VoidDelegate onClick;
        public DragDelegate onDrag;
        public VoidDelegate onDown;
        public VoidDelegate onEnter;
        public VoidDelegate onExit;
        public VoidDelegate onUp;
        public VoidDelegate onSelect;
        public VoidDelegate onUpdateSelect;

        static public GKUIEventTriggerListener Get(GameObject go)
        {
            GKUIEventTriggerListener listener = GK.GetOrAddComponent<GKUIEventTriggerListener>(go);
            return listener;
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null) onClick(gameObject);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null) onDown(gameObject);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null) onEnter(gameObject);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null) onExit(gameObject);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onUp != null) onUp(gameObject);
        }
        public override void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null) onSelect(gameObject);
        }
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelect != null) onUpdateSelect(gameObject);
        }
    }
}