using Unity.Entities;
using Unity.Mathematics;

namespace Data
{
    public struct RandomHolder : IComponentData
    {
        public Random rnd;
        public RandomHolder(uint seed) => rnd = new Random(seed);
        public static implicit operator RandomHolder(Random r) => new RandomHolder(r.state);
        public static implicit operator Random(RandomHolder k) => k.rnd;
    }
}
