using UnityEngine;

public class LoadingBar : MonoBehaviour
{
	private readonly Vector3[] rotations = new Vector3[]
	{
		new Vector3( 0f, 0f, 0f ),
		new Vector3( 0f, 0f, 30f ),
		new Vector3( 0f, 0f, 60f ),
		new Vector3( 0f, 0f, 90f ),
		new Vector3( 0f, 0f, 120f ),
		new Vector3( 0f, 0f, 150f ),
		new Vector3( 0f, 0f, 180f ),
		new Vector3( 0f, 0f, 210f ),
		new Vector3( 0f, 0f, 240f ),
		new Vector3( 0f, 0f, 270f ),
		new Vector3( 0f, 0f, 300f ),
		new Vector3( 0f, 0f, 330f )
	};

#pragma warning disable 0649
	[Range( 0.01f, 1f )]
	[SerializeField]
	private float animationDelay = 0.1f;

	[SerializeField]
	private bool clockwise = true;

	[SerializeField]
	private bool jigglyMovement = false;
#pragma warning disable 0649

	private Transform cachedTransform;

	private float nextFrameTime = 0f;
	private int index = 0;

	private void Awake()
	{
		if( jigglyMovement )
			cachedTransform = transform;
		else
			cachedTransform = transform.Find( "Overlay" );

		nextFrameTime = Time.realtimeSinceStartup + animationDelay;
	}

	private void Update()
	{
		float time = Time.realtimeSinceStartup;
		while( time >= nextFrameTime )
		{
			nextFrameTime += animationDelay;

			if( !clockwise )
				index = ( index + 1 ) % rotations.Length;
			else if( --index < 0 )
				index = rotations.Length - 1;

			cachedTransform.localEulerAngles = rotations[index];
		}
	}
}