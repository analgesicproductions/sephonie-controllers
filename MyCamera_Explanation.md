# Brief Overview

## Initializations

There's some reference to "fixedFollow", etc. Some variables and code here are old, from All Our Asias(!) or Anodyne 2.

## LateUpdate()

Where most of the controls happen. Essentially, a base '**offset**' (236) from the player is taken and transformed based on camera state. Eventually, based on what the player's doing, we're able to create a Quaternion rotation for that **offset** around line 809, and use it to transform the **offset** into an orientation for the camera's position and rotation, relative to the 'targetPos' (i.e., the player).

Further, there's collision avoidance starting around line 820. This checks if the camera's in an invalid state and tries to get it out of that state smoothly.

## Rest of the file

The rest is helper functions or variables related to implementing the camera calculations or collision avoidance!


# Detailed Overview

...coming soon!
