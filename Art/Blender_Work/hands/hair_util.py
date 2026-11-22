
import bpy, bmesh, numpy as np, math, mathutils, os
H=r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work\hands"
def trim(name, ycut=0.13):
    o=bpy.data.objects[name]; me=o.data; bm=bmesh.new(); bm.from_mesh(me)
    kill=[f for f in bm.faces if min(v.co.y for v in f.verts)>ycut]
    bmesh.ops.delete(bm,geom=kill,context='FACES')
    loose=[v for v in bm.verts if not v.link_faces]; bmesh.ops.delete(bm,geom=loose,context='VERTS')
    edges=[e for e in bm.edges if e.is_boundary and all(v.co.y>ycut-0.02 for v in e.verts)]
    r=bmesh.ops.holes_fill(bm,edges=edges,sides=0)
    bmesh.ops.triangulate(bm,faces=r["faces"])
    bm.to_mesh(me); bm.free(); me.update()
def hair(skin_name, hand_parent, out_name, n=260, seed=5):
    rng=np.random.default_rng(seed)
    sk=bpy.data.objects[skin_name]; me=sk.data; me.calc_loop_triangles()
    P=np.array([v.co[:] for v in me.vertices]); N=np.array([v.normal[:] for v in me.vertices])
    T=np.array([t.vertices[:] for t in me.loop_triangles])
    A=np.linalg.norm(np.cross(P[T[:,1]]-P[T[:,0]],P[T[:,2]]-P[T[:,0]]),axis=1)/2
    C=P[T].mean(1); FN=N[T].mean(1); FN/=np.linalg.norm(FN,axis=1,keepdims=True)
    # density: forearm and back of hand; less on the palm side; none on fingers / under the cuff
    w=np.ones(len(T))
    y=C[:,1]
    w*=np.clip((0.096-y)/0.008,0,1)            # stop under the cuff
    w*=np.clip((y+0.058)/0.012,0,1)            # stop before the knuckles
    back=np.clip((-FN[:,0]+0.2)/0.6,0,1)       # back of hand / outer forearm faces -X
    wrist_band=(y>-0.005)                      # forearm: hair all round, thinner on the inner side
    w*=np.where(wrist_band,0.25+0.75*back,back**1.5)
    # no hair on the thumb
    w*=np.clip((0.05-C[:,2])/0.01,0,1)*0+np.where((C[:,2]>0.045)&(y<0.0),0.0,1.0)
    pr=A*w; pr/=pr.sum()
    pick=rng.choice(len(T),size=n,p=pr)
    bm=bmesh.new(); uvl=bm.loops.layers.uv.new("UVMap")
    for ti in pick:
        a,b=rng.random(2)
        if a+b>1: a,b=1-a,1-b
        t=T[ti]; p=P[t[0]]+a*(P[t[1]]-P[t[0]])+b*(P[t[2]]-P[t[0]])
        nrm=N[t[0]]*(1-a-b)+N[t[1]]*a+N[t[2]]*b; nrm/=np.linalg.norm(nrm)
        yv=p[1]
        flow=np.array([0.0,1.0,0.0]) if yv>0.0 else np.array([0.0,0.8,-0.5])   # forearm: toward the elbow; back of hand: toward the wrist and little finger
        flow=flow-nrm*flow.dot(nrm); flow/=np.linalg.norm(flow)
        ang=rng.normal(0,0.35); bi=np.cross(nrm,flow)
        d=flow*math.cos(ang)+bi*math.sin(ang)
        bi=np.cross(nrm,d)
        L=rng.uniform(0.006,0.010) if yv>0 else rng.uniform(0.004,0.007); W=L*0.22
        lift=math.radians(rng.uniform(5,12))
        root=p+nrm*0.0002
        tip=root+L*(d*math.cos(lift)+nrm*math.sin(lift))
        vs=[bm.verts.new(root-bi*W/2),bm.verts.new(root+bi*W/2),bm.verts.new(tip+bi*W/2),bm.verts.new(tip-bi*W/2)]
        f=bm.faces.new(vs); k=int(rng.integers(0,4))
        for l,(u,v) in zip(f.loops,[(k/4,0),((k+1)/4,0),((k+1)/4,1),(k/4,1)]): l[uvl].uv=(u,v)
        f.normal_update()
        if mathutils.Vector(f.normal).dot(mathutils.Vector(nrm))<0: f.normal_flip()
    bmesh.ops.triangulate(bm,faces=bm.faces)
    mh=bpy.data.meshes.new(out_name+"_mesh"); bm.to_mesh(mh); bm.free()
    old=bpy.data.objects.get(out_name)
    if old: bpy.data.objects.remove(old)
    o=bpy.data.objects.new(out_name,mh); bpy.context.scene.collection.objects.link(o)
    o.parent=sk.parent; o.matrix_parent_inverse=sk.matrix_parent_inverse.copy(); o.matrix_world=sk.matrix_world.copy()
    m=bpy.data.materials.get("hands_hair")
    if m is None:
        m=bpy.data.materials.new("hands_hair"); m.use_nodes=True; nt=m.node_tree; b=nt.nodes["Principled BSDF"]
        im=bpy.data.images.load(os.path.join(H,"textures","hands_hair2_basecolor.png"),check_existing=True)
        tx=nt.nodes.new("ShaderNodeTexImage"); tx.image=im
        nt.links.new(tx.outputs["Color"],b.inputs["Base Color"]); nt.links.new(tx.outputs["Alpha"],b.inputs["Alpha"])
        b.inputs["Roughness"].default_value=0.45
        try: m.surface_render_method='DITHERED'
        except Exception: pass
    mh.materials.append(m)
    return len(mh.polygons)
def mirror_to_L(src_name, dst_name, parent_name):
    s=bpy.data.objects[src_name]; me=s.data.copy(); me.name=dst_name+"_mesh"
    for v in me.vertices: v.co.x=-v.co.x
    me.flip_normals()
    old=bpy.data.objects.get(dst_name)
    if old: bpy.data.objects.remove(old)
    o=bpy.data.objects.new(dst_name,me); bpy.context.scene.collection.objects.link(o)
    L=bpy.data.objects["Skin_L"]; o.parent=L.parent; o.matrix_parent_inverse=L.matrix_parent_inverse.copy(); o.matrix_world=L.matrix_world.copy()
def tris():
    t={}
    for o in bpy.data.objects:
        if o.type=='MESH' and o.parent and o.parent.name in("Hand_R","Hand_L"): t[o.name]=sum(len(p.vertices)-2 for p in o.data.polygons)
    t["TOTAL"]=sum(t.values()); return t
