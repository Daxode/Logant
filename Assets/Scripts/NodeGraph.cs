using System.Collections.Generic;
using Data;
using Shapes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using HandleUtility = UnityEditor.HandleUtility;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default|WorldSystemFilterFlags.Editor)]
    public partial class UISystem : SystemBase
    {
        NodeGraph m_Graph;
        EntityToColorSystem m_EntityToColor;
        protected override void OnCreate()
        {
            m_Graph = World.GetExistingSystem<NodeGraph>();
            m_EntityToColor = World.GetExistingSystem<EntityToColorSystem>();
        }
        
        EntityQuery m_NodeQuery;
        NativeArray<Entity> m_NodeEntities;
        void UpdateNodes()
        {
            m_Graph.ClearNodes();
            if (m_NodeEntities.IsCreated)
                m_NodeEntities.Dispose();
            m_NodeEntities = m_NodeQuery.ToEntityArray(Allocator.Persistent);
            Entities.WithStoreEntityQueryInField(ref m_NodeQuery).WithAll<ExecutionLineDataHolder>().ForEach((in Translation t) 
                => m_Graph.AddNode(new Node(t.Value, .7f))).WithoutBurst().Run();
        }
        
        public bool dirty = true;
        protected override void OnUpdate()
        {
            if (dirty) {
                UpdateNodes();
                dirty = false;
            }
            
            // Active Draw path
            var ray = ScreenToRaySystem.ScreenToRay(Mouse.current.position.ReadValue());
            float3 point = ray.GetPoint(-ray.origin.y / ray.direction.y);
            if (Mouse.current.leftButton.wasPressedThisFrame)
                m_Graph.StartNodeDraw(point, (int)PathType.StopPath);
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
                m_Graph.EndNodeDraw(point);
            m_Graph.UpdateNodeDraw(point);

            // Debug Graph Nodes
            if (Mouse.current.rightButton.wasPressedThisFrame)
                foreach (var (fromI, toI, pathType) in m_Graph.Nodes)
                    Debug.Log($"{(PathType)pathType} - From: {m_NodeEntities[fromI]}_{m_EntityToColor.EntityToColor[m_NodeEntities[fromI]]} - To: {m_NodeEntities[toI]}_{m_EntityToColor.EntityToColor[m_NodeEntities[toI]]}");
        }
    }
    
    public enum PathType
    {
        DropPath,
        PickupPath,
        WaitPath,
        StopPath
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
        List<PolylinePath> m_Arrows;
        
        bool m_ActivePathEnabled;
        PolylinePath m_ActivePath;
        NativeList<Node> m_Nodes;
        NativeList<(int, int, int)> m_NodePairs;
        Random random;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ActivePath = new PolylinePath();
            m_Nodes = new NativeList<Node>(100, Allocator.Persistent);
            m_NodePairs = new NativeList<(int, int, int)>(100, Allocator.Persistent);
            m_Arrows = new List<PolylinePath>(100);
            random.InitState();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Nodes.Dispose();
        }

        public NativeArray<(int, int, int)> Nodes => m_NodePairs.AsArray();
        public void ClearNodes() => m_Nodes.Clear();
        public void AddNode(Node node) => m_Nodes.Add(node);

        int? m_ActiveNodeIndex;
        int m_PathID;
        public void StartNodeDraw(float3 position, int pathId)
        {
            m_ActiveNodeIndex = TryGetHitNodeIndex(position);
            m_PathID = pathId;
        }

        public void EndNodeDraw(float3 position)
        {
            var getNode = TryGetHitNodeIndex(position);
            if (getNode.HasValue && getNode != m_ActiveNodeIndex && m_ActiveNodeIndex.HasValue)
            {
                m_NodePairs.Add((m_ActiveNodeIndex.Value, getNode.Value, m_PathID));
                m_Arrows.Add(GetPathForBezier(ref random,m_Nodes[m_ActiveNodeIndex.Value], m_Nodes[getNode.Value]));
            }

            m_ActivePathEnabled = false;
            m_ActiveNodeIndex = null;
        }
        
        static PolylinePath GetPathForBezier(ref Random random, in Node start, in Node end)
        {
            var path = new PolylinePath();
            var startToEnd = end.Position - start.Position;
            var dirStartToEnd = math.normalize(startToEnd);
            var rndDirStart = random.GetFlatDirectionInDirection(dirStartToEnd, .8f);
            var rndDirEnd = random.GetFlatDirectionInDirection(-dirStartToEnd, .8f);

            var pFrom = start.Position + rndDirStart * start.Radius;
            var pTo = end.Position + rndDirEnd * end.Radius;

            // Construct path
            path.AddPoint(pFrom);
            path.BezierTo(pFrom+rndDirStart,pTo+rndDirEnd,pTo,100);
            ThinAtArrowEndPoint(path, pTo);
            
            SetLinearGradientHSV(path, new Color(1f, 0.28f, 0.11f), new Color(1f, 0.85f, 0.08f));
            return path;
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
        
        int? TryGetHitArrowIndex(float3 pos)
        {
            for (var i = 0; i < m_Arrows.Count; i++)
            {
                var bezier = m_Arrows[i];
                var distance = HandleUtility.DistancePointBezier(pos, bezier[0].point, bezier[3].point, bezier[1].point, bezier[2].point);
                if (distance < 0.1f)
                    return i;
            }

            return null;
        }

        public void UpdateNodeDraw(float3 destination)
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

                var endStrength = math.dot(dirActiveToMouse, dirDestToMouse) * .5f + .5f;
                
                // If valid path
                m_ActivePathEnabled = math.any(pFrom != pTo);
                if (!m_ActivePathEnabled) return;
            
                // Construct path
                m_ActivePath.ClearAllPoints();
                m_ActivePath.AddPoint(pFrom);
                m_ActivePath.BezierTo(pFrom+dirActiveToMouse*(1+endStrength),pTo+dirDestToMouse*(1+endStrength*2),pTo,100);
                ThinAtArrowEndPoint(m_ActivePath, pTo);
            }
            else
            {
                var toDest = destination - activeNode.Position;
                var dirToDest = math.normalizesafe(toDest);
                var pTo = activeNode.Position + dirToDest * math.max(activeNode.Radius+0.01f,math.length(toDest));
                var pFrom = activeNode.Position + dirToDest * activeNode.Radius;

                // If valid path
                m_ActivePathEnabled = math.any(pFrom != pTo);
                if (!m_ActivePathEnabled) return;
            
                // Construct path
                m_ActivePath.ClearAllPoints();
                m_ActivePath.AddPoint(pFrom, 1);
                m_ActivePath.LineTo(pTo,100);
                ThinAtArrowEndPoint(m_ActivePath, pTo);
            }
        }

        const float k_ArrowRadius = .3f;
        static void ThinAtArrowEndPoint(PolylinePath path, float3 arrowEndPoint)
        {
            for (var i = path.Count - 1; i >= 0; i--)
            {
                var dist = math.distance(path[i].point, arrowEndPoint);
                path.SetThickness(i, math.clamp(dist * 10, .05f, 1));
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

                foreach (var arrow in m_Arrows)
                {
                    Draw.Polyline(arrow);
                    DrawArrowHead(
                        arrow[arrow.Count-2].point,
                        arrow[arrow.Count-1].point,
                        .15f, k_ArrowRadius, arrow[arrow.Count-1].color);
                }
            }
        }

        #region Utility

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

        #endregion
    }
}
