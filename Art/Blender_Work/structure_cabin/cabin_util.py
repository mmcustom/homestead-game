
import bpy, bmesh, math, random
from mathutils import Vector, noise, Matrix
D=bpy.data
def U(x,y,z): return Vector((-x,-z,y))
W,DP,LR=4.0,5.0,0.14; LD=LR*2; FH=0.3; COURSES=8; ROOFY=FH+COURSES*LD; RIDGE=3.4
DOORW=1.1; DOORC=7; DOORTOP=FH+DOORC*LD
RISE=RIDGE-ROOFY; SLOPE=math.atan2(RISE,W/2)
def new_bm():
    bm=bmesh.new(); bm.loops.layers.uv.new("UVMap"); return bm
def link(name,bm,smooth=True):
    bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
    me=D.meshes.new(name); bm.to_mesh(me); bm.free()
    o=D.objects.new(name,me); bpy.context.collection.objects.link(o)
    for p in me.polygons: p.use_smooth=smooth
    return o
def log(bm,a,b,r,seg=14,rings=10,seed=0):
    uv=bm.loops.layers.uv["UVMap"]
    A=U(*a); B=U(*b); ax=(B-A); L=ax.length; ax.normalize()
    up=Vector((0,0,1)) if abs(ax.z)<0.9 else Vector((1,0,0))
    s1=ax.cross(up).normalized(); s2=ax.cross(s1).normalized()
    bx=noise.noise(Vector((seed*1.7,1,0)))*0.025; by=noise.noise(Vector((seed*1.7,2,0)))*0.025
    taper=0.06*noise.noise(Vector((seed*1.3,3,0)))
    rows=[]
    for i in range(rings+1):
        t=i/rings
        rr=r*(1+taper*(0.5-t))
        c=A+ax*(L*t)+(s1*bx+s2*by)*math.sin(math.pi*t)
        row=[]
        for k in range(seg):
            ang=2*math.pi*k/seg
            w=1+0.05*noise.noise(Vector((math.cos(ang)*2+seed,math.sin(ang)*2,t*L*1.5)))
            row.append(bm.verts.new(c+(s1*math.cos(ang)+s2*math.sin(ang))*rr*w))
        rows.append(row)
    off=((seed*0.6180339)%1.0)*max(0.0,0.98-L*0.12)+0.01
    for i in range(rings):
        for k in range(seg):
            f=bm.faces.new((rows[i][k],rows[i][(k+1)%seg],rows[i+1][(k+1)%seg],rows[i+1][k]))
            for l,(uu,vv) in zip(f.loops,[(i,k),(i,k+1),(i+1,k+1),(i+1,k)]):
                l[uv].uv=(uu/rings*L*0.12+off, vv/seg*0.7*0.5+0.0)
    for row,flip in ((rows[0],True),(rows[-1],False)):
        cc=sum((v.co for v in row),Vector())/len(row); c=bm.verts.new(cc)
        for k in range(seg):
            vs=(row[k],row[(k+1)%seg],c) if not flip else (row[(k+1)%seg],row[k],c)
            f=bm.faces.new(vs)
            for l in f.loops:
                p=l.vert.co-cc; l[uv].uv=(0.75+0.11*p.dot(s1)/r, 0.75+0.11*p.dot(s2)/r)

def sheet(bm,P,nu,nv,thick=0.008,uvscale=(1,1),uvoff=(0,0)):
    """P(u,v)->(Vector pos, Vector normal) in Blender space. builds a double-sided thin sheet with UV u,v in 0..1 scaled"""
    uv=bm.loops.layers.uv["UVMap"]
    top=[];bot=[]
    for i in range(nv+1):
        rt=[];rb=[]
        for j in range(nu+1):
            p,n=P(j/nu,i/nv)
            rt.append(bm.verts.new(p+n*thick/2)); rb.append(bm.verts.new(p-n*thick/2))
        top.append(rt); bot.append(rb)
    def q(vs,uvs):
        f=bm.faces.new(vs)
        for l,u_ in zip(f.loops,uvs): l[uv].uv=(uvoff[0]+u_[0]*uvscale[0],uvoff[1]+u_[1]*uvscale[1])
    for i in range(nv):
        for j in range(nu):
            uvs=[(j/nu,i/nv),((j+1)/nu,i/nv),((j+1)/nu,(i+1)/nv),(j/nu,(i+1)/nv)]
            q([top[i][j],top[i][j+1],top[i+1][j+1],top[i+1][j]],uvs)
            q([bot[i+1][j],bot[i+1][j+1],bot[i][j+1],bot[i][j]],uvs[::-1][1:]+uvs[::-1][:1] if False else [uvs[3],uvs[2],uvs[1],uvs[0]])
    for i in range(nv):
        for (g1,g2,j) in ((top,bot,0),(top,bot,nu)):
            q([g1[i][j],g1[i+1][j],g2[i+1][j],g2[i][j]],[(j/nu,i/nv)]*4)
    for j in range(nu):
        for i in (0,nv):
            q([top[i][j],top[i][j+1],bot[i][j+1],bot[i][j]],[(j/nu,i/nv)]*4)
