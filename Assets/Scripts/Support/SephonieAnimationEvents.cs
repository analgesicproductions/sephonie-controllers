using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SephonieAnimationEvents : MonoBehaviour {
    public bool ignore; // for shadows
    private MyPlayer3D player;
    private void Start() {
        player = GameObject.Find("MyPlayer").GetComponent<MyPlayer3D>();
    }

    public void RunFoot() {
        if (ignore) return;
        if (player != null) {
            player.AnimEvent_Run_FootOnGround();
        }
    }

    public void SprintFoot() {
        
        if (ignore) return;
        if (player != null) {
            player.AnimEvent_Sprint_FootOnGround();
        }
    }
    
}
