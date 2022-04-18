//// using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
//// using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class MyPlayer3D : MonoBehaviour {
    #region Declarations

    private bool anim_wr_hold_from_wall;
    internal bool test_3d_mode = false;
    private bool havent_landed_from_consec_wallrun;
    private float t_sprint_delay;

    private float t_superdash_timeout;
    private float t_return_to_checkpoint;
    private bool hasdirlightfordebug;
    public static bool falling_oob;
    ////internal PlayerVFXHelper vfx;
    ////internal GameObject emission_helper_GO;
    public bool forcePausingFromMyEvent = false;
    internal bool pause_screenshot = false;
    internal MyCamera cam;
    ParticleSystem SplashParticles;
    ParticleSystem WaterRingParticles;
    ParticleSystem HoverParticles;
    int nojumpticks = 30;
    QueryTriggerInteraction qti_ignore;
    ////internal TMP_Text player_debug_text;
    private bool in_postgame;
    private float t_speedpodglow_sfx;
    private float t_poisonshroom_sfx;

    internal bool force_respawn_pos_once;
    internal Vector3 forced_respawn_pos;

    [Header("Regular Properties")] [System.NonSerialized]
    public bool CanShootSpark;

    ////protected UIManagerAno2 ui;
    internal Rigidbody rb;
    internal Vector3 sceneStartPos;
    internal Vector3 mostRecentDoorEntryPos;
    ////DialogueBox dbox;
    CapsuleCollider cc;
    internal Animator animator;
    GameObject fixedHelperTransform;
    ////public GameObject tetherObjectPrefab;
    ////GameObject growingTetherObject;
    ////GameObject placedTetherObject;
    float t_placingTether;
    float t_returnToTether;

    Vector3 tetherScale = new Vector3();
    Vector3 returnToTether_scale = new Vector3();
    Vector3 tetherReturnPosition;
    Vector3 activeModel_originalScale;

    float forceFixedCamEulerY;
    bool forceFixedCamEulerYTillNotPressingDirection;
    public static float next_eulerY = -1;

    bool holdingForward;
    bool holdingBackward;
    bool holdingAnyMovement;

    internal bool OnMovingPlatform = false;

    internal Transform activeModel;

    public static bool ignore_input;

    [Header("Poison Shroom")] public float tm_noPoisonRes = 1f;
    public float tm_hasPoisonRes = 5f;
    float t_poisonShroom = 0;
    private float t_slowHealingAfterExitingPoison = 0; // to 2
    public bool hasPoisonResistance = false;
    private int poisonShroomsInside = 0;


    [Header("Wallrun Physics")] [Range(20f, 80f)]
    public float min_wallrun_approach_angle = 35f;

    [Range(5f, 25f)] public float wallrun_approach_ang_adjust_from_hor_input = 13f;
    float max_wallrun_approach_angle = 90f;
    [Range(0.5f, 20f)] public float allowed_wall_slope_variance = 5f;
    float current_wallrun_speed = 10f;

    [Tooltip(
        "If a wallrun starts, then from approach_angle to (approach_angle-THIS), start vel will range from BASE_SPEED to FALLOFF_SPEED")]
    public float wallrun_speedRampingStartAngleDiff = 8f;

    public float wallrun_decel = 5f;
    public float wallrun_falloff_speed = 2f;
    public float min_wallrun_base_speed = 6f;
    [Range(6f, 15f)] public float base_wallrun_speed = 10f;
    float wallRunJump_xzVel_booster = 1f;
    public float max_wallRunJump_xzVel_booster = 1.5f;

    bool just_wallrun_jumped_and_not_on_ground;
    internal int wallrun_side = 1; // 1 = R, -1 = L
    public float max_wallrun_interior_turn_angle = 60f;
    public float max_wallrun_exterior_turn_angle = 20f;
    float t_wallrunIgnoreGround;
    float tm_wallrunIgnoreGround = 0.7f;

    float last_wallrun_turn_angle_signed;
    internal bool is_wallrunning;
    private bool magnet_wallrunning;
    private Material magnet_wall_mat;
    private Material magnet_wall_mat2;
    private Color magnet_wall_emission;
    private float t_magnet_wall_glow;
    Vector3 temp_wallrun_dir;
    Vector3 temp_facing_euler;
    [Range(0.5f, 2f)] public float wallrun_raycast_lookahead_mul = 0.8f; // relative to radius
    [Range(0.5f, 3f)] public float wallrun_raycast_distance = 1f;
    public float max_wallrun_fall_speed = 7;
    public float wallrun_gravity = 15f;

    [Range(0.2f, 1f)]
    [Tooltip("Percentage of default jump speed at which the upwards velocity of the start of a wall run will be capped.")]
    public float
        max_default_upward_entry_wallrun_speed_mul = 0.7f; // motivation - lets you have low gravity without flying up too high

    // outer curves, inner curves.
    public Vector2 wallrun_outInCurve_FallVelMul = new Vector2(0.85f, 1.15f);
    public float walljump_pushoff_speed = 12.7f;
    public float tm_walljump_pushoff = 0.155f;
    Vector3 last_wallrun_vel_awayPerp;
    Vector3 temp_walljump_pushoff_vec;
    float t_walljump_pushoff;

    bool trigger_jumpstate_enter_wallrun;
    bool trigger_jumpstate_wallrunning__fallOff;
    bool fell_off_wallrun_from_fwdOb;
    float shifting_last_wallrun_turnAngle;
    float t_consec_wallrun_cooldown;
    float t_consec_wallrun_sameside_cooldown;
    int last_wallrun_side;

    [Header("Walk Physics")] public float max_walk_speed = 8f;
    internal bool kb_hor_moves = true;
    internal bool walk_turn_smoothing_on = true;
    [Range(1f, 30f)] public float walk_turning_quickness = 15f;

    float ice_tq = 2.33f;
    [Range(1f, 30f)] public float walk_to_stop_decel_strength = 2f;

    float ice_wtsds = 0.33f;
    [Range(0, 3f)] public float tm_walk_accel_time = 0.24f;
    float t_walk_accel;
    internal Vector3 pausedRBPos;
    private Vector3 pausedRBVel;
    bool wasPausedFixed;
    private bool gravityTurnedOffWhenPaused;
    float horCutoff = 0.1f;
    float outofboundsYVal = -150f;
    internal int outofBoundsMode;
    internal bool in_mud__resetInFixedUpdate;
    private bool was_in_mud_last_FixedUpdate;
    internal bool in_mud__resetByMud;
    private float t_stop_y_vel_when_not_moving_on_slope;
    public float mud_walk_mul = .77f;
    public float mud_sprint_mul = 0.8f;
    public float mud_jump_mul = 0.4f;

    internal static Vector3[] positionBuf = new Vector3[8];
    float positionBufferUpdateInterval = 0.5f;
    float t_updatePositionBuffer;
    internal static int recentPosBufIdx;
    float outOfBoundsWait;

    Vector3 temp_teledash_scale;

    Vector3 dash_shrink_scale_start;

    //Vector3 dash_shrink_scale_end = new Vector3(0.001f, 0.001f, 0.001f);
    Vector3 dash_shrink_scale_end = new Vector3(1, 1, 1);
    float t_dash_windup;

    [Header("Teledash & Sprint Physics")] [Range(0f, 0.5f)]
    public float tm_dash_windup = 0.1f;

    internal float t_dash_cooldown;
    bool my_jp_dash;
    private bool my_jp_sprint;
    private float t_time_since_jp_dash = 0f;
    float tm_dash_cooldown = 0.15f;

    [Tooltip("Casting distance used for regular warping")]
    public float dash_distance_when_nothing_hit = 3f;

    [Tooltip("Casting distance used for dash-interactable objects (walls, switches)")]
    public float dash_magnet_distance = 6f;

    public float dashPod_mul = 2f;
    [Range(0.5f, 1.5f)] public float teledash_max_height_boost = 1f;
    [Range(1.1f, 3f)] public float sprint_speed_multiplier = 2f;
    float sprint_time_to_accel;
    public float MAX_sprint_time_to_accel = 0.5f; // How long it takes to reach the full speed multiplier sprinting gives you

    public float
        sprint_rotVelMul_at_start =
            2f; // Flat multiplier on the player trajectory rotational velocity delta, active during the sprint acceleration defined in MAX_sprint_time_to_accel

    Vector3 localEuler;
    bool jumpedWhileSprinting;
    internal bool is_sprinting_speed_on;
    public float sprint_maxRotVel = 70f;
    float sprint_rotVel;
    public float sprint_rotAccel = 140f;
    float t_sprint_release_to_drag;
    bool buffered_jump_during_teledash;
    float t_sprint_holdingBackInAir;


    [Header("Grapple")] bool grapple_ability_enabled = true;
    public float grapple_indicator_visible_distance = 60f;
    public float grapple_indicator_usable_distance = 50f;
    public float grapple_speed = 20f;
    public float same_grapple_cooldown = 1.1f;
    int grapple_submode;
    float t_grapplePause;
    Vector3 locked_grapple_pos;
    ////internal GrapplePoint closest_grapple_point = null;
    ////GrapplePoint locked_grapple_point;
    bool is_grappling;


    [Header("Ledge Vault")] [Range(0f, 20f)] [Tooltip("Velocity during a vault at which point the flip will start")]
    public float vaultFlipThreshold = 15f;

    bool flippedDuringVault;

    [Range(0f, 20f)] [Tooltip("Velocity during a vault at which point the flip will start")]
    public float grappleFlipThreshold = 15f;

    bool queueGrappleFlip;


    [Range(1f, 3f)]
    // How fast the jump is (relative to defalt jump speed)
    public float vault_jumpVel_multiplier = 1.5f;

    // How fast forward movement is after a vault (applies only if locked jump traj on). Relative to max walk speed
    [Range(0.5f, 2f)] public float vault_forwardSpeed_multiplier = 1f;

    // How long to pause in the wall collision position after a teledash
    [Range(0f, 0.5f)] public float pause_before_vault = 0.2f;

    float t_pause_before_vault;

    // Maximum angle of -1*forward and wall normal to activate ledge vault.
    [Range(30f, 90f)] public float max_vault_angle_from_normal = 60f;
    internal bool just_vaulted_but_didnt_land;
    public float sprint_vault_yVel_mul = 1.1f;
    public float sprint_vault_fwdVel_mul = 0.25f;


    internal int dash_mode;
    internal int dash_mode_inactive = 0;
    internal int dash_mode_windup = 1;
    int dash_mode_teleport = 2;
    int dash_mode_vault_wait = 3;
    private RaycastHit dash_sweep_hit = new RaycastHit();
    internal int dash_mode_prismDash = 4;
    internal bool inside_a_dash_prism = false;
    internal bool inside_a_fixed_dash_prism = false;
    internal Vector3 dash_prism_extSet_startPos;
    Vector3 dash_prism_fixedStartPos;
    internal float dash_prism_max_distance = 100f;
    public float speedPod_dashPrism_distance = 25f;
    public float dash_prism_speed = 33f;
    Vector3 prism_dash_vel;
    internal bool used_dash;
    internal float t_just_hit_wall_in_dashPrism = 0.25f;
    int try_post_superdash_fx = 0;
    internal bool last_superdash_didnt_hit_breakable = true;
        


    Vector3 dash_offset;
    bool stop_at_dash_start = true;


    //[Header("Jump Abilities")]
    internal bool lock_jump_trajectory = false;
    internal bool hold_hover_enabled = false;
    internal bool double_jump_enabled = false;
    Vector3 locked_jump_trajectory_vel_xz;
    bool jump_trajectory_locked;

    #region gliding vars

    internal bool fancy_glide_beginning_on = false;
    internal bool strafe_glide_on = false;
    internal bool has_rotate_after_glideCancel_ability = false;
    internal bool rotation_after_glideCancel_enabled;
    internal float strafe_glide_speed = 10.1f;
    internal float glide_takeoffAccel = 10f;
    float cur_glide_speed;
    internal float glide_initMomentumReducer = 0.5f;
    [Header("Gliding Physics")] public float glide_rotationalAccel = 170f;
    public float glide_maxRotVel = 20f;
    internal float glide_rotationalVel;

    public float glide_stabilizedFall_vel = -3f;

    //[Tooltip("The fall speed that must be attained during a glide before the velocity will start to approach glide_fall_vel.")]
    public float glide_beginToStabilize_velThreshold = -14f;

    //[Tooltip("The rate at which the fall speed during a glide will approach glide_fall_vel.")]
    public float glide_fallSpeedStableRate = 1f;
    public float glide_init_smoothDampSpeed = 0.3f;
    bool trig_js_midairToGlide;
    bool is_moving_to_glide_start_point;
    Vector3 glide_start_point;
    Vector3 glide_init_entry_vel;
    float t_glideOrbTime;


    internal int jump_state_ground = 0;
    internal int jump_state_midair = 1;
    int jump_state_gliding = 101;
    int jumpstate_vine = 102;
    internal int jumpstate_wallrunning = 103;
    int jumpstate_dashprisming = 104;
    int jumpstate_grappling = 105;
    int jumpstate_glideInit = 106;
    internal float tm_glideCancelCooldown = 0.25f;
    Vector3 temp_glideDir_vec;
    float t_glideCancelCooldown;
    Vector3 faceDir_at_glideStart;
    internal float base_glide_speed = 10f;

    #endregion

    int jump_state;
    [Header("Jump Physics")] public float maxMedFallVel = 25f;
    public float jump_initial_vel = 14f;
    public float jump_height = 2.5f;
    private float enter_firstJumpArc_yVelRatioToNormal = 1;
    [Range(1f, 10f)] public float whitePod_jumpVelMul = 1.4f;
    public bool fall_arc_has_xz_slowdown = true;
    bool trigger_vault_jump;

    [Tooltip("The percentage of the maximum XZ velocity that XZ-velocity will decay to (while falling)")]
    public float fall_arc_slowdown_xc_vel_multiplier = 0.8f;

    public float tm_fall_arc_slowdown = 1f;
    float t_fall_arc_slowdown;
    float t_fall_arc_slowdown_wo_moveInput;
    public float tm_delay_after_jump_input = 0.09f;
    float t_delay_after_jump_input;
    float FallDampenValue = 0.25f;
    Vector3 nextJumpVel = new Vector3(0, 0, 0);

    public int Get_jump_state() {
        return jump_state;
    }

    float
        ignoreJumpLanding_Counter; // Mostly used for things that need to change player jumpstate to first arc without touching the ground

    private float t_time_not_on_ground = 0;
    float
        t_time_since_last_jump_input =
            1; // Used for buffering tricks like jumping right before touchng ground. Init'd to 1 to avoid jumping when scene starts.

    Vector3 tempbounceVel;
    internal bool my_jp_jump; // Currently used in: jump from ground, glide, wallrun, bumper entity
    internal int t_fixedInt_justJumped = 0; // Set to 1 after jumping, goes to 0 the next physics step
    bool myJump;
    [Range(0.1f, 0.4f)] public float tm_fallJumpGrace = 0.2f; // Grace period of falling off a ledge and being able to jump still.
    float t_fallJumpGrace;

    bool
        in_firstJumpArc_via_fallingWithoutJump; // Set when falling from a ledge (AFTER the grace period). Reset on entering glide or hitting the ground.

    [Range(0, 1f)]
    public float
        fallWithoutJump_slowdown_xc_vel_multiplier =
            0.1f; // How much you slow down relative to trajectory speed, when falling off a ledge.

    public float tm_slowdown_during_a_fall_from_ledge = 0.7f; // how long it takes the above slowdown value to peak.
    Vector3 temp_noInput_fallArcVel;
    bool hasVariableJumpHeight;
    internal bool in_wind;
    internal bool in_sink_cloud;
    float sink_cloud_min_yVel;
    public float wind_yVel_mul = 1.56f; // 9 Reg jump, 18 vault jump. Slow fall.
    public float wind_upwardsGrav_mul = 0.9f;
    public float wind_downwardsGrav_mul = 0.24f;
    public float vault_jumpVel_multiplier_wind = 1.76f;


    // Hoverboard


    private float starting_model_y_offset = -0.81f;

    Transform ingwen_model;
    ////Transform riyou_model;
    ////Transform amy_model;

    static string MODEL_NAME_AMY = "amy";
    static string MODEL_NAME_RIYOU = "riyou";
    static string MODEL_NAME_INGWEN = "ingwen";
    public static string activePlayerModel = "riyou";

    int scene_initialization_mode;

    #endregion

    #region Awake and Start

    private float coreLavaTimer;
    private bool in_core;
    internal bool died_from_speedorb_or_poison;
    private bool inDungeontest;
    private void Awake() {
        in_scene_initialization_phase = true;
        qti_ignore = QueryTriggerInteraction.Ignore;
        curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (curSceneName == "DungeonTest_3D") {
            inDungeontest = true;
        }

        if (curSceneName == "Layer_4_Core") {
            in_core = true;
        }

        ingwen_model = transform.Find(MODEL_NAME_INGWEN);
        if (Registry.scenes_to_disable_cam_and_player.Contains(curSceneName)) {
            print("Scene is non-playable - disabling camera, player, etc.");
            gameObject.SetActive(false);
        }
    }

    internal string curSceneName = "";

    private Transform testlight;
    void Start() {
        ////if (DashPod.active_white_pods != null) DashPod.active_white_pods.Clear();
        ////SpeedPod.current_used_pods = 0;
        ////if (WhitePod.active_white_pods != null) WhitePod.active_white_pods.Clear();
        ////if (DM._getDS("sawtheending") == 1) {
        ////    in_postgame = true;
        ////}
        if (GameObject.Find("Directional Light") != null) {
            testlight = GameObject.Find("Directional Light").transform;
            hasdirlightfordebug = true;
        }
        ////ui = GameObject.Find("UI").GetComponent<UIManagerAno2>();
        ////player_debug_text = GameObject.Find("DebugText").GetComponent<TMP_Text>();
        ////vfx = GetComponentInChildren<PlayerVFXHelper>();
        ////if (!Registry.DEV_MODE_ON && DM._getDS("2-after-mindshare") == 0) { 
////            grapple_ability_enabled = false;
////        }

        ////if (DM._getDS("3r-mindshare") == 1) hasPoisonResistance = true;


        ColorUtility.TryParseHtmlString("#793D3B", out amy_dash_emission_color);
        ColorUtility.TryParseHtmlString("#793D3B", out gael_dash_emission_color);
        ColorUtility.TryParseHtmlString("#00646D", out ingwen_dash_emission_color);
        ColorUtility.TryParseHtmlString("#628E40", out riyou_dash_emission_color);
        ColorUtility.TryParseHtmlString("#7D936D", out whitePodJump_emission_color);
        
        //emission_helper_GO = transform.Find("Emission Helper").gameObject;
        
        cam = GameObject.Find("MyCamera").GetComponent<MyCamera>();
        rb = gameObject.GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
        ////growingTetherObject = Instantiate(tetherObjectPrefab, null) as GameObject;
////        growingTetherObject.transform.localScale = Vector3.zero;
////        placedTetherObject = Instantiate(tetherObjectPrefab, null) as GameObject;
////        placedTetherObject.transform.localScale = Vector3.zero;

        ingwen_model.gameObject.AddComponent<MyRootMotionHandler>();

        activeModel = ingwen_model; // avoid null errors
        if (SaveManager.active_character != "") {
            activePlayerModel = SaveManager.active_character;
        } 
//        print(activePlayerModel);
       // Registry.gael_playable_in_postgame = true;
        //in_postgame = true;
        //HF.Warning("Gael force enabled");
        SwitchActiveModel(activePlayerModel);
        play_restrict_message = false;


        ////dbox = HF.GetDialogueBox();
        cam = GameObject.Find("MyCamera").GetComponent<MyCamera>();
        fixedHelperTransform = new GameObject();
        fixedHelperTransform.name = name + "fixedHelperTransform";
        // temp WaterRingParticles = transform.Find("WaterRing").GetComponent<ParticleSystem>();
        // temp SplashParticles = transform.Find("Splash").GetComponent<ParticleSystem>();
        // temp doubleJumpParticles = Instantiate(PREFAB_doubleJumpParticles, transform).GetComponent<ParticleSystem>();

        if (Registry.destinationDoorName == "USESAVEPT") {
            transform.position = Registry.enterGameFromLoad_Position;
            Registry.destinationDoorName = "";
        }

        if (Registry.destinationDoorName == "USETETHERPOS") {
            transform.position = SaveManager.tetherPos;
            Registry.destinationDoorName = "";
        }

        ////if (DM._getDS("intro-surface-1") == 1 && DM._getDS("intro-heal-2") == 0) {
////            Toggle_TeledashWallrun(true);
////        }
        

        Registry.MoveObjectToDestinationDoor(gameObject);
        tempV1 = transform.localEulerAngles;
        tempV1.x = 0;
        tempV1.z = 0;
        transform.localEulerAngles = tempV1;


// Sanity Check for combos of returning, last checke 5/25/20
//x - entry from playmode entry
//x - entry, from scene entry
//x - tether, before any tethers
//x - tether, from same scene
//x - tether, from different scene
//x - entry, from respawn plane
//x - tether, from respawn plane

        bool usingepiloguepos = false;
        if (Registry.destinationDoorName == "debugstartpos") {
            print("In epilogue");
            usingepiloguepos = true;
        }
        if (Registry.destinationDoorName == "none") Registry.destinationDoorName = "";
        if (Registry.destinationDoorName != "") {
            GameObject lookGameObj = GameObject.Find(Registry.destinationDoorName);
            Transform lookTarget = null;
            if (lookGameObj != null) {
                lookTarget = lookGameObj.transform.Find("LookTarget");
            } else {
                print("No door: " + Registry.destinationDoorName);
            }

            if (lookTarget == null) {
                print("Warning, door " + Registry.destinationDoorName + " has no LookTarget");
            } else {
                Vector3 oldEu = transform.localEulerAngles;
                transform.LookAt(lookTarget);
                oldEu.y = transform.localEulerAngles.y;
                transform.localEulerAngles = oldEu;
                float nextMediumEulerY = transform.localEulerAngles.y;
                cam.setCameraEulerYRotation(nextMediumEulerY);
            }

            Registry.destinationDoorNameForPauseRespawn = Registry.destinationDoorName;
        } else {
            Registry.destinationDoorNameForPauseRespawn = Registry.GetDefaultSceneEntranceDoor(curSceneName);
        }

        Registry.destinationDoorName = "";

        if (next_eulerY != -1) {
            Vector3 eu = transform.localEulerAngles;
            eu.y = next_eulerY;
            transform.localEulerAngles = eu;
        }

        next_eulerY = -1;

        MyPlayer3D.ignore_input = false;

        if (Registry.justLoaded) {
            if (!usingepiloguepos) transform.position = Registry.enterGameFromLoad_Position;
//            print("----load");
 //           print(transform.position);
            Registry.justLoaded = false;
        }

        for (int i = 0; i < MyPlayer3D.positionBuf.Length; i++) {
            MyPlayer3D.positionBuf[i] = transform.position;
        }

        MyPlayer3D.recentPosBufIdx = 0;
        mostRecentDoorEntryPos = transform.position;
        
        Init_anim_events();


        ////if (DM._getDS("go-fast") == 1) {
////            go_fast();
////        }
    }


    public void go_fast() {
        maxMedFallVel = 33f;
        jump_initial_vel = 30f;
        jump_height = 10f;
        tm_walk_accel_time = 1f;
        max_walk_speed = 24f;
        base_wallrun_speed = 27f;
    }

    public void ungo_fast() {
        maxMedFallVel = 25f;
        jump_initial_vel = 16f;
        jump_height = 3.3f;
        tm_walk_accel_time = 0.33f;
        max_walk_speed = 8f;
        base_wallrun_speed = 10f;
    }

    #endregion

    internal bool force_respawn_from_RespawnPlane;
    internal bool force_respawn_from_death;
    internal bool pause_cam_after_death;
    internal bool force_respawn_to_entrance_from_PauseMenu;
    internal bool require_release_of_cancel_to_sprint; // In case cancel = sprint. Set from dbox and pause
    private bool superdashed_in_sd_zone_wo_hit_something;


    #region FixedUpdate - Movement controls

    private bool sprintDash_held;
    internal bool autosprinting;
    private int autosprint_mode;
    void FixedUpdate() {

             
        if (ignore_input) {
            return;
        }

        if (require_release_of_cancel_to_sprint) {
            if (is_sprinting_speed_on) {
                MyInput.sprintDash = MyInput.jp_sprintDash = false;
                Cancel_sprint_state_and_anim();
                
            }
            if (!MyInput.sprintDash) { // For exiting UI and not sprinting right away 
                require_release_of_cancel_to_sprint = false;
            }

            return;
        }        

        rb.velocity = previous_rb_vel_without_connected_rb;
        if (!isThereAnyReasonToPause()) {
            t_fixedInt_justJumped = 0;
            jumpLogic();
        }

        my_jp_jump = false;

        #region Out of Bounds and Poisoning

        if (outofBoundsMode == 0) {
            bool respawn = false;
            if (MyInput.return_to_checkpoint && !isThereAnyReasonToPause() && cam.ss_mode == 0 && jumpstate_grappling != jump_state && !Registry.DEV_MODE_ON) {
                t_return_to_checkpoint += Time.deltaTime;
                if (t_return_to_checkpoint > 0.33f) {
                    t_return_to_checkpoint = 0;
                    ////SoundPlayer.instance.enter_door();
                    respawn = true;
                }
            } else {
                t_return_to_checkpoint = 0;
            }
            if (transform.position.y < outofboundsYVal) {
                respawn = true;
                falling_oob = true;
                print("<color=red>Respawning due to default OOB Y-value - better to set a custom RespawnPlane</color>");
            }


            if (poisonShroomsInside > 0) {
                if (hasPoisonResistance) {
                    if (HF.TimerStayAtMax(ref t_poisonShroom, tm_hasPoisonRes, Time.fixedDeltaTime)) {
                        respawn = true;
                        ////SoundPlayer.instance.lava_die();
                        ////vfx.Poison_death(transform.position);
                        died_from_speedorb_or_poison = true;
                        pause_cam_after_death = true;
                    }
                } else {
                    if (HF.TimerStayAtMax(ref t_poisonShroom, tm_noPoisonRes, Time.fixedDeltaTime)) {
                        respawn = true;
                        ////SoundPlayer.instance.lava_die();
                        ////vfx.Poison_death(transform.position);
                        died_from_speedorb_or_poison = true;
                        pause_cam_after_death = true;
                    }
                }

                t_slowHealingAfterExitingPoison = 0;
            } else {
                // Healing should be much slower after leaving a poison shroom, to discourage constant hopping and healing.
                t_slowHealingAfterExitingPoison += Time.fixedDeltaTime;
                if (t_slowHealingAfterExitingPoison < 2) {
                    if (hasPoisonResistance) {
                        HF.TimerStayAtMin(ref t_poisonShroom, 0, Time.fixedDeltaTime / 50f * tm_hasPoisonRes);
                    } else {
                        HF.TimerStayAtMin(ref t_poisonShroom, 0, Time.fixedDeltaTime / 50f);
                    }
                } else {
                    if (hasPoisonResistance) {
                        HF.TimerStayAtMin(ref t_poisonShroom, 0, Time.fixedDeltaTime * tm_hasPoisonRes);
                    } else {
                        HF.TimerStayAtMin(ref t_poisonShroom, 0, Time.fixedDeltaTime);
                    }
                }
            }

            if (respawn || force_respawn_from_RespawnPlane || force_respawn_to_entrance_from_PauseMenu ||
                force_respawn_from_death) {
                if (force_respawn_from_death || force_respawn_from_RespawnPlane) {
                    falling_oob = true;
                }

                t_poisonShroom = 0;
                t_speedPodTime = 0;
                t_glideOrbTime = 0;
                ////DashPod.Clear_pods_after_fallout();
                ////WhitePod.Clear_pods_after_fallout();
                force_respawn_from_RespawnPlane = false;
                force_respawn_from_death = false;
                outofBoundsMode = 1;
                outOfBoundsWait = 0.1f;
                
                rb.velocity = Vector3.zero;
                previous_rb_vel_without_connected_rb = Vector3.zero;
                if (is_sprinting_speed_on) Cancel_sprint_state_and_anim();

                if ((in_core || died_from_speedorb_or_poison) && pause_cam_after_death) {
                    activeModel.gameObject.SetActive(false);
                } else {
                    
                    ////ui.StartFade(0.25f, true);
                }
            }
            ////} else if (outofBoundsMode == 1 && ui.fadeMode == 0) {
        } else if (outofBoundsMode == 1) {

            autosprinting = false;
            autosprint_mode = 1;
            if ((in_core || died_from_speedorb_or_poison) && coreLavaTimer < 1f && pause_cam_after_death) {
                coreLavaTimer += Time.fixedDeltaTime;
                if (coreLavaTimer >= 1) {
                    ////ui.StartFade(0.25f, true);
                }

                return;
            }
            
            if (outOfBoundsWait < 0) {
                ////ui.StartFade(0.2f, false);
                outofBoundsMode = 0;
                
                if ((in_core || died_from_speedorb_or_poison) && pause_cam_after_death) {
                    pause_cam_after_death = false;
                    activeModel.gameObject.SetActive(true);
                    coreLavaTimer = 0;
                }
            } else {
                if (outOfBoundsWait >= 0.1f) {

                    // When falling out of bounds:
                    if (cam.ss_mode != 0) {
                        ////transform.position = cam.ss_player_cache_pos;
                    } else if (force_respawn_to_entrance_from_PauseMenu) {
                        force_respawn_to_entrance_from_PauseMenu = false;
                        transform.position = sceneStartPos;
                        cam.transform.position = transform.position;
                        // If there's a tether in the current scene, go there.
                        /*/*
                    } else if (SaveManager.tetherScene != "" && SaveManager.tetherScene == curSceneName) {
                        // Only is "" from start of game till touching a tether
                        transform.position = SaveManager.tetherPos;
                        cam.transform.position = transform.position;

                        localEuler = transform.localEulerAngles;
                        localEuler.y = SaveManager.tether_camEulerY;
                        transform.localEulerAngles = localEuler;                        
                        cam.setCameraEulerYRotation(SaveManager.tether_camEulerY);
                        // Otherwise, return to the entry position of the scene.
                                        */
                    } else {
                    
                        transform.position = sceneStartPos;
                        cam.transform.position = transform.position;
                    }

                    if (force_respawn_pos_once) {
                        force_respawn_pos_once = false;
                        transform.position = forced_respawn_pos;
                    }


                    my_jp_dash = my_jp_jump = false;
                    my_jp_sprint = false;
                    falling_oob = false;
                    require_release_of_cancel_to_sprint = true;
                    cam.Reset_targetPos();
                }

                outOfBoundsWait -= Time.deltaTime;
                return;
            }
        }

        #endregion

        bool controlsOn = true;

        #region pause handling

        /*/*
        if (isThereAnyReasonToPause()) {
            bool touchingGround_ = Is_touching_ground(0.1f);

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUp") ||
                 animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUpSprint") ||
                 animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUpHurt")) {
                AnimSetBool("Falling", true);
            }

            
            if (MyEvent.Is_any_MyEvent_parsing || !dbox.isFinished()) {
                //temp HoverParticles.Stop();
                AnimSetBool("Running", false);
                AnimSetBool("Sprinting", false);
                vfx.Stop_run_dust();
                vfx.Stop_run_trail();
                if (Is_touching_ground(1f)) {
                    AnimSetBool("Falling", false);
                }
            }


            if (!wasPausedFixed) {
                pausedRBPos = transform.position;
                pausedRBVel = rb.velocity;
            }

            // Freeze the player if pause menu open
            if (DataLoader.instance.isPaused || paused_during_linking || pause_screenshot) {
                rb.MovePosition(pausedRBPos);
                rb.velocity = Vector3.zero;
                rb.useGravity = false;
                
                cam.freeze_targetpos_in_link = true;

                gravityTurnedOffWhenPaused = true;

                // Slow down player before starting any dialogue
            } else if (!(touchingGround_ && Mathf.Abs(rb.velocity.x) < 1f && Mathf.Abs(rb.velocity.z) < 1f) &&
                       MyEvent.Is_any_MyEvent_parsing) {
                Vector3 newvel = rb.velocity;
                newvel.x *= 0.65f;
                newvel.y -= Time.deltaTime * 12f;
                if (newvel.y < -10f) newvel.y = -10f;
                if (touchingGround_) {
                    AnimSetBool("Falling", false);
                    newvel.y = 0;
                }

                newvel.z *= 0.65f;
                if (Mathf.Abs(newvel.x) < 1f || Mathf.Abs(newvel.z) < 1f) {
                    newvel.x = 0;
                    newvel.z = 0;
                }

                previous_rb_vel_without_connected_rb = newvel;
                rb.velocity = newvel;
                return;
            } else {
                rb.velocity = Vector3.zero;
                rb.useGravity = false;
                gravityTurnedOffWhenPaused = true;
            }

            controlsOn = false;
            wasPausedFixed = true;
            CanShootSpark = false;
        } else {
            if (gravityTurnedOffWhenPaused) {
                cam.freeze_targetpos_in_link = false;
                gravityTurnedOffWhenPaused = false;
                rb.useGravity = true;
            }

            if (falling_oob) return;
            if (wasPausedFixed) {
                rb.velocity = pausedRBVel;
                wasPausedFixed = false;
            }

            if (HF.TimerDefault(ref t_updatePositionBuffer, positionBufferUpdateInterval)) {
                if (Is_touching_ground(1f)) {
                    MyPlayer3D.recentPosBufIdx++;
                    if (MyPlayer3D.recentPosBufIdx == MyPlayer3D.positionBuf.Length) MyPlayer3D.recentPosBufIdx = 0;
                    MyPlayer3D.positionBuf[MyPlayer3D.recentPosBufIdx]
                        .Set(transform.position.x, transform.position.y, transform.position.z);
                }
            }
        }
        */

        #endregion

        if (controlsOn) {
            
            animator.speed = 1f;
            Update_tether();

            float moveX = MyInput.moveX;
            float moveY = MyInput.moveY;
            if (is_sprinting_speed_on) {
                if (Mathf.Abs(moveX) < 0.05f && Mathf.Abs(moveY) < 0.05f) {
                    moveX = MyInput.camX;
                    if (!MyInput.gamepad_active && kb_hor_moves && moveX < -0.05f) moveX = -1;
                    if (!MyInput.gamepad_active && kb_hor_moves && moveX > 0.05f) moveX = 1;
                }
            }
            
            if (!MyInput.gamepad_active && kb_hor_moves && MyInput.left) moveX = -1;
            if (!MyInput.gamepad_active && kb_hor_moves && MyInput.right) moveX = 1;

            // Recenter while moving: Stop movement if holding recenter and moving (bc this will rotate cam)
            if (!MyInput.gamepad_active && MyInput.recenter) {
                moveX = 0;
                moveY = 0;
            }
            
            if (t_stunned >= 0) {
                moveX = moveY = 0;
                MyInput.sprintDash = false;
                MyInput.dash_separate = false;
                MyInput.jp_dash_separate = false;
                
                t_stunned -= Time.deltaTime;
            }

            sprintDash_held = MyInput.sprintDash;
            bool dashSepOn_and_holdOrJPDashSep = SaveManager.dash_is_separate &&
                                                 (MyInput.dash_separate || MyInput.jp_dash_separate_ignore_sprint);

            if (autosprint_mode == 1) {
                if (!sprintDash_held) autosprint_mode = 0;
                // Start autosprinting when the player presses the sprint input on hte ground
            } else if (!autosprinting && SaveManager.autosprint && sprintDash_held && jump_state == jump_state_ground) {
                autosprinting = true;
            } else if (autosprinting) { 
                if ((my_jp_sprint && jump_state == jump_state_ground) || dashSepOn_and_holdOrJPDashSep) {
                    // Cancel auto-sprint by pressing sprint again. (Code copy pasted in respawn area)
                    autosprinting = false;
                    autosprint_mode = 1; // Wait for sprint to be released so sprinting doesn't happen forever
                } else {
                    // Otherwise, behave as if sprint is held.
                    sprintDash_held = true;
                }
            }
            

            if (MyInput.test_shoot && test_3d_mode) {
                moveX = moveY = 0;
            }

            CanShootSpark = true;
            // Movement controls for regular camera
            holdingForward = false;
            holdingBackward = false;
            holdingForward = moveY > 0.1f;
            holdingBackward = moveY < -0.1f;
            if (holdingBackward) holdingForward = false;

            bool holdingLeft = moveX < -horCutoff;
            bool holdingRight = moveX > horCutoff;
            bool holdingHor = holdingLeft || holdingRight;
            holdingAnyMovement = false;
            if (holdingForward || holdingBackward || holdingHor || is_sprinting_speed_on)
                holdingAnyMovement = true;
            bool isGliding = jump_state == jump_state_gliding;
            bool isGlidingInitializing = jump_state == jumpstate_glideInit || is_moving_to_glide_start_point;

            #region teledash and vault entry

            bool pause_during_dash = false;
            if (dash_mode == dash_mode_windup || dash_mode == dash_mode_teleport || dash_mode == dash_mode_vault_wait)
                pause_during_dash = true;
            if (dash_mode == dash_mode_inactive) {
                bool ignore_dashInput_because_on_ground = jump_state == jump_state_ground;
                if (in_mud__resetInFixedUpdate) ignore_dashInput_because_on_ground = true;
                if (my_jp_dash && t_dash_cooldown <= 0 && t_time_not_on_ground > 0.04f && !used_dash && !ignore_dashInput_because_on_ground && !isGliding &&
                    !isGlidingInitializing && !teledash_and_wallrun_disabled) {
                    Use_dash();
                    cam.just_dashed = true;
                    just_vaulted_but_didnt_land = false;
                    /*/*
                    if (t_speedPodTime > 0 && !superdashed_in_sd_zone_wo_hit_something && SpeedPod.current_used_pods >= 3) {
                        ////ASoundPlayer.instance.teledash(1.0f, 1f);
                        ////ASoundPlayer.instance.poddash();
                        doing_SpeedPod_StrongDash = true;
                        if (!superdash_hack) {
                            t_speedPodTime = 0;
                        } else {
                            superdashed_in_sd_zone_wo_hit_something = true;
                        }

                    } else {
                        SoundPlayer.instance.teledash();
                        
                        if (DashPod.Num_held_pods() > 0) {
                            SoundPlayer.instance.poddash();
                        }
                    }
                    */



                    my_jp_dash = false;
                    dash_mode = dash_mode_windup;

                    
                    /*/*
                    if (DashPod.Num_held_pods() > 0) {
                        Color col = Color.white;
                        ColorUtility.TryParseHtmlString("#660A9FFF", out col);
                        Start_emission_lerp(tm_dash_windup, false, col);
                    } else {
                        Start_emission_lerp(tm_dash_windup, false, Get_active_trio_dashEmission_color());
                    }
                    */

                    t_dash_cooldown = tm_dash_cooldown;
                    t_dash_windup = 0;
                    dash_shrink_scale_start = activeModel.transform.localScale;
                    dash_dir = new Vector3();
                    Vector3 current_velocity = rb.velocity;
                    current_velocity.y = 0;
                    dash_dir = transform.forward;
                    dash_dir.y = 0;
                    dash_dir = dash_dir.normalized;
                    ////if (DashPod.Num_held_pods() > 0) {
                    ////    dash_offset = dash_dir * dash_magnet_distance * dashPod_mul;
                    ////} else {
                        dash_offset = dash_dir * dash_magnet_distance;
                        ////}

                    AnimPlay("DashWindup");
                    if (stop_at_dash_start) rb.velocity = Vector3.zero;
                } else if (my_jp_sprint && t_dash_cooldown <= 0) {
                    t_dash_cooldown = tm_dash_cooldown;
                    if (jump_state == jump_state_ground) { // Hold sprint on ground
                        Start_sprinting_speed_only();
                        Vector3 no_y = rb.velocity;
                        no_y.y = 0;
                        if (no_y.magnitude > 0.2f && holdingAnyMovement) {
                            Vector3 moveVec = cam.transform.forward * moveY + cam.transform.right * moveX;
                            Vector2 moveVec_xz = new Vector2(moveVec.x, moveVec.z);

                            // Sometimes when doing this near walls  your movement is way off from your held dir (due to wall movement parallelization)
                            // If so, then don't rotate (so that it's less disorienting)
                            float ang = Mathf.Abs(Vector2.Angle(new Vector2(no_y.x, no_y.z), moveVec_xz));
                            if (ang < 20f) {
                                Rotate_towards_vector(no_y, 1);
                            }
                        }

                        if (SaveManager.sprint_delay) {
                            t_sprint_delay = 0.4f;
                        }
                        Start_sprinting_anim_only();
                    }
                } else {
                    if (t_dash_cooldown > 0) t_dash_cooldown -= Time.fixedDeltaTime;
                    if (t_just_hit_wall_in_dashPrism > 0) t_just_hit_wall_in_dashPrism -= Time.fixedDeltaTime;
                }
            } else if (dash_mode == dash_mode_windup) {
                t_dash_windup += Time.fixedDeltaTime;
                temp_teledash_scale =
                    Vector3.Lerp(dash_shrink_scale_start, dash_shrink_scale_end, t_dash_windup / tm_dash_windup);
                activeModel.transform.localScale = temp_teledash_scale;
                if (t_dash_windup >= tm_dash_windup) {
                    dash_mode = dash_mode_teleport;
                }
            } else if (dash_mode == dash_mode_teleport) {
                cam.dash_offset_applied = true;
                bool do_vault = false;
                Start_emission_lerp(0.66f,true,Color.white);
                activeModel.transform.localScale = dash_shrink_scale_start;
                
                /*/*
                if (DashPod.Num_held_pods() > 0) {
                    vfx.DashPodTrail();
                    vfx.Dash(transform.position,transform.forward,activeModel.name,true);
                } else {
                    vfx.Dash(transform.position,transform.forward,activeModel.name); // play vfx at original position
                    
                }
                */
                SetCapsuleCastVars(dash_offset * -1, 0.5f);

                /*/*
                if (inside_a_dash_prism) {
                    dash_prism_fixedStartPos =
                        dash_prism_extSet_startPos; // Do this to store the starting pos (in case a DashPrism script changes the extSet one)
                    if (inside_a_fixed_dash_prism) {
                        prism_dash_vel = DashPrism.activeFixedDashPrism_Trajectory * dash_prism_speed;
                        prism_dash_vel.y = 0;
                        Face_transform_to_vector_xz_dir(prism_dash_vel, 1, 0.1f);
                        prism_dash_vel = DashPrism.activeFixedDashPrism_Trajectory * dash_prism_speed;
                    } else {
                        prism_dash_vel =
                            (dash_prism_fixedStartPos -
                             transform.position); // Guaranteed to have magnitude > 0.2f from DashPrism code
                        prism_dash_vel.y = 0;
                        prism_dash_vel = prism_dash_vel.normalized * dash_prism_speed;
                        Face_transform_to_vector_xz_dir(prism_dash_vel, 1, 0.1f);
                    }
                } else if (doing_SpeedPod_StrongDash) {
                    dash_prism_fixedStartPos = dash_prism_extSet_startPos = transform.position;
                    prism_dash_vel = dash_dir;
                    prism_dash_vel.y = 0;
                    prism_dash_vel = prism_dash_vel.normalized * dash_prism_speed;
                    Face_transform_to_vector_xz_dir(prism_dash_vel, 1, 0.1f);
                }
                */

                // Check for obstacles and height, then apply offset.
                SetBoxCastVars(dash_offset * -1, 1f);
                bool first_boxcast_hit = Physics.BoxCast(boxcast_center, boxcast_halfExtants, dash_offset, out dash_sweep_hit,
                    transform.rotation, dash_offset.magnitude + 1f, Registry.lmc_GROUND | Registry.lm_ignorePlayerAndCam,
                    qti_ignore);
                bool safety_raycast_hit = Physics.Raycast(boxcast_center, dash_offset, out RaycastHit safety_hit, dash_offset.magnitude + 1f,
                    Registry.lmc_GROUND | Registry.lm_ignorePlayerAndCam, qti_ignore);
                bool broke_BreakableBlock = false;
                if (first_boxcast_hit || safety_raycast_hit) {
                    bool applyFullDashthrough = false;
                    if (safety_raycast_hit && !first_boxcast_hit) {
                        dash_sweep_hit = safety_hit;
                    }
                    broke_BreakableBlock = Maybe_break_breakableBlock(dash_sweep_hit);
                    //Debug.DrawRay(dash_sweep_hit.point, dash_sweep_hit.normal * 3f, Color.red, 1f);
                    if (1 << dash_sweep_hit.collider.gameObject.layer == Registry.lm_groundIgnoreDash) {
                        ////dash_sweep_hit.collider.GetComponent<FadeWhenBetweenCamAndPlayer>().Trigger_fadeOut();

                        if (Physics.BoxCast(boxcast_center, boxcast_halfExtants, dash_offset, out dash_sweep_hit,
                            transform.rotation, dash_offset.magnitude + 1f, Registry.lmc_GROUND ^ Registry.lm_groundIgnoreDash,
                            qti_ignore)) {
                            // Something was hit, so move as normal/hit wall etc.
                        } else {
                            applyFullDashthrough = true;
                        }
                    }

                    ////SquishyPlatform squishy_platform = dash_sweep_hit.collider.GetComponent<SquishyPlatform>();
                    ////if (squishy_platform != null) {
////                        squishy_platform.MaybeRipple(true);
////                    }

                    // Happens if first, a dashthrough was detected - then beyond that, nothing.
                    // Note that on the FadeWhenBetween... script side, the layermask changing means that dashing won't detecting the wall (and thus magnet) anymore till it's faded back in.
                    // It's technically 'wrong' but probably won't matter in practice... right? lol 
                    
                    if (applyFullDashthrough) {
                        transform.position += dash_offset.normalized * (dash_offset.magnitude + 0.5f);
                    } else {
                        float hitToTransformDis = Vector3.Distance(dash_sweep_hit.point, transform.position);
                        float reduction_mul = (hitToTransformDis / dash_offset.magnitude);
                        Vector3 hit_normal = dash_sweep_hit.normal;
                        float dash_wall_normal_angle_from_ground = Get_angle_from_xz(hit_normal, true);
                        float dash_hit_angle = Mathf.Abs(Vector3.Angle(-1 * dash_offset, hit_normal));
                        //print(dash_hit_angle + "," + dash_wall_normal_angle_from_ground);
                        //print(dash_sweep_hit.collider.name);
                        //Debug.DrawRay(dash_sweep_hit.point, dash_sweep_hit.normal, Color.red, 4);
                        if (dash_hit_angle < max_vault_angle_from_normal && jump_state != jump_state_ground &&
                            dash_wall_normal_angle_from_ground <= 30f) {
                            do_vault = true;
                            if (dash_sweep_hit.collider.gameObject.CompareTag("GripWall")) {
                                used_dash = false;
                                
                            }
                        }

                        bool applied_dash_offset = false;
//                        print(dash_wall_normal_angle_from_ground+","+dash_hit_angle);
                        if (dash_wall_normal_angle_from_ground >= 45f) {
                            // Dashed into a mild slope, so warp up the slope (rather than getting 'stuck' by it).
                            // Do this by treating the dash_offset as the bottom of a triangle, then using the angle to solve for the other triangle side.
                            // tan(x) * XZ = Y
                            Vector3 modified_dash_offset = dash_offset;
                            float xz = modified_dash_offset.magnitude;
                            modified_dash_offset.y = Mathf.Tan(((90f - dash_wall_normal_angle_from_ground) + 5f) / 360f * 6.28f) * xz;

                            
                            //SetBoxCastVars(dash_offset * -1, 1f,1f,0.5f);
                            // Cast up the slope (but angled 5 deg higher)
                            if (!Physics.BoxCast(boxcast_center, boxcast_halfExtants, modified_dash_offset, out dash_sweep_hit,
                                transform.rotation, modified_dash_offset.magnitude + 1f,
                                Registry.lmc_GROUND | Registry.lm_ignorePlayerAndCam, qti_ignore))
                            {
                                // If no hit, then move up the slope.
                                //// if (DashPod.Num_held_pods() > 0) {
////                                    transform.position += modified_dash_offset.normalized * (dashPod_mul);
////                                } else {
                                    transform.position += modified_dash_offset.normalized * (dash_distance_when_nothing_hit);
                                    ////                            }
                                applied_dash_offset = true;
                                //print("Dashed up slope.");
                            } else { // Hit something up the slope, so move to that.
                                hitToTransformDis = Vector3.Distance(dash_sweep_hit.point, transform.position);
                                reduction_mul = (hitToTransformDis / modified_dash_offset.magnitude);
                                transform.position += dash_offset * reduction_mul; // snap to the wall
                                
                                float _ang = Get_angle_from_xz(dash_sweep_hit.normal, true);
                                if (_ang < 40f) {
                                    transform.position -= dash_offset.normalized * 3f;    
                                } else {
                                    transform.position -= dash_offset.normalized * 1.3f;    
                                }
                                applied_dash_offset = true;
//                                print("Dashed up slope into something.");
                            }
                        }

                        if (!applied_dash_offset) {
                            transform.position += dash_offset * reduction_mul; // snap to the wall
                            transform.position -= dash_offset.normalized * 2.5f; // pushback from wall 2.5
                        }
                    }
                } else {
                    // print("Miss!");
                    // float dis = dash_offset.magnitude;
                    // Debug.DrawRay(boxcast_center, (dis + 0.5f) * (dash_offset.normalized), Color.red, 2f);
                    // Debug.DrawRay(boxcast_center+ new Vector3(0,boxcast_halfExtants.y,0), (dis + 0.5f) * (dash_offset.normalized), Color.red, 2f);
                    // Debug.DrawRay(boxcast_center - new Vector3(0, boxcast_halfExtants.y, 0), (dis + 0.5f) * (dash_offset.normalized), Color.red, 2f);
                    
                    // Nothing was hit
                    ////if (DashPod.Num_held_pods() > 0) {
////                        transform.position += (dash_offset.normalized) * dash_distance_when_nothing_hit * dashPod_mul;
////                    } else {
                        transform.position += (dash_offset.normalized) * dash_distance_when_nothing_hit;
                        ////                }
                }

                ////if (DashPod.Num_held_pods() > 0) {
////                    DashPod.Use_pod();
////                }

                t_dash_cooldown = tm_dash_cooldown;

                // Animatiton
                if (broke_BreakableBlock) {
                    doing_SpeedPod_StrongDash = false;
                }
                if (inside_a_dash_prism || doing_SpeedPod_StrongDash) { //temp change to if you're in a dash prism trigger
                    do_vault = false;
                    t_superdash_timeout = 0;
                    dash_mode = dash_mode_prismDash;
                    ////vfx.Start_superdash();
                    //cancelspr
                    AnimSetBool("Running", false);
                    Cancel_sprint_state_and_anim();
                    transform.position = dash_prism_extSet_startPos;
                    jump_state = jumpstate_dashprisming; // move
                    AnimPlay("Vault");
                } else if (!do_vault) {
                    // Holding sprint after a midair dash that didn't vault
                    if (sprintDash_held && !test_3d_mode) {
                        t_consec_wallrun_cooldown = 0.5f;
                        if (!Is_touching_ground()) {
                            //Start_sprinting_speed_only();// Sprint -after teledash offset- if -sprint held-.. wait no, why??? lol
                            AnimPlay("JumpDownSprint");
                        } else {
                            Start_sprinting_anim_only(); // Start sprinting since standing on the ground.
                        }
                    } else {
                        if (jump_state == jump_state_ground) {
                            AnimPlay("Idle");
                        } else {
                            AnimPlay("JumpDown");
                        }
                    }
                } else if (do_vault) {
                    AnimPlay("Vault");
                    //Start_sprinting_speed_only(); // Sprint -from start of vault-
                }

                activeModel.localEulerAngles = Vector3.zero;

                // State change at end of dash input
                if (dash_mode == dash_mode_prismDash) {
                    // nothing
                } else if (do_vault) {
                    dash_mode = dash_mode_vault_wait;
                    t_pause_before_vault = pause_before_vault;
                } else {
                    dash_mode = dash_mode_inactive;
                }
            } else if (dash_mode == dash_mode_vault_wait) {
                if (Mathf.Abs(t_pause_before_vault - pause_before_vault) <= 0.001f) {
                    ////vfx.WallVault(dash_sweep_hit.point, dash_sweep_hit.normal,resetdashfromgrip);
                    resetdashfromgrip = false;
                    ////SoundPlayer.instance.wall_vault_hitWall();
                }
                t_pause_before_vault -= Time.fixedDeltaTime;
                if (t_pause_before_vault <= 0) {
                    dash_mode = dash_mode_inactive;
                    trigger_vault_jump = true;
                }
            } else if (dash_mode == dash_mode_prismDash) {
            }

            #endregion


            #region XZ Velocity Calculations start here, based on the above inputs.

            /* position debug
            SetCapsuleCastVars(transform.forward * -1, 0.5f, 1, 2f, true);
            float castDist2 = 0.5f + ccRadiusWorld + 2f;
            Debug.DrawRay(ccp1, transform.forward * castDist2, Color.red, 2f); */

            #region force movement input if sprinting

            if (is_sprinting_speed_on &&
                ((!sprintDash_held && jump_state == jump_state_ground && t_sprint_release_to_drag <= 0) || IsGliding())) {
                // Cancel when not holding sprint, and -on the ground- OR -gliding-
                Cancel_sprint_state_and_anim();
            }

            if (is_sprinting_speed_on) {
                if (t_sprint_release_to_drag >= 0) t_sprint_release_to_drag -= Time.fixedDeltaTime;
                if (sprintDash_held) t_sprint_release_to_drag = 0f;
                holdingForward = true;
                holdingBackward = false;
            }

            if (!is_sprinting_speed_on) t_consec_wallrun_cooldown = 0;

            #endregion

            #region wallrun controls

            if (is_wallrunning) {
                Face_transform_to_vector_xz_dir(temp_wallrun_dir, 1, -1, Time.fixedDeltaTime * 2f);
                Vector3 prev_wallrun_dir = temp_wallrun_dir;
                // Start a little ahead of movement direction
                Vector3 _start = transform.position + temp_wallrun_dir.normalized * ccRadiusWorld * wallrun_raycast_lookahead_mul;
                float
                    pushaway = 0.5f; // Move raycast start pos away from wall (this makes sharpmer interior turns more accurate since the start pos is less likely to get caught in the wall)
                _start -= transform.right * wallrun_side * pushaway;
                //Debug.DrawRay(_start, transform.right * wallrun_side * (wallrun_raycast_distance+pushaway), Color.red,6f);
                // Cast towards the wall
                if (Physics.Raycast(_start, transform.right * wallrun_side, out RaycastHit hitInfo,
                    wallrun_raycast_distance + pushaway, Registry.lmc_GROUND, qti_ignore)) {
                    Vector3 lookahead_norm_perp = new Vector3();
                    Get_perp_of_v1_closest_to_v2_along_xz(ref lookahead_norm_perp, hitInfo.normal, transform.forward);

                    temp_wallrun_dir = lookahead_norm_perp;
                    float dash_wall_normal_angle_from_ground = Get_angle_from_xz(hitInfo.normal, true);
                    last_wallrun_turn_angle_signed = Vector3.SignedAngle(prev_wallrun_dir, temp_wallrun_dir, Vector3.up);
                    // Signed angle deltas with side attached to wall
                    // L EX -   L INT +
                    // R EX +   R INT -
                    // Use different angle threshold for ext/int turns
                    bool interiorSurface = true;
                    if (wallrun_side == 1 && last_wallrun_turn_angle_signed > 0) interiorSurface = false;
                    if (wallrun_side == -1 && last_wallrun_turn_angle_signed < 0) interiorSurface = false;
                    float max_angle_to_use = max_wallrun_interior_turn_angle;
                    if (interiorSurface == false) max_angle_to_use = max_wallrun_exterior_turn_angle;

                    // Check next turn angle.. too big, fall off.
                    if (Mathf.Abs(last_wallrun_turn_angle_signed) > max_angle_to_use) {
                        trigger_jumpstate_wallrunning__fallOff = true;
                        //print("Fall off from turn too big: " + last_wallrun_turn_angle_signed);
                        // check slope.. too far from 90 deg wall? fall off
                    } else if (dash_wall_normal_angle_from_ground > allowed_wall_slope_variance*1.25f) { // 2020-10-13 - Add a multiplier, so that the margin for falling off is more generous
                        //print("Fall off from wall being not steep: " + dash_wall_normal_angle_from_ground);
                        trigger_jumpstate_wallrunning__fallOff = true;
                    } else {
                        // Check in front - hit? fall off.
                        SetBoxCastVars(transform.forward,0,1,0.5f); // 2020-10-13 add yOffset of 0.5f so that entering wallrun while on a shallow uphills slope won't immediately cause a forward-wall detection.
                        boxcast_halfExtants.x /= 3f;
                        boxcast_halfExtants.z /= 3f;
                        if (Physics.BoxCast(boxcast_center, boxcast_halfExtants, transform.forward, out RaycastHit fwdHitInfo,
                            transform.rotation, boxcast_halfExtants.x * 1.5f, Registry.lmc_GROUND, qti_ignore)) {
                            //if (Physics.Raycast(transform.position, transform.forward, out RaycastHit fwdHitInfo, ccRadiusWorld * 2f, Registry.lmc_GROUND, qti_ignore)) {
                            // transform.right works here because Face_transform..() iscalled above
                            float forwardObstructionAngleDiff = Vector3.SignedAngle(transform.right * -1 * wallrun_side,
                                fwdHitInfo.normal, Vector3.up);
                            if (Mathf.Abs(forwardObstructionAngleDiff) > max_wallrun_interior_turn_angle) {
                                trigger_jumpstate_wallrunning__fallOff = true;
                                fell_off_wallrun_from_fwdOb = true;
                                // Push away from wall a little
                                //Vector3 fwdTempPos = transform.position;
                                //fwdTempPos += -2 * transform.forward;
                                //transform.position = fwdTempPos;
                                rb.velocity = Vector3.zero;
                                //print("Fall off from wall in front: " + forwardObstructionAngleDiff);
                            }
                        }

                        if (was_in_gripshroom_last_frame || was_in_slipwall_last_frame) {
                            fell_off_wallrun_from_fwdOb = true;
                            trigger_jumpstate_wallrunning__fallOff = true;
                            rb.velocity = Vector3.zero;
                        }

                        // Set Forward velocity while wall running
                        if (!trigger_jumpstate_wallrunning__fallOff) {
                            if (!sprintDash_held) {
                                current_wallrun_speed -= Time.fixedDeltaTime * wallrun_decel;
                            } else {
                                current_wallrun_speed += Time.fixedDeltaTime * wallrun_decel;
                            }

                            current_wallrun_speed = Mathf.Clamp(current_wallrun_speed, wallrun_falloff_speed, base_wallrun_speed);

                            if (current_wallrun_speed <= wallrun_falloff_speed) {
                                trigger_jumpstate_wallrunning__fallOff = true;
                            }

                            Vector3 tempVel = temp_wallrun_dir * current_wallrun_speed;
                            tempVel *= Get_SpeedPodFactor();

                            tempVel.y = rb.velocity.y;
                            if (magnet_wallrunning && tempVel.y < 0) tempVel.y = 0;
                            if (magnet_wallrunning) {
                                if (t_magnet_wall_glow <= 0.5f) {
                                    t_magnet_wall_glow += Time.fixedDeltaTime;
                                    if (t_magnet_wall_glow >= 0.5f) t_magnet_wall_glow = 0.5f;
                                    magnet_wall_mat.SetColor("_EmissionColor",Color.Lerp(Color.black,Color.white,t_magnet_wall_glow/0.5f));
                                    if (magnet_wall_mat2 != null) {
                                        magnet_wall_mat2.SetColor("_EmissionColor",Color.Lerp(Color.black,Color.white,t_magnet_wall_glow/0.5f));
                                    }
                                }
                            }
                            rb.velocity = tempVel;
                        }

                        if (anim_wr_hold_from_wall) {
                            if (wallrun_side == 1 && !MyInput.left) {
                                PlayAnimation("Wallrun_R",true);
                                anim_wr_hold_from_wall = false;
                            }
                            if (wallrun_side == -1 && !MyInput.right) {
                                PlayAnimation("Wallrun_L",true);
                                anim_wr_hold_from_wall = false;
                            }

                        } else {
                            if (wallrun_side == 1 && MyInput.left) {
                                PlayAnimation("Wallrun_Lean_L", true);
                                anim_wr_hold_from_wall = true;
                            }
                            if (wallrun_side == -1 && MyInput.right) {
                                PlayAnimation("Wallrun_Lean_R",true);
                                anim_wr_hold_from_wall = true;
                            }
                            
                        }
                    }
                } else {
                    trigger_jumpstate_wallrunning__fallOff = true;
                    // 2020-10-13 - hack to fix issue where wall run starts due to top part of capsule hitting wall, but the wallrun query point is on a lower part of the wall that's too shallow.
                    // The player would normally clip, but this will bounce the player slightly away
                    transform.position -= transform.right * wallrun_side * 1f;
                    //print("Fall off from right/left wall search failing");
                }

                #endregion
            } else if (is_grappling) {
                // No horizontal movement controls in grappling
            } else if (is_moving_to_glide_start_point) {
                rb.velocity = Vector3.zero;
                transform.position = Vector3.SmoothDamp(transform.position, glide_start_point, ref glide_init_entry_vel,
                    glide_init_smoothDampSpeed);
                if (Vector3.Distance(transform.position, glide_start_point) < 0.25f) {
                    is_moving_to_glide_start_point = false;
                    trig_js_midairToGlide = true;
                    ////vfx.Show_glide_wings();
                }

                #region prism dash

            } else if (dash_mode == dash_mode_prismDash) {
                rb.velocity = prism_dash_vel;
                if (doing_SpeedPod_StrongDash) {
                    dash_prism_max_distance = speedPod_dashPrism_distance;
                }

                t_superdash_timeout += Time.fixedDeltaTime;
                    
                if (Vector3.Distance(dash_prism_fixedStartPos, transform.position) > dash_prism_max_distance + 1f || t_superdash_timeout > 1f) {
                    // Start falling, went too far.
                    dash_mode = dash_mode_inactive;
                    if (rb.velocity.y > -0.5f) {
                        AnimPlay("JumpUpSprint"); // Edit later when actually have a prism dash animation
                    } else {
                        AnimSetBool("Falling", true);
                        AnimPlay("JumpDownSprint");
                    }

                    Enter_jumpstate_firstJumpArc();
                    used_dash = false; // Give back dash after falling for fun
                    ////vfx.Stop_superdash();
                } else { // Wait to hit something, then enter a vault.

                    // If player hits a reflector, reflect its velocity about the surface normal and also update the dash start position so player can keep travelling.
                    // (Uses separate raycast to avoid edge normal interpolation of capsulecasts)
                    doing_SpeedPod_StrongDash = false;
                    
                    bool hitReflector = false;
                    SetCapsuleCastVars(transform.forward);
                    if (Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, prism_dash_vel, out RaycastHit reflectorHit, 2f,
                        Registry.lmc_GROUND, qti_ignore)) {
                        
                        if (reflectorHit.collider.name.IndexOf("PrismDashReflector") != -1) {
                            hitReflector = true;
                            prism_dash_vel = Vector3.Reflect(prism_dash_vel, reflectorHit.normal);
                            dash_prism_fixedStartPos = transform.position;
                            Face_transform_to_vector_xz_dir(prism_dash_vel, 1, 0.1f);
                        }
                    }

                    if (!hitReflector) {
                        SetCapsuleCastVars(transform.forward);
                        if (Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, prism_dash_vel, out RaycastHit hitInfo, 1f,
                            Registry.lmc_GROUND, qti_ignore)) {
                            dash_mode = dash_mode_inactive;
                        ////    vfx.Stop_superdash();
////                            vfx.Lava_death(transform.position);

                            rb.velocity = Vector3.zero;
                            Enter_jumpstate_firstJumpArc();

                            Maybe_break_breakableBlock(hitInfo);
                            
                            
                            
                            if (!Is_touching_ground()) {
                                AnimPlay("Vault");
                                trigger_vault_jump = true;
                                used_dash = false; // Superdash always gives regular dash after hittign something
                                if (hitInfo.collider.tag == "GripWall") { // Only relevant to Core level. If hitting a gripwall, give another superdash.
                                    superdashed_in_sd_zone_wo_hit_something = false;
                                } else {
                                    // If you hit somethign that isn't a Gripwall, and in the superdash zone, then don't allow another dash
                                    if (superdash_hack) { // don't allow dash after superdash
                                        used_dash = true;
                                    }
                                }

                                t_just_hit_wall_in_dashPrism = 0.25f;
                                try_post_superdash_fx = 3;
                            } else {
                                AnimPlay("Idle");
                            }
                        }
                    }
                }


                #endregion

                // Walk, sprint, glide controls (ends at Slowdown When No Movement Input)
            } else if (holdingForward || holdingBackward || holdingLeft || holdingRight || isGliding) {
                #region Calculate target velocity if moving

                if (holdingForward || holdingBackward || ((MyInput.gamepad_active || (!MyInput.gamepad_active && kb_hor_moves)) &&
                                                          (holdingLeft || holdingRight))) {
                    AnimSetBool("Running", true);
                } else {
                    AnimSetBool("Running", false);
                }


                if ((was_in_mud_last_FixedUpdate || noSprintTriggers_Inside > 0 /*/*|| vfx.go_ripple.isEmitting*/) && jump_state == jump_state_ground) {
                    animator.speed = 0.7f;
                }
                

                Vector3 previous_vel_xz = rb.velocity;
                Vector3 utilized_vel = Vector3.zero;
                float previous_y_vel = previous_vel_xz.y;
                previous_vel_xz.y = 0;

                Vector3 target_vel_xz = new Vector3();
                if (holdingForward) target_vel_xz = cam.transform.forward;
                if (holdingBackward) target_vel_xz = cam.transform.forward * -1;
                if (is_sprinting_speed_on) target_vel_xz = transform.forward;
                // Only changes velocity for controller. L/R on keyboard are rotation
                Vector3 horPart = new Vector3();
                if (holdingLeft) horPart = cam.transform.right * -1;
                if (holdingRight) horPart = cam.transform.right;

                // Integrate horizontal movement, but only on the ground.
                // If not on the ground, then dampen the horizontal movement.
                if ((MyInput.gamepad_active || (!MyInput.gamepad_active && kb_hor_moves)) && !is_sprinting_speed_on) {
                    // Diagonal movement
                    if (Mathf.Abs(moveX) > 0.05f && (holdingForward || holdingBackward)) {
                        target_vel_xz = moveX * cam.transform.right + moveY * cam.transform.forward;
                        // Only horizontal movement
                    } else if (!(holdingForward || holdingBackward)) {
                        target_vel_xz = horPart;
                    }
                }

                target_vel_xz.y = 0;
                target_vel_xz.Normalize();
                if (MyInput.shortcut) target_vel_xz *= 5f;
                target_vel_xz *= max_walk_speed;
                was_in_mud_last_FixedUpdate = false;
                if (in_mud__resetInFixedUpdate) {
                    target_vel_xz *= mud_walk_mul;
                    was_in_mud_last_FixedUpdate = true;
                } else if (noSprintTriggers_Inside > 0 && !is_sprinting_speed_on) {
                    target_vel_xz *= 0.7f;
                }

                if (t_sprint_delay > 0) {
                    t_sprint_delay -= Time.fixedDeltaTime;
                    target_vel_xz *= 0f;
                }

                target_vel_xz *= wallRunJump_xzVel_booster;
                wallRunJump_xzVel_booster -= Time.fixedDeltaTime;
                if (wallRunJump_xzVel_booster <= 1) wallRunJump_xzVel_booster = 1;

                // Gamepad: Scale velocity via analog input strength
                if (MyInput.gamepad_active && !is_sprinting_speed_on) {
                    float movementAmp = Mathf.Sqrt(moveX * moveX + moveY * moveY);
                    if (movementAmp > 1) movementAmp = 1;
                    float lowerCutoff = 0.4f;
                    float higherCutoff = 0.8f;
                    float slowestSpeed = 0.64f;
                    if (movementAmp < lowerCutoff) {
                        target_vel_xz *= slowestSpeed;
                    } else if (movementAmp < higherCutoff) {
                        target_vel_xz *= slowestSpeed + (1 - slowestSpeed) *
                            ((higherCutoff - movementAmp) / (higherCutoff - (higherCutoff - lowerCutoff)));
                    }
                }

                // if (Input.GetKeyDown(KeyCode.B)) {
                //     print(1);
                // }


                if (tm_walk_accel_time > 0) {
                    if ((is_sprinting_speed_on) && jump_state == jump_state_ground) {
                        // Accelerate slower from a standstill on the ground
                        HF.TimerStayAtMax(ref t_walk_accel, tm_walk_accel_time, Time.fixedDeltaTime / 2f);
                    } else {
                        HF.TimerStayAtMax(ref t_walk_accel, tm_walk_accel_time, Time.fixedDeltaTime);
                    }

                    target_vel_xz.x *= Mathf.Lerp(0f, 1f, t_walk_accel / tm_walk_accel_time);
                    target_vel_xz.z *= Mathf.Lerp(0f, 1f, t_walk_accel / tm_walk_accel_time);
                }

                #endregion

                #region Glide velocity

                // Hold forward/back to speed/slow down gliding
                if (isGliding) {
                    if (!strafe_glide_on) {
                        localEuler = transform.localEulerAngles;
                        float dv = Time.fixedDeltaTime * glide_rotationalAccel;
                        if (MyInput.right) glide_rotationalVel += dv;
                        if (MyInput.left) glide_rotationalVel -= dv;
                        glide_rotationalVel
                            = Mathf.Clamp(glide_rotationalVel, -glide_maxRotVel, glide_maxRotVel);

                        if (!MyInput.left && !MyInput.right) {
                            if (glide_rotationalVel > 0) glide_rotationalVel = Mathf.Clamp(glide_rotationalVel - dv, 0, 500);
                            if (glide_rotationalVel < 0) glide_rotationalVel = Mathf.Clamp(glide_rotationalVel + dv, -500, 0);
                        }

                        localEuler.y += glide_rotationalVel * Time.fixedDeltaTime;
                        transform.localEulerAngles = localEuler;
                    }

                    temp_glideDir_vec = transform.forward;
                    temp_glideDir_vec.y = 0;
                    cur_glide_speed = Mathf.Clamp(cur_glide_speed + Time.fixedDeltaTime * glide_takeoffAccel, 0,
                        base_glide_speed);
                    if (!fancy_glide_beginning_on) cur_glide_speed = base_glide_speed;
                    temp_glideDir_vec = temp_glideDir_vec.normalized * cur_glide_speed;
                    target_vel_xz.x = temp_glideDir_vec.x;
                    target_vel_xz.z = temp_glideDir_vec.z;
                    if (strafe_glide_on) {
                        temp_glideDir_vec = Vector3.zero;
                        if (MyInput.right) {
                            temp_glideDir_vec = transform.right * strafe_glide_speed;
                        } else if (MyInput.left) {
                            temp_glideDir_vec = -1 * transform.right * strafe_glide_speed;
                        }

                        temp_glideDir_vec *= (cur_glide_speed / base_glide_speed);
                        target_vel_xz += temp_glideDir_vec;
                    }

                    #endregion

                    #region Sprinting + Enter Wallrun

                } else if (is_sprinting_speed_on) {
                    // Set Sprinting velocity
                    sprint_time_to_accel = Mathf.Clamp(sprint_time_to_accel + Time.fixedDeltaTime, 0, MAX_sprint_time_to_accel);
                    float sprintHeldFac = sprint_time_to_accel / MAX_sprint_time_to_accel;

                    if (noSprintTriggers_Inside <=  0) { // No sprinting if inside a no Sprint Trigger.
                        if (wasSprintingIn_NoSprintT_ButDidntLand) { // This is set when sprinting in an NST. Unset it once one the ground (It's set so that you don't get sprinting velocity while in jumping from an NST)
                            if (jump_state == jump_state_ground) {
                                wasSprintingIn_NoSprintT_ButDidntLand = false;
                            }   
                        } else {
                            target_vel_xz *= 1 + (sprint_speed_multiplier - 1f) * sprintHeldFac;
                            // Speed up velocity when holding speed pods
                            target_vel_xz *= Get_SpeedPodFactor();
                        }
                    }
                    else {
                         wasSprintingIn_NoSprintT_ButDidntLand = true;
                    }

                    if (in_mud__resetInFixedUpdate) {
                        target_vel_xz *= mud_sprint_mul;
                    }

                    // Rotate player
                    localEuler = transform.localEulerAngles;
                    float dv = Time.fixedDeltaTime * sprint_rotAccel;
                    bool decel_sprint = false;
 //                   if (moveX > 0.25f && (Mathf.Abs(moveY) < 0.75f && MyInput.gamepad_active || !MyInput.gamepad_active)) { // Symmetrical with below left block.
                    if (moveX > 0.15f) { // Symmetrical with below left block.
                        sprint_rotVel += dv;
                        if (sprintHeldFac < 1) {
                            sprint_rotVel += Mathf.Lerp(sprint_rotVelMul_at_start, 1, sprintHeldFac) * dv;
                        }

                        if (just_wallrun_jumped_and_not_on_ground && last_wallrun_side == 1 && sprint_rotVel < 20f) {
                            sprint_rotVel += 3.4f * dv;
                            // Rotate faster back towards a wall after wall jumping (this helps with course correction when pressing away and jumping, since that jump's hor-speed is quite high to aid with wallrun chaining)
                            //print("More right " + sprint_rotVel);
                        }
                    //} else if (moveX < -0.25f && (Mathf.Abs(moveY) < 0.75f && MyInput.gamepad_active || !MyInput.gamepad_active)) {
                    } else if (moveX < -0.15f) {
                        sprint_rotVel -= dv;
                        if (sprintHeldFac < 1) {
                            sprint_rotVel -= Mathf.Lerp(sprint_rotVelMul_at_start, 1, sprintHeldFac) * dv;
                        }

                        if (just_wallrun_jumped_and_not_on_ground && last_wallrun_side == -1 && sprint_rotVel > -20f) {
                            sprint_rotVel -= 3.4f * dv;
                            //print("More left " + sprint_rotVel);
                        }
                    } else {
                        // Controls aren't being held left or right
                        decel_sprint = true;
                    }

                    float tempMaxRotVel = sprint_maxRotVel;
                    // Rotate faster if speeded up    
                    tempMaxRotVel *= 1 + Get_SpeedPodFactor(true) * 0.45f;

                    float reduce_fac = 1;
                    if (moveX >= 0.15f && moveX <= 0.63f) reduce_fac = (moveX - 0.15f) / 0.48f;
                    if (moveX <= -0.15f && moveX >= -0.63f) reduce_fac = (-0.15f - moveX) / 0.48f;

                    float final_tempMaxRotVel = tempMaxRotVel * reduce_fac;
//                    print(final_tempMaxRotVel);
                    sprint_rotVel = Mathf.Clamp(sprint_rotVel, -final_tempMaxRotVel, final_tempMaxRotVel);

                    if (decel_sprint || (Mathf.Abs(moveX) < 0.05f && Mathf.Abs(moveY) < 0.05f)) {
                        if (sprint_rotVel > 0) sprint_rotVel = Mathf.Clamp(sprint_rotVel - 3 * dv, 0, 500);
                        if (sprint_rotVel < 0) sprint_rotVel = Mathf.Clamp(sprint_rotVel + 3 * dv, -500, 0);
                    }

                    float utilizedRotVel = sprint_rotVel;
                    localEuler.y += utilizedRotVel * Time.fixedDeltaTime;
                    transform.localEulerAngles = localEuler;


                    // Slowdown sprint velocity by holding backwards
                    float tm_sprint_holdBackwards = 0.5f;
                    if (just_wallrun_jumped_and_not_on_ground && !sprintDash_held) {
                        t_sprint_holdingBackInAir += Time.fixedDeltaTime;
                    }
                    
                    if (MyInput.moveY < -0.6f && jump_state == jump_state_midair) {
                        t_sprint_holdingBackInAir += Time.fixedDeltaTime;
                    } else if (jump_state != jump_state_midair) {
                        t_sprint_holdingBackInAir = 0;
                    }
                    if (t_sprint_holdingBackInAir >= tm_sprint_holdBackwards) t_sprint_holdingBackInAir = tm_sprint_holdBackwards;

                    float fac = Mathf.Lerp(1f, 0f, t_sprint_holdingBackInAir / tm_sprint_holdBackwards);
                    target_vel_xz *= fac; // Note, holdingBackward note used because sprint cancels it out by default

                    if (fac == 0) Cancel_sprint_state_and_anim();

                    if (just_vaulted_but_didnt_land) target_vel_xz *= sprint_vault_fwdVel_mul;

                    // Reset timers related to wallrunning
                    if (t_consec_wallrun_cooldown >= 0) t_consec_wallrun_cooldown -= Time.fixedDeltaTime;
                    if (t_consec_wallrun_sameside_cooldown >= 0)
                        t_consec_wallrun_sameside_cooldown -=
                            Time.fixedDeltaTime; // technically this would allow you to enter a sprint without resetting this timer but it's so low that it's okay

                    #region Sprint Jump - Maybe enter Wallrun

                    bool ignore_wall_run = dash_mode != dash_mode_inactive || just_vaulted_but_didnt_land || trigger_vault_jump || teledash_and_wallrun_disabled;
                    if (jump_state == jump_state_midair && !ignore_wall_run) {
                        float jumpYOff = 2f;
                        SetCapsuleCastVars(transform.forward * -1, 1f, 1, jumpYOff);

                        float castDist = 1f + ccRadiusWorld + 2f;


//                        Debug.DrawRay(ccp1, transform.forward * castDist, Color.red, 2f);


                        if (active_slipwalls.Count == 0 && Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, transform.forward,
                            out RaycastHit sprintJump_hit, castDist, Registry.lmc_GROUND, qti_ignore) && sprintJump_hit.collider.tag != "NonWallrunnableCollider") {
                            Vector3 hit_normal = sprintJump_hit.normal;
                            string wallrun_tag = sprintJump_hit.collider.tag;
                            float normAng_fromground = Get_angle_from_xz(hit_normal, true);
                            float hitAngle = Mathf.Abs(Vector3.Angle(-1 * transform.forward, hit_normal));
                            float fwd_to_wall_angle =
                                Vector3.SignedAngle(transform.forward, sprintJump_hit.normal * -1, Vector3.up);
                            bool do_wallrun = false;
                            if (fwd_to_wall_angle > 0) wallrun_side = 1;
                            if (fwd_to_wall_angle <= 0) wallrun_side = -1;
                            last_wallrun_side = wallrun_side;
  //                          Debug.DrawRay(sprintJump_hit.point, hit_normal * 2f, Color.green, 2f);
                            //Debug.DrawRay(transform.position, transform.forward, Color.blue, 2f);
//                            print("Wallrun hit " + hitAngle + "/" + normAng_fromground +"/"+ fwd_to_wall_angle);
                            float adjusted_min_wallrun_approach_angle = min_wallrun_approach_angle;
                            int wallrun_hor_input = 0;
                            if (MyInput.right) wallrun_hor_input = 1;
                            if (MyInput.left) wallrun_hor_input = -1;

                            if (wallrun_hor_input != 0) {
                                // Rotating towards wall - allow steeper angles into the wall
                                // (Makes it easier to latch on in mid-air?)
                                if (wallrun_side == wallrun_hor_input)
                                    adjusted_min_wallrun_approach_angle -= wallrun_approach_ang_adjust_from_hor_input;
                                
                                // Rotating away from wall  - require shallower angle to start
                                //if (wallrun_side != wallrun_hor_input)
                                  //  adjusted_min_wallrun_approach_angle += wallrun_approach_ang_adjust_from_hor_input;
                            } else if (havent_landed_from_consec_wallrun) {
                                // Make consec wallrunning easier.
                                adjusted_min_wallrun_approach_angle -= wallrun_approach_ang_adjust_from_hor_input;
                            }

//                            print(normAng_fromground+","+allowed_wall_slope_variance);
//                            print("Min angle: " + adjusted_min_wallrun_approach_angle + "," + max_wallrun_approach_angle + "," + hitAngle);
                            if (t_consec_wallrun_cooldown > 0 ||
                                (t_consec_wallrun_sameside_cooldown > 0 && last_wallrun_side == wallrun_side)) {
                                //   print("Cancel wallrun " + t_consec_wallrun_cooldown);
                            } else if (hitAngle <= max_wallrun_approach_angle &&
                                       hitAngle >= adjusted_min_wallrun_approach_angle &&
                                       normAng_fromground <= allowed_wall_slope_variance) {
                                do_wallrun = true;
                                transform.position = sprintJump_hit.point + (transform.forward * -1) * (ccRadiusWorld + 0.3f) +
                                                     new Vector3(0, -jumpYOff, 0);
                                // 20 or -10
                                if (wallrun_side == 1) activeModel.localEulerAngles = new Vector3(0, 0, 20);
                                if (wallrun_side == -1) activeModel.localEulerAngles = new Vector3(0, 0, -20);
                                Get_perp_of_v1_closest_to_v2_along_xz(ref temp_wallrun_dir, hit_normal, transform.forward);
                                Face_transform_to_vector_xz_dir(temp_wallrun_dir);
                            }

                            if (do_wallrun) {
                                Cancel_sprint_state_and_anim();
                                havent_landed_from_consec_wallrun = true;
                                sprint_rotVel = 0;
                                trigger_jumpstate_enter_wallrun = true;
                                is_wallrunning = true;
                                anim_wr_hold_from_wall = false;
                                used_dash = false;
                                float dA = hitAngle - adjusted_min_wallrun_approach_angle;
                                dA = Mathf.Clamp(dA, 0, wallrun_speedRampingStartAngleDiff);
                                current_wallrun_speed = Mathf.Lerp(min_wallrun_base_speed, base_wallrun_speed,
                                    dA / wallrun_speedRampingStartAngleDiff);
//                                print(current_wallrun_speed);
                                t_wallrunIgnoreGround = tm_wallrunIgnoreGround;
                                AnimSetBool("Running", false);
                                AnimResetTrigger("Jump");
                                ////vfx.Start_wall_run();
                                if (wallrun_side == 1) AnimPlay("Wallrun_R");
                                if (wallrun_side == -1) AnimPlay("Wallrun_L");
                                SetPlayerModelOffsetForWallrun();

                                if (wallrun_tag == "MagnetWall") {
                                    magnet_wallrunning = true;
                                    ////SoundPlayer.instance.magnet_on();
                                    magnet_wall_mat = sprintJump_hit.collider.GetComponent<MeshRenderer>().material;
                                    magnet_wall_mat2 = null;
                                    if (sprintJump_hit.collider.GetComponent<MeshRenderer>().materials.Length > 1) {
                                        magnet_wall_mat2 = sprintJump_hit.collider.GetComponent<MeshRenderer>().materials[1];
                                    }
                                    magnet_wall_emission = Color.black;
                                    t_magnet_wall_glow = 0;
                                }
                            }
                        }
                    }

                    #endregion
                } else { // walking
                    target_vel_xz *= Get_SpeedPodFactor();
                }

                #endregion

                #region Fall-state drag, turn smoothing, jump traj drag

                // Apply drag while falling, up to a specified multiplier.
                float fall_drag_factor = 1;
                bool apply_fall_drag =
                    (jump_state == jump_state_midair || (jump_state == jump_state_ground && t_fallJumpGrace > 0.08f)) &&
                    previous_y_vel < 0 && fall_arc_has_xz_slowdown;

                if (apply_fall_drag) {
                    // Messy - changes the lerping values depending if you're falling (after jumping) or falling (after falling off ledge)
                    float slowdownTime = tm_fall_arc_slowdown;
                    //if (in_firstJumpArc_via_fallingWithoutJump) slowdownTime = tm_slowdown_during_a_fall_from_ledge;
                    HF.TimerStayAtMax(ref t_fall_arc_slowdown, slowdownTime, Time.fixedDeltaTime);
                    float minDrag = fall_arc_slowdown_xc_vel_multiplier;
                    //if (in_firstJumpArc_via_fallingWithoutJump) minDrag = fallWithoutJump_slowdown_xc_vel_multiplier;
                    fall_drag_factor = Mathf.Lerp(1f, minDrag, t_fall_arc_slowdown / slowdownTime);
                    target_vel_xz.x *= fall_drag_factor;
                    target_vel_xz.z *= fall_drag_factor;
                } else {
                    t_fall_arc_slowdown = 0;
                }

                // Smooth out sudden direction changes when walking around
                if (walk_turn_smoothing_on && jump_state == jump_state_ground) {
                    float wtq = walk_turning_quickness;
                    if (active_ice.Count > 0) wtq = ice_tq;
                    utilized_vel = Vector3.Lerp(previous_vel_xz, target_vel_xz, Time.fixedDeltaTime * wtq);
                } else {
                    utilized_vel = target_vel_xz;
                }

                utilized_vel.y = previous_y_vel;

                // Slow down the jump trajectory (if it's locked)
                if (jump_trajectory_locked) {
                    utilized_vel.x = locked_jump_trajectory_vel_xz.x * fall_drag_factor;
                    utilized_vel.z = locked_jump_trajectory_vel_xz.z * fall_drag_factor;
                }

                #endregion

                #region set utilized velocity  and rotate player transform

                rb.velocity = utilized_vel;

                // Rotate player if moving. Don't rotate if gliding or sprinting, or if hor jump mvmt should be dampened enough
                bool rotate_player_to_movement_direction = holdingForward || holdingBackward || holdingHor;
                if (jump_trajectory_locked) rotate_player_to_movement_direction = false;
                if (isGliding || is_sprinting_speed_on) rotate_player_to_movement_direction = false;
                if (rotate_player_to_movement_direction) {
                    if (MyInput.gamepad_active || (!MyInput.gamepad_active && (kb_hor_moves || !holdingBackward))) {
                        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f) {
                            Rotate_towards_vector(rb.velocity,Time.fixedDeltaTime * 10f);
                        }
                    } else {
                        float camEulerY = cam.transform.eulerAngles.y;
                        if (holdingBackward) {
                            camEulerY += 180f;
                            if (camEulerY > 360f) camEulerY -= 360f;
                        }

                        transform.eulerAngles = new Vector3(0, camEulerY, 0);
                    }
                }

                #endregion

                #region Slowdown when no movement input

            } else { // Slow down when no input held

                if (jump_state == jump_state_midair && jump_trajectory_locked) {
                    if (just_vaulted_but_didnt_land) {
                        // don't apply the trajectory bc that'd be weird (instead require forward to be held)
                    } else if (fall_arc_has_xz_slowdown) {
                        temp_noInput_fallArcVel = locked_jump_trajectory_vel_xz;
                        if (rb.velocity.y < 0) {
                            // Same to the code when movement input is held, but velocity is calculated based on the locked vel.
                            float tm = tm_fall_arc_slowdown;
                            if (in_firstJumpArc_via_fallingWithoutJump) tm = tm_slowdown_during_a_fall_from_ledge;
                            HF.TimerStayAtMax(ref t_fall_arc_slowdown_wo_moveInput, tm, Time.fixedDeltaTime);
                            float minDrag = fall_arc_slowdown_xc_vel_multiplier;
                            if (in_firstJumpArc_via_fallingWithoutJump) minDrag = fallWithoutJump_slowdown_xc_vel_multiplier;

                            float fall_drag_factor = Mathf.Lerp(1f, minDrag, t_fall_arc_slowdown_wo_moveInput / tm);
                            temp_noInput_fallArcVel.x *= fall_drag_factor;
                            temp_noInput_fallArcVel.z *= fall_drag_factor;
                        }

                        temp_noInput_fallArcVel.y = rb.velocity.y;
                        rb.velocity = temp_noInput_fallArcVel;
                    } else {
                        t_fall_arc_slowdown_wo_moveInput = 0;
                    }
                } else {
                    if (t_sprint_release_to_drag > 0f) {
                        t_sprint_release_to_drag -= Time.fixedDeltaTime;
                    } else {
                        if (Mathf.Abs(moveY) < 0.2f && Mathf.Abs(moveX) < 0.2f) {
                            tempWalkVel = rb.velocity;
                            float wtsds = walk_to_stop_decel_strength;
                            if (active_ice.Count > 0) wtsds = ice_wtsds;
                            tempWalkVel.z = Mathf.Lerp(tempWalkVel.z, 0, Time.fixedDeltaTime * wtsds);
                            if (Mathf.Abs(tempWalkVel.z) < 0.3f) tempWalkVel.z = 0;
                            tempWalkVel.x = Mathf.Lerp(tempWalkVel.x, 0, Time.fixedDeltaTime * wtsds);
                            if (Mathf.Abs(tempWalkVel.x) < 0.3f) tempWalkVel.x = 0;
                            rb.velocity = tempWalkVel;
                        }
                    }
                }

                // If movement acceleration is on, then having this ensures that if a directional input is released
                // while jumping, you can still move again and have the same XZ velocity without the accel again
                if (jump_state == 0) t_walk_accel = 0;
                if (jump_state == 0) t_fall_arc_slowdown = 0;
                if (jump_state == 0) t_fall_arc_slowdown_wo_moveInput = 0;

                if (t_sprint_release_to_drag <= 0) {
                    AnimSetBool("Running", false);
                }
            }

            #endregion

            #region Final velocity adjustments for entities, dashing, avoiding walls, walljumps

            // Not in geyser: reduce how much the geyser affects XZ velocity by lerping to zero.
           //// if (Geyser.activeGeysers <= 0) {
                geyser_xz_momentum = Vector3.Lerp(geyser_xz_momentum, Vector3.zero, Time.fixedDeltaTime * lerpMul_reduceGeyserXZ);
                if (geyser_xz_momentum.magnitude < 0.5f || Is_touching_ground()) geyser_xz_momentum = Vector3.zero;
                // in geyser: increase y and xz velocity based on parameters from last geyser
                //// } else {
                ////geyser_xz_momentum = Vector3.Lerp(geyser_xz_momentum, Geyser.lastEnteredGeyserVel_XZ,
                ////  Time.deltaTime * lerpMul_increaseGeyserXZ);
////            }

            // The XZ component of the geyser velocity is calculated from its up transform. Use the _reduction to scale it (since it would otherwise be too fast)
            // Potentially this might need to be tweakable per-geyser, but seems okay for now.
            rb.velocity += geyser_xz_momentum * geyserXZ_reduction;

            if (stop_at_dash_start && pause_during_dash) {
                rb.velocity = Vector3.zero;
            }

            my_jp_dash = false;
            my_jp_sprint = false;

            if (!is_wallrunning && dash_mode != dash_mode_prismDash) {
                Adjust_velocity_for_walls(moveY);
            }

            if (t_walljump_pushoff > 0) {
                t_walljump_pushoff -= Time.fixedDeltaTime;
                temp_walljump_pushoff_vec = transform.position;
                temp_walljump_pushoff_vec += last_wallrun_vel_awayPerp * Time.fixedDeltaTime * walljump_pushoff_speed;
                transform.position = temp_walljump_pushoff_vec;
            }

            if (t_bump > 0) {
                t_bump -= Time.fixedDeltaTime;
                bumped_vel.y = rb.velocity.y;
                if (t_bump > 0.2f) {
                    rb.velocity = bumped_vel;
                } else {
                    rb.velocity = Vector3.Lerp(rb.velocity, bumped_vel, t_bump / 0.2f);
                }

                if (t_bump <= 0) t_bump = 0;
            }

            #endregion
        } // end of if(controls on)

        previous_rb_vel_without_connected_rb = rb.velocity;
        //print(rb.velocity);

        // Require a few frames to pass when OnCollisionExit gets called with the attached rigidbody.
        // By waiting, this way, 1 or 2 frame disconnects won't stop the attached rb from contributing to the player velocity (and causing bouncing while falling)
        if (tryDisconnectFromRB) {
            disconnectFromRBFrames++;
            if (disconnectFromRBFrames > 2) {
                disconnectFromRBFrames = 0;
                connected_rb = null;
                tryDisconnectFromRB = false;
            }
        }

        if (connected_rb != null) {
            connected_rb_vel = (connected_rb.position - connected_rb_pos) / Time.fixedDeltaTime;
            connected_rb_pos = connected_rb.position;
            // On the first frame, the calculated connected_rb_vel might be zero, so instead, use the moving platform's calculated velocity.
            if (justEnteredMovingPlatform) {
                justEnteredMovingPlatform = false;
                /*/*
                if (connected_squishyPlatform != null) {
                    connected_rb_vel = connected_squishyPlatform.calculatedVel;
                    connected_squishyPlatform = null;
                } else {
                    connected_rb_vel = connected_movingPlatform.calculatedVel;
                }*/

                if (connected_rb_vel.y < 0) connected_rb_vel.y -= 1f; // minus 1 to make player 'stick' more.
            }

            rb.velocity += connected_rb_vel;
        }

        #endregion

        #region Various single-frame flag resets, timer decrements

        if (!is_sprinting_speed_on) sprint_time_to_accel = 0f;
        was_in_gripshroom_last_frame = false;
        was_in_slipwall_last_frame = false;
        in_mud__resetInFixedUpdate = false;
        if (t_speedPodTime > 0) {
            t_speedPodTime -= Time.fixedDeltaTime;
            ////if (t_speedpodglow_sfx <= 0) SoundPlayer.instance.speedpod_glow();
            t_speedpodglow_sfx += Time.fixedDeltaTime;
            if (t_speedpodglow_sfx >= 4f) t_speedpodglow_sfx = 0;
        } else {
            t_speedpodglow_sfx = 0;
        }
        
        #endregion
    }

    Vector3 temp_wallPlayerModelPos;
    private void SetPlayerModelOffsetForWallrun() {
        temp_wallPlayerModelPos = activeModel.localPosition;
        temp_wallPlayerModelPos.x = wallrun_side == 1 ? 0.25f : -0.25f;
        ingwen_model.transform.localPosition = temp_wallPlayerModelPos;
    }
    private void UnsetPlayerModelOffsetForWallrun() {
        temp_wallPlayerModelPos = activeModel.localPosition;
        temp_wallPlayerModelPos.x = 0;
        ingwen_model.transform.localPosition = temp_wallPlayerModelPos;
    }

    private bool Maybe_break_breakableBlock(RaycastHit dash_sweep_hit) {
        /*/*
        DashReaction collided_dashReaction;
        collided_dashReaction = dash_sweep_hit.collider.gameObject.GetComponent<DashReaction>();
        if (collided_dashReaction != null) {
            if (collided_dashReaction.type == DashReaction.DashReactionType.BreakAndRespawnWithSwitch) {
                collided_dashReaction.Break_fromPlayerDash();
                return true;
            }
        }
*/
        return false;
    }

    internal void Add_speed_pod_time(float t) {
        t_speedPodTime += t;
        if (t_speedPodTime > tm_speedPodTime) t_speedPodTime = tm_speedPodTime;
    }

    private bool superdash_hack;
    internal void SuperDashZoneHack(bool on) {
        if (on) {
            superdash_hack = true;
            t_speedPodTime = 10;
            ////SpeedPod.current_used_pods = 3;
        } else {
            superdash_hack = false;
            t_speedPodTime = 0;
            ////SpeedPod.current_used_pods = 0;
        }
    }
    
    internal float t_speedPodTime;
    bool doing_SpeedPod_StrongDash;
    float tm_speedPodTime = 10f;
    float SpeedPodMaxSpeedup = 0.5f;

    private float Get_SpeedPodFactor(bool noAddOne = false) {
        float speedPodFac = 0;
        if (t_speedPodTime > 0) {
            ////speedPodFac = 1 + 0.15f * Mathf.Min(2, SpeedPod.current_used_pods - 1);
            if (inDungeontest) {
                speedPodFac *= 2f;
            }
        }

        if (noAddOne) return speedPodFac * SpeedPodMaxSpeedup;
        return 1 + speedPodFac * SpeedPodMaxSpeedup;
    }

    #endregion

    #region Jumping

    private float last_touching_ground_check_distance = 0;
    private Collider last_touching_ground_collider;

    void jumpLogic() {
        #region pre-FSM jump calculations

        bool touching_ground = false;
        last_touching_ground_check_distance = -1;
        if (is_sprinting_speed_on && jump_state == jump_state_ground) {
            touching_ground = Is_touching_ground(0.9f); //make  being on the ground 'easier' while sprinting so sprinting down slopes works better?  
        } else {
            touching_ground = Is_touching_ground(0.3f);
        }


        t_time_since_last_jump_input += Time.fixedDeltaTime;
        if (my_jp_jump) t_time_since_last_jump_input = 0;
        //   Debug.DrawRay(transform.position + cc.center * transform.localScale.y, -Vector3.up * (cc.bounds.extents.y + 0.05f));
        if (rb.velocity.y > 2 && jump_state != jump_state_ground) touching_ground = false;
        nextJumpVel = rb.velocity;
        float C_FallDampen = 1f;
        if (hold_hover_enabled && myJump && rb.velocity.y < 0) C_FallDampen = FallDampenValue;

        // Figure out how long to do half a jump based on initial vel and jump height
        float medJumpDeaccelTime = jump_height * 2 / jump_initial_vel;
        float medJumpDeaccel = jump_initial_vel / medJumpDeaccelTime;

        // Stops you from floating weirdly when going up slopes
        rb.useGravity = false;
        if (jump_state == jump_state_ground) rb.useGravity = true;
        if (ignoreJumpLanding_Counter > 0) ignoreJumpLanding_Counter -= Time.fixedDeltaTime;
        if (MyInput.shortcut && myJump && (jump_state == 1 || jump_state == 2)) nextJumpVel.y = jump_initial_vel * 1.5f;

        if (trigger_vault_jump) {
            // Can enter from _firstJumpArc (if exiting climbing), or already be in first jump arc. i.e., no state change needed? Just velocity chanage.
            last_firstJumpArc_starting_y = transform.position.y;
            if (!in_wind) {
                nextJumpVel.y = jump_initial_vel * vault_jumpVel_multiplier;
            } else {
                nextJumpVel.y = jump_initial_vel * vault_jumpVel_multiplier_wind;
            }

            if (is_sprinting_speed_on) nextJumpVel.y *= sprint_vault_yVel_mul;
            Cancel_sprint_state_only();
            jump_trajectory_locked = false; // Force a recalc
            just_vaulted_but_didnt_land = true;
            flippedDuringVault = false;
        }

        if (jump_state != jump_state_ground) {
            t_time_not_on_ground += Time.fixedDeltaTime;
        } else {
            t_time_not_on_ground = 0;
        }

        // Lock and unlock jump trajectory.
        if (jump_state == jumpstate_vine || jump_state == jump_state_ground || jump_state == jump_state_gliding ||
            jump_state == jumpstate_wallrunning) {
            jump_trajectory_locked = false;
        } else if (lock_jump_trajectory && jump_state == jump_state_midair && !jump_trajectory_locked && !MyInput.shortcut) {
            locked_jump_trajectory_vel_xz = rb.velocity;
            locked_jump_trajectory_vel_xz.y = 0;
            // Edge case: walking into a wall and jumping with forward velocity zero. Feels bad, so instead, give enough movement to push over the ledge (if there is one)
            if (trigger_vault_jump) {
                locked_jump_trajectory_vel_xz = transform.forward * vault_forwardSpeed_multiplier * max_walk_speed;
                locked_jump_trajectory_vel_xz.y = 0;
            } else if (holdingAnyMovement && locked_jump_trajectory_vel_xz.magnitude < 0.5f * max_walk_speed) {
                locked_jump_trajectory_vel_xz = transform.forward * 0.5f * max_walk_speed;
                locked_jump_trajectory_vel_xz.y = 0;
            } else if (!holdingAnyMovement) {
                if (locked_jump_trajectory_vel_xz.magnitude < 1f) {
                    locked_jump_trajectory_vel_xz = Vector3.zero;
                } else {
                    // This is running off a ledge or jumping after releasing sprint? So don't change the velocity from what the rigidbody already has.
                }
            }

            jump_trajectory_locked = true;
        }

        if (jump_state != jumpstate_wallrunning) {
            if (t_magnet_wall_glow > 0f) {
                t_magnet_wall_glow -= Time.fixedDeltaTime;
                if (t_magnet_wall_glow <= 0) t_magnet_wall_glow = 0;
                magnet_wall_mat.SetColor("_EmissionColor",Color.Lerp(Color.black,Color.white,t_magnet_wall_glow/0.5f));
                if (magnet_wall_mat2 != null) {
                    magnet_wall_mat2.SetColor("_EmissionColor",Color.Lerp(Color.black,Color.white,t_magnet_wall_glow/0.5f));
                }
            }
        }

        if (MyInput.shortcut) jump_trajectory_locked = false;

        trigger_vault_jump = false;

        if (jump_state == jump_state_ground && !touching_ground) {
        } else {
            t_fallJumpGrace = 0;
        }

        #endregion

        #region Jump - Grappling

        // Jump state logics
        if (jump_state == jumpstate_grappling) {
            // Pause a bit (windup anim?)
            if (grapple_submode == 0) {
                nextJumpVel = Vector3.zero;
                if (HF.TimerDefault(ref t_grapplePause, 0.15f)) {
                    grapple_submode = 1;
                }

                // Move towards the previously set grapple point. 
                // When in distance, return to falling state and reset your dash.
            } else if (grapple_submode == 1) {
                nextJumpVel = locked_grapple_pos - transform.position;
                nextJumpVel = nextJumpVel.normalized * grapple_speed;
                // polish - if hit obstacle, end grapple
                if (Vector3.Distance(transform.position, locked_grapple_pos) < 1f) {
                    ////locked_grapple_point.OnPlayerHit();
                    grapple_submode = 0;
                    is_grappling = false;
                    used_dash = false;
////                    if (locked_grapple_point.consumes_dash) Use_dash();
                    cam.startJumpCamMode();
////                    OnOffBlock.Send_grappletoggle_signal();
                    AnimSetTrigger("GrappleHop");
                    Enter_jumpstate_firstJumpArc();
                    ////                SoundPlayer.instance.grapple_hop();
                    ////vfx.WallVault(transform.position,Vector3.up);
                    ////vfx.White_pod_jump(transform.position);
                    nextJumpVel = Vector3.zero;
                    nextJumpVel.y = jump_initial_vel * 1.5f;
                    queueGrappleFlip = true;
                }
            }

            #endregion
        } else if (jump_state == jumpstate_wallrunning) {
            #region Jump - Wall Running

            if (t_wallrunIgnoreGround >= 0) t_wallrunIgnoreGround -= Time.fixedDeltaTime;
            float fdt = Time.fixedDeltaTime;

            #region wallrun fallspeed

            // Use var approaching zero to keep track of wallrun (because going around low poly surfaces means the wall angle usually only is the turn angle for a frame or two)
            if (Mathf.Abs(last_wallrun_turn_angle_signed) > 0.25f)
                shifting_last_wallrun_turnAngle = last_wallrun_turn_angle_signed;
            if (shifting_last_wallrun_turnAngle > 0) {
                shifting_last_wallrun_turnAngle -= fdt * 10f;
                if (shifting_last_wallrun_turnAngle < 0) shifting_last_wallrun_turnAngle = 0;
            }

            if (shifting_last_wallrun_turnAngle < 0) {
                shifting_last_wallrun_turnAngle += fdt * 10f;
                if (shifting_last_wallrun_turnAngle > 0) shifting_last_wallrun_turnAngle = 0;
            }

            // Fall faster down an exterior curve
            float fallSpeedMul = 1f;
            if (shifting_last_wallrun_turnAngle > 0 && wallrun_side == 1) fallSpeedMul *= wallrun_outInCurve_FallVelMul.y;
            if (shifting_last_wallrun_turnAngle < 0 && wallrun_side == -1) fallSpeedMul *= wallrun_outInCurve_FallVelMul.y;
            // Fall slower down a interior curve
            if (shifting_last_wallrun_turnAngle > 0 && wallrun_side == -1) fallSpeedMul *= wallrun_outInCurve_FallVelMul.x;
            if (shifting_last_wallrun_turnAngle < 0 && wallrun_side == 1) fallSpeedMul *= wallrun_outInCurve_FallVelMul.x;


            // Apply gravity
            nextJumpVel.y -= wallrun_gravity * fdt;
            ////  if (WallrunBooster.doBoost) {
////                nextJumpVel.y = WallrunBooster.g_boost_vel;
////                WallrunBooster.doBoost = false;
////            }

            nextJumpVel.y = Mathf.Clamp(nextJumpVel.y, -max_wallrun_fall_speed * fallSpeedMul, 10000);
            just_wallrun_jumped_and_not_on_ground = false;

            #endregion

            if (my_jp_jump || trigger_jumpstate_wallrunning__fallOff) {
                trigger_jumpstate_wallrunning__fallOff = false;
                is_wallrunning = false;
                if (magnet_wallrunning) {
                    magnet_wallrunning = false;
                    ////SoundPlayer.instance.magnet_off();
                }

                Enter_jumpstate_firstJumpArc();
                AnimSetTrigger("WallrunToJump");
                if (my_jp_jump) t_fixedInt_justJumped = 1;
                // Use movement dir to influence jump traj
                if (my_jp_jump) {
                    float cache_JumpVelY = nextJumpVel.y;
                    nextJumpVel.y = 0;
                    just_wallrun_jumped_and_not_on_ground = true;
                    if (wallrun_side == 1) {
                        if (MyInput.left) {
                            nextJumpVel = Vector3.Lerp(nextJumpVel, -1 * transform.right * nextJumpVel.magnitude, 0.4f);
                            wallRunJump_xzVel_booster = max_wallRunJump_xzVel_booster;
                        } else {
                            nextJumpVel = Vector3.Lerp(nextJumpVel, -1 * transform.right * nextJumpVel.magnitude, 0.2f);
                        }

                        Face_transform_to_vector_xz_dir(nextJumpVel, 0.5f);
                    }

                    if (wallrun_side == -1) {
                        if (MyInput.right) {
                            nextJumpVel = Vector3.Lerp(nextJumpVel, transform.right * nextJumpVel.magnitude, 0.4f);
                            wallRunJump_xzVel_booster = max_wallRunJump_xzVel_booster;
                        } else {
                            nextJumpVel = Vector3.Lerp(nextJumpVel, transform.right * nextJumpVel.magnitude, 0.2f);
                        }

                        Face_transform_to_vector_xz_dir(nextJumpVel, 0.5f);
                    }

                    if (MyInput.moveY < -0.4f) nextJumpVel *= 0.7f;
                    used_dash = false;
                    if (lock_jump_trajectory) {
                        jump_trajectory_locked = true;
                        locked_jump_trajectory_vel_xz = nextJumpVel;
                    }

                    Set_nextJumpVel_doAnim_playSFX(false);
                    if (nextJumpVel.y < cache_JumpVelY) nextJumpVel.y = cache_JumpVelY;

                    t_walljump_pushoff = tm_walljump_pushoff;
                    // Set away from wall, use this to do pushoff
                    if (wallrun_side == 1) last_wallrun_vel_awayPerp = transform.right * -1;
                    if (wallrun_side == -1) last_wallrun_vel_awayPerp = transform.right;
                }
            } else if (Is_touching_ground() && t_wallrunIgnoreGround <= 0) {
                is_wallrunning = false;
                if (magnet_wallrunning) {
                    magnet_wallrunning = false;
                    //// SoundPlayer.instance.magnet_off();
                }

                if (sprintDash_held && !test_3d_mode) {
                    Start_sprinting_anim_only();
                } else {
                    AnimPlay("Run");
                    AnimSetBool("Running", true);
                }

                jump_state = jump_state_ground;
            }

            if (is_wallrunning == false) {
                Start_sprinting_speed_only();
                ////vfx.Stop_wall_run();

                t_consec_wallrun_cooldown = 0.15f;
                t_consec_wallrun_sameside_cooldown = 0.85f;
                if (fell_off_wallrun_from_fwdOb) {
                    fell_off_wallrun_from_fwdOb = false;
                    Cancel_sprint_state_only();
                }
                UnsetPlayerModelOffsetForWallrun();

                if (current_wallrun_speed == wallrun_falloff_speed) Cancel_sprint_state_only();
                shifting_last_wallrun_turnAngle = 0;
                activeModel.localEulerAngles = new Vector3(0, 0, 0);
            }

            #endregion
        } else if (jump_state == jump_state_ground) {
            havent_landed_from_consec_wallrun = false;
            superdashed_in_sd_zone_wo_hit_something = false;
            #region Ground state jump

            // If dashing, or just dashed: allow jump buffering.
            if (dash_mode != 0 || (dash_mode == 0 && my_jp_dash)) {
                if (my_jp_jump) {
                    buffered_jump_during_teledash = true;
                }

                my_jp_jump = false;
            } else {
                if (buffered_jump_during_teledash) {
                    my_jp_jump = true;
                    buffered_jump_during_teledash = false;
                }
            }

            if (t_time_since_last_jump_input < 0.25f) {
                my_jp_jump = true;
            }

            rotation_after_glideCancel_enabled = false;
            //used_glide = false;
            used_dash = false;
            just_wallrun_jumped_and_not_on_ground = false;

            if (nojumpticks > 0) nojumpticks--;


            // Detect Jump
            if ((my_jp_jump && nojumpticks <= 0) /*/*||
                Geyser.activeGeysers > 0*/) {
                if (my_jp_jump) t_fixedInt_justJumped = 1;
                my_jp_jump = false;
                if (!OnMovingPlatform) cam.startJumpCamMode();
                if (MyInput.shortcut) nextJumpVel.y *= 1.5f;
                if (is_sprinting_speed_on) jumpedWhileSprinting = true;
                ////if (Geyser.activeGeysers > 0 && !my_jp_jump) {
                if (false) {
                    Set_nextJumpVel_doAnim_playSFX(true, false, true);
                    Enter_jumpstate_firstJumpArc();
                    nextJumpVel.y = 5f;
                } else if (tm_delay_after_jump_input > 0) {
                    jump_state = 100;
                } else {
                    Set_nextJumpVel_doAnim_playSFX();
                    Enter_jumpstate_firstJumpArc();
                }
            } else if (!touching_ground) {
                if (my_jp_dash) t_fallJumpGrace = tm_fallJumpGrace;
                if (HF.TimerDefault(ref t_fallJumpGrace, tm_fallJumpGrace)) {
                    Enter_jumpstate_firstJumpArc();
                    in_firstJumpArc_via_fallingWithoutJump = true;
                    cam.startJumpCamMode();
                }

                // Apply fake gravity when in coyote time
                nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel;
            }

            // If walking on a slope, add additional y-velocity so you stick closer to it
            Vector3 xz_vel = rb.velocity;
            xz_vel.y = 0;
            bool adjusted_y_vel_on_slope = false;
            
            float moveX = MyInput.moveX;
            float moveY = MyInput.moveY;
            if (is_sprinting_speed_on && SaveManager.sprint_camera_snapping_on) {
                if (Mathf.Abs(moveX) < 0.05f && Mathf.Abs(moveY) < 0.05f) {
                    moveX = MyInput.camX;
                }
            }
            
            
            bool moving_or_sprinting =
                Mathf.Abs(moveY) > 0.1f || Mathf.Abs(moveX) > 0.1f || is_sprinting_speed_on;
            if (moving_or_sprinting) {
                bool slope_up_test_1 = Physics.Raycast(
                    transform.position + transform.up * (-0.45f * cc.height + cc.center.y) * transform.localScale.y,
                    transform.forward, out RaycastHit hitInfoFwd, 1.5f, Registry.lmc_GROUND,
                    qti_ignore);
                // There's a second test here, slightly below and behind the player (the f+u * -0.1), bc at the top edge of a tent-shape sometimes you get stuck (when the first test point misses the slope)
                bool slope_up_test_2 = Physics.Raycast(
                    transform.position + (transform.forward + transform.up)*-0.1f + transform.up * (-0.45f * cc.height + cc.center.y) * transform.localScale.y,
                    transform.forward, out RaycastHit hitInfoFwd2, 1.5f, Registry.lmc_GROUND,
                    qti_ignore);
                    
                    //
                
                // If there's a slope in front of you (meaning moving upwards) then readjust the upwards velocity to not be too high
                if (slope_up_test_1 || slope_up_test_2) {
                    float slopeAng = 0;
                    if (slope_up_test_1) slopeAng = Get_slope_angle(hitInfoFwd.normal);
                    if (!slope_up_test_1 && slope_up_test_2) slopeAng = Get_slope_angle(hitInfoFwd2.normal);
                    //print("Up slope " + slopeAng);
                    if (slopeAng > 5f && slopeAng < 60f) {
                        float ideal_y_vel;
                        ideal_y_vel = Mathf.Tan(Mathf.Deg2Rad * slopeAng) * xz_vel.magnitude;
                        nextJumpVel.y = ideal_y_vel;
                        adjusted_y_vel_on_slope = true;
                    }

                    // Otherwise if moving down and there's a surface below you, apply a velocity
                } else if (rb.velocity.y < -0.5f && (Physics.Raycast(transform.position, transform.up * -1,
                    out RaycastHit hitInfo,
                    1f + (cc.height * transform.localScale.y / 2f), Registry.lmc_GROUND, qti_ignore))) {
                    float slopeAng = Get_slope_angle(hitInfo.normal);
  //                  print("Down slope " + slopeAng);
                    if (slopeAng > 5f && slopeAng < 60f) {
                        float ideal_y_vel;
                        ideal_y_vel = Mathf.Tan(Mathf.Deg2Rad * slopeAng) * xz_vel.magnitude * -1;
                        //Debug.DrawRay(transform.position, transform.up, Color.red, 5f);
                        //Debug.DrawRay(cam.transform.position, transform.up, Color.blue, 5f);
                        nextJumpVel.y =
                            ideal_y_vel * 1.55f; // 1.55x is an arbitrary scaling to make sure the collider stays on the slope
                        adjusted_y_vel_on_slope = true;
                    }

                    if (nextJumpVel.y > -2f) nextJumpVel.y = -2f;
                }
            }

            // Touching flat ground
            // Prevent idle sliding on gentle slopes 
            if (!adjusted_y_vel_on_slope && touching_ground && jump_state == jump_state_ground) {
                
                // When not moving and touching ground
                if (Mathf.Abs(moveX) < 0.1f && Mathf.Abs(moveY) < 0.1f && Physics.Raycast(transform.position+cc.center*transform.localScale.y,
                    transform.up * -1, out RaycastHit hitInfo,
                    1f + (cc.height * transform.localScale.y / 2f), Registry.lmc_GROUND, qti_ignore)) {
                    
                    
                    if (nextJumpVel.y > -2f) nextJumpVel.y = -2f; // Ensure when falling onto the ground that you fall through the height between the detection of being on ground and the collider touchign the ground.
                    if (Get_slope_angle(hitInfo.normal) > 60f) {
                        t_stop_y_vel_when_not_moving_on_slope = 0;
                    } else {
                        SetCapsuleCastVars(Vector3.zero);
                        float d = Vector3.Distance(hitInfo.point,ccBottom);
                        //print(d);
                        // 0.1f is arbitrary cutoff for 'close enough to the ground to not look weird when y-vel is stopped.
                        if (d <= 0.1f) {
                            t_stop_y_vel_when_not_moving_on_slope += Time.fixedDeltaTime;
                        } else {
                            t_stop_y_vel_when_not_moving_on_slope = 0f;
                        }
                        // After 0.5f seconds of the collider within d distance of the ground, actually stop any y velocity movement
                        if (t_stop_y_vel_when_not_moving_on_slope > 0.5f) {
                            nextJumpVel.y = 0f;
                            rb.useGravity = false;
                        }
                    }
                    
                // Moving, on a flat ground.
                } else {
                    t_stop_y_vel_when_not_moving_on_slope = 0;
                    nextJumpVel.y = -2f; // Always be slightly moving downwards on flat land
                }

                if (connected_rb != null && connected_rb_vel.y < 1 && nextJumpVel.y > -1) {
                    nextJumpVel.y = -1f; // On a moving platform that's moving down, set max velocity to -1 so you stick to it
                } 

                // print(nextJumpVel.y);
                
            // Moving on a slope, or changing states, or somehow not near the ground
            } else {
                // print("Not adjusting on gentle slope");
                t_stop_y_vel_when_not_moving_on_slope = 0;
            }
//            Debug.DrawRay(transform.position + transform.up*(-0.45f*cc.height+cc.center.y)*transform.localScale.y, transform.forward*1.5f,Color.red, 0.5f);

//            print(nextJumpVel.y);

            #endregion
        } else if (jump_state == 100) {
            if (HF.TimerDefault(ref t_delay_after_jump_input, tm_delay_after_jump_input, Time.fixedDeltaTime)) {
                Set_nextJumpVel_doAnim_playSFX();

                Enter_jumpstate_firstJumpArc();
                enter_firstJumpArc_yVelRatioToNormal = nextJumpVel.y / jump_initial_vel;
                hasVariableJumpHeight = true;
            }
        } else if (jump_state == jump_state_midair) { //First jump, wait for gruond or double jump

            // If not holding jump flying shortcut
            if (!(MyInput.shortcut && myJump)) {
                if (do_equal_bounce) {
                    nextJumpVel.y = jump_initial_vel;
                    if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hitInfo, 3f, Registry.lmc_GROUND,
                        qti_ignore)) {
                        do_equal_bounce = false; // cancel equalbounce at ceilings
                    }

                    if (last_firstJumpArc_starting_y == -10000f || transform.position.y >= last_firstJumpArc_starting_y) {
                        do_equal_bounce = false;
                    }
                } else {
                    // Apply gravity
                    /*/*
                    if (Geyser.activeGeysers > 0) {
                        hasVariableJumpHeight = false;
                        if (nextJumpVel.y <= 0) {
                            // Fall slower near the top
                            if (transform.position.y >=
                                Geyser.lastEnteredGeyser_meshTopY - Geyser.lastEnteredGeyser_weakZoneOffset) {
                                nextJumpVel.y += Time.fixedDeltaTime * Geyser.lastEnteredGeyserYAccelWhileFalling *
                                                 Geyser.lastEnteredGeyser_weakZoneFallAccelMul;
                            } else {
                                nextJumpVel.y += Time.fixedDeltaTime * Geyser.lastEnteredGeyserYAccelWhileFalling;
                            }
                        } else {
                            nextJumpVel.y += Time.fixedDeltaTime * Geyser.lastEnteredGeyserYAccel;
                            if (nextJumpVel.y >= Geyser.lastEnteredGeyserVel.y) nextJumpVel.y = Geyser.lastEnteredGeyserVel.y;
                        }
                    } else if (in_wind) {
                        if (nextJumpVel.y > 0) {
                            nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel * C_FallDampen * wind_upwardsGrav_mul;
                        } else {
                            nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel * C_FallDampen * wind_downwardsGrav_mul;
                        }
                    } else {*/
                    nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel * C_FallDampen;

                    if (!myJump && !just_vaulted_but_didnt_land && hasVariableJumpHeight) {
                        if (nextJumpVel.y > 0) { // Don't apply this if falling  - not intuitive
                            nextJumpVel.y -= Time.fixedDeltaTime * 0.5f * medJumpDeaccel * C_FallDampen;
                        }
                    }

                    if (in_sink_cloud) {
                        if (nextJumpVel.y < sink_cloud_min_yVel) {
                            nextJumpVel.y = sink_cloud_min_yVel;
                        }
                    }


                    if (do_anim_jumpUp_to_flip) {
//                        print(nextJumpVel.y);
                        if (nextJumpVel.y < 0) {
                            do_anim_jumpUp_to_flip = false;
                            AnimResetTrigger("JumpUp_To_Flip");
                        } else if (nextJumpVel.y < vaultFlipThreshold) {
                            AnimSetTrigger("JumpUp_To_Flip");
                        }
                    }
                }
            }

            if (animator.GetBool("BouncingUpwards") && nextJumpVel.y < 0) {
                AnimSetBool("BouncingUpwards", false);
                
            }


            if (MyInput.shortcut) my_jp_jump = false;
            if (rotation_after_glideCancel_enabled) {
                localEuler = transform.localEulerAngles;
                localEuler.y = cam.transform.localEulerAngles.y;
                transform.localEulerAngles = localEuler;
            }

            if (t_glideCancelCooldown > 0) t_glideCancelCooldown -= Time.fixedDeltaTime;
            if (trigger_jumpstate_enter_wallrun) {
                trigger_jumpstate_enter_wallrun = false;
                jump_state = jumpstate_wallrunning;
                ////vfx.Jump_land_poof();
                jump_trajectory_locked = false;
                // Calculate correct velocity so that you can only gain a fixed amount of height when entering wallrun.
                float max_regular_wallrun_entry_height =
                    5f * enter_firstJumpArc_yVelRatioToNormal *
                    enter_firstJumpArc_yVelRatioToNormal; // Use this so that holding whitepods/etc makes you be able to go higher
                //print(max_regular_wallrun_entry_height);
                float height_left_in_wallrun_entry_arc =
                    max_regular_wallrun_entry_height - (transform.position.y - last_firstJumpArc_starting_y);
                height_left_in_wallrun_entry_arc =
                    Mathf.Clamp(height_left_in_wallrun_entry_arc, 0, max_regular_wallrun_entry_height);

                // Derived via: vt/2 = d = height_left...
                // v = a*t where a = wallrun_gravity
                float new_entry_vel = Mathf.Sqrt(wallrun_gravity * 2 * height_left_in_wallrun_entry_arc);
                //print(new_entry_vel);
                if (nextJumpVel.y > -1) { // Don't boost upwards if already falling
                    nextJumpVel.y = new_entry_vel;
                }

                if (magnet_wallrunning) {
                    nextJumpVel.y = 0;
                }
            } else if (ignoreJumpLanding_Counter <= 0 && touching_ground) {
                // temp HoverParticles.Stop();
                cam.stopJumpCamMode();

                ////if (last_touching_ground_check_distance != -1 && last_touching_ground_collider.GetComponent<MovingPlatform>() != null) {
                if (last_touching_ground_check_distance != -1 && false) {
                    // Don't zero out the y-vel when landing on a moving platform, so you actually register a collision with it
                } else {
                    nextJumpVel.y = 0;
                }

                if (just_vaulted_but_didnt_land) {
                    just_vaulted_but_didnt_land = false;
                    if (sprintDash_held && !test_3d_mode) {
                        Start_sprinting_speed_only(); // Sprint -after landing from vault- while -sprint held-.
                        Start_sprinting_anim_only();
                    } else {
                        Cancel_sprint_state_and_anim();
                    }
                } else {
                    if (sprintDash_held && !test_3d_mode) {
                        Start_sprinting_anim_only();
                        Start_sprinting_speed_only(); // Sprint -after landing- while -sprint held-.
                    }
                }

                jump_state = jump_state_ground;
                
                ////vfx.Jump_land_poof();
                if (in_firstJumpArc_via_fallingWithoutJump) in_firstJumpArc_via_fallingWithoutJump = false;
                t_glideCancelCooldown = 0;
                jumpedWhileSprinting = false;


                // Enter Glide mode
            } else if (trig_js_midairToGlide) {
                trig_js_midairToGlide = false;
                //} else if (my_jp_jump && glide_enabled && !used_glide && dash_mode == 0 && t_glideCancelCooldown <= 0) {
                jump_state = jump_state_gliding;
                if (in_firstJumpArc_via_fallingWithoutJump) in_firstJumpArc_via_fallingWithoutJump = false;
                just_vaulted_but_didnt_land = false;
                //if (only_one_glide) used_glide = true;
                glide_rotationalVel = 0;
                Vector3 tempVel = rb.velocity;
                tempVel.y = 0;

                cur_glide_speed = tempVel.magnitude * glide_initMomentumReducer;
                nextJumpVel.y = jump_initial_vel;
                if (nextJumpVel.y > glide_beginToStabilize_velThreshold) {
                    glide_needToReachFastFallingSpeed = true;
                }

                if (!fancy_glide_beginning_on) {
                    glide_needToReachFastFallingSpeed = false;
                    cur_glide_speed = base_glide_speed;
                }
            }else if (my_jp_jump && SaveManager.infiniteJump) {
                my_jp_jump = false;
                t_fixedInt_justJumped = 1;
                Set_nextJumpVel_doAnim_playSFX();
            } else if (my_jp_jump && grapple_ability_enabled) {
                // Only grapple if there's a grapple point
                /*/*if (closest_grapple_point != null && closest_grapple_point.ClearToPlayer()) {
                    locked_grapple_pos = closest_grapple_point.transform.position;
                    closest_grapple_point.Cooldown(same_grapple_cooldown);
                    locked_grapple_point = closest_grapple_point;
                    is_grappling = true;
                    cam.stopJumpCamMode();
                    Cancel_sprint_state_and_anim();
                    jump_state = jumpstate_grappling;
                    SoundPlayer.instance.use_grapple();
                    AnimPlay("Grapple");
                    Face_transform_to_vector_xz_dir(locked_grapple_pos - transform.position);
                }*/
            } else if (my_jp_jump && double_jump_enabled) {
                my_jp_jump = false;
                t_fixedInt_justJumped = 1;
                Set_nextJumpVel_doAnim_playSFX();
                jump_state = 2;
            }

            if (jump_state != jump_state_midair) {
                hasVariableJumpHeight = false;
                last_firstJumpArc_starting_y = -10000;
                AnimResetTrigger("WallrunToJump");
            }
        } else if (jump_state == jump_state_gliding) {
            // The idea with this is that you don't want to hit the ground too soon during regular jumps - glides should be pretty useless on the ground.
            if (glide_needToReachFastFallingSpeed) { // if not falling fast enough to glide, fall as normal
                nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel * C_FallDampen;
                if (nextJumpVel.y <= glide_beginToStabilize_velThreshold) {
                    glide_needToReachFastFallingSpeed = false;
                }
            } else { // if falling fast enough to glide, begin to change velocity to reach 'gliding fall velocity'.
                if (nextJumpVel.y < glide_stabilizedFall_vel) {
                    nextJumpVel.y += Time.fixedDeltaTime * glide_fallSpeedStableRate;
                    if (nextJumpVel.y > glide_stabilizedFall_vel) nextJumpVel.y = glide_stabilizedFall_vel;
                } else if (nextJumpVel.y > glide_stabilizedFall_vel) {
                    nextJumpVel.y -= Time.fixedDeltaTime * glide_fallSpeedStableRate;
                    if (nextJumpVel.y < glide_stabilizedFall_vel) nextJumpVel.y = glide_stabilizedFall_vel;
                }
            }

            faceDir_at_glideStart = transform.forward;
            faceDir_at_glideStart.y = 0;
            t_glideOrbTime -= Time.fixedDeltaTime;

            if (ignoreJumpLanding_Counter <= 0 && touching_ground) {
                // temp HoverParticles.Stop();
                cam.stopJumpCamMode();
                nextJumpVel.y = 0;
                jump_state = 0;
                t_glideOrbTime = 0;
                ////vfx.Hide_glide_wings();
                if (jumpedWhileSprinting) jumpedWhileSprinting = false;
            } else if (my_jp_jump || dash_mode == dash_mode_windup || t_glideOrbTime <= 0) {
                // cancel out of glide from jump or dash
                // anims
                t_glideOrbTime = 0;
                if (has_rotate_after_glideCancel_ability) rotation_after_glideCancel_enabled = true;
                t_glideCancelCooldown = tm_glideCancelCooldown;
                if (my_jp_jump) {
                    t_fixedInt_justJumped = 1;
                    Set_nextJumpVel_doAnim_playSFX();
                }

                Enter_jumpstate_firstJumpArc();
                ////vfx.Hide_glide_wings();
            }
        } else if (jump_state == 2) { // Second jump, wait for ground.
            if (!(MyInput.shortcut && myJump)) {
                nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel * C_FallDampen;
            }

            /* temp
            if (myJump && nextJumpVel.y < -1f) {
                if (!HoverParticles.isPlaying) {
                    HoverParticles.Play();
                }
            } else {
                if (HoverParticles.isPlaying) {
                    HoverParticles.Stop();
                }
            }
            */
            if (ignoreJumpLanding_Counter <= 0 && touching_ground) {
                // temp HoverParticles.Stop();
                cam.stopJumpCamMode();
                nextJumpVel.y = 0;
                jump_state = 0;
            }
        }

        #region Animate based on Jump State

        if (just_vaulted_but_didnt_land && !flippedDuringVault) {
            if (nextJumpVel.y < vaultFlipThreshold) {
                AnimSetTrigger("VaultFlip");
                flippedDuringVault = true;
            }
        }

        if (queueGrappleFlip) {
            if (nextJumpVel.y < grappleFlipThreshold) {
                AnimSetTrigger("GrappleFlip");
                queueGrappleFlip = false;
            }
        }

        if (nextJumpVel.y < -0.5f && !Is_touching_ground(1f)) {
            AnimSetBool("Falling", true);
        } else if ((animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUp") ||
                    animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUpSprint") ||
                    animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUpHurt")) && nextJumpVel.y <= 0 &&
                   Is_touching_ground(0.5f)) {
            // For reaching a platform but having tiny velocity
            AnimSetBool("Falling", true);
        } else {
            if (!sprintDash_held) {
                AnimSetBool("Sprinting", false);
                ////vfx.Stop_run_dust();
                ////vfx.Stop_run_trail();
            }

            if (jump_state != jump_state_gliding) {
                AnimSetBool("Falling", false);
            }
        }

        #endregion

        if (hold_hover_enabled && myJump && nextJumpVel.y < -7f) nextJumpVel.y = -7f; // Allow hovering when jump is held
        rb.velocity = nextJumpVel;
        if (nextJumpVel.y < -maxMedFallVel) {
            nextJumpVel.y = -maxMedFallVel;
            rb.velocity = nextJumpVel;
        }

        yVel_5FrameCount[yVel_5FrameAverage_idx] = rb.velocity.y;
        yVel_5FrameAverage_idx = (yVel_5FrameAverage_idx + 1) % 5;
        yVel_5FrameAverage = yVel_5FrameCount.Sum() / 5f;
    }

    #endregion

    internal float yVel_5FrameAverage;
    private int yVel_5FrameAverage_idx = 0;
    private float[] yVel_5FrameCount = new float[5];

    #region Movement-related state changing helpers

    private void Start_sprinting_anim_only() {
        AnimSetBool("Sprinting", true);
        AnimPlay("Sprint");
        ////vfx.Start_run_dust();
        ////vfx.Start_run_trail(activeModel.name);
        if (t_time_since_jp_dash < 0.1f) {
            Pulse_emission(Get_active_trio_dashEmission_color(),0.8f,0.1f);
        }
    }

    private void Start_sprinting_speed_only() {
        is_sprinting_speed_on = true;
        sprint_rotVel = 0;
        //print("Enter sprint");
    }

    private void Exit_climbing_mode() {
    }

    private void Cancel_sprint_state_and_anim() {
        AnimSetBool("Sprinting", false);
        
        ////vfx.Stop_run_dust();
        ////vfx.Stop_run_trail();
        Cancel_sprint_state_only();
    }

    private void Cancel_sprint_state_only() {
        //print("Exit sprint");
        wasSprintingIn_NoSprintT_ButDidntLand = false;
        is_sprinting_speed_on = false;
        t_sprint_holdingBackInAir = 0;
    }


    internal void Set_nextJumpVel_doAnim_playSFX(bool applyJumpTrigger = true, bool applyToRigidbody = false,
        bool skipWhitepod = false,bool skipsound=false) {
        nextJumpVel.y = jump_initial_vel;
        if (applyJumpTrigger) AnimSetTrigger("Jump");
        /*/*
        if (WhitePod.Num_held_pods() > 0 && !skipWhitepod) {
            WhitePod.Use_pod();
            Set_emission_color(whitePodJump_emission_color);
            Start_emission_lerp(0.5f,true,Color.white);
            vfx.White_pod_jump(transform.position);
            nextJumpVel.y *= whitePod_jumpVelMul;
            SoundPlayer.instance.jump();
            SoundPlayer.instance.podjump();
        } else if (in_sink_cloud) {
            SoundPlayer.instance.jump();
            //SoundPlayer.instance.podjump();
            nextJumpVel.y *= whitePod_jumpVelMul;
        } else if (HighJumpPlatform.active_highJumpPlatforms > 0) {
            nextJumpVel.y *= whitePod_jumpVelMul;
            SoundPlayer.instance.jump();
            SoundPlayer.instance.podjump();
            HighJumpPlatform.justjumped = true;

        } else {
            if (!skipsound && Geyser.activeGeysers == 0) {
                SoundPlayer.instance.jump();
            }
        }*/

        if (in_wind) {
            nextJumpVel.y *= wind_yVel_mul;
        }

        if (in_mud__resetInFixedUpdate) {
            nextJumpVel *= mud_jump_mul;
        }

        if (noSprintTriggers_Inside > 0) {
            nextJumpVel *= 0.85f;
        }

        if (applyToRigidbody) {
            rb.velocity = nextJumpVel;
        }
    }

    #endregion


    bool tetherPlaced;
    bool justReturnedToTether;

    private void Update_tether() {
        /*/*
        if (Registry.DEV_MODE_ON && Input.GetKey(KeyCode.T) && Is_touching_ground()) {
            t_placingTether += Time.fixedDeltaTime;
            float f = 0.001f + t_placingTether / 1f;
            tetherScale.Set(f, f, f);
            growingTetherObject.transform.localScale = tetherScale;
            growingTetherObject.transform.position = transform.position;
            if (t_placingTether >= 1f) {
                t_placingTether = 0;
                placedTetherObject.transform.position = growingTetherObject.transform.position;
                placedTetherObject.transform.localScale = tetherScale;
                growingTetherObject.transform.localScale = Vector3.zero;
                tetherScale.Set(0, 0, 0);
                tetherReturnPosition = transform.position;
                tetherPlaced = true;
            }
        } else {
            if (tetherScale.x > 0) {
                tetherScale.Set(0, 0, 0);
                growingTetherObject.transform.localScale = tetherScale;
            }

            t_placingTether = 0;
        }

        if (Registry.DEV_MODE_ON && Input.GetKey(KeyCode.R) && !justReturnedToTether && tetherPlaced && Is_touching_ground()) {
            float f = 0.501f - 0.5f * (t_returnToTether / 0.5f);
            returnToTether_scale.Set(f, f, f);
            activeModel.transform.localScale = returnToTether_scale;
            if (HF.TimerDefault(ref t_returnToTether, 0.5f)) {
                Warp_to(tetherReturnPosition);
                returnToTether_scale.Set(0.5f, 0.5f, 0.5f);
                justReturnedToTether = true;
                activeModel.transform.localScale = activeModel_originalScale;
            }
        } else {
            if (!Input.GetKey(KeyCode.R)) justReturnedToTether = false;
            if (t_returnToTether > 0) {
                returnToTether_scale.Set(0.5f, 0.5f, 0.5f);
                activeModel.transform.localScale = activeModel_originalScale;
                t_returnToTether = 0;
            }
        }
        */
    }

    public void Warp_to(Vector3 pos, bool changeCamEulerY = false, float camEulerY = -1) {
        transform.position = pos;
        if (changeCamEulerY) {
            cam.setCameraEulerYRotation(camEulerY);
        }
    }

    public float getVel(bool noY = false) {
        if (noY) {
            return Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        }

        return rb.velocity.magnitude;
    }

    public static int times_new_scene_entered_in_this_session;

    void Update() {
        bool playSounds = true;
        bool playAnimations = true;
        
        if (Registry.DEV_MODE_ON && MyInput.shortcut && Input.GetKeyDown(KeyCode.Alpha4)) {
            hideUItrailer = true;
        }

        if (in_scene_initialization_phase) {
            if (scene_initialization_mode == 0) {
                // Frame 1 - do nothing (wait for event evaluation). Player is positioned/camera moved this frame.
                scene_initialization_mode = 1;
            } else if (scene_initialization_mode == 1) { // Frame 2 - begin player control, or block for events
                ////if (MyEvent.scheduler_event_running) { // This is set by the master event handler
                if (false) { // This is set by the master event handler
                    scene_initialization_mode = 2;
                } else {
                    scene_initialization_mode = 3;
                }
            } else if (scene_initialization_mode == 2) { // Block for events
                ////if (!MyEvent.scheduler_event_running) {
                if (true) {
                    scene_initialization_mode = 3;
                }
            } else if (scene_initialization_mode == 3) { // Extra frame to do stuff
                if (times_new_scene_entered_in_this_session > 0) {
                    // Don't autosave on game load or entering scene from editor. Reset in reset_statics
                    ////ui.Autosave(transform.position, 2);
                }

                sceneStartPos = transform.position; // This is set AFTER all event Finalizations
                scene_initialization_mode = 100;
                times_new_scene_entered_in_this_session++;
            } else if (scene_initialization_mode == 100) {
                in_scene_initialization_phase = false;
                // Done initializing.
            }
        } else {
            if (isThereAnyReasonToPause()) {
                playAnimations = false;
                playSounds = false;
                //  controlsOn = false;
            } else {
                if (MyInput.jpSwitchChars && activeModel.gameObject.activeInHierarchy) {
                    SwitchActiveModel();
                    if (play_restrict_message) {
                        ////dbox.playDialogue("bubble-challenge", 1);
                    }

                    play_restrict_message = false;
                }

                if (MyInput.jpJump) {
                    my_jp_jump = true;
                }

                myJump = MyInput.jump;
                t_time_since_jp_dash += Time.deltaTime;
                if (MyInput.jp_dash_separate && !require_release_of_cancel_to_sprint && !is_wallrunning) {
                    my_jp_dash = true;
                    t_time_since_jp_dash = 0f;
                }

                if (MyInput.jp_sprintDash && !require_release_of_cancel_to_sprint && !is_wallrunning) {
                    my_jp_sprint = true;
                }

                if (test_3d_mode) my_jp_sprint = false;
                if (test_3d_mode) my_jp_dash = false;
            }

            if (playSounds) {
                updateSounds();
            }

            if (playAnimations) {
            }
        }


        if (sprint_time_to_accel > 0) {
            //display_debug_text_next_frame("Sprint Accel Time: " + HF.TruncateFloat(sprint_time_to_accel));
        }

        if (t_glideOrbTime > 0 && !hideUItrailer) {
            ////ui.Set_glide_meter(t_glideOrbTime);
        } else {
            ////ui.Set_glide_meter(0);
        }

        ////if (SpeedPod.current_used_pods >= 3) {
        if (false) {
            if (!emission_osc_playing) {
                Osc_emission();
                emission_osc_playing = true;
            }
        } else {
            if (emission_osc_playing) {
                Stop_osc_emission();
                emission_osc_playing = false;
            }
        }
        if (t_speedPodTime > 0 && !hideUItrailer) {
            //display_debug_text_next_frame("Speed Pod Time: " + HF.TruncateFloat(t_speedPodTime));
            //display_debug_text_next_frame("Pod Combo (speed maxes at 3): " + SpeedPod.current_used_pods);
            ////ui.Set_speed_meter(t_speedPodTime);
        } else {
            ////ui.Set_speed_meter(0);
        }

        if (t_return_to_checkpoint > 0) {
            ////ui.Set_checkpoint_meter(t_return_to_checkpoint,0.33f);
        } else {
            ////ui.Set_checkpoint_meter(0,0.33f);
        }

        if (Registry.DEV_MODE_ON && !Application.isEditor) {
            display_debug_text_next_frame("DEBUG ON");
        }

        /*/*
        if (cam != null) {
            if (cam.t_hold_ss_mode > 0) {
                ui.Set_ss_mode_meter(cam.t_hold_ss_mode,0.3f);
            } else {
                ui.Set_ss_mode_meter(0,0.3f);
            }
        }
        
        if (t_poisonShroom > 0) {
            if (t_poisonshroom_sfx <= 0) SoundPlayer.instance.poisonshroom();
            t_poisonshroom_sfx += Time.deltaTime;
            if (t_poisonshroom_sfx >= 2f) t_poisonshroom_sfx = 0;
            if (!hideUItrailer) {
                if (hasPoisonResistance) {
                    ui.Set_poison_meter(t_poisonShroom, tm_hasPoisonRes);
                } else {
                    ui.Set_poison_meter(t_poisonShroom, tm_noPoisonRes);
                }
            }
        } else {
            t_poisonshroom_sfx = 0;
            ui.Set_poison_meter(0,tm_noPoisonRes);
        }
        */

        if (toggleableDebugTextOn) {
            if (hasdirlightfordebug) {
                display_debug_text_next_frame("Light euler: "+ HF.TruncateFloat(testlight.localRotation.x)+","+HF.TruncateFloat(testlight.localRotation.y)+","+HF.TruncateFloat(testlight.localRotation.z));
                
            }

            display_debug_text_next_frame("Sprint Angular Vel: " + HF.TruncateFloat(sprint_rotVel));
            display_debug_text_next_frame("XZ Speed " +
                                          HF.TruncateFloat(Mathf.Sqrt(rb.velocity.x * rb.velocity.x +
                                                                      rb.velocity.z * rb.velocity.z)));
            display_debug_text_next_frame("Y Speed " + HF.TruncateFloat(rb.velocity.y));
        }

        if (MyInput.shortcut && Input.GetKeyDown(KeyCode.D) && !paused_during_linking) {
            toggleableDebugTextOn = !toggleableDebugTextOn;
        }

        ////player_debug_text.text = "";
        foreach (string s in debug_text_to_show) {
            ////player_debug_text.text += s + "\n";
        }

        debug_text_to_show.Clear();
        
        Shader.SetGlobalVector("Global_PlayerPos",transform.position);

        if (used_dash__wasSetToTrue) {
            if (!used_dash) {
                used_dash__wasSetToTrue = false;
                /*/*vfx.Dash_restore_ring();
                Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
                foreach (Cel_EmissionHelper helper in helpers) {
                    helper.Stop_tint_flicker();
                }
                */
            }
        } else {
            if (used_dash) {
                
                /*/*Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
                foreach (Cel_EmissionHelper helper in helpers) {
                    helper.Do_tint_flicker(10f);
                }
                */
                
                used_dash__wasSetToTrue = true;
            }
        }


        if (rb.velocity.magnitude > 4.5f && (jump_state == jump_state_ground || jump_state == jumpstate_wallrunning))  {
            if (queue_sprint_sound || queue_run_sound) {
                if (was_in_mud_last_FixedUpdate) {
                    ////SoundPlayer.instance.footstep_mud();
                //} else if (vfx.go_ripple.isEmitting) {
                } else if (false) {
                    ////SoundPlayer.instance.footstep_water();
                } else if (noSprintTriggers_Inside > 0) {
                    ////SoundPlayer.instance.footstep_grass();
                } else {
                    ////SoundPlayer.instance.footstep();
                }
            }
        }

        queue_run_sound = queue_sprint_sound = false;


        // DashReaction's logic runs in Update. So, when vaulting from a superdash, try_post.. is set to 3.
        // If the superdash hits a rock, then last_superdash... is set to false. So the screenshake logic doesn't run here.
        // If there's no hit rock, then last_superdash... is true, so the screen shakes
        if (try_post_superdash_fx > 0) {
            try_post_superdash_fx--;
            if (try_post_superdash_fx == 0) {
                if (last_superdash_didnt_hit_breakable) {
                    ////DataLoader.instance.t_timescale_pause = 0.14f;
                    ////SoundPlayer.instance.bumper();
                    cam.shakeon = 3f;
                }
                last_superdash_didnt_hit_breakable = true;
            }
        }
        
    }

    List<string> debug_text_to_show = new List<string>();

    public void display_debug_text_next_frame(string s) {
        debug_text_to_show.Add(s);
    }

    private bool toggleableDebugTextOn = false;
    Vector3 tempWalkVel;
    Vector3 tempWalkVel_NoY = new Vector3();
    Vector3 origWalkVel_NEVERCHANGES;
    Vector3 origWalkVel_NEVERCHANGES_NoY;

    void Adjust_velocity_for_walls(float moveY) {
        tempWalkVel = rb.velocity;
        origWalkVel_NEVERCHANGES = rb.velocity;
        origWalkVel_NEVERCHANGES_NoY = rb.velocity;
        origWalkVel_NEVERCHANGES_NoY.y = 0;

        if (test_3d_mode) return;
        
        int layerMask = Registry.lmc_GROUND; // only detect these
        tempWalkVel_NoY.Set(tempWalkVel.x, 0, tempWalkVel.z);

        // Add a helper y-boost if almost at the lip of something and holding forward/jump.
        // Only do this if your bottom is hitting and your top is not, and you are hitting a vertical-ish wall
        bool gotYBoost = false;
        /* Removed 3/6/2020 - No need bc of teledash vault?
        if (myJump && (tempWalkVel_NoY.x != 0 || tempWalkVel_NoY.z != 0)) {
            // From bottom of collider, towards velocity
            Ray yBoostray = new Ray(transform.position + new Vector3(0, -cc.bounds.extents.y, 0), tempWalkVel_NoY);
    
           // Debug.DrawRay(yBoostray.origin, yBoostray.direction * 3f, Color.blue);
           // Debug.DrawRay(yBoostray.origin + new Vector3(0, 0.8f, 0), yBoostray.direction * 3f, Color.blue);

            // Cast the above ray, then cast again 0.8 units up
            if (Physics.Raycast(yBoostray, out tempHitInfo, 1f, layerMask, qti_ignore)) {
                if (Vector3.Angle(tempHitInfo.normal, new Vector3(tempHitInfo.normal.x, 0, tempHitInfo.normal.z)) < 5f) {
                    yBoostray.origin = yBoostray.origin + new Vector3(0, 0.8f, 0);
                    if (!Physics.Raycast(yBoostray, 1f, layerMask, qti_ignore)) {
                        tempWalkVel = tempWalkVel + new Vector3(0, 1f, 0);
                        if (tempWalkVel.y <= 1) tempWalkVel.y = 1f;
                        tempWalkVel.x = 0; tempWalkVel.z = 0;
                        gotYBoost = true;
                    }
                }
            }
        }
        */


        // Move transform backwards, then sweep test forwards.
        // Set x and z vel to zero if a hit detected. Should prevent issues with physics stopping all v movement, even
        // in the case where collider is exactly up against the wall.
        tempV1 = transform.position;
        tempV2 = transform.position;
        tempV1 -= transform.forward.normalized;
        transform.position = tempV1;
        float castDist = 1.2f;
        if ((is_sprinting_speed_on) && jump_state == jump_state_midair) castDist = 0.3f;
        //if (jump_state == jumpstate_grappling) castDist = 0.3f;
        if (rb.SweepTest(origWalkVel_NEVERCHANGES_NoY, out tempHitInfo, castDist, qti_ignore)) {
            float slopeAngle = Vector3.Angle(tempHitInfo.normal, Vector3.up);
            if ((holdingAnyMovement) && slopeAngle > 45f && jump_state != 0) {
                tempWalkVel.x = 0;
                tempWalkVel.z = 0;
            }
        }

        transform.position = tempV2;

        SetCapsuleCastVars(rb.velocity * -1, dbl_capCast_movement_backup, dbp_cc_r_mul);

        //Debug.DrawRay(ccp1, origWalkVel_NEVERCHANGES_NoY.normalized * dbl_capCast_castDistance,Color.red);
        //Debug.DrawRay(ccp2, origWalkVel_NEVERCHANGES_NoY.normalized * dbl_capCast_castDistance,Color.red);
        // Capsule cast to check if a wall is hit. If so, then change velocity to the closer of the normal's two perp vectors to push along wall
        //string res = "";

        bool ignore_parallelization = false;
        if (jump_state == jumpstate_grappling) ignore_parallelization = true;

        if (!ignore_parallelization && !gotYBoost && Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, origWalkVel_NEVERCHANGES_NoY,
            out tempHitInfo, dbl_capCast_castDistance, layerMask, qti_ignore)) {
            // Raise the capsule cast a little bit - if there's no hit that means this was probably a gentle slope
            //res = "hit 1";
            ccp1.y += 0.86f;
            ccp2.y += 0.86f;
            if (Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, origWalkVel_NEVERCHANGES_NoY, out tempHitInfo,
                dbl_capCast_castDistance, layerMask, qti_ignore)) {
                //res = "hit 2";
                normalWithoutY = tempHitInfo.normal;
                normalWithoutY.y = 0;

                // if you are coming too straight-on at the wall dont slide along wall
                //print("Angle to wall: " + Vector3.Angle(normalWithoutY * -1, origWalkVel_NEVERCHANGES_NoY));
                if (Vector3.Angle(normalWithoutY * -1, origWalkVel_NEVERCHANGES_NoY) < 20f) {
                    tempWalkVel.x = 0;
                    tempWalkVel.z = 0;
                    //print("Too straight, don't slide.");
                } else {
                    perp1.Set(tempHitInfo.normal.z, 0, -tempHitInfo.normal.x);
                    perp2.Set(-tempHitInfo.normal.z, 0, tempHitInfo.normal.x);
                    // Debug.DrawRay(hitInfo.point, hitInfo.normal * 3f, Color.green);
                    float p1v = Vector3.Angle(perp1, origWalkVel_NEVERCHANGES_NoY);
                    float p2v = Vector3.Angle(perp2, origWalkVel_NEVERCHANGES_NoY);
                    float velMag = origWalkVel_NEVERCHANGES_NoY.magnitude;
                    perp1.Normalize();
                    perp2.Normalize();
                    if (p1v < p2v) {
                        origWalkVel_NEVERCHANGES_NoY = velMag * perp1;
                    } else {
                        origWalkVel_NEVERCHANGES_NoY = velMag * perp2;
                    }

                    tempWalkVel.x = origWalkVel_NEVERCHANGES_NoY.x;
                    tempWalkVel.z = origWalkVel_NEVERCHANGES_NoY.z;
                    //print("Slide vel: " + tempWalkVel);
                }
            }
        }

        //print(res + "Angle: "+ origWalkVel_NEVERCHANGES);
        rb.velocity = tempWalkVel;
    }

    Vector3 tempv2_getperp;

    void Get_perp_of_v1_closest_to_v2_along_xz(ref Vector3 perp, Vector3 v1, Vector3 v2) {
        perp1.Set(v1.z, 0, -v1.x);
        perp2.Set(-v1.z, 0, v1.x);
        tempv2_getperp = v2;
        tempV2.y = 0;
        float p1v = Vector3.Angle(perp1, tempv2_getperp);
        float p2v = Vector3.Angle(perp2, tempv2_getperp);
        perp1.Normalize();
        perp2.Normalize();
        if (p1v < p2v) {
            perp.Set(perp1.x, 0, perp1.z);
        } else {
            perp.Set(perp2.x, 0, perp2.z);
        }
    }

    RaycastHit tempHitInfo;
    Vector3 tempV1;
    Vector3 tempV2;
    Vector3 perp1 = new Vector3();
    Vector3 perp2 = new Vector3();
    Vector3 normalWithoutY;

    void updateSounds() {
        if (cam.fixedFollowTopDown) return;
    }

    float cccc = 1.5f;

    private void OnCollisionStay(Collision collision) {
        float moveY = MyInput.moveY;
        // Hack to prevent friction jumping into things?
        if (jump_state != 0 && moveY > 0) {
            rb.velocity = nextJumpVel;
            Vector3 newpos = transform.position;
            if (rb.velocity.y > 0) {
                newpos.y += cccc * Time.deltaTime;
            } else if (rb.velocity.y < -1) {
                newpos.y -= 2.5f * cccc * Time.deltaTime;
            }

            transform.position = newpos;
        }
    }


    public void StopRunning() {
        AnimSetBool("Running", false);
    }


    private List<Collider> active_slipwalls = new List<Collider>();
    private List<Collider> active_ice = new List<Collider>();
    private int noSprintTriggers_Inside;
    private bool wasSprintingIn_NoSprintT_ButDidntLand;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("SlipWall")) {
            was_in_slipwall_last_frame = true;
            active_slipwalls.Add(other);
        }

        if (other.CompareTag("Poison Shroom")) {
            
            ////Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
            ////foreach (Cel_EmissionHelper helper in helpers) {
////                helper.Do_poison_flicker(10f);
////            }
            
            poisonShroomsInside++;
            noSprintTriggers_Inside++;
        }

        if (other.CompareTag("Ice")) {
            active_ice.Add(other);
        }

        if (other.CompareTag("NoSprintTrigger")) {
            noSprintTriggers_Inside++;
            if (other.name.IndexOf("Grass",StringComparison.InvariantCulture) != -1) {
                ////vfx.Start_grass();
            }
        }

        if (other.gameObject.layer == 22) {
            // temp AudioHelper.instance.playOneShot("splash",0.45f,0.9f,3);
            // temp SplashParticles.Play();
            // temp WaterRingParticles.Play();
        }


        if (other.transform.parent != null && other.transform.parent.GetComponent<Rigidbody>() != null) {
            // print("Enter");
            /*/*
            if (other.transform.parent.GetComponent<SquishyPlatform>() != null) {
                tryDisconnectFromRB = false;
                connected_rb = other.transform.parent.GetComponent<Rigidbody>();
                connected_rb_pos = connected_rb.position;
                connected_squishyPlatform = other.transform.parent.GetComponent<SquishyPlatform>();
                rb.velocity = connected_squishyPlatform.calculatedVel;
                justEnteredMovingPlatform = true;
            }
            */
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Ice")) {
            active_ice.Remove(other);
        }

        if (other.CompareTag("SlipWall")) {
            active_slipwalls.Remove(other);
        }

        if (other.CompareTag("Poison Shroom")) {
            poisonShroomsInside--;
            noSprintTriggers_Inside--;
            if (poisonShroomsInside == 0) {
                ////Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
                ////foreach (Cel_EmissionHelper helper in helpers) {
////                    helper.Stop_tint_flicker();
////                }
            }
        }
        
        
        if (other.CompareTag("NoSprintTrigger")) {
            noSprintTriggers_Inside--;
            
            if (noSprintTriggers_Inside == 0 && other.name.IndexOf("Grass",StringComparison.InvariantCulture) != -1) {
                ////vfx.End_grass();
            }
        }

        if (other.gameObject.layer == 22) {
            if (rb.velocity.y > 2f) {
                // temp SplashParticles.Play();
                // temp AudioHelper.instance.playOneShot("splash", 0.35f, 0.8f, 3);
            }

            // temp WaterRingParticles.Stop(true,  ParticleSystemStopBehavior.StopEmitting);
        }


        if (other.transform.parent != null && other.transform.parent.GetComponent<Rigidbody>() != null) {
            if (connected_rb == other.transform.parent.GetComponent<Rigidbody>()) {
                ////connected_squishyPlatform = null;
                DisconnectRB();
            }
        }
    }

    internal Rigidbody connected_rb;
    ////private MovingPlatform connected_movingPlatform;
    ////private SquishyPlatform connected_squishyPlatform;
    private Vector3 previous_rb_vel_without_connected_rb;
    private Vector3 connected_rb_vel;
    private Vector3 connected_rb_pos;
    private bool tryDisconnectFromRB = false;
    private int disconnectFromRBFrames = 0;
    private bool justEnteredMovingPlatform = false;

    private void OnCollisionEnter(Collision other) {
        if (other.rigidbody != null) {
            // print("Enter");
            tryDisconnectFromRB = false;
            connected_rb = other.rigidbody;
            connected_rb_pos = connected_rb.position;
            ////    if (other.gameObject.GetComponent<MovingPlatform>() != null) {
////                connected_movingPlatform = other.gameObject.GetComponent<MovingPlatform>();
////                rb.velocity = connected_movingPlatform.calculatedVel;
////                justEnteredMovingPlatform = true;
////            }
        }
    }

    private void OnCollisionExit(Collision other) {
        //print("exit");
        if (other.rigidbody != null) {
            DisconnectRB();
        }
    }

    public void DisconnectRB(bool no_exit_time = false) {
        tryDisconnectFromRB = true;
        disconnectFromRBFrames = 0;
        if (!no_exit_time) {
        } else {
            connected_rb = null;
        }
    }


    Quaternion facequat = new Quaternion();

    // Ignores Y rotation.
    Vector3 temp_face_to_vector_vec;

    void Face_transform_to_vector_xz_dir(Vector3 v, float partially = 1f, float min_xz_normalized_magnitude = -1f,
        float lerp = -1) {
        temp_face_to_vector_vec = v;
        temp_face_to_vector_vec.Normalize();
        temp_face_to_vector_vec.y = 0;
        //print(temp_face_to_vector_vec.magnitude);
        if (min_xz_normalized_magnitude > 0 && temp_face_to_vector_vec.magnitude <= min_xz_normalized_magnitude) {
            //print("Ignoring facing, magnitude too small: " + temp_face_to_vector_vec.magnitude);
            return;
        }

        v.y = 0;

        facequat.SetLookRotation(v);
        temp_facing_euler = transform.localEulerAngles;
        temp_facing_euler.y = Mathf.LerpAngle(temp_facing_euler.y, facequat.eulerAngles.y, partially);
        if (lerp > 0) {
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, temp_facing_euler, lerp);
        } else {
            transform.localEulerAngles = temp_facing_euler;
        }
    }


    internal bool was_in_gripshroom_last_frame = true;
    internal bool was_in_slipwall_last_frame;

    internal bool resetdashfromgrip;
    private bool hideUItrailer;
    public void Reset_dash(bool grip=false) {
        used_dash = false;
        if (grip) resetdashfromgrip = true;
    }

    public bool IsGliding() {
        return jump_state == jump_state_gliding;
    }

    // Set to -10k when exiting firstJumpArc state
    internal float last_firstJumpArc_starting_y = -10000f;
    bool do_equal_bounce;
    bool do_anim_jumpUp_to_flip;

    private bool used_dash__wasSetToTrue;

    internal void Do_equal_bounce(float a) {
        do_equal_bounce = true;
        do_anim_jumpUp_to_flip = true;
        AnimSetBool("BouncingUpwards", true);
        used_dash = false;
        hasVariableJumpHeight = false;
        if (last_firstJumpArc_starting_y == -10000f) {
            last_firstJumpArc_starting_y = a;
        }

        if (last_firstJumpArc_starting_y - transform.position.y < 3f) last_firstJumpArc_starting_y = transform.position.y + 3f;
        if (jump_state != jump_state_midair) {
            jump_state = jump_state_midair;
            cam.startJumpCamMode();
        }

        AnimSetTrigger("Jump");
        ignoreJumpLanding_Counter = 0.2f;
    }


    public void Do_bounce(Vector3 bounceVel) {
        do_anim_jumpUp_to_flip = true;
        AnimSetBool("BouncingUpwards", true);
        tempbounceVel = rb.velocity;
        tempbounceVel.y = 0;
        tempbounceVel += bounceVel;
        previous_rb_vel_without_connected_rb.y = bounceVel.y;
        hasVariableJumpHeight = false;
        rb.velocity = tempbounceVel;
        nextJumpVel = rb.velocity;
        used_dash = false;
        AnimSetBool("Sprinting", false);
        Set_nextJumpVel_doAnim_playSFX(true, false, true,true);
        cam.startJumpCamMode();
        AnimSetTrigger("Jump");
        ignoreJumpLanding_Counter = 0.3f;
        // temp doubleJumpParticles.Play();
    }

    float t_bump = 0;
    private Vector3 bumped_vel;
    public void Do_bump(Vector3 bumpVel,bool skipsound=false) {
        if (Mathf.Abs(bumpVel.x) > 1f || Mathf.Abs(bumpVel.z) > 1f || is_wallrunning) {
            rb.velocity += bumpVel;
            previous_rb_vel_without_connected_rb = rb.velocity;
            bumped_vel = rb.velocity;
            t_bump = 0.5f;
        } else {
            tempbounceVel = rb.velocity;
            tempbounceVel.y = bumpVel.y;
            previous_rb_vel_without_connected_rb.y = bumpVel.y;
            hasVariableJumpHeight = false;
            rb.velocity = tempbounceVel;
            nextJumpVel = rb.velocity;
            used_dash = false;
            Set_nextJumpVel_doAnim_playSFX(true, false, true,skipsound);
            Enter_jumpstate_firstJumpArc();
            cam.startJumpCamMode();
            AnimSetTrigger("Jump");
            ignoreJumpLanding_Counter = 0.3f;    
        }
    }

    private float t_stunned = 0;
    public void Stun() {
        t_stunned = 0.5f;
    }

    void Enter_jumpstate_firstJumpArc() {
        jump_state = jump_state_midair;
        ////vfx.Stop_run_dust();
        last_firstJumpArc_starting_y = transform.position.y;
    }

    public bool isVelocityBasicallyZero() {
        if (Mathf.Abs(rb.velocity.x) < 1f && Mathf.Abs(rb.velocity.z) < 1f) {
            rb.velocity = Vector3.zero;
            return true;
        }

        return false;
    }

    internal bool paused_during_linking;

    public void Toggle_pauseState_for_linking(bool on) {
        // pause camera here
        print("Player + Cam paused for scanning: " + on);
        PlayAnimation("idle");
        AnimSetBool("Running", false);
        AnimSetBool("Sprinting", false);
        ////vfx.Stop_run_dust();
        ////vfx.Stop_run_trail();
        rb.velocity = Vector3.zero;
        previous_rb_vel_without_connected_rb = Vector3.zero;
        paused_during_linking = on;
        cam.Toggle_pause_for_scanning(on);
    }
    
    ParticleSystem doubleJumpParticles;
    public GameObject PREFAB_doubleJumpParticles;
    private bool glide_needToReachFastFallingSpeed;

    [Header("Cursed Variable Zone")] [Range(0f, 1f)]
    public float dbl_capCast_movement_backup = 0.5f;

    [Range(0.5f, 2f)] public float dbl_capCast_castDistance = 0.5f;
    [Range(1f, 2f)] public float dbp_cc_r_mul = 1.5f;

    public void SetFixedCamEulerYTillNotPressingDirection(float eulerY) {
        forceFixedCamEulerY = eulerY;
        forceFixedCamEulerYTillNotPressingDirection = true;
        if (forceFixedCamEulerY > 0 && forceFixedCamEulerYTillNotPressingDirection) {
            // Do nothing, this just eliminates warnings
        }
    }


    internal bool teledash_and_wallrun_disabled = false;

    public void Toggle_TeledashWallrun(bool off) {
        HF.Warning("Teledash and wallrun set to: "+ !off);
        teledash_and_wallrun_disabled = off;
    }
    //SwitchCharacter

    internal bool amy_restricted;
    internal bool ingwen_restricted;
    internal bool riyou_restricted;
    private bool play_restrict_message;
    
    public void SwitchActiveModel(string forceThisModel = "") {
        Vector3 prevEuler = activeModel.localEulerAngles;

        amy_restricted = false;
        ingwen_restricted = false;
        riyou_restricted = false;

        activeModel.gameObject.SetActive(true);
        if (activeModel != ingwen_model) ingwen_model.gameObject.SetActive(false);
        
        animator = activeModel.GetComponent<Animator>();
        activeModel.localEulerAngles = prevEuler;
        activeModel.localPosition = new Vector3(0, starting_model_y_offset, 0);
        activeModel_originalScale = activeModel.transform.localScale;
    }

    void Init_anim_events() {
        List<Animator> anims = new List<Animator>();
        anims.Add(ingwen_model.GetComponent<Animator>());

        // Functions located in SephonieanimationEvent (The Animator must be on the same GO as the functions)
        // Sephonie... .cs calls the below AnimEvent functions
        foreach (Animator anim in anims) {
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips) {
                if (clip.name.IndexOf("Run") != -1) {
                    AnimationEvent e = new AnimationEvent();
                    e.functionName = "RunFoot";
                    e.time = 1/24f;
                    clip.AddEvent(e);
                    
                    e = new AnimationEvent();
                    e.functionName = "RunFoot";
                    e.time = 13/24f;
                    clip.AddEvent(e);
                } else if (clip.name.IndexOf("Sprint") != -1) {
                    AnimationEvent e = new AnimationEvent();
                    e.functionName = "SprintFoot";
                    e.time = 1/24f;
                    clip.AddEvent(e);
                    e = new AnimationEvent();
                    e.functionName = "SprintFoot";
                    e.time = 13/24f;
                    clip.AddEvent(e);
                }
            }
        }
    }

    bool queue_run_sound;
    private bool queue_sprint_sound;
    internal void AnimEvent_Run_FootOnGround() {
        queue_run_sound = true;
    }
    internal void AnimEvent_Sprint_FootOnGround() {
        queue_sprint_sound = true;
    }


    #region Capsule and Box Cast Initialization Helpers

    internal Vector3 ccp1;
    internal Vector3 ccp2;
    Vector3 ccBottom;
    internal float ccRadiusWorld;
    RaycastHit capCastHit;
    Vector3 cc_tempFwd;

    public float get_cc_bottom_y() {
        return transform.position.y + (cc.center.y - cc.height * 0.5f) * transform.localScale.y;
    }

    public float get_y_vel() {
        return rb.velocity.y;
    }

    internal void SetCapsuleCastVars(Vector3 backup_v, float backup_dis = 0, float radius_multiplier = 1, float yOffset = 0,
        bool debug = false) {
        float dis_ = cc.height * transform.localScale.y / 2 - cc.radius * transform.localScale.z;
        ccp1 = transform.position + cc.center * transform.localScale.y + Vector3.up * dis_;
        ccp2 = transform.position + cc.center * transform.localScale.y - Vector3.up * dis_;
        ccp1.y += yOffset;
        ccp2.y += yOffset;
        ccBottom = transform.position + cc.center * transform.localScale.y;

        if (backup_dis > 0) {
            if (debug) Debug.DrawRay(ccp1, Vector3.up, Color.green, 2f);
            cc_tempFwd = backup_v;
            cc_tempFwd.y = 0;
            cc_tempFwd.Normalize();
            cc_tempFwd *= backup_dis;
            ccp1 += cc_tempFwd;
            ccp2 += cc_tempFwd;
            ccBottom += cc_tempFwd;
            if (debug) Debug.DrawRay(ccp1, Vector3.up, Color.cyan, 2f);
        }

        ccBottom.y -= cc.bounds.extents.y;
        ccRadiusWorld = cc.radius * transform.localScale.z * radius_multiplier;
    }

    internal Vector3 boxcast_center;
    internal Vector3 boxcast_halfExtants = new Vector3();

    internal void SetBoxCastVars(Vector3 backup_v, float backup_dis = 0, float xz_extant_mul = 1, float yOffset = 0,
        bool debug = false) {
        boxcast_center = transform.position;
        boxcast_center.y += yOffset;
        boxcast_center.y += cc.center.y * transform.localScale.y;
        if (backup_dis > 0) {
            if (debug) Debug.DrawRay(boxcast_center, Vector3.up, Color.green, 2f);
            cc_tempFwd = backup_v;
            cc_tempFwd.y = 0;
            cc_tempFwd.Normalize();
            cc_tempFwd *= backup_dis;
            boxcast_center += cc_tempFwd;
            if (debug) Debug.DrawRay(boxcast_center, Vector3.up, Color.red, 2f);
        }

        boxcast_halfExtants.Set(cc.radius * xz_extant_mul * transform.localScale.z, cc.height * transform.localScale.y / 2,
            cc.radius * xz_extant_mul * transform.localScale.z);
    }

    #endregion


    Vector3 dash_dir;
    internal Vector3 dash_dir_vaultring;
    internal Vector3 dash_offset_vaultring;

    internal bool TeledashCast_for_VaultRing(ref RaycastHit hit, float distance_mul) {
        dash_dir_vaultring = transform.forward;
        dash_dir_vaultring.y = 0;
        dash_dir_vaultring = dash_dir_vaultring.normalized;
        /*/*if (SpeedPod.Num_held_pods() >= 3) {
            dash_offset_vaultring = dash_dir_vaultring * speedPod_dashPrism_distance * distance_mul;
        } else if (DashPod.Num_held_pods() > 0) {
            dash_offset_vaultring = dash_dir_vaultring * dash_magnet_distance * distance_mul * dashPod_mul;
        } else {*/
            dash_offset_vaultring = dash_dir_vaultring * dash_magnet_distance * distance_mul;
        //}

        SetBoxCastVars(dash_offset_vaultring * -1, 0.5f);
        if (Physics.BoxCast(boxcast_center, boxcast_halfExtants, dash_offset_vaultring, out hit, transform.rotation,
            dash_offset_vaultring.magnitude + 0.5f, Registry.lmc_vaultRing, qti_ignore)) {
            return true;
        }

        return false;
    }


    float Get_angle_from_xz(Vector3 v, bool absolute = false) {
        return HF.Get_angle_from_xz(v, absolute);
    }

    float Get_slope_angle(Vector3 slopeNormal) {
        return HF.Get_slope_angle(slopeNormal);
    }

    public bool Is_touching_ground(float checkDistance = -1,bool debug=false) {
        if (checkDistance == -1) checkDistance = 0.2f;
        if (debug) print("---");
        
        // note: might be wrong if the capsule ver has a nonzero x or z coordinate
        SetCapsuleCastVars(Vector3.zero);
        ccp1.y += 0.05f;
        ccp2.y += 0.05f;
        if (Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, Vector3.down, out capCastHit, checkDistance, Registry.lmc_GROUND,
            qti_ignore)) {
            
            if (Vector3.Distance(capCastHit.point, ccBottom) < .45f) {
                last_touching_ground_collider = capCastHit.collider;
                last_touching_ground_check_distance = Vector3.Distance(capCastHit.point, ccBottom);
                return true;
            }
            if (debug) print("Capsule hit but too far: "+Vector3.Distance(capCastHit.point, ccBottom));
        }

        // Rather than assuming not being on the ground, this really makes sure the bottom of the capsule isn't near the ground
        bool lastCheck = Physics.Raycast(transform.position + cc.center * transform.localScale.y, -Vector3.up, cc.bounds.extents.y + checkDistance,
            ~(Registry.lmc_PLAYER_NON_COLLIDABLES), qti_ignore);

        if (debug && lastCheck) Debug.DrawRay(transform.position + cc.center * transform.localScale.y, -Vector3.up * (cc.bounds.extents.y+checkDistance),Color.green,5f);
        if (debug && !lastCheck) Debug.DrawRay(transform.position + cc.center * transform.localScale.y, -Vector3.up * (cc.bounds.extents.y+checkDistance),Color.red,5f);
        return lastCheck;
    }

    bool in_scene_initialization_phase;

    // for stuff like - leaving the pause menu and opening up something, waiting for a fade to link
    internal bool paused_during_menu_changes; 
    public bool isThereAnyReasonToPause() {
        return false;
        ////return DataLoader.instance.isChangingScenes || in_scene_initialization_phase || DataLoader.instance.isPaused || paused_during_menu_changes ||
               ////MyCamera.inCinemachineMovieMode || !dbox.isFinished() || MyEvent.Is_any_MyEvent_parsing || paused_during_linking || forcePausingFromMyEvent || MyEvent.scheduler_event_running || pause_screenshot;
    }

    public void PlayAnimation(string _name,bool crossfade=false) {
        if (_name == "idle") _name = "Idle";
        AnimPlay(_name,crossfade);
    }


    readonly Vector3 offscreenpos = new Vector3(0, -10000f, 0);
    Vector3 event_model_original_pos;
    GameObject White_pod_trails;
    GameObject Dash_pod_trails;
    public float lerpMul_reduceGeyserXZ = 1f;
    public float lerpMul_increaseGeyserXZ = 8f;
    public float geyserXZ_reduction = 0.3f;
    private Vector3 geyser_xz_momentum;
    private static readonly int BouncingUpwards = Animator.StringToHash("BouncingUpwards");

    public void ToggleFreezeForScreenshotMode(bool freeze) {
        if (freeze) {
            animator.speed = 0;
            pause_screenshot = true;
        } else {
            animator.speed = 1;
            pause_screenshot = false;
        }
    }
    
    
    public void HideDuringEvent() {
        if (White_pod_trails == null) {
            White_pod_trails = GameObject.Find("WhitePod Trails");
        }

        if (White_pod_trails != null) White_pod_trails.SetActive(false);
        if (Dash_pod_trails == null) {
            Dash_pod_trails = GameObject.Find("DashPod Trails");
        }

        if (Dash_pod_trails != null) Dash_pod_trails.SetActive(false);
        event_model_original_pos = activeModel.localPosition;
        activeModel.localPosition = offscreenpos;
    }

    public void RevealAfterEvent() {
        if (White_pod_trails != null) {
            White_pod_trails.SetActive(true);
        }

        if (Dash_pod_trails != null) {
            Dash_pod_trails.SetActive(true);
        }

        activeModel.localPosition = event_model_original_pos;
    }

    public float Get_dash_magnet_distance() {
        /*/*if (SpeedPod.Num_held_pods() >= 3) {
            return speedPod_dashPrism_distance;
        } else if (DashPod.Num_held_pods() > 0) {
            return dashPod_mul * dash_magnet_distance;
        }*/

        return dash_magnet_distance;
    }

    public void Use_sink_cloud() {
        Do_bounce(Vector3.up*20f);
        Set_nextJumpVel_doAnim_playSFX(true, true);
        in_sink_cloud = false;
        used_dash = false;
    }

    public void Enter_sink_cloud(float v) {
        Use_dash();
        Cancel_sprint_state_and_anim();
        in_sink_cloud = true;
        sink_cloud_min_yVel = v;
    }


    public bool Enter_glide_orb(Vector3 startPoint, float glidetime) {
        if (jump_state == jump_state_midair && dash_mode_inactive == dash_mode) {
            glide_start_point = startPoint;
            t_glideOrbTime = glidetime;
            is_moving_to_glide_start_point = true;
            glide_init_entry_vel = rb.velocity;
            AnimSetTrigger("FlipFromAnywhere");
            ////vfx.Glideorb();
            rb.velocity = Vector3.zero;
            used_dash = false;
            ////SoundPlayer.instance.glideorb();
            return true;
        } else {
            return false;
        }
    }

    public List<Renderer> GetAllRenderers() {
        List<Renderer> rends = new List<Renderer>();
        rends.AddRange(ingwen_model.GetComponentsInChildren<Renderer>());
        return rends;
    }


    internal Color amy_dash_emission_color;
    internal Color gael_dash_emission_color;
    internal Color riyou_dash_emission_color;
    internal Color ingwen_dash_emission_color;
    private Color whitePodJump_emission_color;
    public void Start_emission_lerp(float time, bool returnToInitialColor, Color endColor, float startingLerpFactor=0f) {
        /*/*  Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
        foreach (Cel_EmissionHelper helper in helpers) {
            if (returnToInitialColor) {
                helper.Lerp_to_initialColor(time,startingLerpFactor);
            } else {
                helper.Lerp_to_color_from_initial(time,endColor,startingLerpFactor);
            }
        }
        */
    }

    public void Set_emission_color(Color col) {
        /*/*
        Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
        foreach (Cel_EmissionHelper helper in helpers) {
            helper.Set_emission_color(col);
        }
        */
    }

    public void Pulse_emission(Color col, float intensity, float half_pulse_period) {
        /*/*
        Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
        foreach (Cel_EmissionHelper helper in helpers) {
            helper.Pulse_to_color_from_initial(half_pulse_period,col,intensity);
        }
        */
    }


    private Color Get_active_trio_dashEmission_color() {
        return ingwen_dash_emission_color;
    }

    void Rotate_towards_vector(Vector3 vec, float lerp_factor) {
        Quaternion q = new Quaternion();
        q.SetLookRotation(vec);
        float yEulerFromVel = q.eulerAngles.y;
        Vector3 newLook = Vector3.zero;
        newLook.y = Mathf.LerpAngle(transform.eulerAngles.y, yEulerFromVel, lerp_factor);
        transform.eulerAngles = newLook;
    }

    internal bool recording_on;
    internal bool shadow_recording_on;
    ////internal ShadowFollower shadow_follower;
    ////internal GhostReplay recorder;
    void AnimSetBool(string _name, bool _value) {
        if (recording_on) {
            if (animator.GetBool(_name) != _value) {
                ////recorder.Record_anim("b", _name, _value.ToString());
            }
        }

        if (shadow_recording_on) {
            if (animator.GetBool(_name) != _value) {
                ////shadow_follower.Record_anim("b", _name, _value.ToString());
            }
        }
        animator.SetBool(_name,_value); 
    }
    void AnimSetTrigger(string _name) {
        if (recording_on) {
            ////recorder.Record_anim("set",_name,"");
        }
        
        if (shadow_recording_on) {
            ////shadow_follower.Record_anim("set",_name,"");
        }
        animator.SetTrigger(_name);
    }
    void AnimResetTrigger(string _name) {
        if (recording_on) {
            ////recorder.Record_anim("reset",_name,"");
        }
        if (shadow_recording_on) {
            ////shadow_follower.Record_anim("reset",_name,"");
        }
        animator.ResetTrigger(_name);
    }
    void AnimPlay(string _name,bool crossfade=false) {
        if (recording_on) {
            ////recorder.Record_anim("play",_name,"");
        }
        if (shadow_recording_on) {
            ////shadow_follower.Record_anim("play",_name,"");
        }

        if (crossfade) {
            animator.CrossFadeInFixedTime(_name,0.1f);
        } else {
            animator.Play(_name);
        }

    }

    void Use_dash() {
        if (SaveManager.infinite_dash) return;
        used_dash = true;
    }

    private Color superdash_end_col;
    private Color superdash_start_col;
    private bool initsuperdashcol;
    private bool emission_osc_playing;
    void Osc_emission() {
        if (!initsuperdashcol) {
            initsuperdashcol = true;
            ColorUtility.TryParseHtmlString("#902C00FF", out superdash_start_col);
            ColorUtility.TryParseHtmlString("#2C0E00FF", out superdash_end_col);
        }
        ////Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
        ////foreach (Cel_EmissionHelper helper in helpers) {
////            helper.Start_osc_emission(0.75f, superdash_start_col, superdash_end_col);
////        }

    }

    void Stop_osc_emission() {
        ////    Cel_EmissionHelper[] helpers = emission_helper_GO.GetComponents<Cel_EmissionHelper>();
////        foreach (Cel_EmissionHelper helper in helpers) {
////            helper.Stop_osc_emission();
////        }   
    }
    
    
}