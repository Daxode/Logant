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
            Dependency = triggerJob.Schedule(m_PhysicsWorld.Simulation, Dependency);
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
                            case ExecutionType.AntPickResource:
                                var resourceStorePick = resourceStoreFromEntity[line.ePtr];
                                var registersPick = registersFromEntity[entityA];
                                if (resourceStorePick.Current > 0)
                                {
                                    byte readIndex = indexFromEntity[line.ePtr];
                                    const byte count = 4;
                                    if (registersPick.Read(readIndex, count) == 0) {
                                        registersPick.Write(readIndex, (uint) resourceStorePick.Type, count);
                                        registersPick.Write((byte)(readIndex+count*2), true);
                                        resourceStorePick.Current--;
                                        registersFromEntity[entityA] = registersPick;
                                        resourceStoreFromEntity[line.ePtr] = resourceStorePick;
                                    } else {
                                        if (registersPick.Read((byte)(readIndex+count), count) == 0) {
                                            registersPick.Write((byte)(readIndex+count), (uint) resourceStorePick.Type, count);
                                            registersPick.Write((byte)(readIndex+count*2), true);
                                            resourceStorePick.Current--;
                                            registersFromEntity[entityA] = registersPick;
                                            resourceStoreFromEntity[line.ePtr] = resourceStorePick;
                                        }
                                    }
                                }
                                break;
                            case ExecutionType.AntDropResource:
                                var resourceStoreDrop = resourceStoreFromEntity[line.ePtr];
                                var registersDrop = registersFromEntity[entityA];
                                if (resourceStoreDrop.Current < resourceStoreDrop.Total)
                                {
                                    var readIndex = indexFromEntity[line.ePtr];
                                    const byte count = 4;
                                    var slot0 = registersDrop.Read(readIndex, count);
                                    var slot1 = registersDrop.Read((byte) (readIndex + count), count);
                                    if (slot0 == (uint)resourceStoreDrop.Type)
                                    {
                                        registersDrop.Write(readIndex, 0, count);
                                        registersDrop.Write((byte)(readIndex+count*2), slot1!=0);
                                        resourceStoreDrop.Current++;
                                        registersFromEntity[entityA] = registersDrop;
                                        resourceStoreFromEntity[line.ePtr] = resourceStoreDrop;
                                    } else {
                                        if (registersDrop.Read((byte)(readIndex+count), count) == (uint)resourceStoreDrop.Type) {
                                            registersDrop.Write((byte)(readIndex+count), 0, count);
                                            registersDrop.Write((byte)(readIndex+count*2), slot0!=0);
                                            resourceStoreDrop.Current++;
                                            registersFromEntity[entityA] = registersDrop;
                                            resourceStoreFromEntity[line.ePtr] = resourceStoreDrop;
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
    }
}

