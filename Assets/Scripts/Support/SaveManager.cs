using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
// Needed for FileStream and File
using System.IO;

// Needed for BinaryFormatter
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

// Needed for [Serializable]
using System;
using UnityEngine.Serialization;

public class SaveManager : MonoBehaviour {
    public static bool dontChangeResOnLoadBCChangedInTitle;

    public static bool dpad_cam;
    public static bool flip_sticks;
    public static bool dash_is_separate;
    public static string active_character = "amy";
    public static string tetherScene = "";
    public static Vector3 tetherPos;
    public static float tether_camEulerY;

    public static float cameraDistance = 130f;
    public static float sensitivity = 100f;
    public static float fieldOfView = 85f;
    public static float extraCamRotWithControllerMoveStrength = 50f;
    public static int playtime;
    public static string language = "en";
    internal static int currentInUseFileIndex = 0;
    public static bool invertConfirmCancel;

    public static float brightness = 1.0f;

    public static int linking_difficulty = 1; // 0 = easy, 1 = normal
    private const int Const_highest_difficulty = 1;
    public const int Const_easy_difficulty = 0;
    public const int Const_normal_difficulty = 1;

    public static float volume = 100f;
    public static float sfx_volume = 100f;
    public static bool fullscreen = true;
    public static int winResX;
    public static int winResY;
    public static bool infiniteJump;
    public static bool infinite_dash;
    public static bool sprint_delay;
    public static bool autosprint;
    public static int l_stick_rotation;
    public static int r_stick_rotation;
    public static int customUIScaling = 100;

    public static bool dialogueSkip;
    public static bool keepSkipOnUntilLoadingFromTitle = false;
    public static bool controllerDisable;
    public static int shadowQuality = 3;
    public static bool invincibility = false;
    public static int terrainQuality = 5;
    public static int buttonLabelType;
    public static bool instantText;
    public static bool dithering_on = true;
    public static bool postprocessing_on = true;
    public static bool screenshake_flash_on = true;

    public static int metacoins_notsaved = 0;

    public static bool mouse_camera_is_active;
    public static float mouseCam_accelScale = 100f;
    public static float mouseCam_rotationSpeed = 100f;
    public static float sprint_camera_snapping_speed = 70f;
    public static bool sprint_camera_snapping_on = true;
    public static bool hide_goals = false;
    public static bool hide_map = false;
    public static bool hide_tips = false;

    // Query these directly I guess? None of these need to be initialized since non-presence implies zero
    public static Dictionary<int, int> linked_with_this_species = new Dictionary<int, int>();
    public static Dictionary<int, int> encountered_species = new Dictionary<int, int>(); // used for linklist revisit
    public static Dictionary<int, float> link_times = new Dictionary<int, float>();
    public static Dictionary<int, int> bubble_adventure_state = new Dictionary<int, int>();
}