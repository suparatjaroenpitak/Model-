Russian Soldier Animation Export

Source:
Meshy_AI_Russian_Soldier_Shooting_Pose_Reference_20261126_character.glb

Output:
Meshy_AI_Russian_Soldier_Shooting_Pose_Animated.fbx
Meshy_AI_Russian_Soldier_Shooting_Pose_Animated.blend

Animation clips in the exported FBX:
- Shoot: frames 1-26 at 30 FPS (three short recoil shots).
- AimTurn: frames 1-54 at 30 FPS (slow scan/turn while holding the gun).
- SlowWalk: frames 1-60 at 30 FPS (slow forward movement with alternating leg swing).

Import into Unity:
1. Put the FBX in Assets/Characters/RussianSoldier.
2. In the Model Importer, enable Import Animations.
3. Open the Animations tab and verify the three takes are named Shoot, AimTurn, and SlowWalk.
4. Set Rig > Animation Type to Humanoid only if Unity accepts the generic source skeleton; otherwise use Generic.
5. Use the clips in an Animator Controller.

Notes:
- The source character is already in a crouched aiming pose. The animations preserve that pose and add recoil, root turning, and a conservative slow-walk cycle.
- The weapon is skinned as part of the character mesh rather than a separate weapon object.
- The original GLB was not overwritten. The FBX and Blender source were written to this Animated folder.
