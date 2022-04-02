using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Data
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class EntityToColorSystem : SystemBase
    {
        public NativeHashMap<Entity, FixedString64Bytes> EntityToColor;
        
        EntityQuery m_RenderMeshQuery;
        protected override void OnStartRunning()
        {
            EntityToColor = new NativeHashMap<Entity, FixedString64Bytes>(m_RenderMeshQuery.CalculateEntityCount(), Allocator.Persistent);
            Entities.WithStoreEntityQueryInField(ref m_RenderMeshQuery).ForEach((Entity e, in RenderMesh renderMeshQuery) 
                => EntityToColor[e] = new FixedString64Bytes(ColorUtility.ToHtmlStringRGB(renderMeshQuery.material.color))).WithoutBurst().Run();
        }

        protected override void OnUpdate() {}
        protected override void OnDestroy() => EntityToColor.Dispose();
    }
}
