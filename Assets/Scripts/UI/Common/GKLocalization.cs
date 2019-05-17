using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;

namespace GKUI
{
    public class GKLocalization : MonoBehaviour
    {


        [SerializeField]
        private int _type;
        [SerializeField]
        private int _id;

        // Use this for initialization
        void Start()
        {

            OnLanguageChanged();
            //Debug.Log("GKLocalization Start ID: " + _id);
            PlayerController.Instance().OnLanguageChangedEvent += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            PlayerController.Instance().OnLanguageChangedEvent -= OnLanguageChanged;
            //Debug.Log("GKLocalization OnDestroy ID: " + _id + " Length: " + DataController.Instance().OnLanguageChangedEvent.GetInvocationList().Length);
            //if(null != DataController.Instance().OnLanguageChangedEvent)
            //{
            //    Delegate[] eventList = DataController.Instance().OnLanguageChangedEvent.GetInvocationList();
            //    if (null != eventList)
            //    {
            //        string result = string.Empty;
            //        for (int i = 0, iCount = eventList.Length; i < iCount; i++)
            //        {
            //            Delegate oneEvent = eventList[i];
            //            result += oneEvent.Target + ":" + oneEvent.Method + "\r\n";
            //        }
            //        Debug.Log("result: " + result);
            //    }
            //}
        }

        private void OnLanguageChanged()
        {
            Text content = GK.GetOrAddComponent<Text>(gameObject);
            content.text = DataController.Instance().GetLocalization(_id, (LocalizationSubType)_type);
        }

    }
}