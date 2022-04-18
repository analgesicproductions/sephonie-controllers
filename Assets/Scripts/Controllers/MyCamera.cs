using System.Collections;

using System.Linq;
using System.Collections.Generic;
////using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyCamera : MonoBehaviour {
    internal bool test_3d_mode = false;
    public Vector3 test_offset;
    public float test_x_rot;
    public float sprintAddY_speed = 18f;
    public float sprintLookLimit = 30f;
    private float sprintAddY;
    bool lerpCamPos = false;
	Vector3 posFromTrigger;
	Vector3 STARTposFromTrigger;
	Vector3 STARTrotFromTrigger;
	internal Camera cam;
    ////DialogueBox dbox;
    ////Cinemachine.CinemachineBrain brain;
    ////HoverboardHelper hoverboard;
    private float t_sprint_held;
    private bool sprint_angle_hasnt_reached_player;

    public float shakeon = 0;

    private const int terrain_lm = 1 << 10;

    // Set by doors so camera is rotated correctly when transitioning
    public static float initialCameraRotation;
	float triggerTransitionTime;
	float maxTTT;

    public static bool inCinemachineMovieMode;
	public bool fixedFollow = false; // Set this to have the camera stay in one spot and track the target
	public bool fixedFollowTopDown = false; // Set this to have the camera follow the player like at opdown cam
	Vector3 camXZ;
	Vector3 playerXZ;
    //	SceneData sceneData;

    internal bool just_dashed;
    internal bool dash_offset_applied;
    private float t_dashCamLerp;
    [Range(0.1f,0.8f)]
    public float tm_dashCamLerp = 0.4f;

    private int dash_mode;
    private Vector3 targetPos_at_dashStart;

    private float t_wallrunCamLerp;
    public float tm_wallrunCamLerp = 0.3f;
    public float wallrun_camera_tilt = 30f;
    private int wallrun_mode;
    private Vector3 targetPos_at_wallrunStart;

    public float sprintLerpStrength = 6f;
    public float sprintMaxLerpDelta = 6f;

    GameObject target;
	public float rotateSpeed = 320;
    public float wallRunFollowStrength = 12f;
	Vector3 offset;
    Vector3 tempCamPos = new Vector3();
    [Header("Camera Offsets")]
    [Tooltip("How much the camera looks below the player during a jump")]
    public float mediumYOff = 1.7f;
    [Tooltip("Default downwards angle")]
    public Vector3 mediumOffset;
    [Tooltip("A far, downwards angle")]
    public Vector3 mediumOffsetFar;
    [Tooltip("Default upwards angle")]
    public Vector3 mediumOffsetUp;
    public Vector3 mediumOffsetTopDown;

    int offsetMode = 0;
    GameObject myPlayerGO;
    MyPlayer3D player;
    public float skyboxRotateSpeed = 0;

    private void Awake() {
        setCameraEulerYRotation(initialCameraRotation);
        initialCameraRotation = 0;
        ////brain = GetComponent<Cinemachine.CinemachineBrain>();

        string curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (Registry.scenes_to_disable_cam_and_player.Contains(curSceneName)) {
            enabled = false;
            ////brain.enabled = true;
        }
    }
    void Start () {
        
        Shader.SetGlobalFloat("_PlayerDitherStrength",0);
        offset = mediumOffset * SaveManager.cameraDistance / 100f;
        lerpOffsetCurrentVector = offset;
        offsetMode = 0;

        camXZ = new Vector3();
        playerXZ = new Vector3();

        ////playershadow = GameObject.Find("Player Shadow").GetComponent<PlayerShadowHelper>();
        ////dbox = HF.GetDialogueBox();

        string scenename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        myPlayerGO = GameObject.Find("MyPlayer");
        player = myPlayerGO.GetComponent<MyPlayer3D>();
        ////hoverboard = player.GetComponent<HoverboardHelper>();
        target = myPlayerGO;

        camXZ = new Vector3();
		playerXZ = new Vector3();
        curXOrbitAngle = 0f;

		if (fixedFollow == false) {
			transform.position = target.transform.position + offset;
		}
		targetPos = transform.position;

		//sceneData = GameObject.Find("SceneData").GetComponent<SceneData>();
		cam = GetComponent<Camera>();
        cam.fieldOfView = SaveManager.fieldOfView;
        cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().volumeTrigger = player.transform;
	}
	float target_yEuler = 0f;
    float yEuler_lerpStart = 0f;
	float curXOrbitAngle = 0f;

	public float lowerVertAngleLimit = -20f;
	public float upperVertAngleLimit = 40f;
	float verticalCamSpeedMultiplier = 0.32f;
	float yRot_speedMul = 0.43f;
    bool jumpCam_HasFollowedFaster = false;

	Vector3 targetPos;
	Vector3 nextTargetPos;

	void switchModeInitialization(bool _spline, bool _fixed, float _t) {
		fixedFollow = _fixed; 
		triggerTransitionTime = 0; 
		maxTTT = _t; 
		lerpCamPos = true;
	}

    public void smoothLerpFromVCToNormal(float _transitionTime) {
        triggerTransitionTime = 0;
        maxTTT = _transitionTime;
        lerpCamPos = true;
        STARTposFromTrigger = transform.position;
        STARTrotFromTrigger = transform.eulerAngles;
    }

	public void switchToFollow(float _transitionTime) {
		if (!fixedFollow) return;

		switchModeInitialization(false,false,_transitionTime);
		STARTposFromTrigger = transform.position;
		STARTrotFromTrigger = transform.eulerAngles;
	}

	// Makes the camera transition to a fixed position view, lerping via SmoothStep for _transitionTime seconds.
	public void switchToFixed(Vector3 _newCamPos, float _transitionTime) {
		switchModeInitialization(false,true,_transitionTime);
		// position given from the trigger - destination of the camera.
		posFromTrigger = _newCamPos;
		STARTposFromTrigger = transform.position;
	}

    static float GetFieldOfView() {
        return SaveManager.fieldOfView;
    }

    ////PlayerShadowHelper playershadow;
    bool didInit = false;
    [System.NonSerialized]
    public bool waitingForSizeSwitchConfirmation = false;
    public bool EXT_FORCE_WALKSCALE = false;

    private Vector3 player_pos;
    internal bool playerCameraControlsOff = false;

    internal int ss_mode;
    void LateUpdate() {

        if (MyPlayer3D.ignore_input) {
            MyPlayer3D.ignore_input = false;
        }
        if (skyboxRotateSpeed != 0) {
            float skyRot = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat("_Rotation", skyRot + skyboxRotateSpeed*Time.deltaTime);
        }

        float used_sensitivity = SaveManager.sensitivity;
        if (!MyInput.gamepad_active && SaveManager.mouse_camera_is_active) {
            used_sensitivity = SaveManager.mouseCam_rotationSpeed * 2f;
        }

        ////if (brain.enabled) {
        ////fixedFollow = true;
        ////} else {
            fixedFollow = false;
            ////}
        fixedFollow = false;

        // temp CutsceneManager.deactivateCameras
        /*/*
        if (inCinemachineMovieMode || SaveModule.saveMenuOpen || DataLoader.instance.isPaused || paused_for_scanning || MyEvent.playerTrigNonCutscene_turnsOffCamControls) {
            playerCameraControlsOff = true;
            if (SaveModule.saveMenuOpen || DataLoader.instance.isPaused) {
                
                if (ditherPlayerMode >= 1 && ditherPlayerMode <= 3 && DataLoader.instance.isPaused) {
                    Shader.SetGlobalFloat("_PlayerDitherStrength",0);
                    ditherPlayerMode = 0;
                    player_dither_strength = 0;
                }
                return;
            } else {
                // It's possible that being able to use this camera code may be handy during other pausing instances.
                // the playerCameraControlsOff bool will prevent any player input from moving the camera, but the camera position could still be moved around and say, avoid walls and the like.
            }
        } else {*/
            playerCameraControlsOff = false;
        ////}

        if(!didInit) {
            didInit = true;
            targetPos = target.transform.position; // Camera follows this
            // target always LERPs to this, nextTarget only updates when player leaves deadzone
            nextTargetPos = target.transform.position; 
        }

        player_pos = target.transform.position;
        if (freeze_targetpos_in_link) player_pos = player.pausedRBPos;

        offset = mediumOffset;


        if (test_3d_mode) {
            targetPos = player_pos;
            cam.fieldOfView = 40f;
            transform.position = targetPos + test_offset;
            transform.LookAt(targetPos);
            Vector3 local_euler = transform.localEulerAngles;
            local_euler.x = test_x_rot;
            transform.localEulerAngles = local_euler;
            return;
        }

        
        if (fixedFollow) {
            if (lerpCamPos) {
                float _t = Mathf.SmoothStep(0,1,triggerTransitionTime/maxTTT);
                triggerTransitionTime += Time.deltaTime;
                if (triggerTransitionTime > maxTTT) {
                    lerpCamPos = false;
                }
                transform.position = Vector3.Lerp(STARTposFromTrigger,posFromTrigger,_t);
            }

            nextTargetPos = player_pos;
            targetPos = Vector3.Lerp(targetPos,nextTargetPos,Time.deltaTime*3);

            if (fixedFollowTopDown) {
                camXZ.Set(targetPos.x,transform.position.y,targetPos.z-8);
                playerXZ.Set(targetPos.x,0,targetPos.z); // does nothing
                transform.position = camXZ;
            } 

            transform.LookAt(targetPos);
            return;
        }

        bool holdingForward = MyInput.moveY > 0;
        if (playerCameraControlsOff) holdingForward = false;


        /*/*if (Update_ss_mode()) {
            return;
        }*/

        /* Default Camera behavior */
        nextTargetPos.x = player_pos.x;
        nextTargetPos.z = player_pos.z;
        if (freeze_targetpos_in_link) {
            nextTargetPos.x = player.pausedRBPos.x;
            nextTargetPos.z = player.pausedRBPos.z;
        }
        // When moving forward/back, slowly move the target position to slightly behind the player based on camera facing vector
        Vector3 cam_xz_flipped = transform.forward;
        cam_xz_flipped.y = 0; cam_xz_flipped *= -1; cam_xz_flipped.Normalize();
        if (freeze_targetpos_in_link) {
            cam_xz_flipped.Set(0,0,0);
        }
        Ray r = new Ray(player_pos, cam_xz_flipped); // dont move the target pos through a wall
        if (player.IsGliding()) {
        } else if (holdingForward && !MyInput.recenter && !Physics.Raycast(r, 4f,Registry.lmc_CAMERA_COLLIDABLES)) {
            t_xzCamLag += Time.deltaTime;
        } else {
            t_xzCamLag -= Time.deltaTime;
        }
        
        if (collided_last_frame) {
            t_xzCamLag -= 2*Time.deltaTime;
        }

        collided_last_frame = false;
        float tm_xzCamLag = 1f;
        t_xzCamLag = Mathf.Clamp01(t_xzCamLag/tm_xzCamLag);

        nextTargetPos = Vector3.Lerp(nextTargetPos, nextTargetPos + cam_xz_flipped * 2f, Mathf.SmoothStep(0, 1, t_xzCamLag));
        switch (dash_mode)
        {
            case 1:
            {
                if (dash_offset_applied) {
                    dash_offset_applied = false;
                    dash_mode = 2;
                    t_dashCamLerp = tm_dashCamLerp;
                }
                nextTargetPos = targetPos_at_dashStart;
                break;
            }
            // after dashing, move from old to new position in so amount of time.
            case 2 when t_dashCamLerp <= 0:
                dash_mode = 0;
                break;
            case 2:
                nextTargetPos = Vector3.Lerp(targetPos_at_dashStart, nextTargetPos, Mathf.SmoothStep(1, 0, t_dashCamLerp / tm_dashCamLerp));
                t_dashCamLerp -= Time.deltaTime;
                break;
        }

        if (dash_mode == 0) {
            if (just_dashed) {
                just_dashed = false;
                targetPos_at_dashStart = nextTargetPos;
                targetPos_at_dashStart.y = 0;
                dash_mode = 1;
            }
        }

        // Lerp camera through the wallrun-start player movement
        if (wallrun_mode == 0) {
            if (player.is_wallrunning) { // Wallrun code has offset the player already
                wallrun_mode = 1;
                t_wallrunCamLerp = tm_wallrunCamLerp;
            } else {
                targetPos_at_wallrunStart = nextTargetPos;
            }
        }
        if (wallrun_mode == 1) {
            if (t_wallrunCamLerp <= 0 || !player.is_wallrunning) {
                wallrun_mode = 2;
            } else {
                nextTargetPos = Vector3.Lerp(targetPos_at_wallrunStart, nextTargetPos, Mathf.SmoothStep(1, 0, t_wallrunCamLerp / tm_wallrunCamLerp));
                t_wallrunCamLerp -= Time.deltaTime;
            }
        } else if (wallrun_mode == 2 && !player.is_wallrunning) {
            wallrun_mode = 0;
        }
       
        targetPos.y = player_pos.y;
        if (freeze_targetpos_in_link) {
            targetPos.y = player.pausedRBPos.y;
        }
        
        targetPos.y += mediumYOff;

        // If in jumping mode camera should work a little differently along y-axis

        if (inJumpCam)
        {
            switch (jumpCamMode)
            {
                case 0:
                {
                    if (jumpCamTargetPos.y - player_pos.y < -3) {
                        jumpCamTargetPos.y = player_pos.y - 3;
                    }
                    targetPos = jumpCamTargetPos; // This is set when the player controller tells the camera to enter jump mode.
                    // It's fixed to where the player starts the jump (to simulate a deadzone as the player initially jumps)
                    float jyo = player_pos.y - jumpDeadStartY;
                    if (jyo > jumpDeadYMargin || jyo < -1f ) {
                        jumpCamMode = 1;
                        timeJumpHeld = 0;
                        jumpCam_HasFollowedFaster = false;
                        jumpCameraLookYOff_Current = 0;
                        jumpCamTargetPosVel = Vector3.zero;
                        jumpLookVel = 0;
                    }

                    break;
                }
                case 1:
                {
                    // when player is above margin of jump cam movement, make the camera slowly look further below it.
                    float jyo = player_pos.y - jumpDeadStartY;
                    if (jyo > jumpDeadYMargin) { 
                        jumpCameraLookYOff_Current = Mathf.SmoothDamp(jumpCameraLookYOff_Current, jumpCameraLookYOff, ref jumpLookVel, jumpLookSmoothTime);
                    }
                    else { jumpCameraLookYOff_Current = Mathf.SmoothDamp(jumpCameraLookYOff_Current, 0, ref jumpLookVel, jumpLookSmoothTime); }

                    float playerYVel = target.GetComponent<Rigidbody>().velocity.y;
                    jumpCameraLookYOff_Current = Mathf.Clamp(jumpCameraLookYOff_Current, 0, jumpCameraLookYOff);
                    ////bool followFasterBecauseMovingFast = playerYVel > 19f || Geyser.activeGeysers > 0;
                    bool followFasterBecauseMovingFast = playerYVel > 19f;
                    if (followFasterBecauseMovingFast) {
                        jumpCam_HasFollowedFaster = true;
                    }
                    targetPos.y -= jumpCameraLookYOff_Current;

                    // when bounced up really fast (with Bouncer3D, etc), then quickly lerp to following the player precisely.
                    // The player will remain in the HasFollowedFaster state until the player has NOT been moving upwards really  fast for a seconds
                    // this guarantees that transitioning to anything with the timeJumpHeld var won't lead to choppy transitions.
                    if (jumpCam_HasFollowedFaster) {
                        if (!followFasterBecauseMovingFast) {
                            timeJumpHeld -= Time.deltaTime;
                        } else {
                            timeJumpHeld += Time.deltaTime;
                        }
                        jumpCamTargetPos = Vector3.Lerp(jumpCamTargetPos, targetPos, timeJumpHeld / 2.0f);
                        if (timeJumpHeld <= 0) {
                            jumpCam_HasFollowedFaster = false;
                        }

                        // Using the existing jumpCamTargetPos, we'll slowly lerp it to this new target position, then
                        // set targetPos to it, so the next camera calculations work
                    } else if (playerYVel < 0) {
                        jumpCamTargetPos = Vector3.SmoothDamp(jumpCamTargetPos, targetPos, ref jumpCamTargetPosVel, jumpTargetLerpSmoothtime);
                    } else {
                        // subtract timeJumpHeld to zero here so that in a double jump, if returning to th eplayerYVel< 0 block ,there's no choppiness
                        jumpCamTargetPos = Vector3.SmoothDamp(jumpCamTargetPos, targetPos, ref jumpCamTargetPosVel, jumpTargetLerpSmoothtime);
                    }

                    if (MyInput.shortcut) {
                        jumpCamTargetPos = targetPos;
                    }
                    targetPos = jumpCamTargetPos;
                    break;
                }
                case 2:
                {
                    // Lerp back from following beneath the foot to back to the player.
                    t_JumpReturn += Time.deltaTime;
                    if (targetPos.y - jumpCamTargetPos.y > 2f) {
                        t_JumpReturn += 0.88f * Time.deltaTime;
                    }
                    targetPos.y = Mathf.Lerp(jumpCamTargetPos.y, targetPos.y, t_JumpReturn / tm_JumpReturn);
                    if (t_JumpReturn > tm_JumpReturn) {
                        t_JumpReturn = tm_JumpReturn;
                        jumpCamMode = 0;
                        inJumpCam = false;
                    }

                    break;
                }
            }
        }

        // Addendum: If jump mode is on, the x and z positions will be a little messed up from calculations. Snap them to the player.
        targetPos.x = nextTargetPos.x;
        targetPos.z = nextTargetPos.z;

        //--------------------------------------------------
        //--------------------------------------------------
        // PANNING (y-axis rotation) -----------------------
        //--------------------------------------------------
        //--------------------------------------------------

        bool sprinting = player.is_sprinting_speed_on;
        bool sprint_input_held = MyInput.sprintDash;
        if (player.autosprinting) {
            sprint_input_held = true;
        }
        
        // When starting a sprint, set a flag. It resets when the camera lerps behind the player.
        if (sprinting) {
            if (t_sprint_held == 0) {
                sprint_angle_hasnt_reached_player = true;
            }

            t_sprint_held = 1;
        } else {
            t_sprint_held = 0;  
        }
        
        bool cancelMovementInducedCameraYRot = false;
        
        // If sprint snap disabled, turn off sprinting here (so camera is manually rotated).
        if (!SaveManager.sprint_camera_snapping_on) {
            sprinting = false;
            cancelMovementInducedCameraYRot = true;
        }
        float target_dy_rot = 0;
        float dt = Time.deltaTime;
        if (dt > 0.035f) dt = 0.035f;
        float yRot_speed_scaled = rotateSpeed * dt;
        if (player.IsGliding()) yRot_speed_scaled = player.glide_rotationalVel* dt; // override
        
        float cam_input_x = MyInput.camX;
        
        ////if ((!MyInput.gamepad_active && !SaveManager.mouse_camera_is_active && !player.kb_hor_moves && !sprinting && !hoverboard.isActive &&!player.is_wallrunning) && MyInput.left) cam_input_x = -1;
        ////if ((!MyInput.gamepad_active && !SaveManager.mouse_camera_is_active && !player.kb_hor_moves && !sprinting && !hoverboard.isActive && !player.is_wallrunning) && MyInput.right) cam_input_x = 1;
        if ((!MyInput.gamepad_active && !SaveManager.mouse_camera_is_active && !player.kb_hor_moves && !sprinting  &&!player.is_wallrunning) && MyInput.left) cam_input_x = -1;
        if ((!MyInput.gamepad_active && !SaveManager.mouse_camera_is_active && !player.kb_hor_moves && !sprinting && !player.is_wallrunning) && MyInput.right) cam_input_x = 1;

        
        float convertedMoveX = MyInput.moveX;
        if (!MyInput.gamepad_active && player.kb_hor_moves && MyInput.left) convertedMoveX = -1;
        if (!MyInput.gamepad_active && player.kb_hor_moves && MyInput.right) convertedMoveX = 1;
        // Copy cam input to move input during sprints (so the r stick acts like left, or WASD cam like arrrows)
        if (sprinting && SaveManager.sprint_camera_snapping_on && Mathf.Abs(convertedMoveX) < 0.05f && cam_input_x < -0.05f) convertedMoveX = -1f; 
        if (sprinting && SaveManager.sprint_camera_snapping_on && Mathf.Abs(convertedMoveX) < 0.05f && cam_input_x > 0.05f) convertedMoveX = 1f;
//        print(cam_input_x+","+convertedMoveX);


        if (playerCameraControlsOff) { convertedMoveX = 0; cam_input_x = 0; }

        // Moving camera with arrow keys & R-stick
        float joy_leftMovementThreshold = 0.5f;

        if (player.just_vaulted_but_didnt_land) cancelMovementInducedCameraYRot = true;
        if (sprinting || player.is_wallrunning ){////|| hoverboard.isActive) {
            if (MyInput.gamepad_active || player.kb_hor_moves) {
                if (Mathf.Abs(convertedMoveX) > 0.1f) {
                    if (convertedMoveX > 0.1f) target_dy_rot = cam_input_x * yRot_speed_scaled * yRot_speedMul;
                    if (convertedMoveX < -0.1f) target_dy_rot = -cam_input_x * yRot_speed_scaled * yRot_speedMul;
                    if (cancelMovementInducedCameraYRot) target_dy_rot = 0;
                }
            } else {
                if (Mathf.Abs(cam_input_x) > 0.1f) {
                    target_dy_rot = cam_input_x * yRot_speed_scaled * yRot_speedMul;
                }
            }
        } else if (Mathf.Abs(cam_input_x) > 0.1f) {
            target_dy_rot = cam_input_x * yRot_speed_scaled * yRot_speedMul;
            target_dy_rot *= (used_sensitivity / 100f);
        } else if (Mathf.Abs(convertedMoveX) > joy_leftMovementThreshold) { // Slight camera rotation while moving horizontally with a controller
            yRot_speed_scaled = Mathf.Lerp(0, yRot_speed_scaled, (Mathf.Abs(convertedMoveX) - joy_leftMovementThreshold) / (1 - joy_leftMovementThreshold));
            if (convertedMoveX > 0) target_dy_rot = yRot_speed_scaled * yRot_speedMul * (.13f + (SaveManager.extraCamRotWithControllerMoveStrength/100f));
            if (convertedMoveX < 0) target_dy_rot = -yRot_speed_scaled * yRot_speedMul * (.13f + (SaveManager.extraCamRotWithControllerMoveStrength / 100f));
            target_dy_rot *= 1.3f;
            if (cancelMovementInducedCameraYRot) target_dy_rot = 0;
        }

        // when moving diagonally with kb, rotate camera slower or not at all
        if ((!MyInput.gamepad_active && !player.kb_hor_moves && !sprinting /*&& !hoverboard.isActive*/) && (MyInput.up || MyInput.down)) {
            target_dy_rot *= 0.85f;
        }

        dy_rot = Mathf.Lerp(dy_rot, target_dy_rot, dt * 6.5f);
        if (Mathf.Abs(cam_input_x) <= 0.2f) {
            dy_rot *= 0.875f;
        }
        if (t_face_cam_behind_player > 0) {
            dy_rot = 0;
        }


        if (sprinting) dy_rot = 0;
        target_yEuler += dy_rot;
        if (target_yEuler >= 360) target_yEuler -= 360;
        if (target_yEuler < 0) target_yEuler += 360;

        float this_frame_sprintAddY = 0;
        t_sincePlayerWasWallrunning += Time.deltaTime;
        // Conditoins under which camera should lerp current yEuler to face behind player. uses Clamp() to make it not go too fast.
        if (player.IsGliding() /*|| hoverboard.isActive*/ || (sprinting && !player.just_vaulted_but_didnt_land) ||  player.is_wallrunning || player.dash_mode == player.dash_mode_prismDash) {
            float _lerp = 0;
            float _d_yEuler = 0;
            if (player.is_wallrunning) {
                t_sincePlayerWasWallrunning = 0f;
                if (player.wallrun_side == 1) _lerp = Mathf.LerpAngle(target_yEuler, player.transform.localEulerAngles.y + wallrun_camera_tilt, Time.deltaTime * 4f);
                if (player.wallrun_side == -1) _lerp = Mathf.LerpAngle(target_yEuler, player.transform.localEulerAngles.y - wallrun_camera_tilt, Time.deltaTime * 4f);
                _d_yEuler = Mathf.Clamp(_lerp - target_yEuler, -3.5f, 3.5f);
            } else if (sprinting /*|| hoverboard.isActive*/) {
                if (t_sincePlayerWasWallrunning <= 1.5f) { // Wallrun > Mid-air - slow down the speed at which the camera moves back behind the player, to avoid the camera being pushed back by the wall the player jumped off of.
                    float str_reduction = Mathf.Lerp(10f, 1f, t_sincePlayerWasWallrunning / 1.5f);

                    _lerp = Mathf.LerpAngle(target_yEuler, player.transform.localEulerAngles.y, Time.deltaTime * sprintLerpStrength/str_reduction);
                } else {
                    // When sprinting and initially lerping to the player, slow down until within 2 degs of the player.
                    // Then follow as normal.
                    float user_reduction = SaveManager.sprint_camera_snapping_speed / 100f;
                    if (Mathf.Abs(player.transform.localEulerAngles.y - target_yEuler) < 2f)
                    {
                        sprint_angle_hasnt_reached_player = false;
                    }

                    if (!sprint_angle_hasnt_reached_player) user_reduction = 1;
                    this_frame_sprintAddY = 0;
                    if (MyInput.gamepad_active) {
                        //if (cam_input_x > 0.1f) this_frame_sprintAddY = Mathf.Lerp(0, sprintLookLimit, (cam_input_x - 0.1f) / 0.9f);
                        //if (cam_input_x < -0.1f) this_frame_sprintAddY = Mathf.Lerp(0, -sprintLookLimit, (-cam_input_x - 0.1f) / 0.9f);
                    }

                    if (!sprint_input_held && !sprinting) {
                        user_reduction = 0;
                    }
                    _lerp = Mathf.LerpAngle(target_yEuler, player.transform.localEulerAngles.y, Time.deltaTime * sprintLerpStrength*user_reduction);
                }
                
                _d_yEuler = Mathf.Clamp(_lerp - target_yEuler, -sprintMaxLerpDelta, sprintMaxLerpDelta);
            } else if (player.dash_mode == player.dash_mode_prismDash) {
                _lerp = Mathf.LerpAngle(target_yEuler, player.transform.localEulerAngles.y, Time.deltaTime * 3f);
                _d_yEuler = Mathf.Clamp(_lerp - target_yEuler, -4f, 4f);
            } else if (player.IsGliding()) {
                _lerp = Mathf.LerpAngle(target_yEuler, player.transform.localEulerAngles.y, Time.deltaTime * 6f);
                _d_yEuler = Mathf.Clamp(_lerp - target_yEuler, -4f, 4f);
            } else {
                _lerp = Mathf.LerpAngle(target_yEuler, player.transform.localEulerAngles.y, Time.deltaTime * 3f);
                _d_yEuler = Mathf.Clamp(_lerp - target_yEuler, -1.5f, 1.5f);
            }

            target_yEuler += _d_yEuler;
            // Snap to the exact yEuler behind the player, if current yEuler is close enough
            if (Mathf.Abs(target_yEuler - player.transform.localEulerAngles.y) < 0.1f) target_yEuler = player.transform.localEulerAngles.y;
        } 


        float utilized_yEuler = 0f;
        // Press the R-stick in to make the camera reset looking behind player.
        if (MyInput.recenterShortPress_KB && !playerCameraControlsOff) {
            tm_face_cam_behind_player = 0.55f;
            // make proportional to angle diff
            recenter_end_angle = target.transform.localEulerAngles.y;
            tm_face_cam_behind_player *= Vector3.Angle(transform.forward, target.transform.forward) / 180f;
            t_face_cam_behind_player = tm_face_cam_behind_player;
        }
        if (t_face_cam_behind_player > 0) {
            Quaternion q = Quaternion.LookRotation(transform.forward, transform.up);
            yEuler_lerpStart = q.eulerAngles.y;
            t_face_cam_behind_player -= Time.deltaTime;
            if (t_face_cam_behind_player <= 0) t_face_cam_behind_player = 0;
            float lerped_yEuler = 0;
            lerped_yEuler = Mathf.LerpAngle(yEuler_lerpStart, recenter_end_angle, Mathf.SmoothStep(0, 1, (tm_face_cam_behind_player - t_face_cam_behind_player) / tm_face_cam_behind_player));
            target_yEuler = lerped_yEuler;
        }

        /*/*
        if (player.inside_a_dash_prism && !player.inside_a_fixed_dash_prism) {
            dashPrismLook_cachedEuler = transform.localEulerAngles;
            transform.LookAt(DashPrism.activeDashPrismPos);
            float lookingAtDashPrism_yEuler = transform.localEulerAngles.y;
            transform.localEulerAngles = dashPrismLook_cachedEuler;
            target_yEuler = transform.localEulerAngles.y;
            float newEuler = Mathf.SmoothDampAngle(target_yEuler, lookingAtDashPrism_yEuler, ref dashPrismLookVel, Time.deltaTime);
            float limit = 4f;
            if (target_yEuler - newEuler > limit) {
                target_yEuler -= limit;
            } else if (target_yEuler - newEuler < -limit) {
                target_yEuler += limit;
            } else {
                target_yEuler = newEuler;
            }
        }*/

        //print(Mathf.Abs(target_yEuler - player.transform.localEulerAngles.y) +","+sprint_add_y);
        if (this_frame_sprintAddY == 0) {
            if (sprintAddY > 0) {
                sprintAddY -= Time.deltaTime * 2f * sprintAddY_speed;
                if (sprintAddY <= 0) sprintAddY = 0;
            } else if (sprintAddY < 0) {
                sprintAddY += Time.deltaTime * 2f * sprintAddY_speed;
                if (sprintAddY >= 0) sprintAddY = 0;
            }
            
            if (sprinting && !sprint_input_held) { // After releasing sprint, apply the sprintAddY to the current camera  rotation so that it doesnt' awkwardly lerp back
                target_yEuler += sprintAddY;
                sprintAddY = 0;
            }
        } else {
            if (sprintAddY < this_frame_sprintAddY) sprintAddY += Time.deltaTime * sprintAddY_speed;
            if (sprintAddY > this_frame_sprintAddY) sprintAddY -= Time.deltaTime * sprintAddY_speed;
            sprintAddY = Mathf.Clamp(sprintAddY, -sprintLookLimit, sprintLookLimit);
        }

        utilized_yEuler = target_yEuler + sprintAddY;


        // ---------
        // Now figure out the camera tilt (x-axis rotation) 
        // ---------
        float xOrbitAngleDelta = 0;
        float tilt_speed_dt = rotateSpeed * dt;

        if (MyInput.camY > 0.25f) xOrbitAngleDelta = tilt_speed_dt * verticalCamSpeedMultiplier * Mathf.Abs(MyInput.camY);
        if (MyInput.camY < -0.25f) xOrbitAngleDelta = -tilt_speed_dt* verticalCamSpeedMultiplier * Mathf.Abs(MyInput.camY);
        if (playerCameraControlsOff) xOrbitAngleDelta = 0;

        xOrbitAngleDelta *= (used_sensitivity / 100f);
        curXOrbitAngle += xOrbitAngleDelta;

        if (is_doing_slope_avoidance_rotation) {
            // Trick here is: rotation both happens in the camera controls and slope colAvoid
            // camera_hit... is set in the avoidance each frame. If it's not set in a frame, then there was no slope colAvoid - 
            // which lets us set the target_slope... and know that once that target's reached, the camera is sitting a little comfortably above the slope.
            if (camera_hit_slope_last_frame) {
                curXOrbitAngle += Time.deltaTime * halfSlopeAvoidRotateSpeed;
                target_slope_avoidance_xRot = curXOrbitAngle + 2f;
            } else {
                curXOrbitAngle += Time.deltaTime * halfSlopeAvoidRotateSpeed * 2f;
                if (curXOrbitAngle >= target_slope_avoidance_xRot || curXOrbitAngle >= upperVertAngleLimit) {
                    is_doing_slope_avoidance_rotation = false;
                }
            }
        }
        camera_hit_slope_last_frame = false;


        #region Camera Distance Adjustment
        if (curXOrbitAngle <= lowerVertAngleLimit) curXOrbitAngle = lowerVertAngleLimit;
        if (curXOrbitAngle >= upperVertAngleLimit)  curXOrbitAngle = upperVertAngleLimit;
        float camZoomSpeed = 100f;
        if (!playerCameraControlsOff) {
            if (camDisRestrict_mode == mode_disRestrict_unrestricted) {
                if (!cam_should_use_collider_modified_offsetLength) {
                    if (MyInput.zoomOut) {
                        SaveManager.cameraDistance += Time.deltaTime * camZoomSpeed;
                    } else if (MyInput.zoomIn) {
                        SaveManager.cameraDistance -= Time.deltaTime * camZoomSpeed;
                    }
                }
            }
        }
        SaveManager.cameraDistance = Mathf.Clamp(SaveManager.cameraDistance, Registry.minCamDisPercent, Registry.maxCamDisPercent);

        float utilizedCameraDistance_percentage = SaveManager.cameraDistance;
        float cameraRestrictionSpeed = 100f;
        if (camDisRestrict_mode == mode_disRestrict_whileLowering) {
            if (camDisPercentage_InDistanceRestrictor > restrictedCamDisPercentage) {
                camDisPercentage_InDistanceRestrictor -= Time.deltaTime * cameraRestrictionSpeed;
                if (camDisPercentage_InDistanceRestrictor < restrictedCamDisPercentage) {
                    camDisPercentage_InDistanceRestrictor = restrictedCamDisPercentage;
                }
            } else {
                camDisRestrict_mode = mode_disRestrict_restrictedAllowingZoom;
            }
        } else if (camDisRestrict_mode == mode_disRestrict_restrictedAllowingZoom) {
            if (!cam_should_use_collider_modified_offsetLength) {
                if (MyInput.zoomOut) {
                    camDisPercentage_InDistanceRestrictor += Time.deltaTime * camZoomSpeed;
                } else if (MyInput.zoomIn) {
                    camDisPercentage_InDistanceRestrictor -= Time.deltaTime * camZoomSpeed;
                }
            }
            camDisPercentage_InDistanceRestrictor = Mathf.Clamp(camDisPercentage_InDistanceRestrictor, Registry.minCamDisPercent, restrictedCamDisPercentage);
        } else if (camDisRestrict_mode == mode_disRestrict_whileExiting) {
            if (camDisPercentage_InDistanceRestrictor < SaveManager.cameraDistance) {
                camDisPercentage_InDistanceRestrictor += Time.deltaTime * cameraRestrictionSpeed;
                if (camDisPercentage_InDistanceRestrictor >= SaveManager.cameraDistance) {
                    camDisPercentage_InDistanceRestrictor = SaveManager.cameraDistance;
                }
            } else {
                camDisRestrict_mode = mode_disRestrict_unrestricted;
            }
        }
        if (camDisRestrict_mode != mode_disRestrict_unrestricted) {
            utilizedCameraDistance_percentage = camDisPercentage_InDistanceRestrictor;
        }

        if (cam_should_use_collider_modified_offsetLength) {
            utilizedCameraDistance_percentage = last_uncollided_cam_dis;
        }

        #endregion

        // If toggling camera angle - calculate the current offset (it gets reset a bit above this)
        if (resetCamOffsetToDefault) {
            SetOffsetBasedOnOffsetMode();
            lerpOffsetInitialVector = offset * utilizedCameraDistance_percentage/100f;
        }

        if (resetCamOffsetToDefault) {
            resetCamOffsetToDefault = false;
            offsetMode = 0;
            t_lerpOffset = tm_lerpOffset;
        }

        // Recalc the offset
        SetOffsetBasedOnOffsetMode();
        if (cam_should_use_collider_modified_offsetLength) {
            offset *= collider_modified_camera_distance/100f;
        } else {
            offset *= (utilizedCameraDistance_percentage / 100f);
        }

        // If camera angle was toggled, then start moving the camera to the next target offset
        if (t_lerpOffset > 0) {
            t_lerpOffset -= dt;
            lerpOffsetCurrentVector = Vector3.Lerp(lerpOffsetInitialVector, offset, Mathf.SmoothStep(0, 1, 1 - (t_lerpOffset / tm_lerpOffset)));
            offset = lerpOffsetCurrentVector;
        }

        float extraXTilt, xOrbitAngleForOffsetQuat;
        float xAngle_thresholdForPostOffsetRotation = -20; // Once the offset rotates beyond this, additional degrees will instead be a rotation applied to the local transform of the camera. (This avoids rotating into the ground)
        if (curXOrbitAngle > xAngle_thresholdForPostOffsetRotation) { // from -20 to 0, rotate towards the ground. Beyond -20, lock the offset rotation there and start to tilt camera
            xOrbitAngleForOffsetQuat = curXOrbitAngle;
            extraXTilt = 0; // cancel out the camera tilting
        } else {
            xOrbitAngleForOffsetQuat = xAngle_thresholdForPostOffsetRotation;
            extraXTilt = curXOrbitAngle - xAngle_thresholdForPostOffsetRotation;  // do this so the tilting starts from zero and goes higher (vs just starting at the cutoff)
        }


        // The euler rotation only has a minimum x of zero so the vector isn't rotated into the groun.
        // Afterwards, based on the current x orbit angle, the camera will be rotated higher
        Quaternion rotation = Quaternion.Euler(xOrbitAngleForOffsetQuat, utilized_yEuler, 0);
        // Move camera away from player and look at it.
        transform.position = targetPos - (rotation * offset);
        transform.LookAt(targetPos);
        tempCamPos = transform.localEulerAngles;
        tempCamPos.x += extraXTilt;
        transform.localEulerAngles = tempCamPos;


       
        // Use target's raw pos vs. targetpos, so the lerping targetpos during jumps wont get stuck in ground...
        player_to_cam_ray.origin = player_pos;
        player_to_cam_ray.direction = transform.position - player_pos;
        //Debug.DrawRay(ray.origin,10*ray.direction,Color.red);
        bool pushed_out_of_collider = false;

        float collisionCastBaseDistance = offset.magnitude;

        // Raycast to check if the camera is stuck in a collider.
        // NO: Lengthening the cast distance from just the offset length helps with issues where the camera is barely inside the wall so there's no hit detecton
        last_pushback_yOff = 0;
        if (Physics.Raycast (player_to_cam_ray,out playerToCamHit,collisionCastBaseDistance*1f,Registry.lmc_CAMERA_COLLIDABLES, QueryTriggerInteraction.Ignore)) {
            float slopeAngle = GetSlopeAngle(playerToCamHit.normal);

            cam_to_player_ray.origin = transform.position;
            cam_to_player_ray.direction = player_pos - transform.position;

            // Raycast for obstruction avoidance.
            // Layermask: Ignore inner faces of Terrain, otherwise game will think camera can be inside.
            Physics.Raycast(cam_to_player_ray, out RaycastHit nonoverlapping_obstruction_hit, collisionCastBaseDistance * 0.9f, Registry.lmc_CAMERA_COLLIDABLES ^ terrain_lm, QueryTriggerInteraction.Ignore);

            //Debug.DrawRay(ray.origin, offset.magnitude*0.9f*ray.direction, Color.red);

            // If there's an obstruction in between (but not overlapping) player and cam, don't move the cam.
            if (nonoverlapping_obstruction_hit.point != Vector3.zero && slopeAngle > 60f && !nonoverlapping_obstruction_hit.collider.CompareTag(Registry.tag_CameraMustAvoid)) {
                //Debug.DrawRay(obstructionCheck.point, Vector3.up*2f, Color.yellow);
                // Otherwise, the camera now needs to be moved out of the collider.
                pushed_out_of_collider = false;
            } else {
                // Gradually rotate upwards if stuck in slope
                if (slopeAngle > 5f && slopeAngle < 60f && playerToCamHit.normal.y > 0) {
                    curXOrbitAngle += Time.deltaTime * halfSlopeAvoidRotateSpeed;
                    is_doing_slope_avoidance_rotation = true;
                    camera_hit_slope_last_frame = true;
                } else {
                    PushbackFromCollider(playerToCamHit);
                }
                pushed_out_of_collider = true;
            }
        }

        if (pushed_out_of_collider) {
            collided_last_frame = true;
        }
        
        // Add in the moving average of the y offset that prevents the camera being pushed down into the player.
        update_ring_list(last_pushback_yOff,ref pushback_yOff_index,ringBuf_maxSize,pushback_yOff_floats);
        tempPosForCollision = transform.position;
        float yOffMA = GetListAverage(pushback_yOff_floats);
        if (Mathf.Abs(yOffMA) < Mathf.Abs(last_pushback_yOff)) {
            tempPosForCollision.y -= last_pushback_yOff;
        } else {
            tempPosForCollision.y -= GetListAverage(pushback_yOff_floats);
        }
        transform.position = tempPosForCollision;

        
        
        tempPosForCollision = transform.position - player_pos; 
        transform.position = player_pos + tempPosForCollision.normalized * (tempPosForCollision.magnitude - 1f);

        // post near-plane cast will have a distance from the player. Call this D.
        // add that distance to a ring buffer of positions for a moving average - MA
        // FD refers to the final distance of the camera.
        // If MA > D, then  set FD to D, because this means the current wall is CLOSER than MA, so if you were to set FD to something bigger than D, then the camera might go into the wall.
        // However, if MA <= D, set FD to MA. This happens when the camera un-collides with the wall, so gradually its distance from the player increases. There's no issue with "Lerping" into a wall, because only the DISTANCE of the vector is changing.
        
        float cur_d = Vector3.Distance(transform.position, player_pos);
        update_ring_list(cur_d,ref postCol_ds_ringIndex,ringBuf_maxSize,postCol_ds);
        float ma = GetListAverage(postCol_ds);
        // Snap to the closer distance. 
        if (ma > cur_d) {
            
        } else {
            Vector3 norm_offset = (transform.position - player_pos).normalized;
            transform.position = player_pos + norm_offset * ma;
        }
        
        
        // Offset based on the nearplane. Then keep a moving average of how far the nearplane raycast goes - and use that to smooth similar to above, smooth out how the camera transitions out of nearplane collisions.
        // nearplane cast - Center now starts at the other side of the casting direction. basically makes less of an issue of clipping
        // Viewport values:
        // y 1|
        //    0--1 x
        bool movedByNearPlane = false;
        nearplane_offset = Vector3.zero;
        bool hitWallOnLeft = false;
        bool hitWallOnRight = false;
        if (NearplaneCastAndMove(1f, 0.5f, 1f - nearplaneCastDistance, 0.5f, cam)) {// right to left
            hitWallOnLeft = true;
            movedByNearPlane = true;
        } 
        if (!movedByNearPlane) {
            movedByNearPlane = NearplaneCastAndMove(0, 0.5f, nearplaneCastDistance, 0.5f, cam); // left to right
            if (movedByNearPlane) {
                hitWallOnRight = true;
            }
        }
        if (!movedByNearPlane) movedByNearPlane = NearplaneCastAndMove(0.5f, 1f, 0.5f, 1-nearplaneCastDistance, cam); // top to bottom
        if (!movedByNearPlane) movedByNearPlane = NearplaneCastAndMove(0.5f,0, 0.5f, nearplaneCastDistance, cam); // bottom to top
        if (movedByNearPlane) {
            last_nonzero_nearplane_offset = nearplane_offset;
        }
        update_ring_list(nearplane_offset.magnitude,ref nearplane_offset_ds_ringIndex,ringBuf_maxSize,nearplane_offset_ds);
        float nearplane_ma = GetListAverage(nearplane_offset_ds);
        // Opposite of above: If the MA of the shift is > cur shift - then the nearplane needs to shift by more (i.e. the MA) to smooth the transition.
        // if MA is <= shift, then the nearplane is closer to a collision, so the nearplane must snap to the new shift. 
        if (nearplane_ma > nearplane_offset.magnitude) {
            transform.position += nearplane_offset.normalized*nearplane_ma; // Move the full offset.
        } else {
            // When moving out of a collision, use the previous shift offset to smooth returning to a shift of zero.
            if (!movedByNearPlane) {
                transform.position += last_nonzero_nearplane_offset.normalized * nearplane_ma;
            } else {
                transform.position += nearplane_offset;
            }
        }
        if (nearplane_ma < 0.01f) {
            last_nonzero_nearplane_offset = Vector3.zero;
        }

        // Subtle polish: If moving, and the nearplane is colliding, then rotate the camera slowly so that the nearplane does not collide.
        if (Mathf.Abs(MyInput.moveY) > 0.5f) {
            if (hitWallOnLeft) {
                target_yEuler -= Time.deltaTime * 30f;
            } else if (hitWallOnRight) {
                target_yEuler += Time.deltaTime * 30f;
            }
        }
        

        
        if (lerpCamPos) {
            float _t = Mathf.SmoothStep(0,1,triggerTransitionTime/maxTTT);
            triggerTransitionTime += dt;
            if (triggerTransitionTime > maxTTT) {
                lerpCamPos = false;
            }
            // Need y rotation to lerp correctly
            Vector3 eulerLerp = Vector3.Lerp(STARTrotFromTrigger,transform.eulerAngles,_t);
            eulerLerp.x = Mathf.LerpAngle(STARTrotFromTrigger.x,transform.eulerAngles.x,_t);
            eulerLerp.y = Mathf.LerpAngle(STARTrotFromTrigger.y,transform.eulerAngles.y,_t);
            transform.rotation = Quaternion.Euler(eulerLerp);
            transform.position = Vector3.Lerp(STARTposFromTrigger,transform.position,_t);
        }


        if (shakeon > 0) {
            if (SaveManager.screenshake_flash_on == false) {
                shakeon = 0;
            } else {
                shakeon -= 3 * Time.unscaledDeltaTime;
                transform.position += Vector3.one * 0.1f * Random.value * shakeon;
            }
        }

        if (!playerCameraControlsOff) {
            // Vectors for comparing xz positions
            temp_dither_vec = transform.position;
            temp_dither_vec.y = 0;
            temp_dither_vec_player = player.transform.position;
            temp_dither_vec_player.y = 0;
            

            // print("---");
            // print("y: "+Mathf.Abs(player.transform.position.y - (transform.position.y - 1)));
            // print("xz: "+Vector3.Distance(temp_dither_vec, temp_dither_vec_player));
            // print(ditherPlayerMode+","+player_dither_strength);
            
            // Dithering starts when within a cylinder around the player
            if (ditherPlayerMode == 0) {
                // -1 from player y-pos to account for non-mesh-centered pivot
                if (Mathf.Abs(player.transform.position.y - (transform.position.y -1))  < y_dither_startEnd.x &&
                    Vector3.Distance(temp_dither_vec, temp_dither_vec_player) < xz_dither_startEnd.x) {
                    ditherPlayerMode = 1;
                }
            // Modify a global shadergraph variable that controls dither strength for player materials.
            } else if (ditherPlayerMode == 1) {
                player_dither_strength += Time.deltaTime * 2.3f;
                if (player_dither_strength > 1f) {
                    player_dither_strength = 1f;
                    ditherPlayerMode = 2;
                }
                Shader.SetGlobalFloat("_PlayerDitherStrength",player_dither_strength);
            // Dithering ends when exiting the cylinder around the player
            } else if (ditherPlayerMode == 2) {
                if (Mathf.Abs(player.transform.position.y - (transform.position.y-1))  > y_dither_startEnd.y ||
                    Vector3.Distance(temp_dither_vec, temp_dither_vec_player) > xz_dither_startEnd.y) {
                    ditherPlayerMode = 3;
                }
            } else if (ditherPlayerMode == 3) {
                if (SaveManager.dithering_on) {
                    player_dither_strength -= Time.deltaTime * 2.3f;
                } else {
                    player_dither_strength -= Time.deltaTime * 20.3f;
                }

                if (player_dither_strength < 0) {
                    player_dither_strength = 0;
                    ditherPlayerMode = 0;
                }
                Shader.SetGlobalFloat("_PlayerDitherStrength",player_dither_strength);
            }

        }
    }

    private float player_dither_strength = 0;
    private int ditherPlayerMode = 0;
    Vector3 temp_dither_vec;
    Vector3 temp_dither_vec_player;
    Vector2 xz_dither_startEnd = new Vector2(1.6f,1.8f);
    Vector2 y_dither_startEnd = new Vector2(1.6f,2f);
    
    private Vector3 tempPosForCollision;

    private float GetSlopeAngle(Vector3 n) {
        normalNoY = n; normalNoY.y = 0;
        float slopeAngle = 0f;
        if (normalNoY.magnitude > 0.05f) {
            slopeAngle = 90f - Vector3.Angle(playerToCamHit.normal, normalNoY);
        }
        return slopeAngle;
    }

    Vector3 temp_pushback_pos = new Vector3();
    List<float> pushback_yOff_floats = new List<float>();
    private int pushback_yOff_index;
    private float last_pushback_yOff;
    
    private void PushbackFromCollider(RaycastHit hit) {
        Vector3 diff = hit.point - player_pos; // vector between player and hit
        //temp_pushback_pos = player_pos + diff*0.77f; // Move the camera position a little away frmo the collision to help with avoiding clipping
        temp_pushback_pos = player_pos + diff;
        //temp_pushback_pos += hit.normal * 0.8f; // Move away from the hit surface a little
        //temp_pushback_pos.y = transform.position.y; // preserve the original y position, so the camera doesn't move downwards into the player as much.
        last_pushback_yOff =  temp_pushback_pos.y - transform.position.y; // how much the camera is being pushed away from its original y position 
        transform.position = temp_pushback_pos;
    }

    private void SetOffsetBasedOnOffsetMode() {
        if (MyInput.gamepad_active) {
            offsetMode = 0;
            offset = mediumOffset;
        } else {
            if (offsetMode == 0) offset = mediumOffset;
            if (offsetMode == 1) offset = mediumOffsetFar;
            if (offsetMode == 2) offset = mediumOffsetUp;
            if (offsetMode == 3) offset = mediumOffsetTopDown;
        }
    }

    private Vector3 nearplane_offset;
    
    public float nearplaneCastDistance = 1.3f;
    bool NearplaneCastAndMove(float originX, float originY, float destx,float desty, Camera mainCamera) {
        // Center now starts at the other side of the casting direction. basically makes less of an issue of clipping
        tempPlaneV.Set(originX, originY, mainCamera.nearClipPlane);
        planeCastStart = mainCamera.ViewportToWorldPoint(tempPlaneV); // Border point (e.g. right)
        tempPlaneV.Set(destx, desty, mainCamera.nearClipPlane);
        planeCastDest = mainCamera.ViewportToWorldPoint(tempPlaneV); // a little bit outside the opposite border (e.g. left + 0.2f)
        tempRay.origin = planeCastStart;
        tempRay.direction = planeCastDest - planeCastStart;
        if (Physics.Raycast(tempRay, out tempHitInfo, Vector3.Distance(planeCastStart, planeCastDest), Registry.lmc_CAMERA_COLLIDABLES, QueryTriggerInteraction.Ignore)) {
            nearplane_offset = tempHitInfo.point - planeCastDest;
            return true;
        }
        return false;
    }


    internal int camDisRestrict_mode = 0;
    internal int mode_disRestrict_unrestricted = 0;
    internal int mode_disRestrict_whileLowering = 1;
    internal int mode_disRestrict_restrictedAllowingZoom = 2;
    internal int mode_disRestrict_whileExiting = 3;
    internal float restrictedCamDisPercentage = 5f;
    internal float camDisPercentage_InDistanceRestrictor = 10f;

    bool resetCamOffsetToDefault = false;
    Vector3 normalNoY = new Vector3();
    Ray tempRay = new Ray();
    //SRay aboveSlope_to_cam_ray = new Ray();
    Ray player_to_cam_ray = new Ray();
    Ray cam_to_player_ray = new Ray();
    bool is_doing_slope_avoidance_rotation = false;
    float halfSlopeAvoidRotateSpeed = 30f;
    bool camera_hit_slope_last_frame = false;
    float target_slope_avoidance_xRot = 0;
    RaycastHit tempHitInfo = new RaycastHit();
    RaycastHit playerToCamHit = new RaycastHit();
    //Vector3 last_pos_without_being_in_collider = new Vector3();
    Vector3 tempPlaneV = new Vector3();
    Vector3 planeCastStart = new Vector3();
    Vector3 planeCastDest = new Vector3();
    //Vector3 last_pos_while_pushed_out_of_collider = new Vector3();
    //Vector3 lastPosAdjustedByNearPlane = new Vector3();
    private bool collided_last_frame;

    bool cam_should_use_collider_modified_offsetLength = false; // Disables zooming if true
    float collider_modified_camera_distance = -1f;
    float last_uncollided_cam_dis = -1f;


    // Shifts the player-to-cam vector to left and right to check for collisions
    bool playerToCamIsClearOnCenterLeftAndRight() {
        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
        Vector3 rightMid = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, cam.nearClipPlane));
        Vector3 rightOffset = rightMid - center;
        Ray ray = new Ray(targetPos+rightOffset, transform.position - targetPos);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, offset.magnitude, Registry.lmc_CAMERA_COLLIDABLES, QueryTriggerInteraction.Ignore)) {
            return false;
        }
        ray.origin = targetPos - rightOffset;
        if (Physics.Raycast(ray, out hit, offset.magnitude, Registry.lmc_CAMERA_COLLIDABLES, QueryTriggerInteraction.Ignore)) {
            return false;
        }
        return true;
    }

    public void setCameraNextSceneEulerY(float nextY) {
        initialCameraRotation = nextY;
    }

    public void setCameraEulerYRotation(float newY) {
        target_yEuler = newY;
    }


    Vector3 jumpCamTargetPos = new Vector3(0, 0, 0);
    bool inJumpCam = false;
    int jumpCamMode = 0;
    float timeJumpHeld = 0f;

    public float jumpCameraLookYOff = 2f;
    float jumpCameraLookYOff_Current = 0;
    public float timerLerpToJumpCamera = 0.7f;

    float t_lerpOffset; // Timer for moving between offsets in medium mode
    float tm_lerpOffset = 0.4f;
    Vector3 lerpOffsetInitialVector = new Vector3();
    Vector3 lerpOffsetCurrentVector;

    public float t_xzCamLag = 0;

    float t_JumpReturn = 0;
    public float tm_JumpReturn = 0.4f;
    public float jumpDeadYMargin = 3.5f;
    float jumpDeadStartY = 0f;

    public float finalYOffset = 2.5f;

    float dy_rot = 0;
    internal float t_face_cam_behind_player = 0;
    float tm_face_cam_behind_player = 0;
    float recenter_end_angle = 0;
    private bool camera_pan_clamped_to_player_during_climbing;
    public void startJumpCamMode() {
        // If the player touches the ground but the camera doesn't finish its 'return to normal' lerping, and
        // the player jumps again, then return back to the 'follow beneath the player mode.

        jumpDeadStartY = target.transform.position.y;
        jumpCamTargetPos = targetPos;
        inJumpCam = true;
        jumpCamMode = 0;
    }
    public void stopJumpCamMode() {
        jumpCamMode = 2;
        t_JumpReturn = 0;
    }

    float FOV_onEnterCinemachine = 80;
    public void enterCinemachineMovieMode() {
        if (!enabled) return;
        // GameObject.Find("vcam Main Imitator").GetComponent<MatchCMCamToMainCam>().enabled = false;
        FOV_onEnterCinemachine = cam.fieldOfView;
        ////GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
        inCinemachineMovieMode = true;
    }

    public void exitCinemachineMovieMode() {
        if (!enabled) return;
        if (inCinemachineMovieMode) {
            cam.fieldOfView = FOV_onEnterCinemachine;
        }
        ////GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
        inCinemachineMovieMode = false;
    }

    bool paused_for_scanning = false;
    private float dashPrismLookVel;
    private Vector3 dashPrismLook_cachedEuler;
    private float jumpLookVel;

    public float jumpLookSmoothTime = 1.5f;

    public float jumpTargetLerpSmoothtime = 1.5f;
    private Vector3 jumpCamTargetPosVel;
    private float t_sincePlayerWasWallrunning;

    public void Toggle_pause_for_scanning(bool on) {
        paused_for_scanning = on;
    }

    int ringBuf_maxSize = 20;
    private int postCol_ds_ringIndex = 0;
    private int nearplane_offset_ds_ringIndex = 0;
    private List<float> postCol_ds = new List<float>();
    private List<float> nearplane_offset_ds = new List<float>();
    private Vector3 last_nonzero_nearplane_offset;


    // void update_ring_list(Vector3 v, ref int index, int maxSize, List<Vector3> l) {
    //     if (l.Count < maxSize) {
    //         l.Add(new Vector3(v.x,v.y,v.z));
    //     } else {
    //         l[index].Set(v.x,v.y,v.z);
    //         index = (index + 1) % maxSize;
    //     }
    //}
    
    void update_ring_list(float f, ref int index, int maxSize, List<float> l) {
        if (l.Count < maxSize) {
            l.Add(f);
        } else {
            l[index] = f;
            index = (index + 1) % maxSize;
        }
    }

    
    float GetListAverage(List<float> l) {
        float f = 0;
        for (int i = 0; i < l.Count; i++) {
            f += l[i];
        }

        return f / l.Count;
    }

    internal bool freeze_targetpos_in_link;
    public void Reset_targetPos() {
        targetPos = target.transform.position;
        nextTargetPos = jumpCamTargetPos = targetPos_at_dashStart = targetPos_at_wallrunStart = targetPos;
    }
    //// Deleteed from 1260
    //// Deleted to 1987
}
