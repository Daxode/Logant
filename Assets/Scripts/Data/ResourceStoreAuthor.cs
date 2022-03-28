using Unity.Entities;
using UnityEngine;

namespace Data
{
    public class ResourceStoreAuthor : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] ResourceType type;
        [SerializeField] uint count;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            => dstManager.AddComponentData(entity, new ResourceStore(type, count, count));
    }

    public struct ResourceStore : IComponentData
    {
        public ResourceType Type;
        public uint Left;
        public uint Total;

        public ResourceStore(ResourceType type, uint left, uint total)
        {
            Type = type;
            Left = left;
            Total = total;
        }
    }

    public enum ResourceType
    {
        None,
        Food,
        Key
    }
}