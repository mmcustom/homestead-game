
import bpy, bmesh, math, os
from mathutils import Vector, noise, Matrix
def U(x,y,z): return Vector((-x,-z,y))
def build_vessel(idn, rim, belly, base, ytop, ybot, spout=False, lid=False, legs=True):
    bpy.ops.wm.read_homefile(use_empty=True)
    W=r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work"
    d=os.path.join(W,idn)
    for s in ["","textures","review"]: os.makedirs(os.path.join(d,s),exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(d,idn+".blend"))
    H=ytop-ybot
    def ylerp(t): return ybot+H*t
    outer=[(0.0,ybot),(base*0.6,ybot+0.001),(base,ybot+0.006),(base+(belly-base)*0.6,ylerp(0.2)),(belly,ylerp(0.48)),(belly-0.25*(belly-rim),ylerp(0.75)),(rim,ylerp(0.93)),(rim+0.004,ytop-0.002),(rim+0.005,ytop)]
    inner=[(rim-0.006,ytop),(rim-0.007,ylerp(0.9)),(belly-0.007-0.25*(belly-rim),ylerp(0.75)),(belly-0.007,ylerp(0.48)),(base+(belly-base)*0.6-0.007,ylerp(0.2)),(base-0.008,ybot+0.012),(0.0,ybot+0.008)]
    SEG=56
    def lathe(bm,prof,flip=False):
        rings=[]
        for (r,y) in prof:
            ring=[]
            for k in range(SEG):
                a=2*math.pi*k/SEG
                rr=r*(1+0.006*noise.noise(Vector((math.cos(a)*3,math.sin(a)*3,y*25))))
                ring.append(bm.verts.new(U(rr*math.cos(a),y,rr*math.sin(a))))
            rings.append(ring)
        for i in range(len(rings)-1):
            for k in range(SEG):
                q=(rings[i][k],rings[i][(k+1)%SEG],rings[i+1][(k+1)%SEG],rings[i+1][k])
                bm.faces.new(q[::-1] if flip else q)
        return rings
    bm=bmesh.new()
    lathe(bm,outer)
    lathe(bm,[(rim+0.005,ytop),(rim-0.0005,ytop+0.0025),(rim-0.006,ytop)])  # rounded lip top
    bmesh.ops.remove_doubles(bm,verts=bm.verts,dist=1e-6)
    def tube(pts,rc,segs=10,closed=False,taper=None):
        n=len(pts); rs=[]
        for i,p in enumerate(pts):
            a=pts[(i+1)%n] if closed else pts[min(i+1,n-1)]; b=pts[(i-1)%n] if closed else pts[max(i-1,0)]
            fw=(a-b).normalized(); up=Vector((0,0,1)) if abs(fw.z)<0.9 else Vector((1,0,0))
            u=fw.cross(up).normalized(); v=fw.cross(u).normalized()
            r=rc*(taper(i/(n-1)) if taper else 1)
            rs.append([bm.verts.new(p+(u*math.cos(2*math.pi*j/segs)+v*math.sin(2*math.pi*j/segs))*r) for j in range(segs)])
        m=n if closed else n-1
        for i in range(m):
            A=rs[i];B=rs[(i+1)%n]
            for j in range(segs): bm.faces.new((A[j],A[(j+1)%segs],B[(j+1)%segs],B[j]))
        if not closed: bm.faces.new(rs[0][::-1]); bm.faces.new(rs[-1])
    # ears: small rings standing on the rim at both sides (Unity X)
    for sx in (1,-1):
        cx=sx*(rim+0.007); cy=ytop-0.008
        pts=[U(cx+sx*0.0*math.cos(a),cy+0.008*math.sin(a),0.008*math.cos(a)) for a in [2*math.pi*i/20 for i in range(20)]]
        tube(pts,0.0032,8,closed=True)
    # bail: a wire from ear to ear through the grip at the origin
    pts=[]
    ex=rim+0.007; ey=ytop-0.008
    for i in range(41):
        th=math.pi*i/40
        x=ex*math.cos(th); y=ey+(0-ey)*math.sin(th)**0.85
        pts.append(U(x,y,0.0))
    tube(pts,0.0026,8)
    if legs:
        for k in range(3):
            a=2*math.pi*k/3+math.pi/2
            r0=base*0.72
            p0=U(r0*math.cos(a),ybot+0.006,r0*math.sin(a)); p1=U(r0*1.08*math.cos(a),ybot-0.018,r0*1.08*math.sin(a))
            tube([p0,(p0+p1)/2,p1],0.008,10,taper=lambda t:1-0.35*t)
    if spout:
        P=[U(0,ylerp(0.45),belly-0.01),U(0,ylerp(0.6),belly+0.022),U(0,ylerp(0.78),belly+0.045),U(0,ylerp(0.9),belly+0.058)]
        tube(P,0.014,14,taper=lambda t:1-0.45*t)
    if lid:
        lp=[(0.0,ytop+0.016),(rim*0.25,ytop+0.015),(rim*0.7,ytop+0.009),(rim+0.002,ytop+0.003),(rim+0.002,ytop+0.0005),(rim-0.003,ytop-0.004),(0.0,ytop+0.004)]
        lathe(bm,lp)
        # knob
        tube([U(0,ytop+0.015,0),U(0,ytop+0.028,0)],0.007,12)
        bmesh.ops.create_uvsphere(bm,u_segments=12,v_segments=8,radius=0.011,matrix=Matrix.Translation(U(0,ytop+0.031,0)))
    bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
    me=bpy.data.meshes.new("V_Iron"); bm.to_mesh(me); bm.free()
    o=bpy.data.objects.new("V_Iron",me); bpy.context.collection.objects.link(o)
    for p in me.polygons: p.use_smooth=True
    bm=bmesh.new(); lathe(bm,inner); bmesh.ops.remove_doubles(bm,verts=bm.verts,dist=1e-6)
    me=bpy.data.meshes.new("V_Inside"); bm.to_mesh(me); bm.free()
    oi=bpy.data.objects.new("V_Inside",me); bpy.context.collection.objects.link(oi)
    for p in me.polygons: p.use_smooth=True
    return o,oi
