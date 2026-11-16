import numpy as np
from PIL import Image
rng=np.random.default_rng(7)
FW,FH,COLS,ROWS=256,512,4,4
def lattice(n): return rng.random((n,n))
LAT=[lattice(64) for _ in range(5)]
def vnoise(x,y,lat):
    n=lat.shape[0]
    xi=np.floor(x).astype(int); yi=np.floor(y).astype(int); xf=x-xi; yf=y-yi
    u=xf*xf*(3-2*xf); v=yf*yf*(3-2*yf)
    a=lat[xi%n,yi%n]; b=lat[(xi+1)%n,yi%n]; c=lat[xi%n,(yi+1)%n]; d=lat[(xi+1)%n,(yi+1)%n]
    return (a*(1-u)+b*u)*(1-v)+(c*(1-u)+d*u)*v
def fbm(x,y,oct=5):
    s=0; amp=0.5; f=1
    for o in range(oct):
        s+=amp*vnoise(x*f+o*13.1,y*f+o*7.7,LAT[o]); amp*=0.5; f*=2.03
    return s
def ss(a,b,x): t=np.clip((x-a)/(b-a),0,1); return t*t*(3-2*t)
xs=np.linspace(-1,1,FW); ys=np.linspace(0,1,FH)   # y=0 bottom
X,Y=np.meshgrid(xs,ys)
sheet=np.zeros((FH*ROWS,FW*COLS,4),np.float32)
for f in range(COLS*ROWS):
    t=f*0.19
    # turbulence scrolls upward
    n1=fbm(X*2.2+3, Y*3.0 - t*2.2)
    n2=fbm(X*4.5+9, Y*6.0 - t*3.4)
    sway=0.20*(Y**1.5)*(n1-0.5)*2 + 0.04*np.sin(t*2.7+Y*3)
    w=0.62*(1-Y)**0.65*(0.25+0.75*np.clip((Y+0.02)/0.30,0,1)**0.6) + 1e-4
    d=np.abs(X-sway)/w
    body=np.clip(1-d**2,0,1)
    n3=fbm(X*9+2, Y*12.0 - t*5.0,3)
    I=body**1.2*(0.35+1.1*n2+0.25*(n3-0.5))*(1.05-0.45*Y)*ss(0.0,0.06,Y)
    # licks: break the upper part into tongues
    I*=np.where(Y<0.72,1.0,ss(0.0,0.22,n2*1.35+0.95-Y*1.25))
    I=np.clip(I,0,1.4)
    core=ss(0.55,1.15,I)
    # colour ramp
    red=np.array([0.45,0.04,0.0]); org=np.array([1.0,0.42,0.04]); yel=np.array([1.0,0.80,0.32]); wht=np.array([1.0,0.96,0.82])
    a=ss(0.06,0.6,I)
    c=red*(1-ss(0.1,0.45,I))[...,None]+org*ss(0.1,0.45,I)[...,None]
    c=c*(1-ss(0.6,0.95,I))[...,None]+yel*ss(0.6,0.95,I)[...,None]
    c=c*(1-ss(1.0,1.35,I))[...,None]+wht*ss(1.0,1.35,I)[...,None]
    rgba=np.concatenate([c*a[...,None],a[...,None]],-1)   # premultiplied
    rgba=rgba[::-1]   # image row 0 = top
    r,cidx=divmod(f,COLS)
    sheet[r*FH:(r+1)*FH,cidx*FW:(cidx+1)*FW]=rgba
img=(np.clip(sheet,0,1)*255+0.5).astype(np.uint8)
Image.fromarray(img,'RGBA').save('tool_torch_flame_sheet.png')
# preview on dark bg
bg=np.zeros_like(sheet[...,:3])+0.05
prev=bg*(1-sheet[...,3:4])+sheet[...,:3]
Image.fromarray((np.clip(prev,0,1)*255).astype(np.uint8)).resize((512,1024)).save('preview.png')
print("ok")
