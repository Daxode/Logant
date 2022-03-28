using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AntExecutionSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
    protected override void OnCreate() => m_EndSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

    protected override void OnUpdate()
    {
        var colonyExecutionEntity = GetSingletonEntity<ColonyExecutionData>();
        var colonyExecutionDataBuffer = GetBuffer<ColonyExecutionData>(colonyExecutionEntity, true);
        var deltaTime = Time.DeltaTime;
        var ecb = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        Entities.WithReadOnly(colonyExecutionDataBuffer).ForEach(
            (Entity e, int entityInQueryIndex,              // Indexing
            ref AntState ant, ref AntRegisters registers,   // Ant
            ref PhysicsVelocity vel, in PhysicsMass mass,   // Physics
            in LocalToWorld ltw) =>                         // Transform
        {
            var executionData = colonyExecutionDataBuffer[ant.executionLine];
            var rnd = Random.CreateFromIndex((uint) ant.id);
            
            switch (executionData.type)
            {
                // Control Flow
                case ColonyExecutionType.GoTo:
                    ant.executionLine = (short)(GetComponent<ExecutionLine>(executionData.storageEntity)-1);
                    break;
                case ColonyExecutionType.GoToTrue:
                    var registerIndex = GetComponent<RegisterIndex>(executionData.storageEntity);
                    ant.executionLine = registers[registerIndex] 
                        ? (short)(GetComponent<ExecutionLine>(executionData.storageEntity)-1)
                        : ant.executionLine;
                    break;
                case ColonyExecutionType.Exit:
                    ecb.RemoveComponent<AntState>(entityInQueryIndex, e);
                    break;
                
                // Data
                case ColonyExecutionType.Set:
                    registers.Set(GetComponent<RegisterIndex>(executionData.storageEntity));
                    break;
                case ColonyExecutionType.Copy:
                    var indexes = GetComponent<CopyIndex>(executionData.storageEntity);
                    registers.Write(indexes.To, registers[indexes.From]);
                    break;

                // Exotic
                case ColonyExecutionType.GoToRandom:
                    var executionLineFromEntity = GetBufferFromEntity<ExecutionLineElement>(true);
                    var lines = executionLineFromEntity[executionData.storageEntity];
                    ant.executionLine = (short)(lines[rnd.NextInt(lines.Length)] - 1);
                    break;
                
                // Ant Behaviour
                case ColonyExecutionType.AntMoveTo:
                    // Get Target Location
                    var targetLocation = GetComponent<Translation>(executionData.storageEntity).Value;
                    var flatDir = rnd.NextFloat2Direction() * rnd.NextFloat() * .5f;
                    targetLocation += new float3(flatDir.x,0,flatDir.y);
                    
                    // Calculate Flat Direction to destination
                    var dir = math.normalizesafe(targetLocation - ltw.Position) * .9f;
                    dir *= new float3(1, 0, 1);
                    
                    // Apply Velocity
                    mass.GetImpulseFromForce(dir, ForceMode.Force, in deltaTime, out var impulse, out var massImpulse);
                    vel.ApplyLinearImpulse(in massImpulse, in impulse);
                    vel.ApplyAngularImpulse(in mass, 5f*deltaTime*math.up()*meth.SignedAngle(ltw.Forward, dir, math.up()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            ant.executionLine++;
        }).ScheduleParallel();
        m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}