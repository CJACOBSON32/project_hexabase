# Project Hexabase
Projects hexabase is an ongoing unity package focused on the procedural animation of creatures with legs.

Here's a demo of a creature animated using the tool:

https://user-images.githubusercontent.com/34342644/188280432-5cdf1e99-7fea-4355-a165-1a62f281211b.mp4

The system is also extensible to different sizes of creatures with different numbers of legs:

https://user-images.githubusercontent.com/34342644/188280998-90176afb-4a74-4da6-bcc4-e970cee8e0f3.mp4


## In-engine editor
The project features an in-engine editor that allows the developer to modify aspects like set-radius, idle-time, and step-curve (parameters explained bellow).

https://user-images.githubusercontent.com/34342644/188280586-42691bf7-7869-489c-bdd6-8c8c8261dd29.mp4

### Parameters to adjust
There are a set of adjustable parameters have the following effects on the resulting animation:

| Parameter | Description |
|-----------|-------------|
| `body` | This is the root bone for the animation. Legs will use inverse-kinematics rooted at this GameObject |
| `stand radius` | This is the radius of the red circle seen in the video above. If the leg remains within this circle it will remain idle and not take a step. |
| `step-radius` | This is the radius of the yellow circle seen in the video above. If the leg is outside this radius, it will immediately take a step. |
| `idle-time` | The amount of time a leg should stay idle within the step-radius before self-centering. |
| `step-time` | The amount of time each step should take in seconds. This controls the speed of the stepping animation. |
| `step-curve` | The animation curve of the step. This defaults as a parabola, but any variation on the y-axis with keyframes can be implemented. |
| `leg-targest` | Use this to define gameobjects to be used as ik targets for legs and targets on the ground for the legs to step towards. The ground targets should be direct children of the `body` GameObject and the leg targets should be the foot bones in whatever rig you are animating. |

## How it works
This animation system uses a simple state machine to determine which animation state each leg should be in. A secondary limitation ensures that only one leg-group can be stepping at once.

![Animation_State_Machine](https://user-images.githubusercontent.com/34342644/188281083-f34a7e88-7165-4bf4-b620-6c8290e1e491.jpg)

## Installation
Currently this project is not in a Unity package. You may use this projects as a template if you wish to use the anmiation tool in this state, however be aware it is a work-in-progress.
