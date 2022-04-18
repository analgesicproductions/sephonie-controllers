# Sephonie Camera + Player Controller v 1.0
The player and camera controls for the game Sephonie (released April 12 2022). **Please note the repository's filenames and scripts might have spoilers for Sephonie.**



**AS ALWAYS if you have any questions about the code, feel free to ask here or on Twitter or Discord and I'll update the [FAQ](FAQ.md)**.

You can also skip to the [Player Code Explanation](MyPlayer3D_Explanation.md) or the [Camera Code Explanation](MyCamera_Explanation.md).

## What this is not
1. This does not have: level data, cutscene data, most of the models, SFX, music, dialogue, etc - i.e. this repository is just the necessary code/assets to get Ing-wen running around in a test level.
## Can I use this for ___
Yes! Art assets, animations, levels, code, whatever. Commercial or not. Just include the MIT license.
## How to use this repository
1. This repository uses Unity 2019.4.2f1 on Windows, with URP. In theory, it should work with later versions of Unity as long as you're using the same (or later) versions of URP. Since it only uses the default URP Lit shader, upgrades shouldn't be an issue.
2. You should be able to clone this repo and open it with Unity Hub, and hit play (after some compilation.), and be able to run around with keyboard controls

## Difference from the released game's code
1. References to other entities (e.g. the bouncy clouds, the pods that make you jump high) have been commented out with '////' or '/\*/\*'.
2. Things like VFX, SFX are missing. You can also only use Ing-wen.
3. No gamepad or mouse controls. They rely on Rewired, a paid plugin. I kept all of that code in the Input management script, but commented it out since you'd need to buy the plugin to use it.

## Why does the test level look so funny?
Sephonie uses Shader Graph shaders, which are a bit finicky to set up in a new project (and also the shader code involves some custom scripts of mine). I might consider releasing these at another date, but it's more time commitment than I have right now. So I did the next best thing and just changed all of the level's shaders to URP's default Lit shader, since the focus of this project is just showing the movement/camera.

## Brief Repository Overview
### Packages
Contains a list of the plugins this project uses.
### ProjectSettings
Various unity boilerplate. HOWEVER, this is important as it contains the collision layers/tags that Sephonie uses (in DynamicsManager and TagManager)
### Assets
#### Pipelines
Contains the URP boilerplate stuff for the renderer/etc. This doesn't really matter for this project
#### Scenes
Has the single scene, Layer_1, which contains the player, camera, level geometry, etc.
#### Visual
Mostly self-explanatory. The "Can-Vault Ring.prefab" is the white/yellow ring that appears when jumping near walls. It's already added to the scene.
### Scripts
Scripts are separated into Controllers and Support. Controllers is what's more interesting - Support is just some boilerplate stuff that the camera/player were connected to (stuff like keyboard input, helper function libraries, etc)

#### Controllers
See MyPlayer3D_Explanation.md and MyCamera3D_Explanation.md for an overview of these long files!

**MyPlayer3D** controls the player's movement - walking, jumping, sprinting, dashing, wall running. It also manages grappling, entity interaction, VFX and SFX, character switching, although I commented out/removed those from this project.

**MyCamera** controls the camera! Basic camera rotation, zooming in or out, following the player, collision avoidance, etc. This file also contains Screenshot Mode code, but I removed it from this project.

#### Support
##### **CanVaultRing**
Connected to the "Can-Vault Ring" prefab. Controls the white/yellow ring indicating how close you are to a wall. It does this by checking if the player is in the air, if so, and if the player is at a certain angle, distance to the wall, and the wall is not sloped too much, the ring will appear.

This code is also used with the purple Gripshrooms (not in the project) in a really janky fashion to change their color.

##### **HF**

A heavily edited down version of the helper functions library I made. The remaining functions are for timer management, and some vector math and debug text display things.

##### **MyInput**
Manages keyboard, mouse and controller input (although controller has been commented out and the project doesn't feature a way to activate mouse controls, although the code is there).

Also contains a lot of code related to rebinding and dialogue display of inputs, although this code isn't executed in this project.

Theoretically if you use Rewired, you can uncomment that huge block starting on line 362 to get controller support, although you'd have to have a controls profile set up with the same actions (see string[] action_names).

In any case, there's a lot I could talk about in this file but it's not the focus of the project so moving on!

##### **MyRootMotionHandler**
I made a 3D platformer with cutscenes and I *still* don't know root motion is. This file is needed to make sure the player rotates correctly during a certain animation. It's automatically added to the player game object by the player code.

##### **Registry**
Hehe, yes, I still use the same singleton global class and naming convention that I learned from Flixel and photonstorm back in 2011...

This file is heavily edited down. It's mostly static variables and functions used for game state management/scene transitions.

It's used in this project because of the layer masks and composite layer masks, which are needed for collisions to work correctly (although it's usually for edge case or certain entities not present in this project, so I'm not sure how much of it actually matters in this project)

##### **SaveManager**
Also heavily edited down, none of the saving code is here since it's irrelevant.

Actually, this entire file is irrelevant, it's only here to prevent annoying compile warnings in other files!

##### **SephonieAnimationEvents**
This code is tied to the Animation Controller of Ing-wen, and is used for playing sounds with footsteps. Of course, there are no SFX in this project, so no sounds play - this file is just here because the game spits out errors if I don't include it (due to animation controller settings.)


# A Rant on Controls

You found the secret rant!

It bothers me when people say my controls feel janky or imprecise, because they aren't! (except for wallrun detection in some cases...). It's perfectly possible to master the controls and do cool things.

Calling the controls jank often hides an unwillingness to engage with the rules that Sephonie is setting forth, a stubbornness to try to play Sephonie like it's another game. It's like trying to cut bread with a spoon and calling the spoon janky, despite it being a different design.

Sephonie is presenting a nonstandard design for 3D platformers, and so it will feel different and possibly difficult at first. But that feeling shouldn't immediately be called 'jank' - it's just you learning how to engage with new gameplay rules. A good thing to keep in mind is that what feels natural to you is probably due to game design standardization - if every game has a dodge roll, then a dodge roll will feel 'natural' and the lack of one might feel 'weird'.

E.g., consider Mario 64's triple jump. Does it make 'sense' to jump higher by timing three jumps? Not really. Last time I did that in public I jumped the same height. It's something you learn to use by accepting the rules of Mario. Once you accept and get good at it, it feels fun. It's like the 'dodge roll' in Dark Souls. Totally counterintuitive (try rolling into someone), but feels fun once you accept how the game works.

Anyways, I'm tired of reading PROFESSIONAL "CRITICS" call the controls janky! If they were janky then how are there people speedrunning the game???? How come you finished the game in a reasonable time? It's exactly this kind of reactionary playing that makes it so hard for interesting, new games to flourish. Everything is just expected to work 'like it always has...' So stop it! Stop using the word janky unless the game is actually trying to be janky on purpose!

This isn't an argument that Sephonie's controls have no flaws or possible improvements - it's me taking issue with how people approach our games. If you're going to criticize the game, you need to approach it on its own terms, and THEN start your criticism from there. If you don't want to learn the controls, then instead of saying 'it's janky', you should just say 'I don't have time to learn this, but I appreciate what it's doing."

Anyways, I just wanted to get that out of my system...!






