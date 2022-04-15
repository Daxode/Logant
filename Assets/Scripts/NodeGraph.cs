using Data;
using Shapes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default|WorldSystemFilterFlags.Editor)]
    public partial class UISystem : SystemBase
    {
        NodeGraph m_Graph;
        protected override void OnCreate() => m_Graph = World.GetExistingSystem<NodeGraph>();

        bool notRun = true;
        
        protected override void OnUpdate()
        {
            if (notRun)
            {
                Entities.WithAll<ExecutionLineDataHolder>().ForEach((in Translation t) 
                    => m_Graph.AddNode(new Node(t.Value, .7f))).WithoutBurst().Run();
                notRun = false;
            }
            
            var ray = ScreenToRaySystem.ScreenToRay(Mouse.current.position.ReadValue());
            float3 point = ray.GetPoint(-ray.origin.y / ray.direction.y);
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                Debug.Log("Pressed");
                m_Graph.SetClosestToThisNodeActive(point);
            } else if (Mouse.current.leftButton.wasReleasedThisFrame) {
                Debug.Log("Released");
                m_Graph.ActiveNodeDeactivate();
            }

            m_Graph.DrawToPoint(point);
        }
    }

    public struct Node
    {
        public float3 Position;
        public float Radius;
        public Node(float3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
    
    [WorldSystemFilter(WorldSystemFilterFlags.Default|WorldSystemFilterFlags.Editor)]
    public partial class NodeGraph : SystemBaseDraw
    {
        bool m_ActivePathEnabled;
        PolylinePath m_ActivePath;
        NativeList<Node> m_Nodes;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ActivePath = new PolylinePath();
            m_Nodes = new NativeList<Node>(100, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Nodes.Dispose();
        }

        public void AddNode(Node node) => m_Nodes.Add(node);

        const float k_ArrowRadius = .3f;

        int? m_ActiveNodeIndex;
        public void SetClosestToThisNodeActive(float3 position) => m_ActiveNodeIndex = TryGetHitNodeIndex(position);
        public void ActiveNodeDeactivate()
        {
            m_ActivePathEnabled = false;
            m_ActiveNodeIndex = null;
        }

        int? TryGetHitNodeIndex(float3 position)
        {
            for (var i = 0; i < m_Nodes.Length; i++)
            {
                var node = m_Nodes[i];
                if (math.distancesq(position, node.Position) < node.Radius * node.Radius)
                    return i;
            }
            
            return null;
        }

        public void DrawToPoint(float3 destination)
        {
            // Create Path
            if (!m_ActiveNodeIndex.HasValue) return;
            var activeNode = m_Nodes[m_ActiveNodeIndex.Value];

            var getNode = TryGetHitNodeIndex(destination);
            if (getNode.HasValue && m_ActiveNodeIndex != getNode)
            {
                var activeToMouse = destination - activeNode.Position;
                var dirActiveToMouse = math.normalizesafe(activeToMouse);
                var pFrom = activeNode.Position + dirActiveToMouse * activeNode.Radius;
                
                var destNode = m_Nodes[getNode.Value];
                var destToMouse = destination - destNode.Position;
                var dirDestToMouse = math.normalizesafe(destToMouse);
                var pTo = destNode.Position + dirDestToMouse * destNode.Radius;

                // If valid path
                m_ActivePathEnabled = math.any(pFrom != pTo);
                if (!m_ActivePathEnabled) return;
            
                // Construct path
                m_ActivePath.ClearAllPoints();
                m_ActivePath.AddPoint(pFrom);
                m_ActivePath.BezierTo(pFrom+dirActiveToMouse,pTo+dirDestToMouse,pTo,100);
                ThinAtArrowEndPoint(pTo);
            }
            else
            {
                var toDest = destination - activeNode.Position;
                var dirToDest = math.normalizesafe(toDest);
                var destLengthMinusArrow = math.max(activeNode.Radius+0.01f,math.length(toDest));
                var pTo = activeNode.Position + dirToDest * destLengthMinusArrow;
                var pFrom = activeNode.Position + dirToDest * activeNode.Radius;

                // If valid path
                m_ActivePathEnabled = math.any(pFrom != pTo);
                if (!m_ActivePathEnabled) return;
            
                // Construct path
                m_ActivePath.ClearAllPoints();
                m_ActivePath.AddPoint(pFrom, 1);
                m_ActivePath.LineTo(pTo,100);
                ThinAtArrowEndPoint(pTo);
            }
        }

        void ThinAtArrowEndPoint(float3 arrowEndPoint)
        {
            for (var i = m_ActivePath.Count - 1; i >= 0; i--)
            {
                var dist = math.distance(m_ActivePath[i].point, arrowEndPoint);
                m_ActivePath.SetThickness(i, math.clamp(dist * 10, .05f, 1));
                if (dist > k_ArrowRadius)
                    break;
            }
        }

        protected override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                // Setttings
                Draw.ZTest = CompareFunction.Always;
                Draw.BlendMode = ShapesBlendMode.Transparent;
                Draw.DiscGeometry = DiscGeometry.Flat2D;
                Draw.Thickness = 0.1f;

                // Default constants
                var rotateUp = Quaternion.Euler(90, 0, 0);
                
                // Draw Ring around Nodes
                using (Draw.ColorScope)
                {
                    Draw.Color = new Color(.1f, .1f, .15f, 0.5f);
                    foreach (var node in m_Nodes)
                        Draw.Ring(node.Position, rotateUp, node.Radius);
                }
                
                // Draw Active Arrow Path
                if (m_ActivePathEnabled)
                {
                    SetLinearGradientHSV(m_ActivePath, new Color(0f, 1f, 0.8f), new Color(0f, 0.5f, 1f));

                    Draw.Polyline(m_ActivePath, PolylineJoins.Round);
                    DrawArrowHead(
                        m_ActivePath[m_ActivePath.Count-2].point,
                        m_ActivePath[m_ActivePath.Count-1].point,
                        .15f, k_ArrowRadius, m_ActivePath[m_ActivePath.Count-1].color);
                }
            }
        }

        static void DrawArrowHead(float3 startP, float3 endP, float radius, float length, Color col)
        {
            var dirWithSize = endP - startP;
            var dir = math.normalizesafe(dirWithSize);
            var rotationWithDir = Quaternion.LookRotation(dir);
            var arrowLocation = startP + dir * (math.length(dirWithSize)-length);
            Draw.Cone(arrowLocation, rotationWithDir, radius, length, col);
        }
        
        static float3 RGBToHSV(Color color)
        {
            var blueHSV = float3.zero;
            Color.RGBToHSV(color, out blueHSV.x, out blueHSV.y, out blueHSV.z);
            return blueHSV;
        }

        static void SetLinearGradientHSV(PolylinePath p, Color startColor, Color endColor) => SetLinearGradientHSV(p, RGBToHSV(startColor), RGBToHSV(endColor));
        static void SetLinearGradientHSV(PolylinePath p, float3 startColorHSV, float3 endColorHSV)
        {
            for (var i = 0; i < p.Count; i++)
            {
                var color = math.lerp(startColorHSV, endColorHSV, i / (float) (p.Count - 1));
                p.SetColor(i, Color.HSVToRGB(color.x, color.y, color.z));
            }
        }
    }
}
