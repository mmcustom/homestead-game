
exec(open(r"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work\item_util.py", encoding="utf-8").read())
def ico(bm, c, size, rot=(0, 0, 0), sub=2, noisek=0.0, sd=0.0, mat=0, fn=None):
    tmp = bmesh.new(); bmesh.ops.create_icosphere(tmp, subdivisions=sub, radius=1.0)
    R = Euler(rot).to_matrix()
    for v in tmp.verts:
        p = v.co.copy()
        if fn: p = fn(p)
        k = 1 + noisek * noise.noise(p * 3 + Vector((sd, sd * 0.7, 0)))
        v.co = Vector(c) + R @ Vector((p.x * size[0] * k, p.y * size[1] * k, p.z * size[2] * k))
    for f in tmp.faces: f.material_index = mat
    me = D.meshes.new("t"); tmp.to_mesh(me); tmp.free(); bm.from_mesh(me); D.meshes.remove(me)
def heap_positions(n, R, H, size, seed):
    rnd = random.Random(seed); out = []
    for i in range(n):
        r = R * math.sqrt(rnd.random()); a = rnd.uniform(0, 6.283)
        z = size * 0.5 + H * max(0.0, 1 - (r / R) ** 2) * rnd.uniform(0.5, 1.0)
        out.append(Vector((r * math.cos(a), r * math.sin(a), z)))
    out.sort(key=lambda p: p.z)
    return out
def plate2(bm, outline_top, mat=0, thick=0.0008):
    """two-sided flat shape: outline_top is a list of 3D points fanned from the first"""
    for off, flip in ((thick / 2, False), (-thick / 2, True)):
        vs = [bm.verts.new(p + Vector((0, 0, off))) for p in outline_top]
        for k in range(1, len(vs) - 1):
            tri = [vs[0], vs[k], vs[k + 1]]; f = bm.faces.new(tri[::-1] if flip else tri); f.material_index = mat
def strip2(bm, left, right, mat=0, thick=0.0008):
    """two-sided ribbon between two point lists"""
    for off, flip in ((thick / 2, False), (-thick / 2, True)):
        L_ = [bm.verts.new(p + Vector((0, 0, off))) for p in left]; R_ = [bm.verts.new(p + Vector((0, 0, off))) for p in right]
        for i in range(len(L_) - 1):
            q = [L_[i], R_[i], R_[i + 1], L_[i + 1]]; f = bm.faces.new(q[::-1] if flip else q); f.material_index = mat
