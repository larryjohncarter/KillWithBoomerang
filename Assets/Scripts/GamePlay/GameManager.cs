using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BzKovSoft.CharacterSlicer.Samples;
using System.Linq;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GameManager : SingletonBehaviour<GameManager>
{
    /*  Private Variables
  */
    [Header("Variables")]
    [SerializeField] private bool hasGameStarted = false;
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool hasKilledAll = false;
    [SerializeField] private bool canMoveToNextPlatform;
    [SerializeField] private bool _waterMelonLevel;
    [SerializeField] private bool hitObstacle;
    [SerializeField] private bool requireToSaveFriendlies = false;
    [SerializeField] private bool changingPlatform = false;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private GameObject _platformPanel;
    [SerializeField] private GameObject _boomerang;
    [SerializeField] private GameObject _hands;
    [SerializeField] private GameObject _spawnPoint;
    [SerializeField] private Transform _camera;
    [SerializeField] private bool[] _enemyKilled;
    [SerializeField] private bool[] _friendlySaved;
    [SerializeField] private bool[] _waterMelonSliced;
    [SerializeField] private float platformChangeSpeed;
    [Header("Lists")]
    [SerializeField] private List<CharacterSlicerSampleFast> _enemy;
    [SerializeField] private List<FriendlyNoob> _friendlyNoobs;
    [SerializeField] private List<WaterMelon> _waterMelons;
    [SerializeField] private List<Transform> _levelPlatformSpawns;
    [SerializeField] private List<Transform> _nextPositions;
    [SerializeField] private List<Transform> waypointsParents;
    [SerializeField] private List<Vector3> wayPoint;

    private Animator _handsAnim;
    private Boomerang _Boomerang;
    public int index = 0;
    /*  Public Variables
    */
    public bool HasGameStarted { get { return hasGameStarted; } set { hasGameStarted = value; } }
    public bool IsGameOver { get { return isGameOver; } set { isGameOver = value; } }
    public bool CanMoveToNextPlatform { get { return canMoveToNextPlatform; } set { canMoveToNextPlatform = value; } }
    public bool HitObstacle { get { return hitObstacle; } set { hitObstacle = value; } }

    public bool HasKilledAll { get { return hasKilledAll; } set { hasKilledAll = value; } }
    public bool RequireSavingOfFriendlies { get { return requireToSaveFriendlies; } set { requireToSaveFriendlies = value; } }
    public bool WaterMelonLevel { get { return _waterMelonLevel; } set { _waterMelonLevel = value; } }
    public bool ChangingPlatform { get { return changingPlatform; } set { changingPlatform = value; } }

    public List<CharacterSlicerSampleFast> _Enemy { get { return _enemy; } }
    public int testIndex;
    private void OnEnable()
    {
        _handsAnim = _hands.GetComponentInChildren<Animator>();
    }
    void Start()
    {
        foreach (Transform child in _levelPlatformSpawns[LevelManager.Instance.CurrentLevel])
        {
            _nextPositions.Add(child);
        }
        AddEnemiesToList();
        AddFriendlyNoobsToList();
        AddWaterMelonsToList();
        AddWayPointsToList();
        _waterMelonSliced = new bool[_waterMelons.Count];
        _enemyKilled = new bool[_enemy.Count];
        _friendlySaved = new bool[_friendlyNoobs.Count];
        _Boomerang = _boomerang.GetComponent<Boomerang>();
        if(LevelManager.Instance.CurrentLevel == 3)
        {
            _hands.transform.position = new Vector3(-0.6f, .8f, -17);
        }
        FTemplate.UI.SetProgress(PlayerPrefs.GetInt("Level_No"), 0, true);
        FTemplate.UI.Show(UIElementType.Progressbar);
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && !hasGameStarted && !changingPlatform && !isGameOver && !canMoveToNextPlatform)
        {
            _handsAnim.SetTrigger("Throw");
        }
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    FTemplateNamespace.MultiScreenshotCapture.screenshot = true;
        //}
        CheckIfCharactersDead();
        CheckIfFriendliesAreSaved();
        CheckIfAllEnemiesIsDead();
        CheckIfAllEnemiesAreDeadAndFriendlySaved();
        CheckIfWaterMelonsAreSliced();
    }

    private void AddEnemiesToList()
    {
        GameObject[] g = GameObject.FindGameObjectsWithTag(TagControl.Enemy);
        for (int i = 0; i < g.Length; i++)
        {
            if (g[i].activeInHierarchy)
                _enemy.Add(g[i].GetComponent<CharacterSlicerSampleFast>());

            g[i].transform.parent = null;
        }
    }
    private void AddWayPointsToList()
    {
        GameObject[] g = GameObject.FindGameObjectsWithTag("Waypoint");
        for (int i = 0; i < g.Length; i++)
        {
            if (g[i].activeInHierarchy)
                waypointsParents.Add(g[i].transform);
        }
    }
    private void AddFriendlyNoobsToList()
    {
        GameObject[] g = GameObject.FindGameObjectsWithTag(TagControl.Friendly);
        for (int i = 0; i < g.Length; i++)
        {
            if (g[i].activeInHierarchy)
                _friendlyNoobs.Add(g[i].GetComponent<FriendlyNoob>());

            g[i].transform.parent = null;
        }
        if(_friendlyNoobs.Count == 0)
        {
            requireToSaveFriendlies = false;
        } else
            requireToSaveFriendlies = true;
    }
    private void AddWaterMelonsToList()
    {
        GameObject[] g = GameObject.FindGameObjectsWithTag("Watermelon");
        for (int i = 0; i < g.Length; i++)
        {
            if (g[i].activeInHierarchy)
                _waterMelons.Add(g[i].GetComponent<WaterMelon>());

            g[i].transform.parent = null;
        }
        if(_waterMelons.Count == 0)
        {
            _waterMelonLevel = false;
        } else
            _waterMelonLevel = true;

    }
    private void CheckIfAllEnemiesIsDead()
    {
        if (_enemyKilled.ToList().TrueForAll(x => x) && !isGameOver && !requireToSaveFriendlies)
        {
            hasKilledAll = true;
            isGameOver = true;
            ActivateWinPanel();
        }
    }
    private void CheckIfAllEnemiesAreDeadAndFriendlySaved()
    {
        if(requireToSaveFriendlies)
        {
            if (_friendlySaved.ToList().TrueForAll(x => x) && _enemyKilled.ToList().TrueForAll(x => x) && !isGameOver)
            {
                hasKilledAll = true;
                isGameOver = true;
                ActivateWinPanel();
            }
        }
    }
    private void CheckIfCharactersDead()
    {
        for (int i = 0; i < _enemy.Count; i++)
        {
            if (_enemy[i].IsDead)
            {
                _enemyKilled[i] = true;
            }
        }
    }
    private void CheckIfFriendliesAreSaved()
    {
        if (requireToSaveFriendlies)
        {
            for (int i = 0; i < _friendlyNoobs.Count; i++)
            {
                if (_friendlyNoobs[i].isSaved)
                {
                    _friendlySaved[i] = true;
                }
            }
        }
    }
    private void CheckIfWaterMelonsAreSliced()
    {
        if (_waterMelonLevel)
        {
            for (int i = 0; i < _waterMelons.Count; i++)
            {
                if (_waterMelons[i].isSliced)
                {
                    _waterMelonSliced[i] = true;
                }
            }
        }
    }
    public void ActivateWinPanel()
    {
        FTemplate.UI.SetProgress(PlayerPrefs.GetInt("Level_No"), 3, false);
        _winPanel.SetActive(true);
    }
    public void ActivateLosePanel()
    {
        _losePanel.SetActive(true);
    }
    public void ActivatePlatformPanel()
    {

        _platformPanel.SetActive(true);
    }
    public IEnumerator GoToNextPlatformIEnum()
    {
        if (canMoveToNextPlatform && !isGameOver)
        {
            foreach (Transform child in waypointsParents[index])
            {
                wayPoint.Add(child.transform.position);
            }
            canMoveToNextPlatform = false;
            yield return new WaitForSeconds(1);
            _Boomerang.AirTime = _Boomerang.CoolDown;
            _Boomerang.throwBoomerang = false;
            _boomerang.transform.parent = _hands.transform;
            _hands.transform.DOPath(wayPoint.ToArray(), platformChangeSpeed, PathType.Linear).OnWaypointChange(LookAtWayPoints).OnComplete(() => ResetRotateAndBool()).SetEase(Ease.Linear);
            index++;
            FTemplate.UI.SetProgress(PlayerPrefs.GetInt("Level_No"), index, false);

            yield return new WaitForSeconds(.9f);
            if (wayPoint.Count != 0)
            {
                wayPoint.Clear();
            }
            //_platformPanel.SetActive(false);
        }
    }
    void LookAtWayPoints(int waypointIndex)
    {
        if (waypointIndex < wayPoint.Count - 1)
        {
            DOTween.Kill("LookForward",false);
            _hands.transform.DOLookAt(wayPoint[waypointIndex + 1], 2.5f).SetId("LookForward");
        }
    }
    void ResetRotateAndBool()
    {
        DOTween.Kill("LookForward");
        changingPlatform = false;
        _hands.transform.DORotate(new Vector3(0, 0, 0), .5f);
    }
}