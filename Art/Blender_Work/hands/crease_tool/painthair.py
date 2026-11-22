import numpy as np, sys
from PIL import Image
from scipy import ndimage
tag=sys.argv[1]; NS=int(sys.argv[2]) if len(sys.argv)>2 else 7000; R=2048
pre="hands_skin" if tag=="idle" else "hands_grip_skin"
d=np.load('crease_%s.npz'%tag); P,N,T,TL,UV=d['P'],d['N'],d['T'],d['TL'],d['UV']
rng=np.random.default_rng(21 if tag=="idle" else 22)
A=np.linalg.norm(np.cross(P[T[:,1]]-P[T[:,0]],P[T[:,2]]-P[T[:,0]]),axis=1)/2
C=P[T].mean(1); FN=N[T].mean(1); FN/=np.linalg.norm(FN,axis=1,keepdims=True)
y=C[:,1]
w=np.clip((0.12-y)/0.01,0,1)*np.clip((y+0.058)/0.012,0,1)
back=np.clip((-FN[:,0]+0.2)/0.6,0,1)
back2=np.clip((-FN[:,0]+0.05)/0.45,0,1)
w*=np.where(y>0.01,back2,back**1.5)*(FN[:,0]<0.1)
w*=np.where((C[:,2]>0.045)&(y<0.0),0.0,1.0)
pr=A*w; pr/=pr.sum()
pick=rng.choice(len(T),size=NS,p=pr)
acc=np.zeros((R,R))
pts=[]; wts=[]
for ti in pick:
    t=T[ti]; tl=TL[ti]
    a,b=rng.random(2)
    if a+b>1: a,b=1-a,1-b
    p0,p1,p2=P[t]; u0,u1,u2=UV[tl]*R
    root3=p0+a*(p1-p0)+b*(p2-p0); rootuv=u0+a*(u1-u0)+b*(u2-u0)
    nrm=FN[ti]
    flow=np.array([0,1.0,0]) if root3[1]>0 else np.array([0,0.8,-0.5])
    flow=flow-nrm*flow.dot(nrm); flow/=np.linalg.norm(flow)
    ang=rng.normal(0,0.35); bi=np.cross(nrm,flow); dvec=flow*np.cos(ang)+bi*np.sin(ang)
    E=np.stack([p1-p0,p2-p0],1); c,*_=np.linalg.lstsq(E,dvec,rcond=None)
    duv=np.stack([u1-u0,u2-u0],1)@c          # uv pixels per metre along dvec
    bend=np.stack([u1-u0,u2-u0],1)@np.linalg.lstsq(E,bi,rcond=None)[0]
    L=rng.uniform(0.004,0.008) if root3[1]>0 else rng.uniform(0.003,0.006)
    curv=rng.normal(0,0.25)
    n=int(max(6,np.linalg.norm(duv*L)*2))
    s=np.linspace(0,1,n)
    xy=rootuv[None]+np.outer(s*L,duv)+np.outer(curv*(s*L)**2/L,bend)
    strength=rng.uniform(0.6,1.0)*(1-0.55*s)
    pts.append(xy); wts.append(strength)
xy=np.concatenate(pts); ww=np.concatenate(wts)
col=xy[:,0]; row=R-xy[:,1]
ok=(col>=0)&(col<R-1)&(row>=0)&(row<R-1)
col,row,ww=col[ok],row[ok],ww[ok]
c0=np.floor(col).astype(int); r0=np.floor(row).astype(int); fc=col-c0; fr=row-r0
for dr,dc,wf in ((0,0,(1-fr)*(1-fc)),(0,1,(1-fr)*fc),(1,0,fr*(1-fc)),(1,1,fr*fc)):
    np.maximum.at(acc,(r0+dr,c0+dc),ww*wf*1.6)
acc=np.clip(ndimage.gaussian_filter(acc,0.45)*1.3,0,1)
np.save('hairmask_%s.npy'%tag,acc)
U="/mnt/user-data/uploads/Homestead Game/Art/Blender_Work/hands/textures/"
b=np.asarray(Image.open(U+pre.replace('hands_','hands_').replace('skin','skin11',1)+'_basecolor.png' if False else 'out_%s_basecolor.png'%pre).convert('RGBA')).astype(float)
hc=np.array([70,48,32.0])
k=(0.38*acc)[...,None]
b[...,:3]=b[...,:3]*(1-k)+hc*k
Image.fromarray(b.round().clip(0,255).astype(np.uint8),'RGBA').save('hair_%s_basecolor.png'%pre)
src=np.asarray(Image.open('out_%s_normal.png'%pre).convert('RGBA')).astype(float); n1=src[...,:3]/255*2-1
h=ndimage.gaussian_filter(acc,0.7)*0.35
gy,gx=np.gradient(h)
n=np.stack([n1[...,0]-gx,n1[...,1]+gy,n1[...,2]],-1); n/=np.linalg.norm(n,axis=-1,keepdims=True)
o=np.concatenate([(n*0.5+0.5)*255,src[...,3:]],-1)
Image.fromarray(o.round().clip(0,255).astype(np.uint8),'RGBA').save('hair_%s_normal.png'%pre)
print(tag,'strands',NS,'texels',(acc>0.2).sum())
