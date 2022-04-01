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
            if (a.stub==null||a.points==null||a.points.Length<5) return;
            var entity = GetPrimaryEntity(a);
            
            // Get Entities.
            var homeEntity = GetPrimaryEntity(a.points[0]);
            var foodEntity = GetPrimaryEntity(a.points[1]);
            var buttonEntity = GetPrimaryEntity(a.points[2]);
            var lakeEntity = GetPrimaryEntity(a.points[3]);
            var waterSpawn = GetPrimaryEntity(a.points[4]);
            
            // Make sure they are DataHolders
            DstEntityManager.AddComponentData(homeEntity, new ExecutionLineDataHolder());
            DstEntityManager.AddComponentData(foodEntity, new ExecutionLineDataHolder());
            DstEntityManager.AddComponentData(buttonEntity, new ExecutionLineDataHolder());
            DstEntityManager.AddComponentData(lakeEntity, new ExecutionLineDataHolder());
            DstEntityManager.AddComponentData(waterSpawn, new ExecutionLineDataHolder());

            // Reg 0-8: Cary [Type|4 - Type|4 - Held|1]
            var registerIndexCary0 = new RegisterIndex(0);
            var registerIndexHeld = new RegisterIndex(8);
            // Reg 9-24: Temps
            var registerIndexTmp0 = new RegisterIndex(9);
            
            // GoTo Home
            var goToHomeEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(goToHomeEntity, new ExecutionLineIndex(1));

            // Home PickUp Lines
            var homePickEntity = CreateAdditionalEntity(a.stub);
            var homePickLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(homePickEntity);
            homePickLines.Add(8);
            homePickLines.Add(12);
            
            // // Home HasResource Lines
            var homeHasResourceEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(homeHasResourceEntity, registerIndexHeld);
            DstEntityManager.AddComponentData(homeHasResourceEntity, new ExecutionLineIndex(4));
            
            // // Home HasResource AfterDrop Lines
            var homeHasResourceAfterDropEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(homeHasResourceAfterDropEntity, registerIndexHeld);
            DstEntityManager.AddComponentData(homeHasResourceAfterDropEntity, new ExecutionLineIndex(22));
            
            // Home Resource
            DstEntityManager.AddComponentData(homeEntity, registerIndexCary0);

            
            // Button Pick Resource Lines
            var buttonPickEntity = CreateAdditionalEntity(a.stub);
            var buttonPickLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(buttonPickEntity);
            buttonPickLines.Add(17);
            
            // Button Drop Resource Lines
            var buttonDropEntity = CreateAdditionalEntity(a.stub);
            var buttonDropLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(buttonDropEntity);
            buttonDropLines.Add(1);
            buttonDropLines.Add(12);
            
            // // Button HasResource Lines
            var buttonHasResourceEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(buttonHasResourceEntity, registerIndexHeld);
            DstEntityManager.AddComponentData(buttonHasResourceEntity, new ExecutionLineIndex(11));

            
            // Lake Pick Resource Lines
            var lakePickEntity = CreateAdditionalEntity(a.stub);
            var lakePickLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(lakePickEntity);
            lakePickLines.Add(17);
            
            // Lake Drop Resource Lines
            var lakeDropEntity = CreateAdditionalEntity(a.stub);
            var lakeDropLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(lakeDropEntity);
            lakeDropLines.Add(1);
            
            // // Lake HasResource Lines
            var lakeHasResourceEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(lakeHasResourceEntity, registerIndexHeld);
            DstEntityManager.AddComponentData(lakeHasResourceEntity, new ExecutionLineIndex(16));
            
            // Lake Resource
            DstEntityManager.AddComponentData(lakeEntity, registerIndexCary0);

            
            // GoTo Food
            var goToFoodEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(goToFoodEntity, new ExecutionLineIndex(17));
            
            // Food Pick Resource Lines
            var foodPickEntity = CreateAdditionalEntity(a.stub);
            var foodPickLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(foodPickEntity);
            foodPickLines.Add(1);
            
            // Food Drop Resource Lines
            var foodDropEntity = CreateAdditionalEntity(a.stub);
            var foodDropLines = DstEntityManager.AddBuffer<ExecutionLineIndexElement>(foodDropEntity);
            foodDropLines.Add(1);
            foodDropLines.Add(8);
            
            // // Food HasResource Lines
            var foodHasResourceEntity = CreateAdditionalEntity(a.stub);
            DstEntityManager.AddComponentData(foodHasResourceEntity, registerIndexHeld);
            DstEntityManager.AddComponentData(foodHasResourceEntity, new ExecutionLineIndex(21));
            
            // Food Resource
            DstEntityManager.AddComponentData(foodEntity, registerIndexCary0);

            // WaterSpawn Resource
            DstEntityManager.AddComponentData(waterSpawn, registerIndexCary0);
            
            // --------- Ant-Sembly ==================
            var lines = DstEntityManager.AddBuffer<ExecutionLine>(entity);
            // Entry
            lines.Add((ExecutionType.GoTo, goToHomeEntity));

            // Home[1]:
            lines.Add((ExecutionType.AntMoveTo, homeEntity));
            lines.Add((ExecutionType.GoToTrue, homeHasResourceEntity));
            lines.Add((ExecutionType.GoToRandom, homePickEntity));
            lines.Add((ExecutionType.AntDropResource, homeEntity));
            lines.Add((ExecutionType.AntDropResource, homeEntity));
            lines.Add((ExecutionType.GoToTrue, homeHasResourceAfterDropEntity));
            lines.Add((ExecutionType.AntDestroy));
            
            // Button[7]: 
            lines.Add((ExecutionType.AntMoveTo, buttonEntity));
            lines.Add((ExecutionType.GoToTrue, buttonHasResourceEntity));
            lines.Add((ExecutionType.GoToRandom, buttonPickEntity));
            lines.Add((ExecutionType.GoToRandom, buttonDropEntity));
            
            // Lake[11]:
            lines.Add((ExecutionType.AntMoveTo, lakeEntity));
            lines.Add((ExecutionType.AntPickResource, lakeEntity));
            lines.Add((ExecutionType.GoToTrue, lakeHasResourceEntity));
            lines.Add((ExecutionType.GoToRandom, lakePickEntity));
            lines.Add((ExecutionType.GoToRandom, lakeDropEntity));
            
            // Food[15]:
            lines.Add((ExecutionType.AntMoveTo, foodEntity));
            lines.Add((ExecutionType.AntPickResource, foodEntity));
            lines.Add((ExecutionType.GoToTrue, foodHasResourceEntity));
            lines.Add((ExecutionType.GoToRandom, foodPickEntity));
            lines.Add((ExecutionType.GoToRandom, foodDropEntity));
            
            // waterSpawn[22]
            lines.Add((ExecutionType.AntMoveTo, waterSpawn));
            lines.Add((ExecutionType.AntDropResource, waterSpawn));
            lines.Add((ExecutionType.AntDropResource, waterSpawn));
            lines.Add((ExecutionType.AntDestroy));
        });
    }
}