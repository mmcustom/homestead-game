import numpy as np, sys
from PIL import Image
from scipy import ndimage
from curves import height
tag=sys.argv[1]; R=2048
d=np.load('crease_%s.npz'%tag); P,N,T,TL,UV=d['P']*1000,d['N'],d['T'],d['TL'],d['UV']
# z-buffer of max x over the palm projection (y,z) grid, 0.5 mm cells
g=0.5; ys=np.arange(-140,60,g); zs=np.arange(-40,110,g)
zb=np.full((len(ys),len(zs)),-1e9)
def raster(A,B,C,W,H,fn):
    # A,B,C: 2D pixel coords; calls fn(ix,iy,b0,b1,b2)
    xmin=int(max(np.floor(min(A[0],B[0],C[0])),0)); xmax=int(min(np.ceil(max(A[0],B[0],C[0])),W-1))
    ymin=int(max(np.floor(min(A[1],B[1],C[1])),0)); ymax=int(min(np.ceil(max(A[1],B[1],C[1])),H-1))
    if xmax<xmin or ymax<ymin: return
    X,Y=np.meshgrid(np.arange(xmin,xmax+1)+0.5,np.arange(ymin,ymax+1)+0.5)
    den=(B[1]-C[1])*(A[0]-C[0])+(C[0]-B[0])*(A[1]-C[1])
    if abs(den)<1e-12: return
    b0=((B[1]-C[1])*(X-C[0])+(C[0]-B[0])*(Y-C[1]))/den
    b1=((C[1]-A[1])*(X-C[0])+(A[0]-C[0])*(Y-C[1]))/den
    b2=1-b0-b1
    m=(b0>=-1e-3)&(b1>=-1e-3)&(b2>=-1e-3)
    if m.any(): fn((X[m]-0.5).astype(int),(Y[m]-0.5).astype(int),b0[m],b1[m],b2[m])
for t in T:
    a,b,c=P[t]
    A=((a[1]-ys[0])/g,(a[2]-zs[0])/g); B=((b[1]-ys[0])/g,(b[2]-zs[0])/g); C=((c[1]-ys[0])/g,(c[2]-zs[0])/g)
    def f(ix,iy,b0,b1,b2):
        x=b0*a[0]+b1*b[0]+b2*c[0]
        np.maximum.at(zb,(ix,iy),x)
    raster(A,B,C,len(ys),len(zs),f)
# UV raster: position, normal x, visibility
pos=np.zeros((R,R,3)); nx=np.zeros((R,R)); cov=np.zeros((R,R),bool)
for t,tl in zip(T,TL):
    uv=UV[tl]*R; uv[:,1]=R-uv[:,1]   # image row 0 = top (v=1)
    pa,pb,pc=P[t]; na,nb,nc=N[t]
    def f(ix,iy,b0,b1,b2):
        pos[iy,ix]=b0[:,None]*pa+b1[:,None]*pb+b2[:,None]*pc
        nx[iy,ix]=b0*na[0]+b1*nb[0]+b2*nc[0]; cov[iy,ix]=True
    raster(uv[0],uv[1],uv[2],R,R,f)
iy=np.clip(((pos[...,1]-ys[0])/g).astype(int),0,len(ys)-1); iz=np.clip(((pos[...,2]-zs[0])/g).astype(int),0,len(zs)-1)
front=pos[...,0]>=zb[iy,iz]-1.2
w=np.clip((nx-0.3)/0.3,0,1); w=w*w*(3-2*w)
h=height(pos[...,1],pos[...,2])*w*front*cov
# dilate into the gutter so gradients at island edges stay clean
dist,idx=ndimage.distance_transform_edt(~cov,return_distances=True,return_indices=True)
h=h[idx[0],idx[1]]*np.clip(1-(dist-4)/6,0,1)
np.save('h_%s.npy'%tag,h); np.save('cov_%s.npy'%tag,cov)
print(tag,'covered',cov.mean().round(3),'crease texels',(h<-0.2).sum(),'min',h.min().round(2))
