using Unity.Entities;

namespace Systems.Execution
{
    [UnityEngine.ExecuteAlways]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class ExecutionSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
