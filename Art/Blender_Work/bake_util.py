
import bpy, time
def bake(obj, mat, kind):
    D=bpy.data; s=bpy.context.scene; t=time.time()
    s.render.engine='CYCLES'; s.cycles.device='CPU'
    ob=D.objects[obj]
    for o in s.objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active=ob
    nt=D.materials[mat].node_tree; node=nt.nodes["BAKE_TARGET"]; node.image=D.images[f"{mat}_{kind}"]
    for n in nt.nodes: n.select=False
    node.select=True; nt.nodes.active=node; s.render.bake.margin=16
    b=nt.nodes['Principled BSDF']; out=[n for n in nt.nodes if n.type=='OUTPUT_MATERIAL'][0]
    if kind in ("basecolor","metallic"):
        em=nt.nodes.new('ShaderNodeEmission'); src=b.inputs['Base Color' if kind=="basecolor" else 'Metallic']
        if src.is_linked: nt.links.new(src.links[0].from_socket, em.inputs[0])
        else:
            v=src.default_value; em.inputs[0].default_value=(tuple(v)[:3]+(1,)) if kind=="basecolor" else (v,v,v,1)
        nt.links.new(em.outputs[0], out.inputs[0]); s.cycles.samples=2; bpy.ops.object.bake(type='EMIT')
        nt.links.new(b.outputs[0], out.inputs[0]); nt.nodes.remove(em)
    elif kind=="roughness":
        s.cycles.samples=2; bpy.ops.object.bake(type='ROUGHNESS')
    else:
        s.cycles.samples=4; bpy.ops.object.bake(type='NORMAL',normal_space='TANGENT')
    node.image.save(); return round(time.time()-t,1)
