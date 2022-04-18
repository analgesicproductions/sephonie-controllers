using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Added by MyPlayer3D to the character models
public class MyRootMotionHandler : MonoBehaviour
{
    Vector3 localEuler = new Vector3();
    MyPlayer3D player;
    Animator animator;
    void Start() {
        player = GameObject.Find("MyPlayer").GetComponent<MyPlayer3D>();
        animator = GetComponent<Animator>();
    }


    private void OnAnimatorMove() {
        float delY = animator.deltaRotation.eulerAngles.y;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("DashWindup")) {
            localEuler = transform.localEulerAngles;
            localEuler.y += delY;
            transform.localEulerAngles = localEuler;
        }
    }
}
