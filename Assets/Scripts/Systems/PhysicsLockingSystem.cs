using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Systems
{
    public partial class PhysicsLockingSystem : SystemBase
    {
        protected override void OnUpdate() =>
            Entities.WithAll<InertiaNotLocked>().ForEach((Entity e, ref PhysicsMass mass) =>
            {
                mass.InverseInertia = math.up() * 1f;
                EntityManager.RemoveComponent<InertiaNotLocked>(e);
            }).WithStructuralChanges().Run();
    }
}
