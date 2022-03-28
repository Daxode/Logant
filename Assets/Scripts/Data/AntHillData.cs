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
        public short numberOfAnts;
        public short numberOfAntsSpawned;
        public Random random;
    }
}
