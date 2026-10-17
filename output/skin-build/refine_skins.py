"""Reference fidelity pass, executed only in the live Blender through MCP.
Every deliverable is a separate centered 1m model, never the overview scene.
"""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi
import bmesh
B=bpy.app.driver_namespace['mad_slime_builder']
for key in ['ell','box','mesh','profile','tube','wedge','cavity','material','apply','eyes','paws']:
    globals()[key]=getattr(B,key)

def spow(v,e):return math.copysign(abs(v)**e,v)
def cavity(body,cutter,m='Ink'):
    if material(m) not in list(body.data.materials):body.data.materials.append(material(m))
    index=list(body.data.materials).index(material(m))
    cutter.data.materials.clear()
    for mat in body.data.materials:cutter.data.materials.append(mat)
    for p in cutter.data.polygons:p.material_index=index
    for obj in [body,cutter]:
        bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(obj.data);bm.free()
    mod=body.modifiers.new('Volumetric mouth','BOOLEAN');mod.operation='DIFFERENCE';mod.object=cutter;mod.solver='EXACT';apply(body)
    bpy.data.objects.remove(cutter,do_unlink=True)
def rounded_body(n,p,s,m,power=2.6,N=28,R=14):
    # A smoothly inflated cube, rather than a box with an arbitrary edge bevel.
    x,y,z=p;rx,ry,rz=s;v=[]
    for j in range(R+1):
        lat=-pi/2+pi*j/R;zz=spow(sin(lat),2/power) if lat>=0 else -abs(sin(lat))**.48;r=abs(cos(lat))**(2/power)
        for i in range(N):
            t=2*pi*i/N;v.append((x+rx*r*spow(cos(t),2/power),y+ry*r*spow(sin(t),2/power),z+rz*zz))
    f=[]
    for j in range(R):
        for i in range(N):a=j*N+i;b=j*N+(i+1)%N;f.append((a,b,b+N,a+N))
    return mesh(n,v,f,m)
def surface_y(x,z,p=(0,0,.48),s=(.48,.45,.43),power=2.6):
    return p[1]-s[1]*max(.001,1-abs((x-p[0])/s[0])**power-abs((z-p[2])/s[2])**power)**(1/power)
def silhouette(n,outline,depth,m):
    # Solid face feature with real thickness; outline coordinates are X,Y,Z.
    N=len(outline);v=outline+[(x,y+depth,z) for x,y,z in outline]
    return mesh(n,v,[tuple(reversed(range(N))),tuple(range(N,2*N))]+[(i,(i+1)%N,(i+1)%N+N,i+N) for i in range(N)],m)
def sleepy_eye(x,z,w=.13,h=.065):
    pts=[(x-w/2,surface_y(x-w/2,z)-.008,z),(x+w/2,surface_y(x+w/2,z)-.008,z)]
    for j in range(1,10):
        t=pi*j/10;xx=x+w*.5*cos(t);zz=z-h*sin(t);pts.append((xx,surface_y(xx,zz)-.012,zz))
    return silhouette('Sleepy half eye',pts,.022,'Ink')
def soft_ear(x,y,z,w,h,m):
    outline=[(x-w*.5,y,z),(x-w*.48,y,z+h*.35),(x-w*.32,y,z+h*.98),
             (x-w*.12,y,z+h),(x+w*.50,y,z+h*.28),(x+w*.50,y,z)]
    if x>0:outline=[(2*x-a,b,c) for a,b,c in reversed(outline)]
    o=silhouette('Soft triangular ear',outline,.17,m)
    bevel=o.modifiers.new('Rounded ear rims','BEVEL');bevel.width=.026;bevel.segments=3;apply(o)
    return o
def stripe_patch(y,width=.11):
    v=[];N=20
    for j in range(N+1):
        t=-1.15+2.3*j/N;x=.476*sin(t)
        taper=max(.12,sin(pi*j/N))**.35
        for dy in [-width/2,width/2]:
            yy=y+dy*taper
            zz=.48+.43*max(.001,1-abs(x/.48)**2.6-abs(yy/.45)**2.6)**(1/2.6)
            v.append((x,yy,zz+.003))
    return mesh('Broad back stripe',v,[(2*i,2*i+1,2*i+3,2*i+2) for i in range(N)],'Stripe')
def cat():
    # The stripe boundaries share the body topology: no floating strips or z-fighting.
    ys=sorted(set([-.45,-.445,-.43,-.40,-.35,.35,.40,.43,.445,.45]+[c+d for c in [-.23,0,.23] for d in [-.058,0,.058]]))
    v=[];N=32
    for y in ys:
        r=max(0,1-abs(y/.45)**2.6)**(1/2.6)
        for i in range(N):
            t=2*pi*i/N;v.append((.48*r*spow(cos(t),2/2.6),y,.48+.43*r*spow(sin(t),2/2.6)))
    faces=[]
    for j in range(len(ys)-1):
        for i in range(N):a=j*N+i;b=j*N+(i+1)%N;faces.append((a,b,b+N,a+N))
    body=mesh('Ginger loaf',v,faces,'Orange');body.data.materials.append(material('Stripe'))
    for poly in body.data.polygons:
        c=poly.center
        if c.z>.68 and any(abs(c.y-y)<.058 for y in [-.23,0,.23]):poly.material_index=1
    for x in [-.28,.28]:
        for y in [-.29,.29]:ell('Cream paw',(x,y,.09),(.115,.13,.09),'Cream',12,6)
        soft_ear(x,-.17,.77,.24,.20,'Orange')
        soft_ear(x,-.185,.817,.125,.115,'Pink')
        sleepy_eye(x*.61,.57)
    for x in [-.075,.075]:ell('Cream muzzle',(x,-.447,.445),(.10,.055,.087),'Cream',16,8)
    o=wedge('Pink nose',(0,-.503,.50),.08,-.053,.024,'Pink')
    # An upturned broad tail pressed firmly to the back, with two broad rings.
    pts=[(0,.408,.23),(.04,.47,.27),(.09,.49,.37),(.085,.47,.48),(.04,.45,.55)]
    tail=tube('Thick curled tail',pts,.083,'Orange',10)
    tail.data.materials.append(material('Stripe'))
    for j,p in enumerate(tail.data.polygons):
        if j//10 in [1,3]:p.material_index=1
def dog():
    rounded_body('Cream puppy',(0,0,.48),(.43,.43,.43),'Cream',2.7)
    paws()
    for x in [-.39,.39]:
        e=ell('Drooping chocolate ear',(x,-.035,.61),(.125,.20,.27),'Chocolate',16,10);e.rotation_euler[1]=-.20 if x<0 else .20
    ell('Character left eye patch',(.175,-.410,.62),(.124,.039,.163),'Chocolate',16,8)
    eyes(.62,-.44,.175,(.043,.025,.053))
    for x in [-.082,.082]:ell('Broad cream muzzle',(x,-.43,.38),(.15,.10,.12),'Cream',16,8)
    ell('Charcoal nose',(0,-.531,.45),(.085,.037,.059),'Ink',16,8)
    ell('Right corner tongue',(-.14,-.503,.30),(.047,.026,.072),'Pink',12,8)
    tube('Short upturned tail',[(0,.39,.30),(0,.46,.39),(0,.44,.47)],.07,'Chocolate',10)
def slime():
    # A broad dome with substantial upper shoulders and a flattened, softly uneven base.
    profile('Apple green dome',[(0,.43),(.025,.48),(.12,.50),(.31,.49),(.53,.46),(.70,.39),(.83,.27),(.89,.10),(.90,.02)],'Green',32)
    eyes(.55,-.438,.17,(.047,.033,.051))
def pacman():
    b=rounded_body('Round chomper',(0,0,.47),(.49,.49,.47),'Yellow',2.3,N=32,R=16)
    # Rounded wedge cavity: mouth corners retreat into the body, never wrap around the back.
    verts=[];N=20
    for i in range(N+1):
        t=pi*i/N;x=.49*cos(t);bottom=.29-.085*sin(t)
        verts.extend([(x,-.75,bottom),(x,-.75,.56)])
    # Closed cutter made from a broad front ellipse tapering to a small rear ellipse.
    outline=[]
    for i in range(32):
        t=2*pi*i/32;outline.append((.48*cos(t),-.75,.40+.16*sin(t)))
    back=[(x*.58,-.045,.41+(z-.40)*.18) for x,y,z in outline]
    cutter=mesh('Rounded wedge cutter',outline+back,[tuple(reversed(range(32))),tuple(range(32,64))]+[(i,(i+1)%32,(i+1)%32+32,i+32) for i in range(32)],'Ink')
    cavity(b,cutter)
    for x in [-.16,.16]:
        z=.70;y=-.425
        # Pie cut silhouette. No yellow ball or painted highlight over an oval eye.
        pts=[(x,surface_y(x,z,(0,0,.47),(.49,.49,.47),2.3)-.008,z)]
        for i in range(17):
            t=.35+(2*pi-.80)*i/16;xx=x+.044*cos(t);zz=z+.059*sin(t);pts.append((xx,surface_y(xx,zz,(0,0,.47),(.49,.49,.47),2.3)-.008,zz))
        silhouette('Pie cut eye',pts,.025,'Ink')
def cube():
    B.cube()
def ufo():
    profile('Lilac saucer',[(0,.20),(.035,.38),(.11,.47),(.22,.5),(.36,.49),(.45,.43),(.47,.30)],'Lilac',32)
    profile('Blue hemisphere',[(.45,.365),(.57,.357),(.72,.31),(.84,.22),(.91,.11),(.93,.02)],'Sky',28)
    for t in [0,pi/2,pi,3*pi/2]:
        o=ell('Inset yellow lamp',(.487*cos(t),.487*sin(t),.285),(.057,.023,.059),'Yellow',12,8);o.rotation_euler[2]=t-pi/2
    eyes(.64,-.33,.13,(.034,.024,.049))
def car():
    rounded_body('Coral car body',(0,0,.31),(.45,.47,.25),'Coral',3.3,N=24,R=10)
    # A tapered cabin, broad at its beltline and softly rounded at the cream roof.
    levels=[(.44,.36,.34),(.60,.34,.31),(.82,.29,.255),(.88,.23,.21)]
    v=[];N=24
    for z,rx,ry in levels:
        for i in range(N):
            t=2*pi*i/N;v.append((rx*spow(cos(t),.38),ry*spow(sin(t),.38)+.035,z))
    f=[tuple(reversed(range(N)))]
    for j in range(3):
        for i in range(N):a=j*N+i;b=j*N+(i+1)%N;f.append((a,b,b+N,a+N))
    f.append(tuple(range(3*N,4*N)));cab=mesh('Cream tapered cabin',v,f,'Cream')
    cab.data.materials.append(material('Ink'))
    for poly in cab.data.polygons:
        c=poly.center
        # Windows are integrated into the shell, with cream pillars and a broad cream roof.
        front_or_back=(c.y<-.17 or c.y>.25) and abs(c.x)<.25
        side=abs(c.x)>.25 and -.19<c.y<.24 and abs(c.y-.035)>.045
        if .55<c.z<.79 and (front_or_back or side):poly.material_index=1
    for x in [-.425,.425]:
        for y in [-.29,.29]:
            ell('Chunky wheel',(x,y,.185),(.087,.18,.18),'Ink',16,8)
            ell('Plain cream hub',(x*1.2,y,.185),(.018,.092,.092),'Cream',12,8)
    for x in [-.28,.28]:ell('Cream headlight',(x,-.449,.355),(.073,.027,.078),'Cream',16,8)
    box('Straight grille',(0,-.473,.26),(.31,.02,.054),'Ink',.025,3)
    for x in [-.24,.24]:box('Spoiler support',(x,.377,.51),(.06,.06,.09),'Coral',.025,2)
    box('Small rear spoiler',(0,.405,.56),(.59,.11,.055),'Coral',.027,3)
def snail():
    rounded_body('Peach foot',(0,-.015,.16),(.48,.46,.155),'Peach',3.0,N=24,R=8)
    ell('Round purple shell',(0,.065,.55),(.425,.39,.43),'Purple',28,14)
    rounded_body('Short broad head',(0,-.315,.32),(.31,.21,.24),'Peach',2.4,N=20,R=10)
    for side in [-1,1]:
        pts=[]
        for i in range(50):
            t=.30+2*pi*1.38*i/49;r=.018+.31*i/49
            y=.065+r*cos(t);z=.55+r*sin(t)
            xx=.425*math.sqrt(max(.05,1-((y-.065)/.39)**2-((z-.55)/.43)**2))
            pts.append((side*(xx+.006),y,z))
        tube('Cream shell spiral',pts,.033,'Cream',6)
        tube('Short eye stalk',[(side*.20,-.34,.44),(side*.21,-.37,.65)],.052,'Peach',8)
        ell('Eye stalk tip',(side*.21,-.37,.68),(.083,.075,.085),'Peach',12,8)
        ell('Dark eye',(side*.21,-.438,.69),(.036,.023,.045),'Ink',12,8)
    tube('Determined frown',[(-.13,-.501,.30),(-.07,-.52,.34),(0,-.529,.35),(.07,-.52,.34),(.13,-.501,.30)],.011,'Chocolate',6)
    ell('Tucked tail',(0,.43,.13),(.09,.06,.07),'Peach',12,6)
def ghost():
    profile('Scalloped bell',[(0,.35),(.06,.45),(.19,.48),(.40,.45),(.62,.38),(.79,.27),(.89,.11),(.90,.015)],'White',32,.09)
    for x in [-.42,.42]:ell('Hugging arm',(x,-.035,.32),(.105,.115,.13),'White',16,8)
    eyes(.57,-.407,.16,(.055,.029,.094))
    body=bpy.data.objects['Scalloped bell'];c=ell('Round mouth cutter',(0,-.46,.38),(.037,.08,.045),'Ink',16,8);cavity(body,c)
    ell('Lavender underside',(0,0,.065),(.30,.30,.018),'Lavender',16,6)
def shark():
    # A shared blue/cream boundary loop curves up into the broad front muzzle.
    N=32;R=14;split=6;v=[]
    for j in range(R+1):
        for i in range(N):
            t=2*pi*i/N;boundary_z=.23+.30*max(0,-sin(t))**2
            boundary_lat=math.asin(spow((boundary_z-.45)/.405,2.55/2))
            lat=-pi/2+(boundary_lat+pi/2)*j/split if j<=split else boundary_lat+(pi/2-boundary_lat)*(j-split)/(R-split)
            r=abs(cos(lat))**(2/2.55)
            v.append((.44*r*spow(cos(t),2/2.55),.44*r*spow(sin(t),2/2.55),.45+.405*spow(sin(lat),2/2.55)))
    f=[]
    for j in range(R):
        for i in range(N):a=j*N+i;b=j*N+(i+1)%N;f.append((a,b,b+N,a+N))
    b=mesh('Plump blue shark',v,f,'Blue');b.data.materials.append(material('Cream'))
    for p in b.data.polygons:
        if p.index<split*N:p.material_index=1
    cutter=ell('Smile cavity',(0,-.449,.405),(.295,.195,.12),'Ink',24,10);cavity(b,cutter)
    for x in [-.215,-.072,.072,.215]:wedge('Blunt cream tooth',(x,-.426,.50),.105,-.11,.06,'Cream')
    eyes(.65,-.389,.275,(.032,.026,.042))
    wedge('Short dorsal',(0,.045,.79),.18,.20,.24,'Blue')
    for s in [-1,1]:
        fin=ell('Thick side flipper',(s*.43,.02,.29),(.12,.18,.085),'Blue',16,8);fin.rotation_euler[1]=s*.35
    wedge('Rear tail lobe',(0,.44,.22),.23,.23,.15,'Blue')
def vacuum():
    profile('Butter yellow puck',[(0,.35),(.035,.455),(.12,.49),(.43,.50),(.59,.475),(.64,.42)],'Yellow',32)
    profile('Integrated top dome',[(.62,.42),(.72,.39),(.83,.28),(.90,.13),(.91,.025)],'Yellow',28)
    v=[];N=24
    for z in [.18,.50]:
        for i in range(N+1):t=-.80+1.60*i/N;v.append((.502*sin(t),-.502*cos(t),z))
    o=mesh('Charcoal curved bumper',v,[(i,i+1,i+N+2,i+N+1) for i in range(N)],'Ink');mod=o.modifiers.new('Bumper thickness','SOLIDIFY');mod.thickness=.018;apply(o)
    eyes(.365,-.480,.16,(.032,.023,.060),'Sky')
    box('Low intake',(0,-.503,.183),(.26,.02,.043),'Ink',.02,2)
    ell('Coral power button',(0,-.08,.91),(.078,.078,.017),'Coral',16,8)
def mimic():
    B.mimic()
    box('Solid rear hinge',(0,.405,.48),(.82,.14,.20),'Wood',.05,2)
    # Push the tongue into the mouth opening so it sits on the lower lip.
    o=bpy.data.objects.get('Tongue')
    if o:o.location.z+=.025

BUILDERS={'Slime':slime,'Pacman':pacman,'CubeSlime':cube,'Cat':cat,'Dog':dog,'Ufo':ufo,'Car':car,'Snail':snail,'Ghost':ghost,'Shark':shark,'Vacuum':vacuum,'Mimic':mimic}

def preview(name):
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=40
    scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=100
    scene.world.color=(.22,.22,.22);scene.render.film_transparent=True;scene.view_settings.view_transform='Standard'
    cam=B.camera_at('Preview camera',(1.55,-3,1.25),(0,0,0));scene.camera=cam
    lights=[]
    for loc,power,size in [((-3,-4,5),280,4),((3,-1,2),90,3),((0,3,4),150,3)]:
        bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power;o.data.size=size;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler();lights.append(o)
    scene.render.filepath=str(B.SOURCE/'Previews'/f'{name}.png');bpy.ops.render.render(write_still=True)
    for o in [cam]+lights:bpy.data.objects.remove(o,do_unlink=True)
def build(name):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    BUILDERS[name]();objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
    for o in objs:
        apply(o);group=o.vertex_groups.new(name=o.name);group.add(list(range(len(o.data.vertices))),1,'REPLACE')
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
    coords=[o.matrix_world@v.co for v in o.data.vertices]
    lo=Vector(tuple(min(v[i] for v in coords) for i in range(3)));hi=Vector(tuple(max(v[i] for v in coords) for i in range(3)));center=(lo+hi)/2
    for v,p in zip(o.data.vertices,coords):v.co=Vector(tuple((p[i]-center[i])/(hi[i]-lo[i]) for i in range(3)))
    o.location=(0,0,0);o.rotation_euler=(0,0,0);o.scale=(1,1,1)
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.remove_doubles(threshold=.00001);bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
    mod=o.modifiers.new('Game triangles','TRIANGULATE');apply(o);o.data.name=name+'Mesh'
    if name=='Pacman':
        normals=[None]*len(o.data.loops)
        for poly in o.data.polygons:
            exterior=o.data.materials[poly.material_index].name=='Yellow'
            for loop_index in poly.loop_indices:
                co=o.data.vertices[o.data.loops[loop_index].vertex_index].co
                normal=Vector(tuple(spow(c,1.3) for c in co)).normalized() if exterior else poly.normal
                normals[loop_index]=tuple(normal)
        o.data.normals_split_custom_set(normals)
    bpy.ops.wm.save_as_mainfile(filepath=str(B.SOURCE/'Blender'/f'{name}.blend'))
    # Unity's FBX importer reverses Blender's forward convention: export only the turned mesh.
    for v in o.data.vertices:v.co.x=-v.co.x;v.co.y=-v.co.y
    bpy.ops.export_scene.fbx(filepath=str(B.OUT/'FBX'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_space_transform=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,bake_anim=False)
    for v in o.data.vertices:v.co.x=-v.co.x;v.co.y=-v.co.y
    report={'name':name,'triangles':len(o.data.polygons),'dimensions':[round(v,6) for v in o.dimensions],'center':[0,0,0],'separate_blend':str(B.SOURCE/'Blender'/f'{name}.blend'),'separate_fbx':str(B.OUT/'FBX'/f'{name}.fbx')}
    (B.SOURCE/f'{name}-geometry.json').write_text(json.dumps(report,indent=2))
    preview(name)
    print('REFINED_MCP_MODEL',report)
