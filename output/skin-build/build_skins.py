"""Author the twelve reference skins. Blender coordinates: Z up, face -Y.
User overrides: cubic 1m envelope, pivot at bounding-box center.
Run: blender -b --factory-startup --python output/skin-build/build_skins.py
"""
import bpy, math, json, sys
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/MadSlime/Resources/PlayerModel'
SOURCE = ROOT / 'output/skin-build'
for p in [OUT/'FBX', SOURCE/'Blender', SOURCE/'Previews']:
    p.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
MATS = {}
COLORS = {'Ink':'292934','Green':'8FD83E','Core':'72B735','FaceGreen':'285D28',
 'Yellow':'FFCF37','Orange':'E99A35','Stripe':'BD662A','Cream':'FFF0CC',
 'Pink':'ED8390','Chocolate':'79513C','Lilac':'AA77E5','Sky':'65BCEB',
 'Coral':'EC625E','Purple':'9260C9','Peach':'EFB178','White':'F6F1E9',
 'Lavender':'D9CFE8','Blue':'6AB4DD','Gold':'E2B653','Wood':'96603D','Mouth':'493147'}
def material(name):
    if name in MATS: return MATS[name]
    h=COLORS[name]; srgb=[int(h[i:i+2],16)/255 for i in (0,2,4)]
    rgb=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in srgb]
    m=bpy.data.materials.new(name); m.diffuse_color=(*rgb,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*rgb,1)
    p.inputs['Roughness'].default_value=.78; p.inputs['Metallic'].default_value=0
    MATS[name]=m; return m
def finish(o,n,m,smooth=True):
    o.name=n; o.data.materials.append(material(m))
    for p in o.data.polygons:p.use_smooth=smooth
    return o
def apply(o):
    bpy.context.view_layer.objects.active=o
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    for mod in list(o.modifiers): bpy.ops.object.modifier_apply(modifier=mod.name)
    return o
def ell(n,p,s,m,seg=16,rings=10):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,location=p)
    o=bpy.context.object;o.scale=s;apply(o);return finish(o,n,m)
def box(n,p,s,m,b=.09,segments=3):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=s;apply(o)
    if b:
        mod=o.modifiers.new('Soft toy corners','BEVEL');mod.width=b;mod.segments=segments;apply(o)
    finish(o,n,m)
    # Weighted face normals keep broad panels flat and bevels soft.
    mod=o.modifiers.new('Panel normals','WEIGHTED_NORMAL');mod.keep_sharp=True;apply(o)
    return o
def mesh(n,v,f,m,smooth=True):
    me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me)
    bpy.context.collection.objects.link(o);return finish(o,n,m,smooth)
def profile(n,levels,m,N=24,scallop=0):
    v=[]
    for z,r in levels:
        for j in range(N):
            t=2*pi*j/N;v.append((r*cos(t),r*sin(t),z+scallop*(1+cos(4*t))*.5*max(0,1-z/.22)))
    f=[tuple(reversed(range(N)))]
    for i in range(len(levels)-1):
        for j in range(N):a=i*N+j;b=i*N+(j+1)%N;f.append((a,b,b+N,a+N))
    f.append(tuple((len(levels)-1)*N+j for j in range(N)))
    return mesh(n,v,f,m)
def tube(n,points,r,m,sides=6):
    v=[]
    for i,pt in enumerate(points):
        tangent=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])
        tangent.normalize();axis=Vector((1,0,0))
        if abs(tangent.dot(axis))>.9:axis=Vector((0,1,0))
        u=tangent.cross(axis).normalized();w=tangent.cross(u).normalized()
        for j in range(sides):v.append(Vector(pt)+r*(u*cos(2*pi*j/sides)+w*sin(2*pi*j/sides)))
    f=[tuple(reversed(range(sides)))]
    for i in range(len(points)-1):
        for j in range(sides):a=i*sides+j;b=i*sides+(j+1)%sides;f.append((a,b,b+sides,a+sides))
    f.append(tuple((len(points)-1)*sides+j for j in range(sides)))
    return mesh(n,v,f,m)
def wedge(n,p,w,h,d,m):
    x,y,z=p;v=[(x-w/2,y-d/2,z),(x+w/2,y-d/2,z),(x,y-d/2,z+h),
      (x-w/2,y+d/2,z),(x+w/2,y+d/2,z),(x,y+d/2,z+h)]
    o=mesh(n,v,[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)],m)
    mod=o.modifiers.new('Rounded tips','BEVEL');mod.width=.025;mod.segments=2;apply(o);return o
def eyes(z=.58,y=-.45,x=.19,scale=(.048,.035,.065),m='Ink'):
    for side in [-1,1]:ell('Eye.L' if side<0 else 'Eye.R',(side*x,y,z),scale,m,12,8)
def cavity(body,cutter,m='Ink'):
    body.data.materials.append(material(m));cutter.data.materials.clear();cutter.data.materials.append(material(m))
    mod=body.modifiers.new('Real recessed mouth','BOOLEAN');mod.operation='DIFFERENCE';mod.object=cutter;mod.solver='EXACT';apply(body)
    bpy.data.objects.remove(cutter,do_unlink=True)
def paws(m='Cream'):
    for x in [-.30,.30]:
        for y in [-.29,.29]:ell('Paw',(x,y,.10),(.115,.14,.105),m,12,6)
def slime():
    profile('Dome',[(0,.43),(.025,.49),(.12,.5),(.30,.48),(.48,.43),(.65,.34),(.79,.20),(.83,.04)],'Green')
    eyes(.51,-.408,.17,(.052,.042,.057))
def pacman():
    b=profile('Chomper',[(0,.35),(.03,.44),(.18,.49),(.40,.5),(.62,.45),(.80,.32),(.9,.06)],'Yellow',28)
    # A tapered solid wedge cuts only the front half. Its walls are the mouth interior.
    v=[(-.65,-.8,.21),(.65,-.8,.21),(-.65,-.8,.62),(.65,-.8,.62),(-.22,-.06,.39),(.22,-.06,.39)]
    c=mesh('Mouth cutter',v,[(0,1,3,2),(0,4,5,1),(2,3,5,4),(0,2,4),(1,5,3)],'Ink',False);cavity(b,c)
    # Thick, rounded rim follows the actual opening, leaving the dark volume visible.
    for i,x in enumerate([-.17,.17]):
        e=ell('Pie eye',(x,-.366,.705),(.052,.03,.073),'Ink',12,8)
        notch=ell('Eye notch',(x+.039,-.389,.726),(.036,.042,.023),'Yellow',8,6)
        cavity(e,notch,'Yellow')
def cube():
    box('Block shell',(0,0,.5),(1,1,1),'Green',.03,2)
    for side in [-1,1]:box('Side core',(side*.501,0,.5),(.005,.69,.68),'Core',.005,1)
    box('Back core',(0,.501,.5),(.69,.005,.68),'Core',.005,1)
    box('Front core',(0,-.502,.5),(.72,.008,.72),'Core',.005,1)
    for x in [-.205,.205]:box('Square eye',(x,-.51,.61),(.205,.012,.205),'FaceGreen',0)
    box('Square mouth',(0,-.512,.35),(.12,.014,.12),'FaceGreen',0)
def cat():
    box('Loaf',(0,0,.43),(.92,.89,.78),'Orange',.25,5);paws()
    for x in [-.29,.29]:
        wedge('Ear',(x,-.18,.72),.25,.25,.20,'Orange')
        wedge('Inner ear',(x,-.285,.77),.115,.12,.015,'Pink')
    for x in [-.105,.105]:ell('Muzzle',(x,-.433,.36),(.13,.07,.09),'Cream',12,6)
    for x in [-.19,.19]:box('Sleepy eye',(x,-.447,.53),(.13,.025,.026),'Ink',.012,2)
    wedge('Nose',(0,-.507,.41),.09,-.055,.025,'Pink')
    for y in [-.16,.07,.28]:
        pts=[]
        for i in range(13):
            t=-1.1+2.2*i/12;pts.append((.455*sin(t),y,.46+.355*cos(t)))
        tube('Back stripe',pts,.032,'Stripe',6)
    tube('Curled tail',[(.05,.42,.21),(.20,.47,.25),(.29,.44,.37),(.23,.43,.45),(.13,.44,.42)],.075,'Orange',8)
def dog():
    box('Puppy',(0,0,.46),(.84,.85,.86),'Cream',.25,5);paws()
    for x in [-.39,.39]:
        o=ell('Floppy ear',(x,-.08,.58),(.13,.22,.30),'Chocolate',16,8);o.rotation_euler[1]=-.24 if x<0 else .24
    ell('Left eye patch',(.19,-.407,.61),(.14,.046,.19),'Chocolate',16,8)
    eyes(.63,-.443,.18,(.05,.033,.06))
    for x in [-.11,.11]:ell('Short muzzle',(x,-.445,.38),(.16,.105,.125),'Cream',12,8)
    ell('Nose',(0,-.538,.445),(.10,.045,.065),'Ink',12,6)
    ell('Tongue',(-.16,-.501,.29),(.058,.04,.085),'Pink',12,6)
    tube('Tucked tail',[(0,.39,.27),(.02,.48,.38),(.015,.46,.48)],.065,'Chocolate',8)
def ufo():
    profile('Saucer',[(.10,.12),(.12,.34),(.19,.47),(.28,.5),(.40,.48),(.46,.39),(.47,.12)],'Lilac',32)
    profile('Opaque cockpit',[(.43,.34),(.55,.34),(.71,.29),(.86,.19),(.92,.035)],'Sky',24)
    # Four lamps seated in the rim, cardinal placement.
    for t in [0,pi/2,pi,3*pi/2]:
        o=ell('Rim lamp',(.48*cos(t),.48*sin(t),.34),(.063,.032,.065),'Yellow',12,6);o.rotation_euler[2]=t-pi/2
    eyes(.65,-.301,.12,(.037,.028,.054))
def car():
    box('Chassis',(0,0,.29),(.88,.93,.43),'Coral',.13,3)
    box('Cream cabin',(0,.035,.63),(.73,.68,.57),'Cream',.16,3)
    # Opaque windows inset into the cream roof frame; no anthropomorphic face.
    box('Windshield',(0,-.307,.65),(.57,.035,.27),'Ink',.055,2)
    box('Rear window',(0,.371,.65),(.54,.025,.24),'Ink',.05,2)
    for x in [-.366,.366]:
        for y in [-.115,.16]:box('Side window',(x,y,.65),(.022,.22,.25),'Ink',.04,2)
    for x in [-.435,.435]:
        for y in [-.29,.29]:
            o=ell('Wheel',(x,y,.19),(.09,.183,.183),'Ink',12,6)
            ell('Hub',(x*1.18,y,.19),(.018,.093,.093),'Cream',10,6)
    for x in [-.28,.28]:ell('Headlight',(x,-.462,.35),(.085,.028,.08),'Cream',12,6)
    box('Radiator',(0,-.478,.235),(.33,.025,.065),'Ink',.026,2)
    box('Rear spoiler',(0,.40,.45),(.63,.15,.065),'Coral',.025,2)
def snail():
    box('Fleshy base',(0,-.01,.16),(.93,.91,.29),'Peach',.13,4)
    ell('Shell',(0,.08,.53),(.43,.40,.45),'Purple',20,10)
    box('Broad head',(0,-.29,.31),(.65,.37,.41),'Peach',.16,4)
    for side in [-1,1]:
        pts=[]
        for i in range(40):
            t=i/39*2*pi*1.45;r=.018+.30*i/39
            y=.08+r*cos(t);z=.55+r*sin(t);x=side*(.43*math.sqrt(max(.04,1-((y-.08)/.40)**2-((z-.53)/.45)**2))+.008)
            pts.append((x,y,z))
        tube('Shell spiral',pts,.035,'Cream',6)
        tube('Eye stalk',[(side*.20,-.32,.43),(side*.21,-.37,.65)],.055,'Peach',8)
        ell('Eye pad',(side*.21,-.38,.67),(.085,.075,.09),'Peach',12,6)
        ell('Eye',(side*.21,-.445,.68),(.04,.025,.05),'Ink',12,6)
    tube('Determined mouth',[(-.12,-.475,.29),(-.06,-.49,.32),(0,-.494,.33),(.06,-.49,.32),(.12,-.475,.29)],.013,'Chocolate',6)
def ghost():
    profile('Bell',[(0,.39),(.08,.47),(.22,.48),(.40,.44),(.63,.36),(.81,.23),(.87,.03)],'White',28,.075)
    for x in [-.43,.43]:ell('Arm nub',(x,-.01,.30),(.10,.13,.14),'White',12,6)
    eyes(.52,-.399,.16,(.061,.037,.105))
    b=bpy.data.objects['Bell'];c=ell('Round mouth cutter',(0,-.467,.31),(.044,.10,.054),'Ink',16,8);cavity(b,c)
    ell('Recessed underside',(0,0,.05),(.29,.29,.024),'Lavender',16,6)
def shark():
    b=box('Shark body',(0,0,.41),(.88,.90,.77),'Blue',.27,5)
    belly=ell('Cream muzzle',(0,-.28,.29),(.405,.255,.26),'Cream',20,10)
    for body in [b,belly]:
        c=ell('Mouth cutter',(0,-.49,.405),(.30,.26,.12),'Ink',20,8);cavity(body,c)
    # Exactly four thick, blunt teeth, seated at the upper lip.
    for x in [-.22,-.075,.075,.22]:wedge('Tooth',(x,-.477,.492),.10,-.10,.065,'Cream')
    eyes(.60,-.414,.28,(.036,.026,.046))
    wedge('Dorsal fin',(0,.05,.70),.20,.22,.25,'Blue')
    for s in [-1,1]:
        o=ell('Flipper',(s*.43,.03,.27),(.135,.19,.09),'Blue',12,6);o.rotation_euler[1]=s*.35
    wedge('Tucked tail',(0,.43,.20),.27,.22,.15,'Blue')
def vacuum():
    profile('Puck',[(0,.35),(.04,.46),(.16,.49),(.43,.50),(.59,.46),(.64,.39)],'Yellow',32)
    profile('Raised dome',[(.60,.40),(.71,.36),(.82,.25),(.87,.045)],'Yellow',24)
    # Panel wraps around the cylindrical front, rather than hovering as a flat block.
    v=[];N=16
    for z in [.18,.49]:
        for i in range(N+1):t=-.84+1.68*i/N;v.append((.503*sin(t),-.503*cos(t),z))
    panel=mesh('Front bumper',v,[(i,i+1,i+N+2,i+N+1) for i in range(N)],'Ink')
    mod=panel.modifiers.new('Panel thickness','SOLIDIFY');mod.thickness=.02;apply(panel)
    eyes(.36,-.486,.16,(.036,.023,.071),'Sky')
    box('Intake',(0,-.495,.16),(.29,.03,.054),'Ink',.02,2)
    ell('Power button',(0,-.08,.862),(.085,.085,.022),'Coral',16,6)
def mimic():
    box('Lower chest',(0,0,.27),(.94,.92,.43),'Wood',.10,3)
    for x in [-.33,.33]:
        for y in [-.31,.31]:box('Foot',(x,y,.07),(.22,.23,.14),'Wood',.055,2)
    # Closed cavity recessed into a lip; lid opens toward front, seated at the rear hinge.
    box('Mouth interior',(0,0,.48),(.81,.76,.22),'Mouth',.075,3)
    box('Rear hinge',(0,.405,.48),(.82,.14,.20),'Wood',.05,2)
    arc=[]
    for i in range(15):
        t=pi*i/14;y=-.46*cos(t);arc.append((y,.60-.10*(y+.46)/.92+.36*sin(t)))
    v=[(x,y,z) for x in [-.48,.48] for y,z in arc];N=len(arc)
    lid=mesh('Arched lid',v,[tuple(reversed(range(N))),tuple(range(N,2*N))]+[(i,(i+1)%N,(i+1)%N+N,i+N) for i in range(N)],'Wood')
    mod=lid.modifiers.new('Lid edge softness','BEVEL');mod.width=.035;mod.segments=2;apply(lid)
    for x in [-.28,.28]:
        # Two continuous broad straps wrapping the lid cross section.
        pts=[(x,y*1.025,z+.012) for y,z in arc]
        # Use rectangular ribbon with thickness, broad across X.
        v=[]
        for _,y,z in pts:v.extend([(x-.055,y,z),(x+.055,y,z)])
        o=mesh('Gold strap',v,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(pts)-1)],'Gold')
        mod=o.modifiers.new('Strap thickness','SOLIDIFY');mod.thickness=.018;apply(o)
        for y in [-.464,.464]:box('Lower strap',(x,y,.28),(.11,.022,.35),'Gold',.012,1)
    for x in [-.15,.15]:
        o=ell('Mischievous eye',(x,-.426,.73),(.046,.025,.058),'Ink',12,6);o.rotation_euler[1]=x*2
        box('Blunt tooth',(x*1.35,-.411,.50),(.11,.07,.145),'Cream',.025,2)
    ell('Tongue',(0,-.43,.405),(.10,.09,.045),'Pink',12,6)
    box('Lock',(0,-.485,.64),(.135,.055,.15),'Gold',.035,2)
    box('Keyhole',(0,-.517,.64),(.035,.008,.055),'Ink',.015,2)

BUILDERS={'Slime':slime,'Pacman':pacman,'CubeSlime':cube,'Cat':cat,'Dog':dog,'Ufo':ufo,'Car':car,'Snail':snail,'Ghost':ghost,'Shark':shark,'Vacuum':vacuum,'Mimic':mimic}
def camera_at(name,pos,target):
    bpy.ops.object.camera_add(location=pos);c=bpy.context.object;c.name=name;c.rotation_euler=(Vector(target)-c.location).to_track_quat('-Z','Y').to_euler();c.data.type='ORTHO';c.data.ortho_scale=1.75;return c
def render_asset(name):
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24
    scene.render.resolution_x=480;scene.render.resolution_y=480;scene.render.resolution_percentage=100
    scene.world.color=(.32,.32,.32);scene.render.film_transparent=True
    scene.view_settings.view_transform='Standard'
    c=camera_at('Preview camera',(1.8,-3,1.7),(0,0,0));scene.camera=c
    lights=[]
    for p,power,size in [((-3,-4,5),450,4),((3,-1,2),180,3),((0,3,4),300,3)]:
        bpy.ops.object.light_add(type='AREA',location=p);o=bpy.context.object;o.data.energy=power;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler();lights.append(o)
    scene.render.filepath=str(SOURCE/'Previews'/f'{name}.png');bpy.ops.render.render(write_still=True)
    for o in [c]+lights:bpy.data.objects.remove(o,do_unlink=True)
def build(name,fn):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    fn();objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
    # Export one named mesh with material slots; retain authored regions as vertex groups.
    for o in objs:
        apply(o);g=o.vertex_groups.new(name=o.name);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
    # Make all dimensions 1m and center the real geometry about the pivot.
    coords=[o.matrix_world@v.co for v in o.data.vertices];lo=Vector(tuple(min(v[i] for v in coords) for i in range(3)));hi=Vector(tuple(max(v[i] for v in coords) for i in range(3)));center=(lo+hi)/2
    for v,p in zip(o.data.vertices,coords):v.co=Vector(tuple((p[i]-center[i])/(hi[i]-lo[i]) for i in range(3)))
    o.location=(0,0,0);o.rotation_euler=(0,0,0);o.scale=(1,1,1)
    # Clean winding, triangulate deterministically for mobile import.
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
    mod=o.modifiers.new('Game triangles','TRIANGULATE');apply(o)
    o.data.name=name+'Mesh'
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Blender'/f'{name}.blend'))
    bpy.ops.export_scene.fbx(filepath=str(OUT/'FBX'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_space_transform=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,bake_anim=False,path_mode='AUTO')
    stats={'name':name,'triangles':len(o.data.polygons),'vertices':len(o.data.vertices),'dimensions':[round(x,6) for x in o.dimensions],'pivot':'bounds center','materials':sorted(set(m.name for m in o.data.materials if m))}
    render_asset(name);return stats
names=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else list(BUILDERS)
stats=[build(n,BUILDERS[n]) for n in names]
(SOURCE/'geometry-report.json').write_text(json.dumps(stats,indent=2))
(SOURCE/'palette.json').write_text(json.dumps(COLORS,indent=2))
print('SKIN_BUILD_COMPLETE',json.dumps(stats))
