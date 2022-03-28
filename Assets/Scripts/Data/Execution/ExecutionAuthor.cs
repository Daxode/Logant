using Systems.GameObjects;
using Unity.Entities;
using UnityEngine;

public class ExecutionConversionSystem : GameObjectConversionSystem
{
    [SerializeField] NodeObject[] points;

    protected override void OnUpdate()
    {
        // Get Entities.
        var homeEntity = conversionSystem.GetPrimaryEntity(points[0]);
        var foodEntity = conversionSystem.GetPrimaryEntity(points[1]);
        var buttonEntity = conversionSystem.GetPrimaryEntity(points[2]);
        var lakeEntity = conversionSystem.GetPrimaryEntity(points[3]);
        
        // Make sure they are DataHolders
        dstManager.AddComponentData(homeEntity, new ExecutionLineDataHolder());
        dstManager.AddComponentData(foodEntity, new ExecutionLineDataHolder());
        dstManager.AddComponentData(buttonEntity, new ExecutionLineDataHolder());
        dstManager.AddComponentData(lakeEntity, new ExecutionLineDataHolder());

        // // Home PickUp Lines
        // var homePickUpsEntity = conversionSystem.DstEntityManager.CreateEntity(typeof(ExecutionLineDataHolder));
        // var homePickUpsLines = dstManager.AddBuffer<ExecutionLineIndexElement>(homePickUpsEntity);
        // homePickUpsLines.Add(2);
        // homePickUpsLines.Add(4);
        //
        // // Lake PickUp Lines
        // var lakePickUpsEntity = conversionSystem.DstEntityManager.CreateEntity(typeof(ExecutionLineDataHolder));
        // var lakePickUpsLines = dstManager.AddBuffer<ExecutionLineIndexElement>(lakePickUpsEntity);
        // lakePickUpsLines.Add(0);
        //
        var lines = dstManager.AddBuffer<ExecutionLine>(entity);
        // Home:
        lines.Add((ExecutionType.AntMoveTo, homeEntity));
        // lines.Add((ExecutionType.GoToRandom, homePickUpsEntity));
        //
        // // Button: 
        // lines.Add((ExecutionType.AntMoveTo, buttonEntity));
        // lines.Add((ExecutionType.Exit));
        //
        // // Lake:
        // lines.Add((ExecutionType.AntMoveTo, lakeEntity));
        // lines.Add((ExecutionType.GoToRandom, lakePickUpsEntity));
    }
}