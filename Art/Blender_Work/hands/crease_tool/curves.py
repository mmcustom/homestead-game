import numpy as np
# polylines in palm projection (y, z) in mm; depth in mm-ish units, width mm
CURVES = {
 # name: (points, depth, width)
 "wrist1":   ([(4,-17),(3,-5),(2,8),(2,22),(3,34),(5,42)], 1.0, 0.75),
 "wrist2":   ([(10,-16),(9,-3),(9,10),(9,24),(10,36)], 0.6, 0.6),
 "thenar":   ([(-46,41),(-39,30),(-30,22),(-20,17),(-10,15),(-1,16)], 1.0, 0.8),
 "head":     ([(-46,42),(-45,32),(-43,20),(-40,8),(-37,-4),(-35,-12)], 0.9, 0.75),
 "heart":    ([(-52,-17),(-54,-5),(-56,7),(-58,18),(-61,28)], 0.8, 0.7),
 
 "minor2":   ([(-30,4),(-22,6),(-15,4)], 0.3, 0.45),
}
def seg_dist(Y,Z,pts):
    d=np.full(Y.shape,1e9)
    for (y0,z0),(y1,z1) in zip(pts,pts[1:]):
        dy,dz=y1-y0,z1-z0; L2=dy*dy+dz*dz
        t=np.clip(((Y-y0)*dy+(Z-z0)*dz)/L2,0,1)
        d=np.minimum(d,np.hypot(Y-(y0+t*dy),Z-(z0+t*dz)))
    return d
def _n(Y,Z,seed):
    r=np.random.default_rng(seed); v=np.zeros(Y.shape)
    for k in range(5):
        f=r.uniform(0.15,0.6); a=r.uniform(0,6.3); ph=r.uniform(0,6.3)
        v+=np.sin(f*(np.cos(a)*Y+np.sin(a)*Z)+ph)/(k+1)
    return v/1.6
def height(Y,Z):
    h=np.zeros(Y.shape)
    Y=Y+0.35*_n(Y,Z,1); Z=Z+0.35*_n(Y,Z,2)
    mod=np.clip(0.75+0.35*_n(Y,Z,3),0.35,1.1)
    for pts,dep,w in CURVES.values():
        d=seg_dist(Y,Z,pts)
        # taper the ends
        h-=dep*mod*np.exp(-(d/(w*(0.85+0.2*mod)))**2)
    return h
