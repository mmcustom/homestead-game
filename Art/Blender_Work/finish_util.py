
import bpy, os, numpy as np
from mathutils import Vector
D=bpy.data
def smart_uv(names):
    for n in names:
        bpy.ops.object.select_all(action='DESELECT'); o=D.objects[n]; o.select_set(True); bpy.context.view_layer.objects.active=o
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=1.15,island_margin=0.005); bpy.ops.object.mode_set(mode='OBJECT')
def iron_mat(name,c0,c1,rust,rough=(0.55,0.8),scale=160):
    m=D.materials.new(name); m.use_nodes=True; t=m.node_tree
    for n in list(t.nodes): t.nodes.remove(n)
    L=t.links.new
    out=t.nodes.new("ShaderNodeOutputMaterial"); b=t.nodes.new("ShaderNodeBsdfPrincipled"); L(b.outputs[0],out.inputs[0])
    tc=t.nodes.new("ShaderNodeTexCoord")
    n1=t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value=scale; n1.inputs["Detail"].default_value=10; L(tc.outputs["Object"],n1.inputs["Vector"])
    n2=t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value=22; n2.inputs["Detail"].default_value=6; L(tc.outputs["Object"],n2.inputs["Vector"])
    cr=t.nodes.new("ShaderNodeValToRGB"); cr.color_ramp.elements[0].color=c0; cr.color_ramp.elements[1].color=c1; L(n1.outputs["Fac"],cr.inputs[0])
    rs=t.nodes.new("ShaderNodeValToRGB"); rs.color_ramp.elements[0].position=0.6; rs.color_ramp.elements[1].position=0.72; L(n2.outputs["Fac"],rs.inputs[0])
    mx=t.nodes.new("ShaderNodeMix"); mx.data_type='RGBA'; mx.inputs[7].default_value=rust; L(rs.outputs[0],mx.inputs[0]); L(cr.outputs[0],mx.inputs[6])
    L(mx.outputs[2],b.inputs["Base Color"]); b.inputs["Metallic"].default_value=1.0
    rr=t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value=rough[0]; rr.inputs[4].default_value=rough[1]; L(n1.outputs["Fac"],rr.inputs[0])
    ru=t.nodes.new("ShaderNodeMath"); ru.operation='MAXIMUM'; L(rr.outputs[0],ru.inputs[0])
    rm=t.nodes.new("ShaderNodeMath"); rm.operation='MULTIPLY'; rm.inputs[1].default_value=0.9; L(rs.outputs[0],rm.inputs[0]); L(rm.outputs[0],ru.inputs[1]); L(ru.outputs[0],b.inputs["Roughness"])
    bp=t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value=0.35; bp.inputs["Distance"].default_value=0.0003; L(n1.outputs["Fac"],bp.inputs["Height"]); L(bp.outputs[0],b.inputs["Normal"])
    return m
def review(tgt,campos,name="proc_3q.png",key=4,res=(800,800)):
    sc=bpy.context.scene
    sc.render.engine='CYCLES'; sc.cycles.device='CPU'; sc.cycles.samples=48; sc.view_settings.view_transform='AgX'
    sc.render.resolution_x,sc.render.resolution_y=res
    if not sc.world: sc.world=D.worlds.new("W")
    sc.world.use_nodes=True; bg=sc.world.node_tree.nodes["Background"]; bg.inputs[0].default_value=(0.6,0.65,0.7,1); bg.inputs[1].default_value=0.3
    tgt=Vector(tgt)
    for nm,off,e,s in [("Key",(0.3,-0.3,0.35),key,0.18),("Fill",(-0.4,-0.1,0.05),key*0.3,0.25)]:
        if nm in D.objects: continue
        l=D.lights.new(nm,'AREA'); l.energy=e; l.size=s; ob=D.objects.new(nm,l); sc.collection.objects.link(ob); ob.location=tgt+Vector(off)
        ob.rotation_euler=(tgt-ob.location).to_track_quat('-Z','Y').to_euler()
    co=D.objects.get("ReviewCam")
    if co is None:
        cam=D.cameras.new("cam"); cam.lens=60; co=D.objects.new("ReviewCam",cam); sc.collection.objects.link(co)
    sc.camera=co; co.data.type='PERSP'; co.location=Vector(campos); co.rotation_euler=(tgt-co.location).to_track_quat('-Z','Y').to_euler()
    sc.render.filepath=os.path.join(os.path.dirname(D.filepath),"review",name); bpy.ops.render.render(write_still=True)
def prep_bake(prefix,sizes):
    src=open(r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work\bake_util.py",encoding="utf-8").read()
    tx=D.texts.get("bake_util") or D.texts.new("bake_util"); tx.from_string(src); tx.use_fake_user=True
    tex=os.path.join(os.path.dirname(D.filepath),"textures")
    for k,size in sizes:
        mat=prefix+k; nt=D.materials[mat].node_tree
        node=nt.nodes.get("BAKE_TARGET") or nt.nodes.new("ShaderNodeTexImage"); node.name="BAKE_TARGET"
        for kind in ["basecolor","roughness","normal"]:
            n=f"{mat}_{kind}"
            if n in D.images: D.images.remove(D.images[n])
            im=D.images.new(n,size,size); im.colorspace_settings.name='sRGB' if kind=="basecolor" else 'Non-Color'
            im.filepath_raw=os.path.join(tex,n+".png"); im.file_format='PNG'
def masks_and_export(idn,prefix,keys,metal):
    tex=os.path.join(os.path.dirname(D.filepath),"textures")
    for k in keys:
        name=prefix+k; ro=D.images[name+"_roughness"]; sz=ro.size[0]
        rough=np.array(ro.pixels[:],dtype=np.float32).reshape(-1,4)
        out=np.zeros_like(rough); out[:,0]=1.0 if k in metal else 0.0; out[:,1]=1; out[:,3]=1-rough[:,0]
        n=name+"_mask"
        if n in D.images: D.images.remove(D.images[n])
        im=D.images.new(n,sz,sz,alpha=True); im.colorspace_settings.name='Non-Color'; im.alpha_mode='STRAIGHT'
        im.pixels.foreach_set(out.ravel()); im.filepath_raw=os.path.join(tex,n+".png"); im.file_format='PNG'; im.save()
    bpy.ops.object.select_all(action='DESELECT')
    for ob in D.objects:
        if ob.type in ('MESH','EMPTY'): ob.select_set(True)
    fbx=os.path.join(os.path.dirname(D.filepath),idn+"_model.fbx")
    bpy.ops.export_scene.fbx(filepath=fbx,use_selection=True,object_types={'EMPTY','MESH'},apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',bake_space_transform=True,mesh_smooth_type='FACE',add_leaf_bones=False,path_mode='STRIP',embed_textures=False)
    bpy.ops.wm.save_mainfile()
    return os.path.getsize(fbx)
