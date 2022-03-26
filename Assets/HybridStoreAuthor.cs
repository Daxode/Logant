using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class HybridStoreAuthor : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new UIStore());
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.Editor|WorldSystemFilterFlags.Default)]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial class HybridStoreGathererSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        var hybridStore = Object.FindObjectOfType<HybridStore>();
        this.SetSingleton(new UIStore{Doc = hybridStore.screenDoc});
        Debug.Log($"updated {nameof(HybridStore)}");
    }

    protected override void OnUpdate() {}
}

public class UIStore : IComponentData
{
    public UIDocument Doc;
}
