using UnityEngine;

public class MaterialTextureOffsetAnimator : MonoBehaviour
{
#pragma warning disable 0649
	[System.Serializable]
	private struct AnimatedProperty
	{
		public string property;
		public Vector2 scrollSpeed;
	}

	[SerializeField]
	private AnimatedProperty[] properties = new AnimatedProperty[1] { new AnimatedProperty() { property = "_MainTex" } };
#pragma warning restore 0649

	private Material material;

	private int[] propertyIDs;
	private Vector2[] offsets;

	private void Awake()
	{
		if( properties.Length == 0 )
		{
			Destroy( this );
			return;
		}

		material = GetComponent<Renderer>().material;

		propertyIDs = new int[properties.Length];
		offsets = new Vector2[properties.Length];
		for( int i = 0; i < properties.Length; i++ )
			propertyIDs[i] = Shader.PropertyToID( properties[i].property );
	}

	private void Update()
	{
		float dt = Time.deltaTime;
		for( int i = 0; i < properties.Length; i++ )
		{
			Vector2 offset = offsets[i] + dt * properties[i].scrollSpeed;
			material.SetTextureOffset( propertyIDs[i], offset );
			offsets[i] = offset;
		}
	}
}