using UnityEngine;

public class AnimatorStartTimeRandomizer : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private float speedMin = 1f;
	[SerializeField]
	private float speedMax = 1f;
#pragma warning restore 0649

	private void Start()
	{
		Animator animator = GetComponent<Animator>();
		animator.Play( 0, -1, Random.value );
		animator.speed = Random.Range( speedMin, speedMax );
	}
}