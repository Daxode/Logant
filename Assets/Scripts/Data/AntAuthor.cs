using Unity.Entities;
using UnityEngine;

namespace Data
{
    public class AntAuthor : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // Needed for Execution
            dstManager.AddComponentData(entity, new Registers());
            dstManager.AddComponentData(entity, new ExecutionState());
            dstManager.AddComponentData(entity, new RandomHolder());
            
            // Makes it only spin around y-axis.
            dstManager.AddComponentData(entity, new InertiaNotLocked());
        }
    }
}
