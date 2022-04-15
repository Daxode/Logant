using Data;
using Unity.Entities;

namespace Systems.Execution
{
    [UnityEngine.ExecuteAlways]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class ExecutionSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            if (TryGetSingleton<GlobalData>(out var globalData) && !globalData.HasStarted) 
                return;
            
            base.OnUpdate();
        }
    }
}
