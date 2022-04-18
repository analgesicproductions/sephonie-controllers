using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game related global variables that are used by many scripts
public class Registry : MonoBehaviour {

    public static string tag_CameraMustAvoid = "Camera Must Avoid";

    public static string[] scenes_to_disable_cam_and_player = new string[] { "C0_1_Observer", "C0_1_Observer_Melos", "C0_2_Boat", "C0_3_Outro" };

    public static int lmc_GROUND = 1 << 0 | 1 << 1 |  1 << 10 | 1 << 14 | 1 << 15 | 1 << 16;
    public static int lmc_CAMERA_COLLIDABLES = 1 << 0 | 1 << 1 | 1 << 10 | 1 << 15 | 1 << 16;
    public static int lmc_PLAYER_NON_COLLIDABLES = 1 << 9 | 1 << 21 | 1 << 11 | 1 << 17 | 1 << 25;
    public static int lmc_vaultRing = 1 << 0 | 1 << 1 | 1 << 10 |1 << 11 | 1 << 14 | 1 << 15 | 1 << 22;

    public static int lm_default = 1 << 0;
    public static int lm_transparentSolids = 1 << 1; // sorting fix
    public static int lm_player = 1 << 9;
    public static int lm_terrain = 1 << 10;
    public static int lm_ignorePlayerAndCam = 1 << 11; // Used for turning off player collisions with certain objects that still need to be 'solid' for raycast purposes with Can-vault Ring, etc
    public static string layerName_ignoreCam = "IgnoresCamera";
    public static int lm_ignoreCam = 1 << 14; // Useful for ceiling decoration that's collidable but can get in the way of the camera. Also counts as ground
    public static int lm_groundIgnoreDash = 1 << 15; // For Passthrough walls - can xor this to have raycasts skip it, but keep camera casts hitting it
    public static int lm_groundCustomUse = 1 << 16; // For things that are ground, but need to be distinguished by particular scripts for avoidance (e.g. dashprism avoiding a raycast from its center)
    public static int lm_groundCustomUse_INDEX = 16;
    public static int lm_audioThings = 1 << 21;
    public static int lm_ladders = 1 << 22;
    public static int lm_ignorePlayerCamAndDash = 1 << 25;
    //public static int lm_gripwall = 1 << 18;
    

    public static float minCamDisPercent = 90f;
    public static float maxCamDisPercent = 200f;

    public static string NEW_GAME_SCENE = "C0_1_Observer";
    public static string NEW_GAME_STARTING_POSITION_OBJECT = "StartPos";
    public static string FLAG_returntosurface_ability = "returntosurfaceability";
    public static string FLAG_saw_ending = "sawtheending";

    public static string destinationDoorNameForPauseRespawn = "";
	public static string destinationDoorName = "";

	public static string enterGameFromLoad_SceneName;
	public static Vector3 enterGameFromLoad_Position;
	public static bool justLoaded = false; // reset in Update() on the DataLoader instance.

    public static bool DEV_MODE_ON = true;



    public static bool DestinationDoorIsTwoDeep = false;
    public static void MoveObjectToDestinationDoor(GameObject g) {
        if (DestinationDoorIsTwoDeep) {
            string[] parts = destinationDoorName.Split(new string[] { "||" },System.StringSplitOptions.None);
            GameObject dest = GameObject.Find(parts[0]).transform.Find(parts[1]).Find(parts[2]).gameObject;
            g.transform.position = dest.transform.position;
            DestinationDoorIsTwoDeep = false;
        } else if (GameObject.Find(destinationDoorName) != null) {
            g.transform.position = GameObject.Find(destinationDoorName).transform.position;
        }
    }



    public static string GetDefaultSceneEntranceDoor(string sceneName) {
        if (sceneName == "Layer_1_2") return "DefaultPos";
        if (sceneName == "Layer_1_1") return "DefaultPos";
        print("Warning! No default scene entrance door in SCENE: " + sceneName);
        return "none";
    }
}