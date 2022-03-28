using Unity.Entities;
using Unity.Mathematics;

namespace Data
{
    public struct RandomKeeper : IComponentData
    {
        Random m_Rnd;
        public RandomKeeper(uint seed) => m_Rnd = new Random(seed);
        public static implicit operator RandomKeeper(Random r) => new RandomKeeper(r.state);
        public static implicit operator Random(RandomKeeper k) => k.m_Rnd;
    }
}
