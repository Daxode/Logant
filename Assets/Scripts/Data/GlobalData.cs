using Unity.Entities;

namespace Data
{
    [GenerateAuthoringComponent]
    public struct GlobalData : IComponentData
    {
        public float SpawnInterval;
    }
}
