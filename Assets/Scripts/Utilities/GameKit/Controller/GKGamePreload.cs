using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GKController
{
    public class GKGamePreload : MonoBehaviour
    {

        public static GKGamePreload instance = null;

        // Use this for initialization
        protected void Start()
        {
            instance = this;
            DontDestroyOnLoad(this);
            StartCoroutine(Preload());
        }

        public virtual IEnumerator Preload()
        {
            yield return null;
        }
    }
}
