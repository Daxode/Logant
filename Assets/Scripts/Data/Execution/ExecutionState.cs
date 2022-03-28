using System;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ExecutionState : IComponentData
{
    public short id;
    public short executionLine;
}