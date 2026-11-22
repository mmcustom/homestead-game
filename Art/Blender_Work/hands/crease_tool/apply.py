import numpy as np, sys
from PIL import Image
tag=sys.argv[1]; K=float(sys.argv[2]) if len(sys.argv)>2 else 1.0
U="/mnt/user-data/uploads/Homestead Game/01_Unity_Project/Homestead/Assets/Resources/Art/"
pre="hands_skin" if tag=="idle" else "hands_grip_skin"
d=np.load('crease_%s.npz'%tag); P,T,TL,UV=d['P']*1000,d['T'],d['TL'],d['UV']
# mm per texel (palm-ish triangles, median)
a3=np.linalg.norm(np.cross(P[T[:,1]]-P[T[:,0]],P[T[:,2]]-P[T[:,0]]),axis=1)/2
uv=UV[TL]*2048; e1=uv[:,1]-uv[:,0]; e2=uv[:,2]-uv[:,0]; a2=np.abs(e1[:,0]*e2[:,1]-e1[:,1]*e2[:,0])/2
mmpt=np.median(np.sqrt(a3/np.maximum(a2,1e-9)))
h=np.load('h_%s.npy'%tag)
gy,gx=np.gradient(h/mmpt)    # mm of depth per mm of surface... h is in 'depth units' ~ 0.15 mm
s=0.35*K
n2=np.stack([-gx*s, gy*s, np.ones_like(h)],-1)
src=np.asarray(Image.open(U+pre+'_normal.png').convert('RGBA')).astype(float); nimg=src[...,:3]/255*2-1
n1=nimg
n=np.stack([n1[...,0]+n2[...,0], n1[...,1]+n2[...,1], n1[...,2]*n2[...,2]],-1)
n/=np.linalg.norm(n,axis=-1,keepdims=True)
o=np.concatenate([(n*0.5+0.5)*255,src[...,3:]],-1); Image.fromarray(o.round().clip(0,255).astype(np.uint8),'RGBA').save('out_%s_normal.png'%pre)
c=np.clip(-h,0,None)    # crease strength 0..~1.5
b=Image.open(U+pre+'_basecolor.png'); mode=b.mode; b=np.asarray(b.convert('RGBA')).astype(float)
dark=1-0.16*K*np.clip(c,0,1.2)
b[...,0]*=dark; b[...,1]*=dark*0.97; b[...,2]*=dark*0.97
Image.fromarray(b.round().clip(0,255).astype(np.uint8),'RGBA').convert(mode).save('out_%s_basecolor.png'%pre)
m=np.asarray(Image.open(U+pre+'_mask.png').convert('RGBA')).astype(float)
m[...,1]*=1-0.35*np.clip(c,0,1.2)
m[...,3]*=1-0.15*np.clip(c,0,1.2)
Image.fromarray(m.round().clip(0,255).astype(np.uint8),'RGBA').save('out_%s_mask.png'%pre)
print(tag,'mm/texel',round(mmpt,3),'max slope',round(float(np.abs(np.stack([gx,gy])).max()*s),2),'basecolor mode',mode)
