using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public static class meth
{
    // Returns the angle in degrees between /from/ and /to/. This is always the smallest
    [BurstCompile]
    static float Angle(float3 from, float3 to)
    {
        var denominator = math.sqrt(math.lengthsq(from) * math.lengthsq(to));
        if (denominator < 1e-15F)
            return 0F;

        var dot = math.clamp(math.dot(from, to) / denominator, -1F, 1F);
        return math.degrees(math.acos(dot));
    }

    // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
    // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
    // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
    [BurstCompile]
    public static float SignedAngle(float3 from, float3 to, float3 axis)
    {
        var unsignedAngle = Angle(from, to);
        var sign = math.sign(math.dot(math.cross(from, to), axis));
        return unsignedAngle * sign;
    }
    
    /// <summary>
    /// Find the roots of a cubic bezier curve in order to find minimum and maximum
    /// </summary>
    [BurstCompile]
    public static FixedList64Bytes<float> FindRoots(float2 p0, float2 p1, float2 p2, float2 p3) {
        var roots = new FixedList64Bytes<float>();

        var a = 3 * (-p0 + 3*p1 - 3*p2 + p3);
        var b = 6 * (p0 - 2*p1 + p2);
        var c = 3 * (p1 - p0);

        // along x
        float discriminantX = b.x * b.x - 4 * a.x * c.x;
        if (discriminantX < 0) {
            // No roots
        } else if (discriminantX == 0) {
            // one real root
            var rootx = (-b.x) / (2 * a.x);
            if (rootx >=0 && rootx <= 1) {
                roots.Add(rootx);
            }
        } else if (discriminantX > 0) {
            // Two real roots
            var rootx1 = (-b.x + math.sqrt(discriminantX)) / (2 * a.x);
            var rootx2 = (-b.x - math.sqrt(discriminantX)) / (2 * a.x);
            if (rootx1 >=0 && rootx1 <= 1) {
                roots.Add(rootx1);
            }
            if (rootx2 >=0 && rootx2 <= 1) {
                roots.Add(rootx2);
            }
        }

        // along y
        var discriminantY = b.y * b.y - 4 * a.y * c.y;
        if (discriminantY < 0) {
            // No roots
        } else if (discriminantY == 0) {
            // one real root
            var rooty = (-b.y) / (2 * a.y);
            if (rooty >=0 && rooty <= 1) {
                roots.Add(rooty);
            }
        } else if (discriminantY > 0) {
            // Two real roots
            var rooty1 = (-b.y + math.sqrt(discriminantY)) / (2 * a.y);
            var rooty2 = (-b.y - math.sqrt(discriminantY)) / (2 * a.y);
            if (rooty1 >=0 && rooty1 <= 1) {
                roots.Add(rooty1);
            }
            if (rooty2 >=0 && rooty2 <= 1) {
                roots.Add(rooty2);
            }
        }

        return roots;
    }
    
    [BurstCompile]
    public static float3 GetDirectionInDirection(this ref Random random, float3 dir, float angle)
    {
        var rot = quaternion.AxisAngle(random.NextFloat3Direction(), angle);
        return  math.mul(rot, dir);
    }

    [BurstCompile]
    public static float3 GetFlatDirectionInDirection(this ref Random random, float3 dir, float angle)
    {
        var rot = quaternion.AxisAngle(math.up(), random.NextFloat(-angle,angle));
        return  math.mul(rot, dir);
    }
}