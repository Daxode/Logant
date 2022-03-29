using Unity.Entities;
using UnityEngine;

namespace Data
{
    public class ResourceStoreAuthor : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] ResourceType type;
        [SerializeField] uint current;
        [SerializeField] uint total;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            => dstManager.AddComponentData(entity, new ResourceStore(type, current, total));
    }

    public struct ResourceStore : IComponentData
    {
        public ResourceType Type;
        public uint Current;
        public uint Total;

        public ResourceStore(ResourceType type, uint current, uint total)
        {
            Type = type;
            Current = current;
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