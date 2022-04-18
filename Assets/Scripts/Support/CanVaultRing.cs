using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanVaultRing : MonoBehaviour
{
    MeshRenderer ring_mr;
    MeshRenderer filled_ring_mr;
    Material ring_mat;
    Material filled_ring_mat;
    float ring_alpha = 0.3f;
    float filled_ring_alpha = 0.7f;
    Vector3 tempscale = new Vector3();
    Color tempcol = new Color();
    MyPlayer3D player;
    RaycastHit hit = new RaycastHit();
    public float distanceMul = 2f;
    void Start()
    {
        ring_mr = GetComponent<MeshRenderer>();
        filled_ring_mr = transform.Find("Can-Vault Filled Ring").GetComponent<MeshRenderer>();
        ring_mat = ring_mr.material;
        filled_ring_mat = filled_ring_mr.material;
        ////HF.GetPlayer(ref player);
        player = GameObject.Find("MyPlayer").GetComponent<MyPlayer3D>();
        if (player == null) {
            enabled = false;
            return;
        }
        ring_mr.enabled = filled_ring_mr.enabled = false;
        transform.parent = null;
        ColorUtility.TryParseHtmlString("#ffd677ff", out filled_col);
    }

    int mode_start = 0;
    int mode_invisible = 1;
    int mode_visible = 2;
    int mode = 0;
    Vector3 _origin = new Vector3();
    public float floorOffset = 0.1f;

    public float closenessCutoff = 2.1f;
    ////GripShroom gripShroom_ref = null;
    ////DashReaction dashReaction_ref = null;
    public bool slopeDebug = false;

    bool Slope_angle_valid(Vector3 normal) {
        float angle = HF.Get_slope_angle(normal);
        return angle >= 70f && angle <= 110f;
    }
    void Update()
    {
        
        if (Registry.DEV_MODE_ON && MyInput.shortcut && Input.GetKeyDown(KeyCode.Alpha4)) {
            Destroy(gameObject);
            return;
        }
        bool hit_something = false;
        if (mode == mode_start) {
            mode = mode_invisible;
        } else if (mode == mode_invisible) {
            if (player.Get_jump_state() == player.jump_state_midair && !player.isThereAnyReasonToPause() && !player.used_dash && !player.teledash_and_wallrun_disabled && !player.in_mud__resetByMud) {
                if (player.TeledashCast_for_VaultRing(ref hit, distanceMul)) {
                    hit_something = true;
                }
                if (hit_something && hit.distance > closenessCutoff && Slope_angle_valid(hit.normal)) {
                    mode = mode_visible;

                    t_allowEnablingRendererWhileValid += Time.deltaTime;
                    // Prevent flickering
                    if (t_allowEnablingRendererWhileValid > 0.075f) {
                        ring_mr.enabled = filled_ring_mr.enabled = true;
                    }
                    RefreshVisuals();
                } else {
                    t_allowEnablingRendererWhileValid = 0;
                }
            }
        } else if (mode == mode_visible) {
            if (player.Get_jump_state() != player.jump_state_midair || player.used_dash || player.isThereAnyReasonToPause()) {
                ring_mr.enabled = filled_ring_mr.enabled = false;
                mode = mode_invisible;
            } else {
                if (player.TeledashCast_for_VaultRing(ref hit, distanceMul)) {
                    hit_something = true;
                }

                if (hit_something && hit.distance > closenessCutoff && Slope_angle_valid(hit.normal)) {

                    t_allowEnablingRendererWhileValid += Time.deltaTime;
                    // Prevent flickering
                    if (t_allowEnablingRendererWhileValid > 0.075f) {
                        ring_mr.enabled = filled_ring_mr.enabled = true;
                    }
                    RefreshVisuals();
                } else {
                    t_allowEnablingRendererWhileValid = 0;
                    ring_mr.enabled = filled_ring_mr.enabled = false;
                    mode = mode_invisible;
                }
            }
        }


        if (slopeDebug && hit_something) {
            Debug.Log(HF.Get_slope_angle(hit.normal),hit.collider.gameObject);
            Debug.DrawRay(hit.point, hit.normal * 3f, Color.red, 5f);
        }

        float calc_dis = player.Get_dash_magnet_distance();
        calc_dis = calc_dis + extra_distance - adjustment_for_raycastDetectors;
        if (hit_something && hit.distance <= calc_dis) {
            if (hit.transform.parent != null) {
                ////gripShroom_ref = hit.transform.parent.GetComponent<GripShroom>();
                ////if (gripShroom_ref != null) {
                ////gripShroom_ref.Maybe_change_visuals();
                ////}
                ////dashReaction_ref = hit.transform.parent.GetComponent<DashReaction>();
                ////if (dashReaction_ref != null) {
////                    dashReaction_ref.Change_indicator_color();
////                }
            }
        }

    }
    float extra_distance = 0.5f;
    float adjustment_for_raycastDetectors = 0.25f; // Since raycast detectors need to remain fairly close to their triggers, make the player have to be close enough to them so that the player's teledash will definitely connect for the vault
    //Vector3 modified_hitNormal = new Vector3();
    void RefreshVisuals() {
        SetColor_BasedOnDistance(hit.distance);
        SetScaleViaDistance(player.Get_dash_magnet_distance() * distanceMul + extra_distance, player.Get_dash_magnet_distance() + extra_distance - adjustment_for_raycastDetectors, hit.distance);
        transform.localPosition = hit.point;
        _origin = hit.point;
        // could be used to fix weird angles of it, if we even use the vaultring
        //modified_hitNormal = hit.normal;
        //if (modified_hitNormal.y > 0.3f) modified_hitNormal = 0.f
        // okay dn't actualy use the hit point's y value - scale it to the players. (bc sometimes the hit of the capsule collide gets 'stuck')
        _origin.y = player.transform.position.y;
        _origin += hit.normal * floorOffset;
        _origin.y += 1.75f;
        transform.position = _origin;
        transform.LookAt(transform.position + hit.normal * -1);
    }

    private Color filled_col;
    void SetColor_BasedOnDistance(float dis) {
        if (dis <= player.Get_dash_magnet_distance() + extra_distance - adjustment_for_raycastDetectors) {
            SetColor(filled_col, filled_ring_alpha,true);
        } else {
            SetColor(Color.white, ring_alpha);
        }
     }

    void SetScaleViaDistance(float maxVisibleDis, float minValidDistance, float d) {
        tempscale = filled_ring_mr.transform.localScale;
        if (d <= minValidDistance) {
            tempscale = Vector3.one;
        } else {
            tempscale = Vector3.one * 0.75f * ((maxVisibleDis - d) / (maxVisibleDis - minValidDistance));
        }
        filled_ring_mr.transform.localScale = tempscale;

    }
    float t_allowEnablingRendererWhileValid = 0;
    void SetColor(Color col, float a,bool hidering=false) {
        tempcol = col;
        tempcol.a = a;
        filled_ring_mat.SetColor("_BaseColor", tempcol);

        if (hidering) {
            tempcol.a = 0;
        }
        ring_mat.SetColor("_BaseColor", tempcol);

    }
}
