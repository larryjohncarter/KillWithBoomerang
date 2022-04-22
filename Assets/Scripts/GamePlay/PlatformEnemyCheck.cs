using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BzKovSoft.CharacterSlicer.Samples;
using System.Linq;
using BzKovSoft.ObjectSlicer.Samples;

public class PlatformEnemyCheck : MonoBehaviour
{
    [SerializeField] private List<CharacterSlicerSampleFast> enemiesInPlatform;
    [SerializeField] private bool[] _enemyKilled;
    [SerializeField] private List<FriendlyNoob> _friendlyNoobs;
    [SerializeField] private bool[] _friendlySaved;
    [SerializeField] private List<WaterMelon> _waterMelon;
    [SerializeField] private bool[] _waterMelonSliced;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if(child.gameObject.CompareTag(TagControl.Enemy))
             enemiesInPlatform.Add(child.GetComponent<CharacterSlicerSampleFast>());
            if (child.gameObject.CompareTag(TagControl.Friendly))
                _friendlyNoobs.Add(child.GetComponent<FriendlyNoob>());
            if (child.gameObject.CompareTag("Watermelon"))
            {
                GameManager.Instance.WaterMelonLevel = true;
                _waterMelon.Add(child.GetComponent<WaterMelon>());
            }
        }
        _enemyKilled = new bool[enemiesInPlatform.Count];
        _friendlySaved = new bool[_friendlyNoobs.Count];
        _waterMelonSliced = new bool[_waterMelon.Count];
    }
    // Update is called once per frame
    void Update()
    {
        CheckIfEnemiesOnPlatformDead();
    }
    void CheckIfEnemiesOnPlatformDead()
    {
        for (int i = 0; i < enemiesInPlatform.Count; i++)
        {
            if (enemiesInPlatform[i].IsDead)
            {
                _enemyKilled[i] = true;
            }
        }
        if (GameManager.Instance.RequireSavingOfFriendlies)
        {
            for (int j = 0; j < _friendlyNoobs.Count; j++)
            {
                if (_friendlyNoobs[j].isSaved)
                {
                    _friendlySaved[j] = true;
                }
            }
        }
        if (GameManager.Instance.WaterMelonLevel)
        {
            for (int k = 0; k < _waterMelon.Count; k++)
            {
                if (_waterMelon[k].isSliced)
                {
                    _waterMelonSliced[k] = true;
                }
            }
        }
        IfAllEnemiesDeadMoveToNextPlatform();
    }
    public void IfAllEnemiesDeadMoveToNextPlatform()
    {
        if (GameManager.Instance.IsGameOver)
            return;
        if (_enemyKilled.ToList().TrueForAll(x => x) && !GameManager.Instance.WaterMelonLevel && _friendlyNoobs.Count == 0)
        {
          //  GameManager.Instance.ActivatePlatformPanel();
            GameManager.Instance.CanMoveToNextPlatform = true;
            GetComponent<PlatformEnemyCheck>().enabled = false;
        } 
        if (GameManager.Instance.RequireSavingOfFriendlies)
        {
            if(_enemyKilled.ToList().TrueForAll(x => x) && _friendlySaved.ToList().TrueForAll(x => x) && _friendlyNoobs.Count > 0)
            {
              //  GameManager.Instance.ActivatePlatformPanel();
                GameManager.Instance.CanMoveToNextPlatform = true;
                GetComponent<PlatformEnemyCheck>().enabled = false;
            }
        }
        if (GameManager.Instance.WaterMelonLevel)
        {
          if(_waterMelonSliced.ToList().TrueForAll(x => x)&& _enemyKilled.ToList().TrueForAll(x => x) && _friendlySaved.ToList().TrueForAll(x => x))
            {
              //  GameManager.Instance.ActivatePlatformPanel();
                GameManager.Instance.CanMoveToNextPlatform = true;
                GetComponent<PlatformEnemyCheck>().enabled = false;
            }
        }
    }
}