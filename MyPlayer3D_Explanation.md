This is an explanation of the player code!

For the most part, this is not overly entangled with the camera code, although the player code does reference the camera's orientation (to decide what direction holding forward should translate to, etc.)

Are the practices here used best practice? No, but they did end up making a good game, so that's all that matters!

I'm going to start with a high-level conceptual overview of the file before going more into specifics.

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

This is an important function! Because of how Sephonie is coded to NOT use unity's built in physics, which feel terrible, this function runs some collision code to see if we're walking near a wall, and changes the player's movement to be parallel to it. It's not coded perfectly but mostly works. It's why you can't walk right up against a wall in Sephonie. I think there's a better way to do this but it's beyond me.

### 2082 - Walljumping

This little code pushes you away from the wall when jumping from a wallrun.

### 2158-2225

Misc functions related to the player. Stuff like editing physics with fusion cells, etc.


## 2227-2986 - jumpLogic




