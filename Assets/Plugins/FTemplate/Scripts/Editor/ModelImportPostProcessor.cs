using UnityEngine;
using UnityEditor;

namespace FTemplateNamespace
{
	public class ModelImportPostProcessor : AssetPostprocessor
	{
		private void OnPostprocessModel( GameObject go )
		{
			FixMeshRendererPropertiesRecursive( go.transform );
		}

		private void FixMeshRendererPropertiesRecursive( Transform obj )
		{
			for( int i = obj.childCount - 1; i >= 0; i-- )
				FixMeshRendererPropertiesRecursive( obj.GetChild( i ) );

			Renderer renderer = obj.GetComponent<Renderer>();
			if( renderer )
			{
				renderer.allowOcclusionWhenDynamic = false;
				renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
				renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

				if( renderer is SkinnedMeshRenderer )
					( (SkinnedMeshRenderer) renderer ).skinnedMotionVectors = false;
			}
		}
	}
}