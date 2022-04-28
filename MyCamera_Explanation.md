# Brief Overview

## Initializations

There's some reference to "fixedFollow", etc. Some variables and code here are old, from All Our Asias(!) or Anodyne 2.

## LateUpdate()

Where most of the controls happen. Essentially, a base '**offset**' (236) from the player is taken and transformed based on camera state. Eventually, based on what the player's doing, we're able to create a Quaternion rotation for that **offset** around line 809, and use it to transform the **offset** into an orientation for the camera's position and rotation, relative to the 'targetPos' (i.e., the player).

Further, there's collision avoidance starting around line 820. This checks if the camera's in an invalid state and tries to get it out of that state smoothly.

## Rest of the file

The rest is helper functions or variables related to implementing the camera calculations or collision avoidance!


# Detailed Overview

## Awake() (Line 82)

initialCameraRotation is static, this is so other scripts can set what the starting angle of the camera should be. I don't think this practice is ideal, though.

## Start()

You might see "fixedFollow" here and there - this is old code from All Our Asias/Anodyne 2 that was used for fixed camera angles. You can ignore it for Sephonie's purposes.

The rest is just initializations.

## switchModeInitialization to switchToFixed

Not used in Sephonie IIRC. I'm actually surprised how much stuff I just left in here from Anodyne 2...

## LateUpdate()

The main update code for the camera. I forget exactly why this uses LateUpdate vs Update - it might be related to the game's input manager which refreshes itself in Update(), so the camera code needed to get the latest input..?

### 208 - pausing

Some code for pausing the camera. Commented out for this project, but there are times where you need the player to be unable to control the camera.

###  233

In Sephonie the game only tracks the player, which is the 'target' in this case. During linking the game still shows the 3D view of the scene (and things move). The player can sometimes jitter slightly, which is why the game will then track a fixed position (pausedRBPos) during ONYX linking.

### 239 - test_3d_mode

This is unused prototyping code. Likewise, you can ignore the 'fixed follow' stuff after.

## 274 to 311 - Camera lagging behind player

When moving forward, the game will gradually lerp to target a point slightly behind the player. That lerping happens on line 311, using "t_xzCamLag". I usually name my timers with the prefix "t_" and their max value "tm_".

I don't have a good justification for doing this. I guess the idea could be you get a better view of the level when moving forward? I think I just copied this from some other game though, it might be pointless.

## 311 Smooth air dashing

When dashing in the air, you teleport forward a bit. This code makes sure the camera doesn't jut forward too fast.

## 344 Smooth wallrun entry

Likewise, entering a wallrun involves teleporting the player slightly. This code ensures that the camera smoothly follows.

Since both of these teleportations only apply along the XZ-plane, on line 363 I just set the target position's y value to the player's. I apply an offset 'mediumYOff' to look at a point slightly higher than the center of the player model.

## 372 to 463 Jump Camera

This code is messy, I don't really like the implementation. The point is to not move the camera every single time you jump - only move it when the player moves far enough off the ground (less nauseating). Here I use a kind of janky approach involving how far you've moved, but nowadays I think I'd just find the player's screenspace position and begin to closely track the player's y-position, if the player moves outside of a 'deadzone' box in screenspace. 

It's further janky, because the player jumping in the player controller actually tells the camera code to enter 'jump camera mode', which gets messy. Anyways, it 'works' but isn't ideal.

... to be continued...










