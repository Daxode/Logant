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
        public ushort Total;
        public ushort Current;
    }
    
    public class AntHill : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [SerializeField] GameObject antPrefab;
        [SerializeField] ushort numberOfAnts;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AntPrefab {prefab = conversionSystem.GetPrimaryEntity(antPrefab)});
            dstManager.AddComponentData(entity, new AntHillData {Total = numberOfAnts});
            dstManager.AddComponentData(entity, new RandomHolder(417));
        }
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) => referencedPrefabs.Add(antPrefab);
    }

}
