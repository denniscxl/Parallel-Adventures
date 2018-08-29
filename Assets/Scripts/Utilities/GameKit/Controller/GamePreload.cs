using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GKController
{
    public class GamePreload : MonoBehaviour
    {

        public static GamePreload instance = null;

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
