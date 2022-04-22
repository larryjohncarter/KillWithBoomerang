using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BzKovSoft.CharacterSlicer.Samples;
using UnityEngine.Events;
public class Enemy : MonoBehaviour
{
    bool isDead;
    bool doItonce;
    public UnityEvent _test;
    public Boomerang _boomerang;

    void Start()
    {
        _test.Invoke();
    }

    public void Test()
    {
        if (GetComponent<CharacterSlicerSampleFast>().IsDead && !doItonce)
        {
            isDead = true;
            doItonce = true;
        }

        if (isDead)
        {
            isDead = false;
           // Debug.Log(_boomerang.AirTime);
            if (_boomerang.AirTime <= _boomerang.CoolDown)
                _boomerang.AirTime += 1.5f;
            else if(_boomerang.AirTime >= _boomerang.CoolDown)
            {
                _boomerang.AirTime = _boomerang.CoolDown;
            }
           // Debug.Log(_boomerang.AirTime);

          //  Debug.Log("Airtime added");
        }
    }
}
