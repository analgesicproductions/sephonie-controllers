I'm going to start with a high-level conceptual overview of the file before going more into specifics.

Note, that due to time, I don't explain the vector math in any detail. If you want me to, let me know and I can!

# A Rant on Controls

It bothers me when people say my controls feel janky or imprecise, because they aren't (except for wallrun detection in some cases...). Calling the controls jank often hides an unwillingness to engage with the rules that Sephonie is setting forth, a stubbornness to try to play Sephonie like it's another game. Sephonie is presenting an alternate design vision for 3D platformers, and so it WILL feel different and possibly difficult at first. But that feeling isn't 'jank' - it's the process changing your expectations from the kind of platforming control one is conditioned to feel as natural (which is due to the influence Mario 64, quirky in its own ways, has influenced platformers.) 

E.g., consider Mario 64's triple jump. Does it make 'sense' to jump higher by timing three jumps? Honestly, it doesn't. It's something you LEARN to use by accepting how Mario works! And once you accept and get good at it, it feels fun. It's like the 'dodge roll' in Dark Souls. Totally counterintuitive, but feels fun once you accept how the game works.

Anyways, I'm tired of reading PROFESSIONAL "CRITICS" call the controls janky! If they were janky then how are there people speedrunning the game???? How come you finished the game in a reasonable time? It's exactly this kind of reactionary playing that makes it so hard for interesting, new games to flourish. Everything is just expected to work 'like it always has...' So stop it! I'm sick of hearing the same shit over and over.

Anyways, I just wanted to get that out of my system... onwards with the code!

# Philosophy

This code uses common tricks like coyote time, jump buffering, boosting players over edges they barely miss. I see those less as tricks and more as ways to deal with the imprecision of moving around 3D space. The "can vault ring" (the yellow ring) is our biggest trick for making it easy to dash vault!

There's also some decisions to make movement more natural - having the horizontal movement separate from vertical is a big part of this. Moving horizontally in the air or on the ground more or less works the same, which is important so as to give the player a physical intuition of how far they can go when jumping. It also helps build a better sense of what kind of 'movement state' you're in and what rules apply.

Knowing these kinds of consistencies is also useful when level designing! I also prefer to cancel animations or transition animations quickly and snappily. The point isn't realism in movement, it's being able to really intuit what kinds of rocks you'll be able to reach or climb, etc.

The code also uses custom 'physics' (gravity) so I can really precisely control how fast you're moving.

# Brief Overview

This file handles movement, collisions with the environment, and translates player input into motion on the MyPlayer GameObject's Rigidbody. This file also issues commands to other components like the Animator, or SFX and VFX scripts.

Here's a brief overview of the functions (skipping minor ones)

## _Awake()_ and _Start()_ 

are used normally - for initializing some variables, connecting to other references, etc.

## _FixedUpdate()_ 
takes the player's input and translates that into changing the character's movement state. It also handles pausing and falling out of bounds. 

## _jumpLogic()_ 
is called from _FixedUpdate()_. It mostly handles the player's y-position, velocity and acceleration.
  
## _Update()_ 
contains code that can be used to force the player to wait a few frames at the start of a new scene. In Unity you can't poll for button-press inputs in FixedUpdate() due to timing, so this function also polls for button-presses (like jumping, sprinting, switching characters.) 

It also contains code that updates the radial meters, debug text (such as player velocity), playing some SFX.

## After that

After that are miscellaneous functions. These are called as needed by the main code.

We have some vector math used for polishing movement, reacting to entering collisions or triggers, various code for managing entity interactions (e.g. _Do_bump()_ for running into those orange-purple bumper creatures).

There's random functions related to switching the active player model (_SwitchActiveModel()_), some raycasting helpers (_SetCapsuleCastVars()_), dealing with player visibility in cutsenes (_HideDuringEvent()_), VFX management (_Start_emission_lerp_()), etc.

# Detailed Overview

## Line 1-423: Initializations

This is an incredibly messy part of the code where variables are initialized. If it's public, that means it was something I was at some point tweaking in Unity's inspector. If it's internal, that just means some other script usually needs read access to it (e.g. _died_from_speedorb_or_poison_ would be referenced by Fusion Cells so they know when to reset.)

The variables here are organized a little bit - most of the private variables are just used internally for managing state (e.g. timers, like _t_return_t_checkpoint_ which is used for keeping track of how long the 'return to checkpoint' button has been held.)

Then we have variables related to Poison Mushrooms, Wallrunning, Walking, Mud, Dash Vaults, Sprinting, Grappling Gliding, Jumping, with random stuff thrown in. This level of organization was enough for me, since usually working on the codebase involves dealing with one or two 'parts' of the player code. I don't think this would work well for a team...

## 424 - Awake()

Just some boilerplate initializations here. Also the player checks to see if it needs to turn off for certain cutscene-only scenes. Notably, Awake() might run before other systems are initialized, which is why there are no references to my other non-static code here.

## 446 - Start()

Initializing variables. Sometimes I initialize stuff in the inspector, I'm a little inconsistent.

Of interest might be 541, which deals with deciding where to spawn the player depending on whether you're entering from a scene change, game load, etc.

## 602 - go_fast/ungo_fast

Used for a little postgame secret! These change the player physics.

## 635 FixedUpdate()

655 - What's happening here is that if you're on a moving platform, your velocity is actually the player plus the platform. I want to do calculations ignoring the platform (and then adding it in later), which is why the player's velocity 'removes' the moving platform here.

656 - As long as the game's not paused, _jumpLogic()_ is called which decides the player's y-velocity and jump state. We'll come back to this later!

After that, out of bounds is handled. The player might be killed by a respawn plane, poison mushrooms, etc. I also have a hard-coded _outofboundsYVal_ which is a value in which, if the player falls below, will always respawn.

The code here is commented out, but falling out of bounds/dying will fade the screen to black (746, etc). When the screen is black, the player/camera move. There's also a variation here allowing for some explosion VFX to play when dying from poison mushrooms or fusion cells.

### 826 - pause handling

The player's code can be paused for an annoying number of reasons - ONYX Links, pause menu, dialogue, falling out of bounds. This is all commented out because this project has no pausing.

If you read through this section, you'll see some code that saves the player's velocity before freezing it (so you don't move while in the pause menu, etc)

### If controls are on... 919-2102

If 'controlsOn' is false, then the player can't move. Important for some stuff! This conditional block is really long.

924 - Checks for directional input, or pressing the sprint button. Eventually that's translated to _holdingLeft_, _holdingRight_, etc around line 985. That's because there might be conditions in which we need to cancel these inputs.

### Dash Vaulting 997-1320

_dash_mode_ tracks the state of a Dash Vault (called dash or teledash in the code.) The _dash_mode_inactive_ state is where entering dash vault, OR a sprint starts.

If you're in the air and dash, then you enter a _dash_mode_windup_ state, then teleporting, maybe breaking blocks, and warping some distance. The exciting collision logic for this can be found at 1139.

In the case of fusion dashes, at 1265 the code branches so that you start flying through the air. This puts dash_mode into _dash_mode_prismDash_, and the 'moving through the air' code is then handled elsewhere (like in the jump code, or further down at 1496 to move the player and decide when to end the fusiondash). The "Dash Prism" refers to an unused entity that acted like a cannon.

### Movement 1326

From here we have more code determining if you're going to be sprinting. Pretty much from here on is deciding what the final horizontal (or XZ) velocity will be. The y-velocity is still handled back up, inside of _jumpLogic()_. I think I decided to split things up this way so that stuff like air control behaves similar to walking, and because stuff like gravity works mostly the same in every horizontal movement state. I'm not sure this was the best way to organize things as it could get slightly confusing jumping around, but it works well enough for the relatively small moveset Sephonie has.

### Wallrunning 1354-1482

Wallrunning code. The player enters this state from elsewhere. The main idea here is that various raycasts checks that there's still a wall to wallrun on, and that it's not too shallow (so you don't wallrun on a slope), and that there's no obstacle in front of you.

If you should keep wallrunning, then the XZ-velocity is set accordingly. I also handle some animation stuff here like holding away from the wall.

### Grappling 1482

you can't move while grappling, so nothing happens here.

### Gliding initialization 1484

When jumping into one of those dandelion puffs, the player moves here.

### "Prism Dash" 1496

An old name - this is actually fusion dashing. The player keeps moving until hitting something, travelling too far, or reaching an arbitrary timeout (sometimes dashes get stuck on weird geometry, so this helps prevent softlocks).

You'll see "reflector" mentioned here - there used to be an entity that bounces you during fusion dashes! 

### Walking, sprinting, gliding - 1577

1599-1670 - The camera's orientation is used to decide what direction holding forward goes in. Depending on the player state (being in mud, holding down debug shortcuts (MyInput.shortcut, which is "C" in this project), the base walk velocity is modified.

There's code to scale walk velocity by how hard a joystick is tilted, as well as some lerping that accelerates you from zero.

1677-1718 handles gliding, which is a gentle, lerping motion based on held direction.

### 1719 - Sprinting

Most of the complicated part of sprinting is the movement feel of rotating. It was a late addition, but actually your sprint rotation speed used to be static (based on holding left or right), but we made it gradual (line 1779). If you don't hold a sprint input, your rotation gradually slows (1787).

This sprinting motion (or at least speed?) also apply to air motion - sprint jumping off a platform, or off a wall.

You'll also see timers here (1818) which deal with preventing you from entering consecutive wallruns on the same wall too quickly.

### 1825 Entering wallrun

This is in the sprinting code! If the player's jumping, the game casts the player's capsule collider to see if there's a wall to wallrun on. It only works if you're at a shallow enough angle.

Holding left or right (1853) helps the game 'cheat' a bit. If you're rotating towards a wall, it's easier to latch onto it (you can come at it from a more perpendicular angle). Still, this probably isn't a perfect approach - in certain contexts, wallruns get triggered when a player meant to do a wall vault. This is usually when not playing too carefully and sprinting all the time.

If a wallrun IS entered (1888), like ten million variables get set that decide the velocity to enter the wallrun, play animations, etc.

### Walking 1925

And if you're just walking... wow! Look how easy that is.

### Fall drag

I'm actually not sure what this does anymore. I think it relates to me trying to make sure that XZ drag (while in the air) felt the same whether you walk off of a cliff or jump into the air.

You might see "jump_trajectory_locked" here and there - that's old code referring to tests we did where you had no air control. It felt bad! For Sephonie, at least.

### Model rotatoin

1977 Handles where the player model should rotate to depending on movement direction.

###Drag

Lastly we have other drag at 2000, which runs when no movement input is held and you're on the ground. It's fairly straightforward (multiplying to bring the velocity down to zero), but changes in some situations, like standing on ice (which we removed from the game)

### Etc 2040

There's code here for dealing with the change in your velocity from the Geysers in Layer 3, weird edge cases.

### 2079 - Adjust_velocity_for_walls

This is an important function! More details around line 3388. 

### 2082 - Walljumping

This little code pushes you away from the wall when jumping from a wallrun.

### 2158-2225

Misc functions related to the player. Stuff like editing physics with fusion cells, etc.

## 2227-2986 - jumpLogic

This function is a bit of a mess, but it roughly goes:

### 2230-2270 

calculations related to using gravity, some frame timers (like coyote time things), triggering extra jumps from dash vaults, checking if you're on the ground

### 2280-2302 
ignore this, it's from old prototyping

### 2329
The conditionals here determine behavior for each state of jumping.

2329 (grappling) - This is just a lerp to the grapple point (Ribbat) position, transitioning back into the regular jumping state with _Enter_jumpstate_firstJumpArc_.

### 2363 - wallrunning 
The start of this is old code that would change how fast you're falling in a wallrun based on the curvature. This is not really intuitive so I made it so the curvature has no effect. After that, at 2405 it checks for players jumping off from the wall (_my_jp_jump_ = just pressed jump).

Depending on what side you're wallrunning on, the game applies a velocity for the jump and returns to the normal 'falling/jumping' jump state.

2458 checks to see if you've touched the ground during a wallrun. 2475 - if you've left wallrun for any reason, various timers have to be set and values changed back.

### 2493 - on the ground

This checks for jumping from the ground. Some code here is used for buffering jumps (jumping before actually touching the ground, a common game-feel technique). Line 2525 is the actual code for starting the process of getting you off the ground. Geysers can also do this to you. Line 2543 gets you off the ground if you've fallen  or are just not touching the ground due to some entity interaction in the game.

### Ground slope polish

Every platformer coder's favorite code!

2556-2668 involve a bunch of terrible stuff. Like 2585 deals with walking UP a slope and making sure your y-velocity isn't too high so that you start bumping weirdly. Likeiwse 2598 does that for walking down slopes. The math isn't too complex, but it can be a pain in the ass to debug when figuring it out.

2619 is a polish that stops you from sliding down really gentle slopes. 2653 is a hack to keep you from not 'connecting' to moving platforms. 

### 2669 - jump_state 100

This was used to give a delay to jumping to make jumping feel 'weightier'. I can't remember what this delay is set to, I might have removed it or it's only a frame or two to really just give the slightest touch of 'weight' to the player.

### 2677 - midair

This manages how fast you fall. You'll see commented out code related to Thraskias's chamber and the wind, as well as geysers and other entities.

### 2757 midair state transitions

2757 deals with entering a wallrun while falling and modifying your Y velocity. You also have some code at 2783 related to entering sprinting states when landing on the ground from a fall.

2818 Gliding (we're still midair) Triggered by the dandelion entity, this just runs the start of the gliding animation where you kind of poof upwards.

You'll also see entering grappling (2843) around here, unused code related to double jumps, the infinite jump implementation.

### 2869 -gliding

This makes falling while gliding feel kinda 'smooth' like a hang glider. Also handles jumping out of the glide!

### Rest of jump logic

The rest is handling animation state logic, like "Falling" which changes if you should look like you're falling. Idk, I don't thikn how I chose to divide logic between the animator controller and hardcoded animation changes - i don't think it makes much sense, but it works! But that's why there are some weird hiccups in animations in certain edge cases.

## More misc code

### 2994 - Start_sprinting_anim_only, Start_sprinting_speed_only

Helper functions used for playing certain animations when entering sprinting (which happens a lot).

### 3010 - Exit_climbing_mode

You used to be able to climb vines! Not anymore...

### 3029 - Set_nextJumpVel_doAnim_playSFX
Used when jumping, does stuff like play SFX, VFX if needed (like jumping while holding a white pod). Also adjusts your velocity if you're  in mud or something. 

The idea here is that you might have your initial jump velocity set in a lot of places. There's a lot of code shared, with slight variations, so it makes sense to consolidate here.


### 3081 Update_tether

Debug functions for placing and removing custom checkpoints. Mainly used when coding all the movement logic for the first time.

## 3145 - Update()

You'll see the 3153 "in_scene_initialization_phase". This is important. You OFTEN don't want the player code to do ANYTHING for a few frames when entering a scene. This is because stuff like cutscenes might need time to initialize, and the player should be able to be moved by those other things. Likewise, it's a good spot to put stuff like autosaving when entering a scene, which often depends on player state.

### 3189 -Switching players

Switching players happens in Update() bc it has nothing to do with physics. In this project you only have ing-wen, but the SwitchActiveModel function would check the game state to see if Riyou or Amy are playable, depending on the scene.

### my_jp_jump, etc

These are set here because you can't consistently check key-pressed-down events in FixedUpdate().

### Meters, emission 3229
VFX need to be updated more frequently than the physics timestep (50fps), which is why I update this stuff here. Stuff like the 'return to checkpoint' radial meter, or entering screenshot mode.

### 3291 debug text
I have a section that displays debug text on screen. Useful for showing physics values when messing around or testing.

### 3317 - more vfx

- stuff like flashing when dashing, the flickering when you don't have a dash available.


## 3388 Adjust_velocity_for_walls

Because of how Sephonie is coded to NOT use unity's built in physics, which feel terrible for precision platforming, this function runs some collision code to see if we're walking near a wall, and changes the player's movement to be parallel to it. It's not coded perfectly but mostly works. It's why you can't walk right up against a wall in Sephonie. I think there's a better way to do this but it's beyond me.

Basically I do a rigidbody sweep to see how parallel the player is walking into a wall. If too parallel, I find the vector that goes along the wall and change the velocity to that direction.

## The rest of the code

### 3531 - OnCollisionStay

Moving platforms suck. Here are some hacks that help ensure you stay on a moving platform.

### 3558 - OnTriggerEnter

Code that deals with stuff like walls you can't run on (we do this by making trigger volumes that cancel your wallrun - called "SlipWall"). You also enter stuff like Poison Shroom or tall grass (NoSprintTrigger) here. Or triggering water splashes (3586).

### 3701 Face_transform_to_vector_xz_dir

helper function to rotate the player in a direction towards something.

### 3725

You'll see a lot of random variables here. A lot of times I'llbe adding something and need to know that the player was just doing something - was_in_gripshroom_last_frame - etc. Or that the player was told to do something like **do_anim_jumpUp_to_flip**. Usually when coding this I'm coding quite fast so I just stick the variables wherever.

### Do_bounce, Do_bump

code called from other entities (like the bouncy cloud) that make the player bounce upwards or bump in a direction. These functions can be tricky since they need to interrupt movement and jumping logic, which is why a bunch of stuff is modified.

### 3850 - cursed variables

These are things related to capsule collisions and tuning those collisions. I didn't really go into this, but a lot of times I might take the character's forward vector, back it up a bit, before doing a capsule cast, for better accuracy

### 3894 - init_anim_events

boilerplate! For making it so we can play sounds when stepping on hte ground.

### 3936

This is code related to getting variables related to collisions. It's easy to make a mistake when typing these formulas, so I have them stored as helper functions. ESPECIALLY 3953's SetCapsuleCastVars, which you need to do EVERY TIME you do a capsule cast lol

### BoxCasts

Likewise I sometimes use BoxCasts. This is because unity sometimes give completely wrong results for capsule casts under certain circumstances (like hitting an edge of a mesh)

### 4037 - istouchingground

You'll see two checks here - a capsule cast and a raycast. The raycast is 'insurance' because sometimes the capsule cast is wrong...

### The rest

boilerplate for hiding the player during events! Stuff like determining how far dashes should go. Recording functions (4218) used for the Shadow Followers and the tutorial ghosts. Emission stuff.









