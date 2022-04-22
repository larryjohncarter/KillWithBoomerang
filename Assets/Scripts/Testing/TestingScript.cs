using UnityEngine;
using UnityEngine.Events;

public class TestingScript : MonoBehaviour
{
    public GameObject testRope;

    private void FixedUpdate()
    {
        Collider[] hitColliders = Physics.OverlapBox(transform.position,transform.localScale);
        foreach(Collider hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<Boomerang>())
            {
                testRope.SetActive(true);
                transform.parent.gameObject.SetActive(false);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
#endif
}
