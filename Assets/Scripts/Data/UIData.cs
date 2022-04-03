using Unity.Entities;
using UnityEngine;

namespace Data
{
    [GenerateAuthoringComponent]
    public class UIData : IComponentData
    {
        public Color ColorR;
        public Color ColorG;
        public Color ColorB;
        public Texture2D texture;
    }
}
