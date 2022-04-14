using System;
using Unity.Entities;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering;

#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public abstract partial class SystemBaseDraw : SystemBase {
		protected virtual int layer => int.MaxValue;

		/// <summary>Override this to draw Shapes in immediate mode. This is called once per camera. You can draw using this code: using(Draw.Command(cam)){ // Draw here }</summary>
		/// <param name="cam">The camera that is currently rendering</param>
		protected abstract void DrawShapes(Camera cam);

		protected override void OnUpdate() {}

		void OnCameraPreRender( Camera cam ) {
			switch( cam.cameraType ) {
				case CameraType.Reflection:
					return; // Don't render in reflection probes in case we run this script in the editor
			}
			if((cam.cullingMask & (1 << layer)) == 0 )
				return; // scene & game view cameras should respect culling layer settings if you tell them to

			DrawShapes( cam );
		}

		#if (SHAPES_URP || SHAPES_HDRP)
			#if UNITY_2019_1_OR_NEWER
				protected override void OnCreate() => RenderPipelineManager.beginCameraRendering += DrawShapesSRP;
				protected override void OnDestroy()  => RenderPipelineManager.beginCameraRendering -= DrawShapesSRP;
				void DrawShapesSRP( ScriptableRenderContext ctx, Camera cam ) => OnCameraPreRender( cam );
			#else
				protected override void OnCreate() => Debug.LogWarning( "URP/HDRP immediate mode doesn't really work pre-Unity 2019.1, as there is no OnPreRender or beginCameraRendering callback" );
			#endif
		#else
		protected override void OnCreate() => Camera.onPreRender += OnCameraPreRender;
		protected override void OnDestroy() => Camera.onPreRender -= OnCameraPreRender;
		#endif
	}
}