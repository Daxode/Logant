using Unity.Physics;
using UnityEngine;
using Unity.Mathematics;

public static class meth
{
    // Returns the angle in degrees between /from/ and /to/. This is always the smallest
    public static float Angle(float3 from, float3 to)
    {
        // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
        var denominator = math.sqrt(math.lengthsq(from) * math.lengthsq(to));
        if (denominator < 1e-15F)
            return 0F;

        var dot = math.clamp(math.dot(from, to) / denominator, -1F, 1F);
        return math.degrees(math.acos(dot));
    }

    // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
    // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
    // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
    public static float SignedAngle(float3 from, float3 to, float3 axis)
    {
        var unsignedAngle = Angle(from, to);
        var sign = math.sign(math.dot(math.cross(from, to), axis));
        return unsignedAngle * sign;
    }
}