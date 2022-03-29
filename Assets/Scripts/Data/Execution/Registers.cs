using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace Data
{
    public struct Registers : IComponentData
    {
        /// <summary>
        /// 128x 1-bit registers
        /// </summary>
        v128 m_Registers;
    
        
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
                var bit = 1Ul << (index - 64);
                return bit == (m_Registers.ULong1 & bit);
            }
            else
            {
                var bit = 1Ul << index;
                return bit == (m_Registers.ULong0 & bit);
            }
        }
        
        public uint Read(byte index, byte count)
        {
            var reg = new Registers();
            for (byte writeIndex = 0; writeIndex < count; writeIndex++) 
                reg.Write(writeIndex, Read((byte)(count+writeIndex)));
            return reg.m_Registers.UInt0;
        }
        
        public void Write(byte index, uint val, byte count)
        {
            var reg = new Registers();
            reg.m_Registers.UInt0 = val;
            for (byte readIndex = 0; readIndex < count; readIndex++) 
                Write((byte)(index+readIndex), reg.Read(readIndex));
        }
    
        public bool this[byte i] => Read(i);
    
        
        public void Set(byte index)
        {
            if (index > 64) {
                m_Registers.ULong1 = m_Registers.ULong1 | 1Ul << (index - 64);   
            } else {
                m_Registers.ULong0 = m_Registers.ULong0 | (1Ul << index);
            }
        }
    
        
        public void Write(byte index, bool val)
        {
            if (index > 64)
                m_Registers.ULong1 = val
                    ? m_Registers.ULong1 | (1Ul << index - 64)
                    : m_Registers.ULong1 & ~ (1Ul << index - 64);
            else
                m_Registers.ULong0 = val
                    ? m_Registers.ULong0 | (1Ul << index)
                    : m_Registers.ULong0 & ~ (1Ul << index);
        }
    }
    
    public struct RegisterIndex : IComponentData
    {
        byte m_Index;

        public RegisterIndex(byte index) => m_Index = index;
        public static implicit operator RegisterIndex(byte i) => new RegisterIndex {m_Index = i};
        public static implicit operator byte(RegisterIndex elem) => elem.m_Index;
    }
    
    [InternalBufferCapacity(2)]
    public struct RegisterIndexElement : IBufferElementData
    {
        byte m_Index;
        public RegisterIndexElement(byte index) => m_Index = index;
        public static implicit operator RegisterIndexElement(byte i) => new RegisterIndexElement {m_Index = i};
        public static implicit operator byte(RegisterIndexElement elem) => elem.m_Index;
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
    
    public struct Count : IComponentData
    {
        byte m_Count;

        public Count(byte count) => m_Count = count;
        public static implicit operator Count(byte count) => new Count(count);
        public static implicit operator byte(Count elem) => elem.m_Count;
    }
}
