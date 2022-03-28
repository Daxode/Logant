using Unity.Entities;
using UnityEngine;

namespace Data
{
    public struct ExecutionLineDataHolder : IComponentData {}
    
    public class ExecutionLineDataHolderAuthor : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) 
            => dstManager.AddComponentData(entity, new ExecutionLineDataHolder());
    }
}
