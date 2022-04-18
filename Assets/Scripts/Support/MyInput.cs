using System;
using System.Collections.Generic;
////using JetBrains.Annotations;
using UnityEngine;
////using Rewired;

public class MyInput : MonoBehaviour {
    private Vector2 temp_v2;
    
    public static int mouse_visible_state;
    public bool force_mouse_cam;
    public bool force_lh_mouse_controls;
    public bool force_rh_mouse_controls;
    public float mouse_smoothing = 0.9f;
    private float last_mouse_dx;
    private float last_mouse_dy;
    
    // Settings/General state
	public static MyInput o;
    public static float joyMoveThreshold = 0.2f;
	public static bool invertX = false;
	public static bool invertY = false;
    public static bool invertYMove = false;
	public static bool gamepad_active; // Only to be set at end of input check loop
    public static bool shortcut;
    ////public static Player rewiredPlayer;
    public static bool kbHasPriority = true; // Internal use only! Not reset each frame. Important to start as true, so that the first controller inputs trigger a UI refresh for buttons
    public static bool jpConfirmCONTROLLER;


    // Linking
    public static KeyCode KC_battle_quickRotate;
    public static bool jpQuickRotate;

    public static KeyCode KC_erase;
    public static bool jpErase;
    public static bool erase;

    public static KeyCode KC_scan_inputShift;
    public static bool scan_inputShift;

    public static KeyCode KC_link_endTurn;
    public static bool jpEndTurn;
    public static bool endTurn;

    public static KeyCode KC_undo;
    public static bool undo;

    public static KeyCode KC_placePiece;
    public static bool jpPlacePiece;

    public static bool rotateRight;
    public static bool rotateLeft;
    public static bool jpRotateRight;
    public static bool jpRotateLeft;

    // Shared
    public static bool jpHome;

    public static KeyCode KC_up;
    public static KeyCode KC_right;
    public static KeyCode KC_down;
    public static KeyCode KC_left;
    public static KeyCode KC_cam_up;
    public static KeyCode KC_cam_right;
    public static KeyCode KC_cam_down;
    public static KeyCode KC_cam_left;
    
    
    public static bool up;
    public static bool jpUp;
    public static bool jrUp;
    public static bool right;
    public static bool jpRight;
    public static bool down;
    public static bool jpDown;
    public static bool left;
    public static bool jpLeft;
    public static bool anyDir;

    private static int jpUpMode;
    private static int jpDownMode;
    private static int jpRightMode;
    private static int jpLeftMode;
    public static bool jpAnyDir;
    public static float moveX;
    public static float moveY;


    public static KeyCode KC_confirm;
    public static bool confirm;
    public static bool jpConfirm;


    public static KeyCode KC_cancel;
    public static bool cancel;
    public static bool jpCancel;



    public static KeyCode KC_pause;
    public static bool pause;
    public static bool jpPause;

    // Field (3D)
    public static KeyCode KC_switch_characters;
    public static bool jpSwitchChars;
    public static bool switchChars;

    public static KeyCode KC_teledash_and_sprint;
    public static bool jp_sprintDash;
    public static bool sprintDash;
    public static bool gamepad__sprintDash_held_last_frame;

    public static KeyCode KC_dash_only;
    public static bool jp_dash_separate;
    public static bool jp_dash_separate_ignore_sprint;
    public static bool dash_separate;
    
    public static KeyCode KC_talk;
    public static bool talk;
    public static bool jpTalk;

    public static KeyCode KC_jump;
    public static bool jump;
    public static bool jpJump;
    public static bool jrJump;

    public static KeyCode KC_return_to_checkpoint;
    public static bool return_to_checkpoint;
    public static bool jp_return_to_checkpoint;

    public static KeyCode KC_camera_shift;
    public static bool recenter;
    private static bool jpRecenter;
    public static bool recenterShortPress_KB;
    public static bool recenterLongHold_KB;
    private static float t_recenterHeld;
    private static int recenterDetectionMode;

    public static KeyCode KC_zoomIn;
    public static KeyCode KC_zoomOut;
    public static bool zoomIn;
    public static bool zoomOut;
    public static bool jp_zoomIn;
    public static bool jp_zoomOut;

    public static KeyCode KC_ss_mode;
    public static bool jp_screenshot_mode;
    public static bool screenshot_mode;

    public static bool test_shoot;


	public static float camY;
    public static float camX;
	bool gamepad_camUp;
	bool gamepad_camDown;
    bool gamepad_camRight;
    bool gamepad_camLeft;
    private bool gamepad_L2_held_last_frame;
    private bool gamepad_R2_held_last_frame;
    

    // Face URDL, L1 L2 R1 R2, Start/Select, R3 L3.
    private static string[] action_names = new[] {
        "Talk", "CancelSkip", "Jump", "Dash", "ZoomOut", "SwitchCharacters", "ZoomIn", "Teledash", "Pause", "Select", "R3", "L3"
    };
    private bool[] gpID_to_held = new bool[13]; 
    private bool[] gpID_to_jp = new bool[13]; 
    private bool[] gpID_to_jr = new bool[13];

    // Used for accessing the gamepad arrays.
    public const int GAMEPAD_face_up = 0;
    public const int GAMEPAD_face_right = 1;
    public const int GAMEPAD_face_down = 2;
    public const int GAMEPAD_face_left = 3;
    public const int GAMEPAD_L1 = 6;
    public const int GAMEPAD_L2 = 5;
    public const int GAMEPAD_R1 = 4;
    public const int GAMEPAD_R2 = 7;
    public const int GAMEPAD_start = 8;
    public const int GAMEPAD_select = 9;
    public const int GAMEPAD_R3 = 10;
    public const int GAMEPAD_L3 = 11;
    public const int GAMEPAD_undefined = 12;

    
    public class MyBind {
        public int gp_ID; // corresponds to the above gamepad stuff.
    }

    public static MyBind[] KBI_to_gp_binding;

    /*/*
    public static void Transfer_loadDataBindings_to_game(string s) {
        Dictionary<int, int> d = SaveManager.string_to_IntIntDict(s);
        for (int i = 0; i < d.Count; i++) {
            KBI_to_gp_binding[i].gp_ID = d[i];
        }
    }

    public static string Get_bindings_as_string() {
        Dictionary<int, int> d = new Dictionary<int, int>();
        for (int i = 0; i < KBI_to_gp_binding.Length; i++) {
            d.Add(i,KBI_to_gp_binding[i].gp_ID);
        }
        return SaveManager.IntIntDict_to_string(d);
    }
    */
    

    bool is_melos;
	void Awake () {
        if (o != null ) {
            Destroy(gameObject);
            return;
        } else {
            o = this;
        }
        DontDestroyOnLoad(this);
        if (Application.isEditor) {
            //print(Environment.UserName);
            if (Environment.UserName.IndexOf("hantani",StringComparison.InvariantCulture) != -1) {
                is_melos = true;
            }
        }

        // Indices into this are the same as those for 'keybinds' labels.
        KBI_to_gp_binding = new MyBind[38];
        for (int i = 0; i < KBI_to_gp_binding.Length; i++) KBI_to_gp_binding[i] = new MyBind();

        Use_preset_gamepad_default();

        // Arrows
        KC_up = KeyCode.UpArrow;
        KC_right = KeyCode.RightArrow;
        KC_down = KeyCode.DownArrow;
        KC_left = KeyCode.LeftArrow;
        
        KC_cam_up = KeyCode.W;
        KC_cam_right = KeyCode.D;
        KC_cam_down = KeyCode.S;
        KC_cam_left = KeyCode.A;

        KC_zoomIn = KeyCode.E;
        KC_zoomOut = KeyCode.Q;
        KC_ss_mode = KeyCode.F;

        KC_switch_characters = KeyCode.Tab;
        KC_link_endTurn = KeyCode.Tab;

        KC_return_to_checkpoint = KeyCode.LeftControl;
        KC_erase = KeyCode.E;

        KC_scan_inputShift = KeyCode.LeftShift;
        KC_camera_shift = KeyCode.LeftShift;

        KC_talk = KeyCode.Space;
        KC_pause = KeyCode.Return;

        KC_teledash_and_sprint = KeyCode.Z;
        KC_dash_only = KeyCode.C;
        KC_battle_quickRotate = KeyCode.Z;
        KC_cancel = KeyCode.Z;

        KC_jump = KeyCode.X;
        KC_confirm = KeyCode.X;
        KC_placePiece = KeyCode.X;

        KC_undo = KeyCode.Backspace;
        #region rewired boilerplate
        print("Subscribing to ReWired events.");
        ////rewiredPlayer = ReInput.players.GetPlayer(0);
        ////ReInput.ControllerConnectedEvent += OnControllerConnected;
        ////ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
        ////ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;
    }

    public static bool force_link_player_refresh;
    public static int activeGamepads;
    /*/*
    void OnControllerConnected(ControllerStatusChangedEventArgs args) {
        Debug.Log("A controller was connected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
        //if (args.name.ToLower().IndexOf("dualshock") != -1) alwaysUsePS4 = true;
        activeGamepads++;
        force_link_player_refresh = true;
    }

    void OnControllerDisconnected(ControllerStatusChangedEventArgs args) {
        Debug.Log("A controller was disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
        activeGamepads--;
        if (DataLoader.instance != null && !DataLoader.instance.isPaused) {
            DataLoader.instance.forcePause = true;
        }
        if (activeGamepads < 0) activeGamepads = 0;
        force_link_player_refresh = true;
    }
    // This function will be called when a controller is about to be disconnected
    // You can get information about the controller that is being disconnected via the args parameter
    // You can use this event to save the controller's maps before it's disconnected
    void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args) {
        Debug.Log("A controller is being disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
        
    }

    void OnDestroy() {
        ReInput.ControllerConnectedEvent -= OnControllerConnected;
        ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
        ReInput.ControllerPreDisconnectEvent -= OnControllerPreDisconnect;
    }
    */
    #endregion



	void Update () {
        // Reset input variables.
        scan_inputShift = false;
        rotateRight = rotateLeft = false;
        jpRotateLeft = jpRotateRight = false;
        jpErase = false; //unused (only erase hold matters..?)
        erase = false;
        jpEndTurn = false;
        endTurn = false;
        jpQuickRotate = false;
        undo = false;

        up = right = down = left = jpUp = jrUp = jpRight = jpDown = jpLeft = false;
        gamepad_camUp = gamepad_camRight = gamepad_camDown = gamepad_camLeft = false;

        pause = jpPause = false;

        jpPlacePiece = false;
        jump = jpJump = jrJump = false;
        cancel = jpCancel = false;

        confirm = jpConfirm = false;
        jpConfirmCONTROLLER = false;
        return_to_checkpoint = false;
        jp_return_to_checkpoint = false;

        jp_sprintDash =  sprintDash = false;
        dash_separate = jp_dash_separate = jp_dash_separate_ignore_sprint = false;
        
        jpTalk = talk = false;
        moveX = moveY = camY = camX = 0;

        jpRecenter = recenter = false;

        zoomIn = zoomOut = jp_zoomIn = jp_zoomOut = false;

        switchChars = jpSwitchChars = false;

        jp_screenshot_mode = false;
        screenshot_mode = false;

        
        bool hadGamepadInput = false;

        //// This is where controller input would be detected, but Rewired is a paid plugin so I've commented this out. 
        /*/*
        #region Detect gamepad input
        if (!SaveManager.controllerDisable && activeGamepads > 0) {
            // "Select"
            // "L3"
            
            // Use the default rewired actions (In the future, it's better if the action names are just called 'face right', etc, and then in the game code here, those actions take on game meaning.)
            // So like even if it's rewi...GetButton("Jump"), that should be understood as "GetButton(FaceDown). It's just a pain to change the labels otherwise

            for (int i = 0; i < action_names.Length; i++) {
                if (i == GAMEPAD_R2 || i == GAMEPAD_L2) {
                    gpID_to_held[i] = rewiredPlayer.GetAxis(action_names[i]) > 0.1f;
                    bool held_last_frame = false;
                    if (i == GAMEPAD_R2) held_last_frame = gamepad_R2_held_last_frame;
                    if (i == GAMEPAD_L2) held_last_frame = gamepad_L2_held_last_frame;
                    
                    if (!held_last_frame) {
                        gpID_to_jp[i] = gpID_to_held[i];
                        gpID_to_jr[i] = false;
                    } else if (held_last_frame) {
                        gpID_to_jr[i] = !gpID_to_held[i];
                        gpID_to_jp[i] = false;
                    }
                    
                    if (i == GAMEPAD_R2) gamepad_R2_held_last_frame = gpID_to_held[i];
                    if (i == GAMEPAD_L2) gamepad_L2_held_last_frame = gpID_to_held[i];
                } else {
                    gpID_to_held[i] = rewiredPlayer.GetButton(action_names[i]);
                    gpID_to_jp[i] = rewiredPlayer.GetButtonDown(action_names[i]);
                    gpID_to_jr[i] = rewiredPlayer.GetButtonUp(action_names[i]);
                }
                
                //if (gamepad_pressed[i]) print("Pressed "+action_names[i]);
                //if (gamepad_released[i]) print("Released "+action_names[i]);
            }

            jump = gpID_to_held[KBI_to_gp_binding[KBI_jump].gp_ID];
            jpJump = gpID_to_jp[KBI_to_gp_binding[KBI_jump].gp_ID];
			jrJump = gpID_to_jr[KBI_to_gp_binding[KBI_jump].gp_ID];
            jpPlacePiece = gpID_to_jp[KBI_to_gp_binding[KBI_placePiece].gp_ID];
            jpTalk = gpID_to_jp[KBI_to_gp_binding[KBI_talk].gp_ID];
            talk = gpID_to_held[KBI_to_gp_binding[KBI_talk].gp_ID];
            if (gamepad__sprintDash_held_last_frame == false) {
                jp_sprintDash = gpID_to_jp[KBI_to_gp_binding[KBI_sprintdash].gp_ID];
            }
            sprintDash = gpID_to_held[KBI_to_gp_binding[KBI_sprintdash].gp_ID];
            dash_separate = gpID_to_held[KBI_to_gp_binding[KBI_sepDash].gp_ID];
            jp_dash_separate_ignore_sprint = jp_dash_separate = gpID_to_jp[KBI_to_gp_binding[KBI_sepDash].gp_ID];
            gamepad__sprintDash_held_last_frame = sprintDash;
            jpQuickRotate = gpID_to_jp[KBI_to_gp_binding[KBI_quickRotate].gp_ID];
            return_to_checkpoint = gpID_to_held[KBI_to_gp_binding[KBI_checkpoint].gp_ID];
            jp_return_to_checkpoint = gpID_to_jp[KBI_to_gp_binding[KBI_checkpoint].gp_ID];
            jpPause = gpID_to_jp[KBI_to_gp_binding[KBI_pause].gp_ID];
            pause = gpID_to_held[KBI_to_gp_binding[KBI_pause].gp_ID];
            jpConfirmCONTROLLER = jpConfirm = gpID_to_jp[KBI_to_gp_binding[KBI_confirm].gp_ID];
            confirm = gpID_to_held[KBI_to_gp_binding[KBI_confirm].gp_ID];
            cancel = gpID_to_held[KBI_to_gp_binding[KBI_cancel].gp_ID];
            jpCancel = gpID_to_jp[KBI_to_gp_binding[KBI_cancel].gp_ID];
            zoomIn = gpID_to_held[KBI_to_gp_binding[KBI_zoomIn].gp_ID];
            zoomOut = gpID_to_held[KBI_to_gp_binding[KBI_zoomOut].gp_ID];
            jp_zoomIn = gpID_to_jp[KBI_to_gp_binding[KBI_zoomIn].gp_ID];
            jp_zoomOut = gpID_to_jp[KBI_to_gp_binding[KBI_zoomOut].gp_ID];
            jpHome = rewiredPlayer.GetButtonDown("Home"); // Home not rebindable.
            jpRecenter = gpID_to_jp[KBI_to_gp_binding[KBI_recenter].gp_ID];
            recenter = gpID_to_held[KBI_to_gp_binding[KBI_recenter].gp_ID];
            jpErase = gpID_to_jp[KBI_to_gp_binding[KBI_erase].gp_ID];
            erase = gpID_to_held[KBI_to_gp_binding[KBI_erase].gp_ID];
            endTurn = gpID_to_held[KBI_to_gp_binding[KBI_endTurn].gp_ID];
            switchChars = gpID_to_held[KBI_to_gp_binding[KBI_switchChar].gp_ID];
            jpSwitchChars = gpID_to_jp[KBI_to_gp_binding[KBI_switchChar].gp_ID];
            jpRotateRight = gpID_to_jp[KBI_to_gp_binding[KBI_rotRight].gp_ID];
            jpRotateLeft = gpID_to_jp[KBI_to_gp_binding[KBI_rotLeft].gp_ID];
            undo = gpID_to_held[KBI_to_gp_binding[KBI_undo].gp_ID];
            if (SaveManager.invertConfirmCancel) {
                confirm  = cancel;
                jpConfirm = jpConfirmCONTROLLER = jpCancel;
                cancel = gpID_to_held[KBI_to_gp_binding[KBI_confirm].gp_ID]; // Not a typo
                jpCancel = gpID_to_jp[KBI_to_gp_binding[KBI_confirm].gp_ID]; // also not a typo
            }

            jp_screenshot_mode = gpID_to_jp[KBI_to_gp_binding[KBI_ss_mode].gp_ID];
            screenshot_mode = gpID_to_held[KBI_to_gp_binding[KBI_ss_mode].gp_ID];

            // Get raw stick values.
            float r_stick_y = rewiredPlayer.GetAxis("CamVert"); 
            float r_stick_x = rewiredPlayer.GetAxis("CamHor");
            float l_stick_x = rewiredPlayer.GetAxis("MoveHor"); 
            float l_stick_y = rewiredPlayer.GetAxis("MoveVert");
            
            
            // Rotate if needed.
            if (SaveManager.r_stick_rotation > 0) {
                temp_v2.Set(r_stick_x,r_stick_y);
                HF.Rotate_vec2_ccw(ref temp_v2,-SaveManager.r_stick_rotation);
                r_stick_x = temp_v2.x;
                r_stick_y = temp_v2.y;
            }
            if (SaveManager.l_stick_rotation > 0) {
                temp_v2.Set(l_stick_x,l_stick_y);
                HF.Rotate_vec2_ccw(ref temp_v2,-SaveManager.l_stick_rotation);
                l_stick_x = temp_v2.x;
                l_stick_y = temp_v2.y;
            }
            
            // Get D-pad
            float dpad_x = 0;
            float dpad_y = 0;
            if (rewiredPlayer.GetButton("DU")) dpad_y = 1f;
            if (rewiredPlayer.GetButton("DD")) dpad_y = -1f;
            if (rewiredPlayer.GetButton("DR")) dpad_x= 1f;
            if (rewiredPlayer.GetButton("DL")) dpad_x= -1f;

            // Flip the movement/camera inputs if needed.
            float gamepad_move_x;
            float gamepad_move_y;
            if (SaveManager.flip_sticks) {
                gamepad_move_x = r_stick_x;
                gamepad_move_y = r_stick_y;
                camX = l_stick_x;
                camY = -l_stick_y;
            } else {
                gamepad_move_x = l_stick_x;
                gamepad_move_y = l_stick_y;
                camX = r_stick_x;
                camY = -r_stick_y;
            }
            // Only flip the (default) left-stick, not dpad. 
            // Currently just a switch workaround.
            if (invertYMove) gamepad_move_y *= -1; 

            // If dpad used as camera, then set camera inputs to dpad values.
            // (Unless paused, in the title, or linking, or making a choice.)
            if (SaveManager.dpad_cam) {
                if (dpad_x != 0) camX = dpad_x;
                if (dpad_y != 0) camY = -dpad_y;
                if (DataLoader.instance.isPaused || DataLoader.instance.isTitle || LinkTrigger.in_linking || SephonieChoicePrompt.prompt_open) {
                    if (dpad_x != 0) gamepad_move_x = dpad_x;
                    if (dpad_y != 0) gamepad_move_y = dpad_y;
                }
            } else {
                if (dpad_x != 0) gamepad_move_x = dpad_x;
                if (dpad_y != 0) gamepad_move_y = dpad_y;
            }

            //if (invertY) camY *= -1;
            //if (invertX) camX *= -1;
            gamepad_camUp = camY > 0.1f;
            gamepad_camDown = camY < -0.1f;
            gamepad_camRight = camX > 0.1f;
            gamepad_camLeft = camX < -0.1f;
            

            left =  gamepad_move_x < -0.3f;
            right = gamepad_move_x  > 0.3f;
            down = gamepad_move_y < -0.3f;
            up = gamepad_move_y > 0.3f;

            if (jpLeftMode == 0 && gamepad_move_x < -0.9f) {
                jpLeft = true;
                jpLeftMode = 1;
            } else if (jpLeftMode == 1 && !left) jpLeftMode = 0;


            if (jpRightMode == 0 && gamepad_move_x > 0.9f) {
                jpRight = true;
                jpRightMode = 1;
            } else if (jpRightMode == 1 && !right) jpRightMode = 0;

            if (jpUpMode == 0 && gamepad_move_y > 0.9f) {
                jpUp = true;
                jpUpMode = 1;
            } else if (jpUpMode == 1 && !up) {
                jpUpMode = 0;
                jrUp = true;
            }

            if (jpDownMode == 0 && gamepad_move_y < -0.9f) {
                jpDown= true;
                jpDownMode= 1;
            } else if (jpDownMode == 1 && !down) jpDownMode = 0;

            moveX = gamepad_move_x;
            moveY = gamepad_move_y;

            // Turn off keyboard's priority if gamepad input detected.
            if (left || right || up || down || switchChars || return_to_checkpoint || sprintDash || gamepad_camLeft || zoomOut || zoomIn ||gamepad_camRight || gamepad_camUp || gamepad_camDown || jpPlacePiece || jpJump || jpCancel ||jpTalk || jpPause) {
                hadGamepadInput = true;
                if (kbHasPriority) {
                    kbHasPriority = false;
                    force_link_player_refresh = true;
                    print("Gamepad now has priority over keyboard.");
                }
            }
		}
        #endregion
        */


        #region Detect keyboard input
        if (kbHasPriority) {
            jpCancel = Input.GetKeyDown(KC_cancel);
            cancel = Input.GetKey(KC_cancel);
            jpConfirm = Input.GetKeyDown(KC_confirm);
            confirm = Input.GetKey(KC_confirm);
        }


        // Jump / QR defaults to cancel as well  (Z)
        jump |= Input.GetKey(KC_jump);
        jpJump |= Input.GetKeyDown(KC_jump);
        jrJump |= Input.GetKeyUp(KC_jump);
        jpPlacePiece |= Input.GetKeyDown(KC_placePiece);
        jpQuickRotate |= Input.GetKeyDown(KC_battle_quickRotate);

        // control
        return_to_checkpoint |= Input.GetKey(KC_return_to_checkpoint);
        jp_return_to_checkpoint |= Input.GetKeyDown(KC_return_to_checkpoint);

        // c
        dash_separate |= Input.GetKey(KC_dash_only);
        jp_dash_separate |= Input.GetKeyDown(KC_dash_only);
        jp_dash_separate_ignore_sprint = jp_dash_separate;
        
        // z
        sprintDash |= Input.GetKey(KC_teledash_and_sprint);
        jp_sprintDash |= Input.GetKeyDown(KC_teledash_and_sprint);
        // e
        jpErase |= Input.GetKeyDown(KC_erase);
        erase |= Input.GetKey(KC_erase);

        // Tab
        switchChars |= Input.GetKey(KC_switch_characters);
        jpSwitchChars |= Input.GetKeyDown(KC_switch_characters);
        jpEndTurn |= Input.GetKeyDown(KC_link_endTurn);
        endTurn |= Input.GetKey(KC_link_endTurn);

        // Return & Escape
        pause |= Input.GetKey(KC_pause)|| Input.GetKey(KeyCode.Escape);
        jpPause |= Input.GetKeyDown(KC_pause) || Input.GetKeyDown(KeyCode.Escape);

        // Space
        talk |= Input.GetKey(KC_talk);
        jpTalk |= Input.GetKeyDown(KC_talk);

        // W/E
        jp_zoomOut |= Input.GetKeyDown(KC_zoomOut);
        jp_zoomIn |= Input.GetKeyDown(KC_zoomIn);
        zoomOut |= Input.GetKey(KC_zoomOut);
        zoomIn |= Input.GetKey(KC_zoomIn);

        // L-Shift
        jpRecenter |= Input.GetKeyDown(KC_camera_shift);
        recenter |= Input.GetKey(KC_camera_shift);
        scan_inputShift |= Input.GetKey(KC_scan_inputShift);

        undo |= Input.GetKey(KC_undo);
        
        jp_screenshot_mode |= Input.GetKeyDown(KC_ss_mode);
        screenshot_mode |= Input.GetKey(KC_ss_mode);
        
        recenterShortPress_KB = false;
        recenterLongHold_KB = false;
        #region recenter long/shortpress 
        if (recenterDetectionMode == 0 && jpRecenter) {
            recenterDetectionMode = 1;
        } else if (recenterDetectionMode == 1) {
            t_recenterHeld += Time.deltaTime;
            if (!recenter) {
                if (t_recenterHeld < 0.2f) {
                    recenterShortPress_KB = true;
                }
                t_recenterHeld = 0;
                recenterDetectionMode = 0;
            } else if (t_recenterHeld >= 0.2f && kbHasPriority) {
                recenterLongHold_KB = true;
            }
        }
        #endregion


        #region directional keybord inputs
        
        
        // If there is no controller input, then allow the keyboard to change the movement values used to move the player
        if (!up && !down) {
			if (Input.GetKey(KC_up)) {
				moveY = 1;
			} else if (Input.GetKey(KC_down)) {
				moveY = -1;
			}
		}

        if (!left && !right) {
            if (Input.GetKey(KC_left)) {
                moveX = -1;
            } else if (Input.GetKey(KC_right)) {
                moveX = 1;
            }
        }

        if (!jpRecenter && kbHasPriority) {
            if (recenterLongHold_KB) {
                if (Input.GetKey(KC_left)) {
                    camX = -1;
                } else if (Input.GetKey(KC_right)) {
                    camX = 1;
                }

                if (Input.GetKey(KC_up)) {
                    camY = -1;
                } else if (Input.GetKey(KC_down)) {
                    camY = 1;
                }
            }

            if (Input.GetKey(KC_cam_up)) camY = -1;
            if (Input.GetKey(KC_cam_down)) camY = 1;
            if (Input.GetKey(KC_cam_right)) camX = 1;
            if (Input.GetKey(KC_cam_left)) camX = -1;
        }

        #region Mouse lock logic  
        if (mouse_visible_state == 0) { // Mousecam off.
            if (SaveManager.mouse_camera_is_active) {
                Cursor.lockState = CursorLockMode.Locked;
                mouse_visible_state = 1;
            }
        } else if (mouse_visible_state == 1) { // Mousecam on.
            if (!SaveManager.mouse_camera_is_active){ // Turn mousecam off
                Cursor.lockState = CursorLockMode.None;
                mouse_visible_state = 0;
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                Cursor.lockState = CursorLockMode.None; // Hit escape (to temporarily show cursor)
                mouse_visible_state = 2;
            }
        } else if (mouse_visible_state == 2) { // Mousecam on, temporarily showing cursor
            if (Input.GetMouseButtonDown(0)) { // Click to re-lock cursor.
                Cursor.lockState = CursorLockMode.Locked;
                mouse_visible_state = 1;
            } else if (!SaveManager.mouse_camera_is_active) { // User turns mousecam off, so show cursor.
                Cursor.lockState = CursorLockMode.None;
                mouse_visible_state = 0;
            }
        }
        #endregion

        if (force_mouse_cam) {
            force_mouse_cam = false;
            SaveManager.mouse_camera_is_active = true;
        }

        if (force_lh_mouse_controls) {
            Use_preset_lhMouse();
        } else if (force_rh_mouse_controls) {
            Use_preset_rhMouse();
        }

        force_rh_mouse_controls = force_lh_mouse_controls = false;
        
        if (SaveManager.mouse_camera_is_active && !gamepad_active) {
            camX = camY = 0;
        }
        float mouse_dx = Input.GetAxis("Mouse X");
        float mouse_dy = Input.GetAxis("Mouse Y");
        //print(mouse_dx+","+mouse_dy);
        // X-axis is - to + from L to R
        // Y-Axis is - to + from D to U

        if (Mathf.Abs(mouse_dx) > 0.1f) mouse_dx *= 1.4f;
        if (Mathf.Abs(mouse_dy) > 0.1f) mouse_dy *= 1.4f;
      
        // If mouse accel goes up, then the mouse should feel more responsive so fudge the values
        if (SaveManager.mouseCam_accelScale >= 100f) {
            float accel_fac = SaveManager.mouseCam_accelScale / 100f;
            if (mouse_dx > 0.1f) mouse_dx = Mathf.Clamp(mouse_dx*accel_fac,0.1f,1f);
            if (mouse_dx < -0.1f) mouse_dx = Mathf.Clamp(mouse_dx*accel_fac,-1f,-0.1f);
            if (mouse_dy > 0.1f) mouse_dy = Mathf.Clamp(mouse_dy*accel_fac,0.1f,1f);
            if (mouse_dy < -0.1f) mouse_dy = Mathf.Clamp(mouse_dy*accel_fac,-1f,-0.1f);
        }
        mouse_dx = Mathf.Clamp(mouse_dx, -1, 1);
        mouse_dy = Mathf.Clamp(mouse_dy, -1, 1);

        float used_smoothing = mouse_smoothing;
        if (SaveManager.mouseCam_accelScale < 50f) {
            used_smoothing = Mathf.Lerp(1f, mouse_smoothing, SaveManager.mouseCam_accelScale / 50f);
        }
        mouse_dx = Mathf.Lerp(last_mouse_dx, mouse_dx, used_smoothing);
        mouse_dy = Mathf.Lerp(last_mouse_dy, mouse_dy, used_smoothing);
        last_mouse_dx = mouse_dx;
        last_mouse_dy = mouse_dy;
        
        if (SaveManager.mouse_camera_is_active&& !gamepad_active) {
            camX = mouse_dx;
            camY = mouse_dy * -1; // Add -1 here so that moving up points camera up
        }

        
        
        if (invertX) camX *= -1;
        if (invertY) camY *= -1;

        up |= Input.GetKey(KC_up);
		down |= Input.GetKey(KC_down);
		left |= Input.GetKey(KC_left);
		right |= Input.GetKey(KC_right);

		jpUp |= Input.GetKeyDown(KC_up);
		jpDown |= Input.GetKeyDown(KC_down);
		jpLeft |= Input.GetKeyDown(KC_left);
		jpRight |= Input.GetKeyDown(KC_right);
		jrUp|= Input.GetKeyUp(KC_up);


        anyDir = false;
        jpAnyDir = false;
        if (up || down || right || left) {
            anyDir = true;
        }
        if (jpUp  || jpDown || jpLeft || jpRight) {
            jpAnyDir = true;
        }

        if (kbHasPriority) {
            rotateRight |= right;
            rotateLeft |= left;
            jpRotateLeft |= jpLeft;
            jpRotateRight |= jpRight;
        }
        #endregion


        #endregion

        // Set keyboard as priority if input detected
        if (!hadGamepadInput && !kbHasPriority) {
            if (left || right || up || down || rotateRight || rotateLeft || jpPlacePiece || jpJump || jpCancel || jpTalk || jpPause ||  return_to_checkpoint ) {
                kbHasPriority = true;
                force_link_player_refresh = true;
                print("Keyboard now has priority over gamepad.");
            }
        }
        if (kbHasPriority || activeGamepads <= 0) {
            gamepad_active = false;
        } else {
            gamepad_active = true;
        }

        if (!SaveManager.dash_is_separate) {
            dash_separate = sprintDash;
            jp_dash_separate = jp_sprintDash;
        }

        shortcut = false;
		if (Registry.DEV_MODE_ON) {
			shortcut = Input.GetKey(KeyCode.C);
            if (!is_melos) shortcut |= Input.GetKey(KeyCode.LeftControl);
            ////if (gamepad_active) {
            ////    shortcut |= rewiredPlayer.GetAxis("DebugShortcut") > 0.1f;
            ////}
		}
        
        
        test_shoot = false;
        if (kbHasPriority && Input.GetKey(KeyCode.C) || MyInput.gamepad_active && MyInput.cancel) {
            test_shoot = true;
        }
	}

    public static void Apply_perm_to_temp() {
        temp_KC_cancel = KC_cancel;
        temp_KC_confirm = KC_confirm;
        temp_KC_left = KC_left;
        temp_KC_down = KC_down;
        temp_KC_right = KC_right;
        temp_KC_up = KC_up;
    }
    public static void Apply_perm_to_temp_gamepad() {
        temp_gp_cancel = KBI_to_gp_binding[KBI_cancel].gp_ID;
        temp_gp_confirm = KBI_to_gp_binding[KBI_confirm].gp_ID;
    }
    public static void Use_preset_rhMouse() {
        SaveManager.mouse_camera_is_active = true;
        KC_up = KeyCode.W;
        KC_left = KeyCode.A;
        KC_down = KeyCode.S;
        KC_right = KeyCode.D;
        
        KC_cam_up = KeyCode.I;
        KC_cam_left = KeyCode.J;
        KC_cam_down = KeyCode.K;
        KC_cam_right = KeyCode.L;
        
        KC_talk = KeyCode.T;
        
        KC_switch_characters = KeyCode.Tab;
        KC_camera_shift = KeyCode.LeftShift;
        KC_jump = KeyCode.Mouse0;
        KC_teledash_and_sprint = KeyCode.Mouse1;
        KC_dash_only = KeyCode.Space;
        KC_return_to_checkpoint = KeyCode.LeftControl;
        KC_zoomIn = KeyCode.E;
        KC_zoomOut = KeyCode.R;
        
        KC_confirm = KeyCode.Mouse0;
        KC_cancel = KeyCode.Mouse1;
        
        KC_placePiece = KeyCode.Mouse0;
        KC_battle_quickRotate = KeyCode.Mouse1;
        KC_link_endTurn = KeyCode.Tab;
        KC_erase = KeyCode.E;
        KC_undo = KeyCode.Z;
        KC_scan_inputShift = KeyCode.LeftShift;

        KC_ss_mode = KeyCode.Y;

    }

    public static void Use_preset_lhMouse() {
        SaveManager.mouse_camera_is_active = true;
        // Can't conflict
        KC_up = KeyCode.UpArrow;
        KC_left = KeyCode.LeftArrow;
        KC_down = KeyCode.DownArrow;
        KC_right = KeyCode.RightArrow;
        
        KC_cam_up = KeyCode.W;
        KC_cam_left = KeyCode.A;
        KC_cam_down = KeyCode.S;
        KC_cam_right = KeyCode.D;
        
        KC_talk = KeyCode.Space;
        
        KC_switch_characters = KeyCode.P;
        KC_camera_shift = KeyCode.RightShift;
        KC_jump = KeyCode.Mouse0;
        KC_teledash_and_sprint = KeyCode.Mouse1;
        KC_dash_only = KeyCode.RightControl;
        KC_return_to_checkpoint = KeyCode.Backspace;
        KC_zoomIn = KeyCode.I;
        KC_zoomOut = KeyCode.O;
        
        KC_confirm = KeyCode.Mouse0;
        KC_cancel = KeyCode.Mouse1;
        
        KC_placePiece = KeyCode.Mouse0;
        KC_battle_quickRotate = KeyCode.Mouse1;
        KC_link_endTurn = KeyCode.N;
        KC_erase = KeyCode.Backspace;
        KC_undo = KeyCode.L;
        KC_scan_inputShift = KeyCode.RightShift;

        KC_ss_mode = KeyCode.F;

    }
    public static void Use_preset_keyboard() {
        SaveManager.mouse_camera_is_active = false;
        KC_up = KeyCode.UpArrow;
        KC_left = KeyCode.LeftArrow;
        KC_down = KeyCode.DownArrow;
        KC_right = KeyCode.RightArrow;
        
        KC_cam_up = KeyCode.W;
        KC_cam_left = KeyCode.A;
        KC_cam_down = KeyCode.S;
        KC_cam_right = KeyCode.D;

        KC_talk = KeyCode.Space;
        KC_switch_characters = KeyCode.Tab;
        KC_camera_shift = KeyCode.LeftShift;
        KC_jump = KeyCode.X;
        KC_teledash_and_sprint = KeyCode.Z;
        KC_dash_only = KeyCode.C;
        KC_return_to_checkpoint = KeyCode.LeftControl;
        KC_zoomIn = KeyCode.Q;
        KC_zoomOut = KeyCode.E;

        KC_confirm = KC_jump;
        KC_cancel = KC_teledash_and_sprint;
        KC_ss_mode = KeyCode.F;
        Use_default_linking_keys();

    }
    public static void Use_preset_keyboard_wasd() {
        SaveManager.mouse_camera_is_active = false;
        KC_up = KeyCode.W;
        KC_left = KeyCode.A;
        KC_down = KeyCode.S;
        KC_right = KeyCode.D;
        
        KC_cam_up = KeyCode.I;
        KC_cam_left = KeyCode.J;
        KC_cam_down = KeyCode.K;
        KC_cam_right = KeyCode.L;

        KC_talk = KeyCode.Space;
        KC_switch_characters = KeyCode.Tab;
        KC_camera_shift = KeyCode.RightShift;
        KC_jump = KeyCode.Comma;
        KC_teledash_and_sprint = KeyCode.Period;
        KC_dash_only = KeyCode.M;
        KC_return_to_checkpoint = KeyCode.RightControl;
        KC_zoomIn = KeyCode.U;
        KC_zoomOut = KeyCode.O;
        
        KC_ss_mode = KeyCode.H;

        KC_confirm = KC_jump;
        KC_cancel = KC_teledash_and_sprint;
        Use_wasd_linking_keys();

    }

    public static void Use_default_linking_keys() {
        KC_placePiece = KeyCode.X;
        KC_battle_quickRotate = KeyCode.Z;
        KC_link_endTurn = KeyCode.Tab;
        KC_erase = KeyCode.E;
        KC_undo = KeyCode.Backspace;
        KC_scan_inputShift = KeyCode.LeftShift;
    }
    public static void Use_wasd_linking_keys() {
        KC_placePiece = KeyCode.Comma;
        KC_battle_quickRotate = KeyCode.Period;
        KC_link_endTurn = KeyCode.Tab;
        KC_erase = KeyCode.E;
        KC_undo = KeyCode.Backspace;
        KC_scan_inputShift = KeyCode.RightShift;
    }

    public static string getTimeString(int seconds, bool no_hours=false) {
        string s; // 6059
        int sec = seconds % 60; // 59
        seconds -= sec; // 6000
        int min = seconds / 60; // 100
        min = min % 60; // 40
        int hour = seconds - min * 60; // 1 = 6000 - 40*60
        hour /= 3600;
        if (no_hours) {
            min = seconds / 60;
            s = string.Format("{0:D2}:{1:D2}", min, sec);
        } else {
            if (hour > 99) {
                s = string.Format("{0:D4}:{1:D2}:{2:D2}", hour, min, sec);
            } else {
                s = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
            }
        }

        return s;
    }    
    
    public static string getTimeString_float(float seconds, bool no_hours=false,string format = "F2") {
        string s; // 6059
        int seconds_int = (int) seconds;
        float fraction = (seconds - seconds_int);
        int sec = seconds_int % 60; // 59
        seconds_int -= sec; // 6000
        int min = seconds_int / 60; // 100
        min = min % 60; // 40
        int hour = seconds_int - min * 60; // 1 = 6000 - 40*60
        hour /= 3600;


        float sec_with_decimal = sec + fraction;
        if (no_hours) {
            min = seconds_int / 60;
            s = string.Format("{0:D2}:{1,0:00.00}", min, sec_with_decimal);
        } else {
            if (hour > 99) {
                s = string.Format("{0:D4}:{1:D2}:{2,0:00.00}", hour, min, sec_with_decimal);
            } else {
                s = string.Format("{0:D2}:{1:D2}:{2,0:00.00}", hour, min, sec_with_decimal);
            }
        }

        return s;
    }

    public static bool tagForcePS4;
    public static bool tagForceXB1;
    public static bool tagForceSwitch;
    private static bool alwaysUsePS4;
    private static bool alwaysUseSwitch;
	public static string replaceTags(string s) {
        s = s.Replace("{RIGHTICON}", "<sprite=20>");
        s = s.Replace("{LEFTICON}", "<sprite=21>");
        s = s.Replace("{DOWNICON}", "<sprite=22>");
        s = s.Replace("{UPICON}", "<sprite=23>");
        if (s.IndexOf("{TIME}",StringComparison.InvariantCulture) != -1) {
            s = s.Replace("{TIME}", getTimeString(SaveManager.playtime));
        }

        ////if (s.IndexOf("{LINKTIME}",StringComparison.InvariantCulture) != -1) {
////            s = s.Replace("{LINKTIME}", getTimeString_float(Battle_PlayerController.last_battleTimeForDialogue,true));
        ////    }

        tagForcePS4 = tagForceXB1 = tagForceSwitch = false;
        if (SaveManager.buttonLabelType == 1) {
            tagForcePS4 = true;
        } else if (SaveManager.buttonLabelType == 2) {
            tagForceXB1 = true;
        } else if (SaveManager.buttonLabelType == 3 || SaveManager.buttonLabelType == 4) {
            tagForceSwitch = true;
        }

        bool force_gamepad_labels = false;
        if (temp_gp_cancel > -1) force_gamepad_labels = true; // Show gamepad labels in gamepad rebinding even if accessing with keyboard
        if ((!SaveManager.controllerDisable && activeGamepads > 0 && gamepad_active) || force_gamepad_labels) {
            ////s = s.Replace("{CAMERA}", DM.inst.getRaw("controlLabels", 3));
            ////s = s.Replace("{MOVEMENT}", DM.inst.getRaw("controlLabels", 0));
            // 9-12, triangle, circle, cross, Square

            bool isPS4 = false;
            bool isSwitch = false;
            if (alwaysUsePS4 && SaveManager.buttonLabelType == 0) isPS4 = true;
            if (alwaysUseSwitch && SaveManager.buttonLabelType == 0) isSwitch = true;
            /*/*
            if (activeGamepads > 0 && !SaveManager.controllerDisable && rewiredPlayer.controllers.GetLastActiveController() != null && rewiredPlayer.controllers.GetLastActiveController().name.ToLower().IndexOf("dualshock", StringComparison.InvariantCulture) != -1) {
                if (SaveManager.buttonLabelType == 0) {
                    isPS4 = true;
                    alwaysUsePS4 = true;
                }
            } else if (activeGamepads > 0 && !SaveManager.controllerDisable && rewiredPlayer.controllers.GetLastActiveController() != null) {
                string controlname = rewiredPlayer.controllers.GetLastActiveController().name;
                if (controlname.IndexOf("Joy-Con", StringComparison.InvariantCulture) != -1 || controlname.IndexOf("Nintendo Switch", StringComparison.InvariantCulture) != -1 || controlname.IndexOf("Pro Controller", StringComparison.InvariantCulture) != -1) {
                    if (SaveManager.buttonLabelType == 0) {
                        isSwitch = true;
                        alwaysUseSwitch = true;
                    }
                }
            }*/
            if (SaveManager.invertConfirmCancel) {
                s = s.Replace("{CANCEL}", "{TEMP}");
                s = s.Replace("{CONFIRM}", "{CANCEL}");
                s = s.Replace("{TEMP}", "{CONFIRM}"); 
            }

            int spriteset = 2;
            if (tagForcePS4 || isPS4) {
                spriteset = 0;
                
            } else if (tagForceSwitch || isSwitch) {
                spriteset = 1;
            }
            s = s.Replace("{SPRINT}", Get_gp_sprite(KBI_sprintdash,spriteset));
            if (SaveManager.dash_is_separate) {
                s = s.Replace("{DASH}", Get_gp_sprite(KBI_sepDash,spriteset));
            } else {
                if (temp_gp_cancel > -1) {
                    // Hack so that sep dash in the controls rebinding always displays right
                    s = s.Replace("{DASH}", Get_gp_sprite(KBI_sepDash, spriteset));
                } else {
                    s = s.Replace("{DASH}", Get_gp_sprite(KBI_sprintdash, spriteset));
                }
            }
            s = s.Replace("{TALK}", Get_gp_sprite(KBI_talk,spriteset)); // Triangle/xY
            s = s.Replace("{CANCEL}", Get_gp_sprite(KBI_cancel,spriteset));  // Circle/xB
            s = s.Replace("{CONFIRM}", Get_gp_sprite(KBI_confirm,spriteset)); // Cross/xA
            s = s.Replace("{JUMP}", Get_gp_sprite(KBI_jump,spriteset)); // Cross/xA
            s = s.Replace("{PAUSE}", Get_gp_sprite(KBI_pause,spriteset)); // "the Options Button"
            s = s.Replace("{SWITCHPLAYER}", Get_gp_sprite(KBI_switchChar,spriteset)); // L2
            s = s.Replace("{ZOOMOUT}", Get_gp_sprite(KBI_zoomOut,spriteset)); // r1
            s = s.Replace("{ZOOMIN}", Get_gp_sprite(KBI_zoomIn,spriteset)); // l1
            
            s = s.Replace("{ERASE}", Get_gp_sprite(KBI_erase,spriteset)); // Circle/
            s = s.Replace("{CHECKPOINT}", Get_gp_sprite(KBI_checkpoint,spriteset)); // Circle/
            s = s.Replace("{PLACEPIECE}",Get_gp_sprite(KBI_placePiece,spriteset)); // Cross, Same as CONFIRM!
            s = s.Replace("{ENDTURN}",Get_gp_sprite(KBI_endTurn,spriteset)); // R2
            s = s.Replace("{ROTATE}","{ROTATELEFT}/{ROTATERIGHT}"); // l1, R1
            s = s.Replace("{ROTATERIGHT}",Get_gp_sprite(KBI_rotRight,spriteset)); // r1
            s = s.Replace("{ROTATELEFT}",Get_gp_sprite(KBI_rotLeft,spriteset)); // l1
            
            
            s = s.Replace("{CAMERAMODE}", Get_gp_sprite(KBI_ss_mode,spriteset));
            s = s.Replace("{RECENTER}", Get_gp_sprite(KBI_recenter,spriteset)); // R3
            s = s.Replace("{QUICKROTATE}",Get_gp_sprite(KBI_quickRotate,spriteset)); // Square
            s = s.Replace("{UNDO}",Get_gp_sprite(KBI_undo,spriteset)); // L2

            s = s.Replace("{ROTATESHIFT}",replaceKeyCodeNames(KC_camera_shift)); // SAME AS RECENTER!
            s = s.Replace("{ZOOMINK}", replaceKeyCodeNames(KC_zoomIn));
            s = s.Replace("{ZOOMOUTK}", replaceKeyCodeNames(KC_zoomOut));
            
            s = s.Replace("{SEPDASH}", Get_gp_sprite(KBI_sepDash, spriteset));
            return s;
		}

        // confirm, cancel, pause, up down left right, camera_up, camera_down
        // camera, movement

        // 3D Only
        ////s = s.Replace("{CAMERA}", DM.inst.getRaw("controlLabels", 2));
        s = s.Replace("{CAMKEYS}", replaceKeyCodeNames(KC_cam_up)+"/"+replaceKeyCodeNames(KC_cam_down)+"/"+replaceKeyCodeNames(KC_cam_right)+"/"+replaceKeyCodeNames(KC_cam_left));
        s = s.Replace("{SPRINT}", replaceKeyCodeNames(KC_teledash_and_sprint)); 
        s = s.Replace("{DASH}", replaceKeyCodeNames(get_dash_keyCode())); 
        s = s.Replace("{SEPDASH}", replaceKeyCodeNames(KC_dash_only)); 
        s = s.Replace("{SWITCHPLAYER}", replaceKeyCodeNames(KC_switch_characters)); 
        s = s.Replace("{CONFIRM}",replaceKeyCodeNames(KC_confirm));
		s = s.Replace("{CANCEL}",replaceKeyCodeNames(KC_cancel));
        s = s.Replace("{JUMP}", replaceKeyCodeNames(KC_jump));
        s = s.Replace("{RECENTER}",replaceKeyCodeNames(KC_camera_shift));  
        s = s.Replace("{ZOOMIN}", replaceKeyCodeNames(KC_zoomIn));
        s = s.Replace("{ZOOMOUT}", replaceKeyCodeNames(KC_zoomOut));  
        s = s.Replace("{ZOOMINK}", replaceKeyCodeNames(KC_zoomIn));
        s = s.Replace("{ZOOMOUTK}", replaceKeyCodeNames(KC_zoomOut));
        s = s.Replace("{CHECKPOINT}", replaceKeyCodeNames(KC_return_to_checkpoint));
        s = s.Replace("{CAMERAMODE}", replaceKeyCodeNames(KC_ss_mode));
        
        // Linking Only
        s = s.Replace("{ERASE}", replaceKeyCodeNames(KC_erase));
        s = s.Replace("{PLACEPIECE}",replaceKeyCodeNames(KC_placePiece));
        s = s.Replace("{ENDTURN}",replaceKeyCodeNames(KC_link_endTurn));
        s = s.Replace("{ROTATESHIFT}",replaceKeyCodeNames(KC_scan_inputShift));
        s = s.Replace("{QUICKROTATE}",replaceKeyCodeNames(KC_battle_quickRotate)); 
        s = s.Replace("{UNDO}",replaceKeyCodeNames(KC_undo)); 
        
        // Both
        ////s = s.Replace("{MOVEMENT}", DM.inst.getRaw("controlLabels", 1));
        s = s.Replace("{TALK}", replaceKeyCodeNames(KC_talk));
        s = s.Replace("{PAUSE}",replaceKeyCodeNames(KC_pause));
        s = s.Replace("{UP}",replaceKeyCodeNames(KC_up));
		s = s.Replace("{RIGHT}",replaceKeyCodeNames(KC_right));
		s = s.Replace("{DOWN}",replaceKeyCodeNames(KC_down));
		s = s.Replace("{LEFT}",replaceKeyCodeNames(KC_left));

        return s;
	}

    public static KeyCode get_dash_keyCode() {
        ////if (DataLoader.instance.isPaused) return KC_dash_only; // hack so settings show correct
        if (SaveManager.dash_is_separate) {
            return KC_dash_only;
        } else {
            return KC_teledash_and_sprint;
        }
    }

	public static string replaceKeyCodeNames(KeyCode k) {
		if (k == KeyCode.UpArrow) return "<sprite=23>";
		if (k == KeyCode.RightArrow) return "<sprite=20>";
		if (k == KeyCode.DownArrow) return "<sprite=22>";
		if (k == KeyCode.LeftArrow) return "<sprite=21>";
        if (k == KeyCode.LeftShift) return "Left Shift";
        if (k == KeyCode.RightShift) return "Right Shift";
        if (k == KeyCode.LeftControl) return "Left CTRL";
        if (k == KeyCode.RightControl) return "Right CTRL";
        if (k == KeyCode.Comma) return "Comma (,)";
        if (k == KeyCode.Period) return "Period (.)";
        if (k == KeyCode.Mouse0) return "Left Click";
        if (k == KeyCode.Mouse1) return "Right Click";
        if (k == KeyCode.Mouse4) return "Mouse 4";
        if (k == KeyCode.Mouse5) return "Mouse 5";
        if (k == KeyCode.Mouse6) return "Mouse 6";
		return k.ToString();
	}
    
    public static KeyCode Convert_keybindIndex_to_keycode(int keybind_index) {
        if (keybind_index == 0) return temp_KC_up;
        if (keybind_index == 1) return temp_KC_down;
        if (keybind_index == 2) return temp_KC_left;
        if (keybind_index == 3) return temp_KC_right;
        
        if (keybind_index == 4) return KC_teledash_and_sprint;
        if (keybind_index == 5) return KC_jump;
        if (keybind_index == 6) return KC_talk;
        if (keybind_index == KBI_cancel) return temp_KC_cancel;
        if (keybind_index == 8) return KC_pause;
        if (keybind_index == 9) return KC_switch_characters;
        if (keybind_index == 10) return KC_zoomIn;
        if (keybind_index == 11) return KC_zoomOut;
        
        if (keybind_index == 13) return KC_battle_quickRotate;
        if (keybind_index == 16) return KC_scan_inputShift;
        if (keybind_index == 17) return KC_link_endTurn;
        if (keybind_index == 18) return KC_erase;
        if (keybind_index == 19) return KC_undo;
        
        if (keybind_index == KBI_confirm) return temp_KC_confirm;
        if (keybind_index == 21) return KC_camera_shift;
        if (keybind_index == 22) return KC_placePiece;
        if (keybind_index == 23) return KC_dash_only;
        if (keybind_index == 24) return KC_return_to_checkpoint;

        if (keybind_index == 33) return KC_cam_up;
        if (keybind_index == 34)return KC_cam_right;
        if (keybind_index == 35)return KC_cam_down;
        if (keybind_index == 36)return KC_cam_left;
        if (keybind_index == 37)return KC_ss_mode;
        return KeyCode.None;
    }
public static string Convert_keybindIndex_to_gamepadIcon(int keybind_index) {

    string s_replace = "";
        if (keybind_index == 0) s_replace = "{UPICON}";
        if (keybind_index == 1) s_replace = "{DOWNICON}";
        if (keybind_index == 2) s_replace = "{LEFTICON}";
        if (keybind_index == 3) s_replace = "{RIGHTICON}";
        
        if (keybind_index == 4) s_replace = "{SPRINT}";
        if (keybind_index == 5) s_replace = "{JUMP}";
        if (keybind_index == 6) s_replace = "{TALK}";
        if (keybind_index == KBI_cancel) s_replace = "{CANCEL}";
        if (keybind_index == 8) s_replace = "{PAUSE}";
        if (keybind_index == 9) s_replace = "{SWITCHPLAYER}";
        if (keybind_index == 10) s_replace = "{ZOOMIN}";
        if (keybind_index == 11) s_replace = "{ZOOMOUT}";
        
        if (keybind_index == 13) s_replace = "{QUICKROTATE}";
        if (keybind_index == 14) s_replace = "{ROTATELEFT}";
        if (keybind_index == 15) s_replace = "{ROTATERIGHT}";
        if (keybind_index == 16) s_replace = "---";
        if (keybind_index == 17) s_replace = "{ENDTURN}";
        if (keybind_index == 18) s_replace = "{ERASE}";
        if (keybind_index == 19) s_replace = "{UNDO}";
        
        if (keybind_index == KBI_confirm) s_replace = "{CONFIRM}";
        if (keybind_index == 21) s_replace = "{RECENTER}";
        if (keybind_index == 22) s_replace = "{PLACEPIECE}";
        if (keybind_index == 23) s_replace = "{DASH}";
        if (keybind_index == 24) s_replace = "{CHECKPOINT}";
        if (keybind_index == 37) s_replace = "{CAMERAMODE}";
        return replaceTags(s_replace);
}


    public static int KBI_up = 0;
    public static int KBI_down = 1;
    public static int KBI_left = 2;
    public static int KBI_right = 3;
    public static int KBI_sprintdash = 4;
    public static int KBI_jump = 5;
    public static int KBI_talk = 6;
    public static int KBI_cancel = 7;
    public static int KBI_pause = 8;
    public static int KBI_switchChar = 9;
    public static int KBI_zoomIn = 10;
    public static int KBI_zoomOut = 11;
    // no 12 needed  (ui only)
    public static int KBI_quickRotate = 13;
    public static int KBI_rotLeft = 14;
    public static int KBI_rotRight = 15;
    // no 16 needed (no rot toggle on gamepad)
    public static int KBI_endTurn = 17;
    public static int KBI_erase = 18;
    public static int KBI_undo = 19;
    public static int KBI_confirm = 20;
    public static int KBI_recenter = 21;
    public static int KBI_placePiece = 22;
    public static int KBI_sepDash = 23;
    public static int KBI_checkpoint = 24;
    public static int KBI_movement = 25; // Gamepad only
    public static int KBI_camera = 26; // gamepad only
    public static int KBI_ss_mode = 37;
    
    
    
    public static KeyCode temp_KC_cancel;
    public static KeyCode temp_KC_confirm;
    public static KeyCode temp_KC_up;
    public static KeyCode temp_KC_down;
    public static KeyCode temp_KC_left;
    public static KeyCode temp_KC_right;
    // Note that URDL and confirm/cancel are only temp
    public static void Reassign_keycode(KeyCode new_keycode, int keybind_index_to_update) {
        if (keybind_index_to_update == 0) temp_KC_up = new_keycode;
        if (keybind_index_to_update == 1) temp_KC_down = new_keycode;
        if (keybind_index_to_update == 2) temp_KC_left = new_keycode;
        if (keybind_index_to_update == 3) temp_KC_right = new_keycode;
        
        if (keybind_index_to_update == 4) KC_teledash_and_sprint = new_keycode;
        if (keybind_index_to_update == 5) KC_jump = new_keycode;
        if (keybind_index_to_update == 6) KC_talk = new_keycode;
        if (keybind_index_to_update == KBI_cancel) temp_KC_cancel = new_keycode;
        if (keybind_index_to_update == 8) KC_pause = new_keycode;
        if (keybind_index_to_update == 9) KC_switch_characters = new_keycode;
        if (keybind_index_to_update == 10) KC_zoomIn = new_keycode;
        if (keybind_index_to_update == 11) KC_zoomOut = new_keycode;
        
        if (keybind_index_to_update == 13) KC_battle_quickRotate = new_keycode;
        if (keybind_index_to_update == 16) KC_scan_inputShift = new_keycode;
        if (keybind_index_to_update == 17) KC_link_endTurn = new_keycode;
        if (keybind_index_to_update == 18) KC_erase = new_keycode;
        if (keybind_index_to_update == 19) KC_undo = new_keycode;
        
        if (keybind_index_to_update == KBI_confirm) temp_KC_confirm = new_keycode;
        if (keybind_index_to_update == 21) KC_camera_shift = new_keycode;
        if (keybind_index_to_update == 22) KC_placePiece = new_keycode;
        if (keybind_index_to_update == 23) KC_dash_only = new_keycode;
        if (keybind_index_to_update == 24) KC_return_to_checkpoint = new_keycode;
        
        if (keybind_index_to_update == 33) KC_cam_up = new_keycode;
        if (keybind_index_to_update == 34) KC_cam_right = new_keycode;
        if (keybind_index_to_update == 35) KC_cam_down = new_keycode;
        if (keybind_index_to_update == 36) KC_cam_left = new_keycode;
        if (keybind_index_to_update == 37) KC_ss_mode= new_keycode;
    }

    public static void Init_temp_keybindings() {
        temp_KC_cancel = KC_cancel;
        temp_KC_confirm = KC_confirm;
        temp_KC_up = KC_up;
        temp_KC_down = KC_down;
        temp_KC_left = KC_left;
        temp_KC_right = KC_right;
    }

    public static int temp_gp_cancel = -1;
    public static int temp_gp_confirm = -1;
    public static void Init_temp_gamepad_bindings() {
        temp_gp_cancel = KBI_to_gp_binding[KBI_cancel].gp_ID;
        temp_gp_confirm = KBI_to_gp_binding[KBI_confirm].gp_ID;
    }

    public static void Finalize_gamepad_bindings() {
        KBI_to_gp_binding[KBI_cancel].gp_ID = temp_gp_cancel;
        KBI_to_gp_binding[KBI_confirm].gp_ID = temp_gp_confirm;
        temp_gp_cancel = -1;
        temp_gp_confirm = -1;
    }
    
    // Used to finalize the UI controls in the settings menu (which don't change till exiting from the keybinding submenus)
    public static void Finalize_keybindings() {
        KC_confirm = temp_KC_confirm;
        KC_cancel = temp_KC_cancel;
        KC_up = temp_KC_up;
        KC_down = temp_KC_down;
        KC_right = temp_KC_right;
        KC_left = temp_KC_left;
    }

    public static void Use_preset_gamepad_default() {
        KBI_to_gp_binding[KBI_up].gp_ID = GAMEPAD_undefined;
        KBI_to_gp_binding[KBI_down].gp_ID = GAMEPAD_undefined;
        KBI_to_gp_binding[KBI_left].gp_ID = GAMEPAD_undefined;
        KBI_to_gp_binding[KBI_right].gp_ID = GAMEPAD_undefined;
        
        // Sprintdash, Jump, Talk, Cancel, Pause, Switchchar, zoom in, zoom out
        KBI_to_gp_binding[KBI_sprintdash].gp_ID = GAMEPAD_R2;
        KBI_to_gp_binding[KBI_jump].gp_ID = GAMEPAD_face_down;
        KBI_to_gp_binding[KBI_talk].gp_ID = GAMEPAD_face_up;
        KBI_to_gp_binding[KBI_cancel].gp_ID = GAMEPAD_face_right;
        KBI_to_gp_binding[KBI_pause].gp_ID = GAMEPAD_start;
        KBI_to_gp_binding[KBI_switchChar].gp_ID = GAMEPAD_L2;
        KBI_to_gp_binding[KBI_zoomIn].gp_ID = GAMEPAD_L1;
        KBI_to_gp_binding[KBI_zoomOut].gp_ID = GAMEPAD_R1;
        
        // Place, Quick rotate, rot L, rot R, Rot Toggle
        KBI_to_gp_binding[12].gp_ID = GAMEPAD_undefined; // Link UI Instructions only
        KBI_to_gp_binding[KBI_quickRotate].gp_ID = GAMEPAD_face_left;
        KBI_to_gp_binding[KBI_rotLeft].gp_ID = GAMEPAD_L1;
        KBI_to_gp_binding[KBI_rotRight].gp_ID = GAMEPAD_R1;
        
        KBI_to_gp_binding[16].gp_ID = GAMEPAD_undefined; // Nothing - rotate toggle
        
        // End turn, erase, Undo, Confirm
        KBI_to_gp_binding[KBI_endTurn].gp_ID = GAMEPAD_R2; 
        KBI_to_gp_binding[KBI_erase].gp_ID = GAMEPAD_face_right; 
        KBI_to_gp_binding[KBI_undo].gp_ID = GAMEPAD_L2;  
        KBI_to_gp_binding[KBI_confirm].gp_ID = GAMEPAD_face_down;
        
        // Recenter, place, sepdash, checkpoint, movement and camera
        KBI_to_gp_binding[KBI_recenter].gp_ID = GAMEPAD_R3; 
        KBI_to_gp_binding[KBI_placePiece].gp_ID = GAMEPAD_face_down; 
        KBI_to_gp_binding[KBI_sepDash].gp_ID = GAMEPAD_face_left;  
        KBI_to_gp_binding[KBI_checkpoint].gp_ID = GAMEPAD_face_right; 
        KBI_to_gp_binding[KBI_movement].gp_ID = GAMEPAD_undefined; // Not rebindable in the simple mode 
        KBI_to_gp_binding[KBI_camera].gp_ID = GAMEPAD_undefined;// Not rebindable in the simple mode

        KBI_to_gp_binding[KBI_ss_mode].gp_ID = GAMEPAD_L3;

    }

    public static int Get_just_pressed_gamepad_id() {
        for (int i = 0; i < action_names.Length; i++) {
            ////if (rewiredPlayer.GetButtonDown(action_names[i])) {
            ////return i;
            ////}
        }

        return -1;
    }

    public static string Get_gp_sprite(int kbi, int sprite_set) {
        int gp_id = KBI_to_gp_binding[kbi].gp_ID;
        if (temp_gp_cancel > -1 && kbi == KBI_cancel) gp_id = temp_gp_cancel;
        if (temp_gp_confirm > -1 && kbi == KBI_confirm) gp_id = temp_gp_confirm;
        int frame = 0;
        if (sprite_set == 0) { // ps4
            if (gp_id == GAMEPAD_face_up) frame = 1;
            if (gp_id == GAMEPAD_face_right) frame = 2;
            if (gp_id == GAMEPAD_face_down) frame = 3;
            if (gp_id == GAMEPAD_face_left) frame = 0;
            if (gp_id == GAMEPAD_R1) frame = 5;
            if (gp_id == GAMEPAD_R2) frame = 7;
            if (gp_id == GAMEPAD_L1) frame = 4;
            if (gp_id == GAMEPAD_L2) frame = 6;
            if (gp_id == GAMEPAD_R3) return "R3";
            if (gp_id == GAMEPAD_L3) return "L3";
            ////if (gp_id == GAMEPAD_start) return DM.inst.getRaw("controlLabels",20);
            ////if (gp_id == GAMEPAD_select) return DM.inst.getRaw("controlLabels",28);
        } else if (sprite_set == 1) {//switch
            if (SaveManager.buttonLabelType == 4) {
                if (gp_id == GAMEPAD_face_up) frame = 19;
                if (gp_id == GAMEPAD_face_right) frame = 9;
                if (gp_id == GAMEPAD_face_down) frame = 8;
                if (gp_id == GAMEPAD_face_left) frame = 18;
            } else {
                if (gp_id == GAMEPAD_face_up) frame = 18;
                if (gp_id == GAMEPAD_face_right) frame = 8;
                if (gp_id == GAMEPAD_face_down) frame = 9;
                if (gp_id == GAMEPAD_face_left) frame = 19;
            }

            if (gp_id == GAMEPAD_R1) frame = 25;
            if (gp_id == GAMEPAD_R2) frame = 27;
            if (gp_id == GAMEPAD_L1) frame = 24;
            if (gp_id == GAMEPAD_L2) frame = 26;
            ////if (gp_id == GAMEPAD_R3) return DM.inst.getRaw("controlLabels",3);
            ////if (gp_id == GAMEPAD_L3) return DM.inst.getRaw("controlLabels",0);
            ////if (gp_id == GAMEPAD_start) return DM.inst.getRaw("controlLabels",22);
            ////if (gp_id == GAMEPAD_select) return DM.inst.getRaw("controlLabels",29);
            
        } else if (sprite_set == 2) { //xb
            if (gp_id == GAMEPAD_face_up) frame = 13;
            if (gp_id == GAMEPAD_face_right) frame = 11;
            if (gp_id == GAMEPAD_face_down) frame = 10;
            if (gp_id == GAMEPAD_face_left) frame = 12;
            if (gp_id == GAMEPAD_R1) frame = 15;
            if (gp_id == GAMEPAD_R2) frame = 17;
            if (gp_id == GAMEPAD_L1) frame = 14;
            if (gp_id == GAMEPAD_L2) frame = 16;
            ////if (gp_id == GAMEPAD_R3) return DM.inst.getRaw("controlLabels",3);
            ////if (gp_id == GAMEPAD_L3) return DM.inst.getRaw("controlLabels",0);
            ////if (gp_id == GAMEPAD_start) return DM.inst.getRaw("controlLabels",13);
            ////if (gp_id == GAMEPAD_select) return DM.inst.getRaw("controlLabels",30);
        }

        return "<sprite=" + frame + ">";
    }
    
}
