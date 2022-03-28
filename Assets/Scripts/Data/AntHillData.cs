using Unity.Entities;
using Unity.Mathematics;

namespace Data
{
    public struct AntPrefab : IComponentData
    {
        public Entity prefab;
    }

    public struct AntHillData : IComponentData
    {
        public ushort numberOfAnts;
        public ushort numberOfAntsSpawned;
    }
}
