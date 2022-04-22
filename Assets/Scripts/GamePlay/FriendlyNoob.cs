using BzKovSoft.ObjectSlicer.Samples;
using System.Collections;
using UnityEngine;

public class FriendlyNoob : MonoBehaviour
{
    public bool isSaved;
    public Rigidbody rb;
    public BoxCollider cc;
    public GameObject charReference;
    public ObjectSlicerSample ropeSliced;
    public Rigidbody rope;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        StartCoroutine(CheckIfRopeIsSliced());
    }
    private IEnumerator CheckIfRopeIsSliced()
    {
        if (ropeSliced.isSliced)
        {
            anim.enabled = false;
            rope.isKinematic = false;
            rope.useGravity = true;
            isSaved = true;
        }
        if (isSaved)
        {
            charReference.SetActive(false);
            rb.isKinematic = false;
            rb.useGravity = true;
            yield return new WaitForSeconds(.5f);
            //Dissolve olucak
            rope.gameObject.SetActive(false);
        }
    }
}
