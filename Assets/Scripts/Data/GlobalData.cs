using Unity.Entities;
using UnityEngine;

namespace Data
{
    [GenerateAuthoringComponent]
    public struct GlobalData : IComponentData
    {
        public float SpawnInterval;
    }
}
