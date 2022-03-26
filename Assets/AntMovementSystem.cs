using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial class AntMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var colonyExecutionEntity = GetSingletonEntity<ColonyExecutionData>();
        var colonyExecutionDataBuffer = GetBuffer<ColonyExecutionData>(colonyExecutionEntity, true);
        var deltaTime = Time.DeltaTime;

        Entities.WithReadOnly(colonyExecutionDataBuffer).ForEach((ref PhysicsVelocity vel, in AntState ant, in Translation translation) =>
        {
            var executionData = colonyExecutionDataBuffer[ant.executionLine];

            switch (executionData.type)
            {
                case ColonyExecutionType.GoTo:
                    break;
                case ColonyExecutionType.Stop:
                    break;
                case ColonyExecutionType.AntMoveTo:
                    var targetsFromEntity = GetBufferFromEntity<EntityBufferElement>(true);
                    var targets = targetsFromEntity[executionData.storageEntity];
                    var targetLocation = GetComponent<Translation>(targets[ant.id % targets.Length]);
                    vel.Linear = math.normalizesafe(targetLocation.Value - translation.Value) * deltaTime * 100;
                    break;
                case ColonyExecutionType.AntWaitOn:
                    break;
                default:
                    break;
            }

        }).ScheduleParallel();
    }
}