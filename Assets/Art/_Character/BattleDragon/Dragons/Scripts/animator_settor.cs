using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class animator_settor_ : MonoBehaviour
{

    public string _name = "";
    public float time = 0.0f;

    // Use this for initialization
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (null != animator && "" != _name)
        {
            animator.Play(_name, -1, time);
            animator.speed = 0;

            //  animation[_name].wrapMode = WrapMode.Loop;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
