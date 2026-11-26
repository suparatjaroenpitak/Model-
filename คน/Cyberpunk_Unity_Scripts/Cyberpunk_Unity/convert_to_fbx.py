# Run in Blender background: blender -b -P convert_to_fbx.py -- input.glb output.fbx
import bpy, sys
from pathlib import Path
args=sys.argv[sys.argv.index('--')+1:]
if len(args)!=2: raise SystemExit('Provide input.glb and output.fbx')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(Path(args[0]).resolve()))
for ob in bpy.data.objects:
    if ob.type=='ARMATURE':
        ob.show_in_front=True
bpy.ops.export_scene.fbx(filepath=str(Path(args[1]).resolve()),object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0.0,path_mode='COPY',embed_textures=True,axis_forward='-Z',axis_up='Y')
