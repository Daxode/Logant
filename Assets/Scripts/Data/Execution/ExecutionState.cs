using System;
using Unity.Entities;
using UnityEngine;

public struct ExecutionState : IComponentData
{
    public ushort id;
    public ushort executionLine;
}