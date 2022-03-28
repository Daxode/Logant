using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Serialization.Json;
using UnityEngine;

[GenerateAuthoringComponent]
public struct AntState : IComponentData
{
    public short id;
    public short executionLine;
}

[GenerateAuthoringComponent]
public struct AntRegisters : IComponentData
{
    v128 Registers; //32 1-bit registers
    
    [BurstCompile]
    bool Read(byte index)
    {
        // if (X86.Sse2.IsSse2Supported)
        // {
        //     var bit = X86.Sse2.shu(new v128(1), index); // bit = 1 << index
        //     return X86.Sse2.and_si128() // return bit == Registers & bit
        // } else if (Arm.Neon.IsNeonSupported)
        // {
        // }
        // else throw new InvalidOperationException("Couldn't find valid CPU architecture!");

        if (index > 64)
        {
            var bit = 1Ul << (index-64);
            return bit == (Registers.ULong1 & bit);
        }
        else
        {
            var bit = 1Ul << index;
            return bit == (Registers.ULong0 & bit);
        }
    }
    
    public bool this[byte i] => Read(i);

    [BurstCompile]
    public void Set(byte index)
    {
        if (index > 64)
            Registers.ULong1 |= 1Ul << (index - 64);
        else
            Registers.ULong0 |= 1Ul << index;
    }
    
    [BurstCompile]
    public void Write(byte index, bool val)
    {
        if (index > 64)
            Registers.ULong1 = val 
                ? Registers.ULong1 |  (1Ul << index-64) 
                : Registers.ULong1 &~ (1Ul << index-64);
        else
            Registers.ULong0 = val 
                ? Registers.ULong0 |  (1Ul << index) 
                : Registers.ULong0 &~ (1Ul << index);
    }
}

public struct RegisterIndex : IComponentData
{
    byte m_Index;
    public static implicit operator RegisterIndex(byte i) => new RegisterIndex {m_Index = i};
    public static implicit operator byte(RegisterIndex elem) => elem.m_Index;
}

public struct CopyIndex : IComponentData
{
    public byte From;
    public byte To;
    public CopyIndex(byte from, byte to)
    {
        From = from;
        To = to;
    }
}