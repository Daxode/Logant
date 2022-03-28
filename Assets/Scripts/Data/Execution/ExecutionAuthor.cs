using System.Collections.Generic;
using Data;
using Systems.GameObjects;
using Unity.Entities;
using UnityEngine;

public class ExecutionAuthor : MonoBehaviour
{
    public NodeObject[] points;
    public ExecutionLineDataHolderAuthor stub;
}

[UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
public class ExecutionConversionSystemPrefab : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ExecutionAuthor a) =>
        {
            if (a.stub) DeclareReferencedPrefab(a.stub.gameObject);;
        });
    }
}

public class ExecutionConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ExecutionAuthor a) =>
        {
            if (a.stub==null||a.points==null||a.points.Length<4) return;
            var entity = GetPrimaryEntity(a);
            
            // Get Entities.
            var homeEntity = GetPrimaryEntity(a.points[0]);
            var foodEntity = GetPrimaryEntity(a.points[1]);
            var buttonEntity = GetPrimaryEntity(a.points[2]);
            var lakeEntity = GetPrimaryEntity(a.points[3]);
            
            // Make sure they are DataHolders
            DstEntityManager.AddComponentData(homeEntity, new ExecutionLineDataHolder());
            DstEntityManager.AddComponentData(foodEntity, new ExecutionLineDataHolder());
            DstEntityManager.AddComponentData(buttonEntity, new ExecutionLineDataHolder());
            DstEntityManager.AddComponentData(lakeEntity, new ExecutionLineDataHolder());

            // Reg 0: HasFood
            var registerIndexFood = new RegisterIndex(0);
            // Reg 1: Holding
            var registerIndexHeld = new RegisterIndex(1);
            // Reg 2-5: Cary Amount
            var registerIndexCaryAmount = new RegisterIndex(2);
            // Reg 6: Tmp0
            var registerIndexTmp0 = new RegisterIndex(6);
            
            // GoTo Home
            var goToHomeEntity = CreateAdditionalEntity(a.stub);
            var goToHome = DstEntityManager.AddComponentData(goToHomeEntity, new ExecutionLineIndex(2));
            
            // Home PickUp Lines
            var homePickUpsEntity = CreateAdditionalEntity(a.stub);
            var homePickUpsLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(homePickUpsEntity);
            homePickUpsLines.Add(2);
            homePickUpsLines.Add(4);
            
            // // Home HasFood Lines
            var homeHasFoodEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(homeHasFoodEntity, registerIndexFood);
            DstEntityManager.AddComponentData(homeHasFoodEntity, new ExecutionLineIndex(4));

            // Lake PickUp Lines
            var lakePickUpsEntity = CreateAdditionalEntity(a.stub);
            var lakePickUpsLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(lakePickUpsEntity);
            lakePickUpsLines.Add(0);
            
            var lines = DstEntityManager.AddBuffer<ExecutionLine>(entity);
            // Entry
            lines.Add((ExecutionType.GoTo, goToHomeEntity));

            // Home:
            lines.Add((ExecutionType.AntMoveTo, homeEntity));
            lines.Add((ExecutionType.GoToTrue, homeHasFoodEntity));
            lines.Add((ExecutionType.GoToRandom, homePickUpsEntity));
            lines.Add((ExecutionType.AntDestroy));
            
            // Button: 
            lines.Add((ExecutionType.AntMoveTo, buttonEntity));
            lines.Add((ExecutionType.GoToTrue, buttonEntity));
            lines.Add((ExecutionType.GoToRandom, buttonEntity));
            lines.Add((ExecutionType.GoToRandom, buttonEntity));
            
            // Lake:
            lines.Add((ExecutionType.AntMoveTo, lakeEntity));
            lines.Add((ExecutionType.GoToTrue, lakeEntity));
            lines.Add((ExecutionType.GoToRandom, lakeEntity));
            lines.Add((ExecutionType.GoToRandom, lakeEntity));
            
            // Food:
            lines.Add((ExecutionType.AntMoveTo, foodEntity));
            lines.Add((ExecutionType.GoToTrue, lakeEntity));
            lines.Add((ExecutionType.GoToTrue, lakeEntity));
            lines.Add((ExecutionType.GoToRandom, lakeEntity));
            lines.Add((ExecutionType.GoToRandom, lakeEntity));
        });
    }
}