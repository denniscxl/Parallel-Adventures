using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using GKBase;

namespace GKUI
{
    public class UIDragDropItem : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {

        public enum InteractionType
        {
            Expand,
            Exchange
        }

        #region PublicField
        public InteractionType interactionType = InteractionType.Expand;
        public GameObject moveRoot = null;
        public string gridTag = "";
        public float duration = 0.5f;
        #endregion

        #region PrivateField
        private Vector3 lastPoint = Vector3.zero;
        private float screenWidthHalf = 0;
        private float screenHeightHalf = 0;
        private GameObject gridRoot = null;
        #endregion

        #region PublicMethod
        public void OnDrag(PointerEventData eventData)
        {
            GetComponent<RectTransform>().pivot.Set(0, 0);
            transform.position = Input.mousePosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {

            lastPoint = transform.localPosition;
            transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            gridRoot = transform.parent.gameObject;
            transform.parent = moveRoot.transform;

            SetRatcast(false);

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);

            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.tag == gridTag)
            {

                var gridObj = eventData.pointerCurrentRaycast.gameObject;

                GameObject ratcastObj = eventData.pointerCurrentRaycast.gameObject;


                switch (interactionType)
                {
                    case InteractionType.Expand:

                        GameObject tGrid = null;

                        if (string.Equals(ratcastObj.name, gameObject.name))
                        {

                            // Icon.
                            tGrid = ratcastObj.transform.parent.gameObject;

                        }
                        else
                        {

                            // Grid.
                            tGrid = ratcastObj;

                        }

                        transform.parent = tGrid.transform;
                        gridObj = tGrid;

                        break;
                    case InteractionType.Exchange:

                        UIDragDropItem exchangeObj = null;

                        if (string.Equals(ratcastObj.name, gameObject.name))
                        {

                            // Icon.
                            exchangeObj = ratcastObj.GetComponent<UIDragDropItem>();

                        }
                        else
                        {

                            // Grid.
                            exchangeObj = GK.FindChildOfType<UIDragDropItem>(ratcastObj);

                        }

                        if (null != exchangeObj)
                        {

                            Transform t = gridRoot.transform;

                            transform.parent = exchangeObj.transform.parent;
                            gridRoot = exchangeObj.transform.parent.gameObject;

                            exchangeObj.transform.parent = t;
                            exchangeObj.gridRoot = t.gameObject;

                            Vector3 tTargetPos = new Vector3(lastPoint.x + screenWidthHalf + exchangeObj.gridRoot.transform.localPosition.x,
                                lastPoint.y + screenHeightHalf + exchangeObj.gridRoot.transform.localPosition.y, lastPoint.z);

                            exchangeObj.transform.DOMove(tTargetPos, duration);

                        }
                        else
                        {

                            // The parent node does not change.
                            RollBackPos();

                        }

                        break;
                }
            }
            else
            {
                RollBackPos();
            }

            SetRatcast(true);
        }
        #endregion

        #region PrivateMethod
        private void Start()
        {
            screenWidthHalf = UIController.width * 0.5f;
            screenHeightHalf = UIController.height * 0.5f;
        }

        private void RollBackPos()
        {
            transform.parent = gridRoot.transform;
            Vector3 tTargetPos = new Vector3(lastPoint.x + screenWidthHalf + gridRoot.transform.localPosition.x,
                lastPoint.y + screenHeightHalf + gridRoot.transform.localPosition.y, lastPoint.z);
            transform.DOMove(tTargetPos, duration, false);
        }

        private void SetRatcast(bool b)
        {
            if (null != transform.GetComponent<Image>())
            {
                transform.GetComponent<Image>().raycastTarget = b;
            }
            else if (null != transform.GetComponent<RawImage>())
            {
                transform.GetComponent<RawImage>().raycastTarget = b;
            }
        }
        #endregion
    }

}