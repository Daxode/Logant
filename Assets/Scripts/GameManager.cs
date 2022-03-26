using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine.UIElements;

public partial class GameManager : SystemBase
{
    bool m_ButtonClicked;
    protected override void OnStartRunning()
    {
        var doc = this.GetSingleton<UIStore>().Doc;
        if (doc.rootVisualElement.Q(className: "Switch") is Button button)
        {
            button.text = "Found This Button!";
            button.RegisterCallback<ClickEvent>(ev => m_ButtonClicked = true);
        }
    }
    
    protected override void OnUpdate()
    {
        if (m_ButtonClicked)
        {
            Entities.ForEach((ref PhysicsVelocity vel, in PhysicsMass mass) =>
            {
                vel.ApplyLinearImpulse(in mass, math.up() * 10f);
            }).Run();
            m_ButtonClicked = false;
        }
    }
}