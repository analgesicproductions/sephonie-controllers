using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HF {

    public static bool TimerDefault(ref float timer, float timerMax, float increment = 0) {
        if (increment == 0) increment = Time.deltaTime;
        timer += increment;
        if (timer > timerMax) {
            timer -= timerMax;
            return true;
        }
        return false;
    }

    public static bool TimerStayAtMax(ref float timer, float timerMax, float increment = 0) {
        if (timer >= timerMax) {
            return true;
        }
        if (increment == 0) increment = Time.deltaTime;
        timer += increment;
        if (timer >= timerMax) {
            timer = timerMax;
            return true;
        }
        return false;
    }

    public static bool TimerStayAtMin(ref float timer, float timerMin=0, float increment = 0) {
        if (timer <= timerMin) {
            return true;
        }
        if (increment == 0) increment = Time.deltaTime;
        timer -= increment;
        if (timer <= timerMin) {
            timer = timerMin;
            return true;
        }
        return false;
    }

    public static Vector3 temp_v_getanglefromxz = new Vector3();
    public static float Get_angle_from_xz(Vector3 v, bool absolute = false) {
        temp_v_getanglefromxz = v.normalized;
        temp_v_getanglefromxz.y = 0;
        if (temp_v_getanglefromxz.magnitude < 0.02f && v.y > 0) return 90f;
        if (temp_v_getanglefromxz.magnitude < 0.02f && v.y < 0) return -90f;
        float ang = Vector3.Angle(temp_v_getanglefromxz, v);
        if (absolute) return Mathf.Abs(ang);
        return ang;
    }

    public static float Get_slope_angle(Vector3 slopeNormal) {
        return Vector3.Angle(slopeNormal, Vector3.up);
    }


    public static void Warning(string s, UnityEngine.Object context=null) {
        s = s.Replace(">", "\\>");
        s = s.Replace("<", "\\<");
        if (context != null) {
            Debug.Log("<color=red>" + s + "</color>");
        } else {
            Debug.Log("<color=red>" + s + "</color>", context);
        }
    }

    public static float TruncateFloat(float f) {
        return ((int)(f * 100)) / 100f;
    }
}
