using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Data
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class EntityToColorSystem : SystemBase
    {
        protected override void OnCreate() => RequireForUpdate(GetEntityQuery(typeof(ExecutionLineDataHolder)));

        EntityQuery m_RenderMeshQuery;
        public NativeHashMap<Entity, FixedString64Bytes> EntityToColor;
        protected override void OnStartRunning()
        {
            EntityToColor = new NativeHashMap<Entity, FixedString64Bytes>(m_RenderMeshQuery.CalculateEntityCount(), Allocator.Persistent);
            Entities.WithStoreEntityQueryInField(ref m_RenderMeshQuery).ForEach((Entity e, in RenderMesh renderMeshQuery) 
                => EntityToColor[e] = new FixedString64Bytes(ColorUtility.ToHtmlStringRGB(renderMeshQuery.material.color))).WithoutBurst().Run();
        }
        protected override void OnStopRunning() => EntityToColor.Dispose();
        
        protected override void OnUpdate() {}
    }
}
