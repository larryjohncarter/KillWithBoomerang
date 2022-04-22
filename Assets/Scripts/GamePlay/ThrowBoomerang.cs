using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBoomerang : MonoBehaviour
{
    public GameObject _boomerang;
    public GameObject _fakeBoomerang;
    public GameObject _trail;
    public void ThrowBoomerangAction()
    {
        _boomerang.GetComponent<Boomerang>().TapToThrow();
        _trail.SetActive(true);

    }

    public void CatchBoomerang()
    {
        _fakeBoomerang.GetComponentInChildren<MeshRenderer>().enabled = true; 
        _boomerang.GetComponentInChildren<MeshRenderer>().enabled = false;
        _trail.SetActive(false);
    }
}
