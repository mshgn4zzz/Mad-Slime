using UnityEngine;
using UnityEngine.Rendering;

namespace MadSlime.Levels.Logic
{
    public class LevelSurfaceFactory
    {
        public LevelSurface Create(Mesh mesh, Transform parent)
        {
            MeshRenderer background = CreateSurface("LowerEnvironment", mesh, parent);
            MeshRenderer edge = CreateSurface("PlatformEdge", mesh, parent);

            return new LevelSurface(background, edge, mesh.bounds.size);
        }

        private MeshRenderer CreateSurface(string name, Mesh mesh, Transform parent)
        {
            GameObject surface = new GameObject(name);

            surface.transform.SetParent(parent, false);
            surface.AddComponent<MeshFilter>().sharedMesh = mesh;

            MeshRenderer renderer = surface.AddComponent<MeshRenderer>();

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            return renderer;
        }
    }
}
