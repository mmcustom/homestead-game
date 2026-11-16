
import bpy, bmesh, math, random
from mathutils import Vector, noise, Matrix, Euler
exec(open(r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work\structure_cabin\cabin_util.py",encoding="utf-8").read())
def board(bm,c,size,rot=(0,0,0),idx=None,uoff=None,warp=0.0):
    """box in Unity space: centre c, size (x,y,z), rotation Euler degrees (Unity order approx XYZ); plank UVs: u along the longest axis (3 m = 1), v in board strip idx/16"""
    uv=bm.loops.layers.uv["UVMap"]
    if idx is None: idx=random.randrange(16)
    if uoff is None: uoff=random.uniform(0,max(0.0,1-max(size)/3.0))
    la=max(range(3),key=lambda i:size[i])
    R=Euler([math.radians(a) for a in rot],'YXZ').to_matrix()
    C=Vector(c); h=[s/2 for s in size]
    corners={}
    for sx in (-1,1):
        for sy in (-1,1):
            for sz in (-1,1):
                p=Vector((sx*h[0],sy*h[1],sz*h[2]))
                if warp: p[ (la+1)%3 ]+=warp*noise.noise(Vector((p[la]*2,idx,0)))
                corners[(sx,sy,sz)]=bm.verts.new(U(*(C+R@p)))
    def coord(key,ax):  # 0..1 along axis
        return (key[ax]+1)/2
    faces=[]
    for ax in range(3):
        for s in (-1,1):
            keys=[k for k in corners if k[ax]==s]
            o=[i for i in range(3) if i!=ax]
            # order around the face
            ring=sorted(keys,key=lambda k:math.atan2(k[o[1]],k[o[0]]))
            if s<0: ring=ring[::-1]
            f=bm.faces.new([corners[k] for k in ring])
            for l,k in zip(f.loops,ring):
                if ax==la:
                    l[uv].uv=(uoff+0.01+0.02*coord(k,o[0]), (idx+0.3+0.4*coord(k,o[1]))/16)
                else:
                    other=[i for i in o if i!=la][0]
                    l[uv].uv=(uoff+coord(k,la)*size[la]/3.0, (idx+coord(k,other))/16)
    return
def plank_tris(o): return sum(len(p.vertices)-2 for p in o.data.polygons)
def roof_slab(bm,x0,x1,z0,z1,y_front,y_back,thick,nx=20,nz=10,lumpy=0.06):
    """thatch slab over x0..x1 (unity), z0 (back) .. z1 (front), underside height linear between y_back and y_front; cabin thatch UVs (u along x, v from front (0) to back (0.5))"""
    uv=bm.loops.layers.uv["UVMap"]
    def P(i,j,top):
        x=x0+(x1-x0)*i/nx; z=z1+(z0-z1)*j/nz; t=j/nz
        y=y_front+(y_back-y_front)*t
        if top:
            y+=thick*(1+lumpy/thick*noise.noise(Vector((x*1.5,z*1.5,3))))
            if j==nz or j==0: y-=thick*0.35
        return bm.verts.new(U(x,y,z))
    G={tp:[[P(i,j,tp) for i in range(nx+1)] for j in range(nz+1)] for tp in (True,False)}
    def q(vs,uvs):
        f=bm.faces.new(vs)
        for l,u_ in zip(f.loops,uvs): l[uv].uv=u_
    for tp in (True,False):
        g=G[tp]
        for j in range(nz):
            for i in range(nx):
                vs=[g[j][i],g[j][i+1],g[j+1][i+1],g[j+1][i]]
                uvs=[(i/nx,j/nz*0.5),((i+1)/nx,j/nz*0.5),((i+1)/nx,(j+1)/nz*0.5),(i/nx,(j+1)/nz*0.5)]
                if tp: vs=vs[::-1]; uvs=uvs[::-1]
                q(vs,uvs)
    T,B=G[True],G[False]
    for i in range(nx):
        for j in (0,nz):
            q([T[j][i],T[j][i+1],B[j][i+1],B[j][i]],[(i/nx,0.56),((i+1)/nx,0.56),((i+1)/nx,0.6),(i/nx,0.6)])
    for j in range(nz):
        for i in (0,nx):
            q([T[j][i],T[j+1][i],B[j+1][i],B[j][i]],[(0.12,0.62+j/nz*0.3),(0.12,0.62+(j+1)/nz*0.3),(0.18,0.62+(j+1)/nz*0.3),(0.18,0.62+j/nz*0.3)])
def stoneblob(bm,c,size,seed,flat=0.75):
    r=bmesh.ops.create_icosphere(bm,subdivisions=2,radius=1.0)
    for v in r["verts"]:
        p=v.co.copy(); k=1+0.2*noise.noise(p*2.5+Vector((seed,seed*1.3,seed*2.1)))
        q=Vector((p.x*size[0]*k,p.y*size[1]*k,p.z*size[2]*k))
        q.y=max(min(q.y,size[1]*flat),-size[1]*flat)
        v.co=U(c[0]+q.x,c[1]+q.y,c[2]+q.z)
def objlink(name,bm,smooth=True):
    bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
    me=D.meshes.new(name); bm.to_mesh(me); bm.free()
    o=D.objects.new(name,me); bpy.context.collection.objects.link(o)
    for p in me.polygons: p.use_smooth=smooth
    return o
def empty(name,pos=(0,0,0),parent=None):
    e=D.objects.new(name,None); e.location=U(*pos); bpy.context.collection.objects.link(e); e.parent=parent; return e
def setmat(o,name,col=(0.5,0.5,0.5,1)):
    m=D.materials.get(name)
    if m is None:
        m=D.materials.new(name); m.use_nodes=True; m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value=col; m.diffuse_color=col
    o.data.materials.clear(); o.data.materials.append(m)
def newfile(idn):
    import os
    bpy.ops.wm.read_homefile(use_empty=True)
    d=r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"+"\\"+idn
    for s in ["","textures","review"]: os.makedirs(os.path.join(d,s),exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(d,idn+".blend"))
def shot(tgt,locs,prefix="shape"):
    import os
    sc=bpy.context.scene
    sc.render.engine='BLENDER_WORKBENCH'; sc.display.shading.light='STUDIO'; sc.display.shading.color_type='MATERIAL'
    sc.render.resolution_x=900; sc.render.resolution_y=650
    co=D.objects.get("ReviewCam")
    if co is None:
        cam=D.cameras.new("cam"); cam.lens=35; co=D.objects.new("ReviewCam",cam); sc.collection.objects.link(co)
    sc.camera=co; out=[]
    for k,loc in enumerate(locs):
        co.location=loc; co.rotation_euler=(Vector(tgt)-co.location).to_track_quat('-Z','Y').to_euler()
        p=os.path.join(os.path.dirname(D.filepath),"review",f"{prefix}_{k}.png"); sc.render.filepath=p; bpy.ops.render.render(write_still=True); out.append(p)
    return out

def stone_mat(name,base0,base1,moss=True,scale=7):
    m=D.materials[name] if name in D.materials else D.materials.new(name)
    m.use_nodes=True; t=m.node_tree
    for n in list(t.nodes): t.nodes.remove(n)
    L=t.links.new
    out=t.nodes.new("ShaderNodeOutputMaterial"); b=t.nodes.new("ShaderNodeBsdfPrincipled"); b.name="Principled BSDF"; L(b.outputs[0],out.inputs[0])
    tc=t.nodes.new("ShaderNodeTexCoord")
    n1=t.nodes.new("ShaderNodeTexNoise"); n1.inputs["Scale"].default_value=scale; n1.inputs["Detail"].default_value=10; L(tc.outputs["Object"],n1.inputs["Vector"])
    n2=t.nodes.new("ShaderNodeTexNoise"); n2.inputs["Scale"].default_value=60; n2.inputs["Detail"].default_value=12; L(tc.outputs["Object"],n2.inputs["Vector"])
    vo=t.nodes.new("ShaderNodeTexVoronoi"); vo.inputs["Scale"].default_value=3; L(tc.outputs["Object"],vo.inputs["Vector"])
    bw=t.nodes.new("ShaderNodeRGBToBW"); L(vo.outputs["Color"],bw.inputs[0])
    cr=t.nodes.new("ShaderNodeValToRGB"); e=cr.color_ramp.elements; e[0].color=base0; e[1].color=base1; L(bw.outputs[0],cr.inputs[0])
    ov=t.nodes.new("ShaderNodeMix"); ov.data_type='RGBA'; ov.blend_type='OVERLAY'; ov.inputs[0].default_value=0.45; L(cr.outputs[0],ov.inputs[6]); L(n1.outputs["Color"],ov.inputs[7])
    last=ov.outputs[2]
    if moss:
        mz=t.nodes.new("ShaderNodeMapRange"); mz.inputs[1].default_value=0.58; mz.inputs[2].default_value=0.68; L(n1.outputs["Fac"],mz.inputs[0])
        mm=t.nodes.new("ShaderNodeMix"); mm.data_type='RGBA'; mm.inputs[7].default_value=(0.12,0.16,0.06,1); L(mz.outputs[0],mm.inputs[0]); L(last,mm.inputs[6]); last=mm.outputs[2]
    L(last,b.inputs["Base Color"])
    rr=t.nodes.new("ShaderNodeMapRange"); rr.inputs[3].default_value=0.7; rr.inputs[4].default_value=0.95; L(n2.outputs["Fac"],rr.inputs[0]); L(rr.outputs[0],b.inputs["Roughness"])
    bp=t.nodes.new("ShaderNodeBump"); bp.inputs["Strength"].default_value=0.7; bp.inputs["Distance"].default_value=0.006; L(n2.outputs["Fac"],bp.inputs["Height"]); L(bp.outputs[0],b.inputs["Normal"])
    m.diffuse_color=base1
    return m
