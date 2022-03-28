using System;
using Data;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Systems.Execution
{
    [UpdateInGroup(typeof(ExecutionSystemGroup))]
    public partial class AntTriggerSystem : SystemBase
    {
        StepPhysicsWorld m_PhysicsWorld;

        protected override void OnCreate() => m_PhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();

        protected override void OnUpdate()
        {
            var executionEntity = GetSingletonEntity<ExecutionLine>();
            var executionLines = GetBuffer<ExecutionLine>(executionEntity, true);
            var stateFromEntity = GetComponentDataFromEntity<ExecutionState>();
            var dataHolderFromEntity = GetComponentDataFromEntity<ExecutionLineDataHolder>(true);
            
            var triggerJob = new AntTriggerJob
            {
                stateFromEntity = stateFromEntity,
                dataHolderFromEntity = dataHolderFromEntity,
                resourceStoreFromEntity = GetComponentDataFromEntity<ResourceStore>(),
                executionLines = executionLines,
                registersFromEntity = GetComponentDataFromEntity<Registers>(),
                indexFromEntity = GetComponentDataFromEntity<RegisterIndex>()
            };
            // var antPickDropJob = new AntPickUpAndDropOffTriggerJob
            // {
            //     stateFromEntity = stateFromEntity,
            //     dataHolderFromEntity = dataHolderFromEntity,
            //     executionLines = executionLines
            // };
            Dependency = triggerJob.Schedule(m_PhysicsWorld.Simulation, Dependency);
            //var moveToHandle = moveToJob.Schedule(m_PhysicsWorld.Simulation, Dependency);
            //var antPickDropHandle = antPickDropJob.Schedule(m_PhysicsWorld.Simulation, Dependency);
            //Dependency = JobHandle.CombineDependencies(moveToHandle, antPickDropHandle);
        }

        struct AntTriggerJob : ITriggerEventsJob
        {
            // Ant
            public ComponentDataFromEntity<ExecutionState> stateFromEntity;
            public ComponentDataFromEntity<Registers> registersFromEntity;
            [ReadOnly] public DynamicBuffer<ExecutionLine> executionLines;

            // NodeObject
            [ReadOnly] public ComponentDataFromEntity<ExecutionLineDataHolder> dataHolderFromEntity;
            [ReadOnly] public ComponentDataFromEntity<RegisterIndex> indexFromEntity;
            public ComponentDataFromEntity<ResourceStore> resourceStoreFromEntity;


            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var entityB = triggerEvent.EntityB;

                if (stateFromEntity.HasComponent(entityA) && dataHolderFromEntity.HasComponent(entityB))
                {
                    var state = stateFromEntity[entityA];
                    var line = executionLines[state.executionLine];
                    if (line.ePtr == entityB)
                    {
                        switch (line.type)
                        {
                            case ExecutionType.AntMoveTo:
                                break;
                            case ExecutionType.AntPickUp:
                                // RW
                                var resourceStore = resourceStoreFromEntity[line.ePtr];
                                var registers = registersFromEntity[entityA];
                                if (resourceStore.Left > 0)
                                {
                                    // RO
                                    var readIndex = indexFromEntity[line.ePtr];
                                
                                    //
                                    const byte count = 4;
                                    uint slot;
                                    byte index=readIndex;
                                    // do
                                    // {
                                    //     slot = registers.Read(index, count);
                                    //     if (slot == 0)
                                    //     {
                                    //         registers.Write(index, (uint) resourceStore.Type, count);
                                    //         registers.Write((byte)(index+count*2), true);
                                    //         resourceStore.Left--;
                                    //         registersFromEntity[line.ePtr] = registers;
                                    //         resourceStoreFromEntity[line.ePtr] = resourceStore;
                                    //     }
                                    //     index += count;
                                    // } while (slot!=0);
                                    
                                    if (registers.Read(readIndex, count) == 0) {
                                        registers.Write(index, (uint) resourceStore.Type, count);
                                        registers.Write((byte)(index+count*2), true);
                                        resourceStore.Left--;
                                        registersFromEntity[line.ePtr] = registers;
                                        resourceStoreFromEntity[line.ePtr] = resourceStore;
                                    } else {
                                        if (registers.Read((byte)(readIndex+count), count) == 0) {
                                            registers.Write((byte)(readIndex+count), (uint) resourceStore.Type, count);
                                            registers.Write((byte)(readIndex+count*2), true);
                                            resourceStore.Left--;
                                            registersFromEntity[line.ePtr] = registers;
                                            resourceStoreFromEntity[line.ePtr] = resourceStore;
                                        }
                                    }
                                }
                                break;
                            default: return;
                        }
                        
                        state.executionLine++;
                        stateFromEntity[entityA] = state;
                    }
                }
            }
        }
        
        // struct AntPickUpAndDropOffTriggerJob : ITriggerEventsJob
        // {
        //     // Ant
        //     public ComponentDataFromEntity<ExecutionState> stateFromEntity;
        //     [ReadOnly] public DynamicBuffer<ExecutionLine> executionLines;
        //
        //     // NodeObject
        //     [ReadOnly] public ComponentDataFromEntity<ExecutionLineDataHolder> dataHolderFromEntity;
        //     public ComponentDataFromEntity<ResourceStore> resourceStoreFromEntity { get; set; }
        //
        //     public void Execute(TriggerEvent triggerEvent)
        //     {
        //         var entityA = triggerEvent.EntityA;
        //         var entityB = triggerEvent.EntityB;
        //
        //         if (stateFromEntity.HasComponent(entityA) && dataHolderFromEntity.HasComponent(entityB) && resourceStoreFromEntity.HasComponent(entityB))
        //         {
        //             var antState = stateFromEntity[entityA];
        //             var resourceStore = stateFromEntity[entityA];
        //             var line = executionLines[antState.executionLine];
        //             if (line.type != ExecutionType.AntMoveTo) return;
        //             if (line.ePtr == entityB)
        //             {
        //                 antState.executionLine++;
        //                 stateFromEntity[entityA] = antState;
        //             }
        //         }
        //     }
        // }
    }
}

