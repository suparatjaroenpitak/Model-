"""Create simple reusable animation clips from the supplied rigged soldier GLB.

The source is a crouched aiming pose with a 48-bone generic SmartRigArmature
and no source actions. This script preserves the source GLB and writes a new
Blender source file plus one FBX containing Shoot, AimTurn, and SlowWalk takes.
"""

import math
import os
import bpy
from mathutils import Vector


INPUT_GLB = r"D:\โมเดลเกม\Model เกม\คน\Meshy_AI_Russian_Soldier_Shooting_Pose_Reference_20261126_character.glb"
OUTPUT_DIR = r"D:\โมเดลเกม\Model เกม\คน\Animated"
OUTPUT_FBX = os.path.join(OUTPUT_DIR, "Meshy_AI_Russian_Soldier_Shooting_Pose_Animated.fbx")
OUTPUT_BLEND = os.path.join(OUTPUT_DIR, "Meshy_AI_Russian_Soldier_Shooting_Pose_Animated.blend")

FPS = 30
ACTION_CHANNELBAGS = {}


def reset_and_import():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=INPUT_GLB)

    armature = next((obj for obj in bpy.data.objects if obj.type == "ARMATURE"), None)
    if armature is None:
        raise RuntimeError("The GLB does not contain an armature.")

    # Keep only the skinned character mesh and its armature. The source GLB
    # also contains a cube/icosphere presentation setup that must not enter FBX.
    keep = {armature}
    for obj in bpy.data.objects:
        if obj.type == "MESH" and obj.find_armature() == armature:
            keep.add(obj)

    for obj in list(bpy.data.objects):
        if obj not in keep:
            bpy.data.objects.remove(obj, do_unlink=True)

    armature.name = "RussianSoldier_Armature"
    armature.data.name = "RussianSoldier_Skeleton"
    armature.show_in_front = True
    armature.animation_data_clear()
    armature.rotation_mode = "XYZ"
    armature.location = (0.0, 0.0, 0.0)
    armature.rotation_euler = (0.0, 0.0, 0.0)
    armature.scale = (1.0, 1.0, 1.0)

    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "XYZ"
        pose_bone.rotation_euler = (0.0, 0.0, 0.0)
        pose_bone.location = (0.0, 0.0, 0.0)
        pose_bone.scale = (1.0, 1.0, 1.0)

    return armature


def new_action(name, armature):
    """Create an Action using Blender 4.4+ / 5.x layered Action API."""
    action = bpy.data.actions.new(name)
    slot = action.slots.new("OBJECT", armature.name)
    layer = action.layers.new("Layer")
    strip = layer.strips.new(type="KEYFRAME")
    ACTION_CHANNELBAGS[action.name] = strip.channelbags.new(slot)
    action.use_fake_user = True
    return action, slot


def curve(action, data_path, index, keys, interpolation="BEZIER"):
    channelbag = ACTION_CHANNELBAGS[action.name]
    fcurve = channelbag.fcurves.find(data_path, index=index)
    if fcurve is None:
        fcurve = channelbag.fcurves.new(data_path=data_path, index=index)
    for frame, value in keys:
        point = fcurve.keyframe_points.insert(frame, value)
        point.interpolation = interpolation
    return fcurve


def object_channels(action, location_keys, rotation_keys):
    for axis in range(3):
        curve(action, "location", axis, [(f, v[axis]) for f, v in location_keys])
        curve(action, "rotation_euler", axis, [(f, v[axis]) for f, v in rotation_keys])


def bone_channel(action, bone_name, axis, keys):
    data_path = 'pose.bones["%s"].rotation_euler' % bone_name
    curve(action, data_path, axis, keys)


def set_action_range(action, start, end):
    action.frame_start = start
    action.frame_end = end
    action.use_fake_user = True


def make_shoot_action(armature):
    action, slot = new_action("Shoot", armature)
    frames = [(1, (0.0, 0.0, 0.0)), (3, (0.0, 0.022, -0.010)), (6, (0.0, 0.0, 0.0)),
              (13, (0.0, 0.020, -0.009)), (16, (0.0, 0.0, 0.0)),
              (23, (0.0, 0.018, -0.008)), (26, (0.0, 0.0, 0.0))]
    rotations = [(1, (0.0, 0.0, 0.0)), (3, (-0.025, 0.0, 0.0)), (6, (0.0, 0.0, 0.0)),
                 (13, (-0.022, 0.0, 0.0)), (16, (0.0, 0.0, 0.0)),
                 (23, (-0.020, 0.0, 0.0)), (26, (0.0, 0.0, 0.0))]
    object_channels(action, frames, rotations)

    # Small forearm/hand recoil layered on top of the existing aiming pose.
    for bone_name, amplitude in (("Bone_020", 0.060), ("Bone_025", 0.055),
                                 ("Bone_019", 0.035), ("Bone_024", 0.035)):
        bone_channel(action, bone_name, 0, [(1, 0.0), (3, amplitude), (6, 0.0),
                                            (13, amplitude * 0.9), (16, 0.0),
                                            (23, amplitude * 0.8), (26, 0.0)])

    set_action_range(action, 1, 26)
    return action


def make_aim_turn_action(armature):
    action, slot = new_action("AimTurn", armature)
    locations = [(1, (0.0, 0.0, 0.0)), (18, (0.0, 0.0, 0.0)),
                 (36, (0.0, 0.0, 0.0)), (54, (0.0, 0.0, 0.0))]
    rotations = [(1, (0.0, 0.0, math.radians(-12.0))),
                 (18, (0.0, 0.0, math.radians(18.0))),
                 (36, (0.0, 0.0, math.radians(-5.0))),
                 (54, (0.0, 0.0, math.radians(24.0)))]
    object_channels(action, locations, rotations)

    # The source weapon is part of the skinned mesh. Rotating the armed root
    # keeps the gun and both hands coherent while the soldier scans left/right.
    set_action_range(action, 1, 54)
    return action


def make_slow_walk_action(armature):
    action, slot = new_action("SlowWalk", armature)
    end = 61
    location_keys = []
    rotation_keys = []
    for frame in range(1, end + 1, 5):
        phase = 2.0 * math.pi * (frame - 1) / 60.0
        location_keys.append((frame, (0.0, (frame - 1) / 60.0 * 1.20, 0.018 * math.sin(2.0 * phase))))
        rotation_keys.append((frame, (0.018 * math.sin(2.0 * phase), 0.0, 0.0)))
    object_channels(action, location_keys, rotation_keys)

    # Generic SmartRig bone mapping discovered from the source hierarchy:
    # Bone_009/008/007/006 and Bone_014/013/012/011 are the two leg chains.
    # Local X is used for a conservative walk swing that preserves the crouch.
    for frame in range(1, end + 1, 5):
        phase = 2.0 * math.pi * (frame - 1) / 60.0
        left = math.sin(phase)
        right = math.sin(phase + math.pi)
        for bone_name, value in (("Bone_009", 0.18 * left), ("Bone_008", -0.12 * left),
                                 ("Bone_007", 0.07 * left), ("Bone_014", 0.18 * right),
                                 ("Bone_013", -0.12 * right), ("Bone_012", 0.07 * right)):
            bone_channel(action, bone_name, 0, [(frame, value)])

    set_action_range(action, 1, 60)
    return action


def select_character(armature):
    bpy.ops.object.select_all(action="DESELECT")
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    for obj in bpy.data.objects:
        if obj.type == "MESH" and obj.find_armature() == armature:
            obj.select_set(True)


def assign_action(armature, action):
    armature.animation_data_create()
    armature.animation_data.action = action
    if action.slots:
        armature.animation_data.action_slot = action.slots[0]


def export_fbx(armature):
    select_character(armature)
    assign_action(armature, bpy.data.actions.get("Shoot"))

    scene = bpy.context.scene
    scene.render.fps = FPS
    scene.frame_start = 1
    scene.frame_end = 60

    bpy.ops.export_scene.fbx(
        filepath=OUTPUT_FBX,
        use_selection=True,
        object_types={"ARMATURE", "MESH"},
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        primary_bone_axis="Y",
        secondary_bone_axis="X",
        axis_forward="-Z",
        axis_up="Y",
        apply_unit_scale=True,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_all_actions=True,
        bake_anim_use_nla_strips=False,
        bake_anim_force_startend_keying=True,
        bake_anim_step=1.0,
        path_mode="COPY",
        embed_textures=True,
    )


def main():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    armature = reset_and_import()
    shoot = make_shoot_action(armature)
    aim_turn = make_aim_turn_action(armature)
    slow_walk = make_slow_walk_action(armature)

    # Keep the three actions discoverable in the .blend source as well.
    assign_action(armature, shoot)
    bpy.context.scene.frame_start = 1
    bpy.context.scene.frame_end = 60
    bpy.context.scene.render.fps = FPS
    bpy.ops.wm.save_as_mainfile(filepath=OUTPUT_BLEND)
    export_fbx(armature)

    print("ANIMATION_EXPORT_OK")
    print("FBX", OUTPUT_FBX)
    print("BLEND", OUTPUT_BLEND)
    print("ACTIONS", [shoot.name, aim_turn.name, slow_walk.name])
    print("BONES", len(armature.data.bones))


if __name__ == "__main__":
    main()
