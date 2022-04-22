using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using com.adjust.sdk;

public class LevelManager : SingletonBehaviour<LevelManager>
{
    private int _currentLevel = 0;
    [SerializeField] private int _targetFrameRate = 60;
    private const string levelPrefsName = "Level_No";
    public int CurrentLevel { get { return _currentLevel; } }
    [SerializeField] private GameObject _tapToThrow;
    [SerializeField] private List<GameObject> _levels;
    public bool showOnce = true;
    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = _targetFrameRate;
        _currentLevel = PlayerPrefs.GetInt(levelPrefsName, 0);
        _currentLevel %= _levels.Count;
        for (int i = 0; i < _levels.Count; i++)
        {
            if (_levels[i] != _levels[_currentLevel])
            {
                _levels[i].SetActive(false);
            }
        }
        _levels[_currentLevel].SetActive(true);
        Time.timeScale = 1;
        
    }

    void Start()
    {
       // FTemplate.UI.Show(UIElementType.FPSCounter);
        FB.LogAppEvent("level_started", parameters: GetFacebookParams());

#if UNITY_IOS
        /* Mandatory - set your iOS app token here */
        InitAdjust("YOUR_IOS_APP_TOKEN_HERE");
#elif UNITY_ANDROID
        /* Mandatory - set your Android app token here */
        InitAdjust("nibgaj32c0lc");
#endif
    }

    void Update()
    {
        if (GameManager.Instance.HasGameStarted)
        {
            _tapToThrow.SetActive(false);
            //if (showOnce)
            //    FTemplate.UI.Show(UIElementType.SwipeTutorial);
        }
    }
    public void OnClick_ContinueLevel()
    {
        _currentLevel += 1;
        FB.LogAppEvent("level_complete", parameters: GetFacebookParams());
        PlayerPrefs.SetInt(levelPrefsName, PlayerPrefs.GetInt(levelPrefsName) + 1);
        SceneManager.LoadScene(0);
    }
    //public void OnClick_ContinuePlatform()
    //{
    //    StartCoroutine(GameManager.Instance.GoToNextPlatformIEnum());
    //    StartCoroutine(Boomerang.Instance.TestOne());
    //}
    public void OnClick_FailedLevel()
    {
        FB.LogAppEvent("level_failed", parameters: GetFacebookParamsForFail());
        SceneManager.LoadScene(0);
    }
    private Dictionary<string, object> GetFacebookParams()
    {
        return new Dictionary<string, object>()
    {
        { AppEventParameterName.Level, PlayerPrefs.GetInt(levelPrefsName) },
    };
    }
    private Dictionary<string, object> GetFacebookParamsForFail()
    {
        return new Dictionary<string, object>()
    {
        { AppEventParameterName.Level, PlayerPrefs.GetInt(levelPrefsName) },
        { "PlatformIndex", GameManager.Instance.index }
    };
    }
    private void InitAdjust(string adjustAppToken)
    {
        var adjustConfig = new AdjustConfig(
            adjustAppToken,
            AdjustEnvironment.Production, // AdjustEnvironment.Sandbox to test in dashboard
            true
        );
        adjustConfig.setLogLevel(AdjustLogLevel.Info); // AdjustLogLevel.Suppress to disable logs
        adjustConfig.setSendInBackground(true);
        new GameObject("Adjust").AddComponent<Adjust>(); // do not remove or rename

        // Adjust.addSessionCallbackParameter("foo", "bar"); // if requested to set session-level parameters

        //adjustConfig.setAttributionChangedDelegate((adjustAttribution) => {
        //  Debug.LogFormat("Adjust Attribution Callback: ", adjustAttribution.trackerName);
        //});

        Adjust.start(adjustConfig);

    }

}
