using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using BzKovSoft.CharacterSlicer.Samples;

public class Boomerang : SingletonBehaviour<Boomerang>
{
    [SerializeField] private float _coolDownTime;
    [SerializeField] private float _airTime;
    [Tooltip("Speed of the lookatObject going forward")]
    [SerializeField] private float movementSpeed;
    [Tooltip("Speed of the Boomerang going forward")]
    [SerializeField] private float boomerangSpeed;
    [SerializeField] private Transform _boomerangModel;
    [SerializeField] private CinemachineVirtualCamera vCam;
    [SerializeField] private float rotSpeed;
    [SerializeField] private List<float> _levelBasedCoolDown;
    private bool _isHendl;
    private Vector3 _prevMousePosition;
    private Vector3 prevPos;
    private Vector3 oldPosBeforeComingBack;
    private Vector3 _lookAtObjPrevPos;
    private Rigidbody rb;
    private Tweener _tween;
    private CinemachineTransposer transposer;
    private float fixedDeltaTime;
    private float minTimeScale = .35f;
    private float maxtimeScale = 1;
    private float _time = 0.0f;
    private float myDeltaTime;
    private Animator _handsAnim;
    public bool isReturning;
    public bool throwBoomerang;
    public bool isDragging = false;
    public bool shouldFill;
    public bool shouldSlowDown = false;
    public float EPSILON = 0.1f;
    [Tooltip("Speed of the rotation for the look at target")]
    public float speed;
    public float slowDownFactor = 0.05f;
    public float distance;
    public float speedForTime = 4;
    public GameObject lookAtObject;
    public GameObject coolDownImageGo;
    public GameObject fakeBoomerang;
    public GameObject _brains;
    public Transform _hands;
    public Transform curve_Point;
    public Image coolDownImage;
    public float airTimeAdd;
    public MeshRenderer _boomerangMR;
    public float AirTime { get { return _airTime; } set { _airTime = value; } }
    public float CoolDown { get { return _coolDownTime; } }
    private bool shouldCatch = true;
    public float lerpSpeed;
    public Transform _cameraTransform;
    public float distanceFrom;
    public Vector3 _prevCameraTransform;
    public bool shouldFollowBoomerang;
    public float ySpeed;
    public GameObject _handsGmodel;
    public Vector3 offset;
    public float yOffset;
    public float xOffset;
    public GameObject _boomerangCamera;
    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        myDeltaTime = Time.deltaTime;
        fixedDeltaTime = Time.fixedDeltaTime;
        _handsAnim = _hands.GetComponent<Animator>();
    }
    private void OnDisable()
    {
        if (InputManager.Instance == null)
            return;
        InputManager.Instance.OnPointerUpEvent = null;
        InputManager.Instance.OnDragEvent = null;
        InputManager.Instance.OnPointerDownEvent = null;
    }
    private void Start()
    {
        _coolDownTime = _levelBasedCoolDown[LevelManager.Instance.CurrentLevel];
        _airTime = _levelBasedCoolDown[LevelManager.Instance.CurrentLevel];
    }

    private void Update()
    {
        if (throwBoomerang)
        {
            ForwardMovement();
            CoolDownImageFilling();
        }
        FillAirTime();
        Lose();
        StartCoroutine(WinPosition());
        StartCoroutine(CheckIfThereIsAnythingInfront());
        MakeAirTimeAlwaysEqualToCoolDown();
        ReturnBoomerangToHandWithArc();
        StartCoroutine(MoveToPlatform());
        InputHandler();
        if (GameManager.Instance.HasGameStarted && LevelManager.Instance.showOnce)
        {
            // HideTutorial();
            StartCoroutine(ShowTutorialCoroutine());
        }
        if (shouldFollowBoomerang)
        {
            //   Vector3 test = transform.position - transform.forward * distanceFrom;
            //  _cameraTransform.position = new Vector3(test.x * xOffset, yOffset, test.z);
            //transform.position - transform.forward * distanceFrom;
            //_cameraTransform.LookAt(transform);
            //  Quaternion rotation = Quaternion.LookRotation(transform.position - _cameraTransform.position);
            //  _cameraTransform.rotation = Quaternion.Euler(offset) * rotation;
            _cameraTransform.gameObject.SetActive(false);
            _boomerangCamera.SetActive(true);
        }
        if (GameManager.Instance.ChangingPlatform)
        {
            GetComponent<BoxCollider>().isTrigger = false;
        }
    }
    private void ForwardMovement()
    {
        transform.Translate(0, 0, boomerangSpeed * Time.deltaTime);
        Vector3 rot = Vector3.up * rotSpeed * Time.deltaTime;
        _boomerangModel.Rotate(new Vector3(0,rot.y,0));
    }

    private void InputHandler()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isHendl = true;
            _prevMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && _isHendl)
        {
            Vector2 delta = Input.mousePosition - _prevMousePosition;
            ControlTheBoomerang(delta);
        }
        if (Input.GetMouseButtonUp(0))
        {
            _isHendl = false;
            StartCoroutine(ReturnWhenPointerUp());
        }
    }
    private void HideTutorial()
    {
        float remainingWaitTime = 4f - Time.timeSinceLevelLoad;
        if (remainingWaitTime <= 0f)
        {
            Debug.Log("Test");
            FTemplate.UI.Hide(UIElementType.SwipeTutorial);
            LevelManager.Instance.showOnce = false;
        }
    }
    private IEnumerator ShowTutorialCoroutine()
    {
        FTemplate.UI.Show(UIElementType.SwipeTutorial);

        //while (!Input.GetMouseButtonDown(0))
        //    yield return null;

        // Show tutorial for at least 2 seconds
        float remainingWaitTime = 4f - Time.timeSinceLevelLoad;
        if (remainingWaitTime > 0f)
            yield return BetterWaitForSeconds.WaitRealtime(remainingWaitTime);
        FTemplate.UI.Hide(UIElementType.SwipeTutorial);

        LevelManager.Instance.showOnce = false;
    }
    public void TapToThrow()
    {
        if (GameManager.Instance.IsGameOver)
            return;
        if (!GameManager.Instance.HasGameStarted && !isReturning && !GameManager.Instance.ChangingPlatform && !GameManager.Instance.CanMoveToNextPlatform )
        {
            if (!GetComponent<BoxCollider>().isTrigger)
                GetComponent<BoxCollider>().isTrigger = true;
            _handsGmodel.SetActive(false);
            shouldCatch = true;
            fakeBoomerang.GetComponentInChildren<MeshRenderer>().enabled = false;
            _boomerangMR.enabled = true;
            _time = 0;
            prevPos = transform.position;
            _lookAtObjPrevPos = lookAtObject.transform.position;
            vCam.Follow = transform;
            vCam.LookAt = transform;
            GameManager.Instance.HasGameStarted = true;
            throwBoomerang = true;
            shouldFollowBoomerang = true;
            ActivateCoolDownImage();
            shouldFill = false;

            _boomerangModel.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }
    private void ControlTheBoomerang(Vector2 delta)
    {
        if (GameManager.Instance.IsGameOver)
            return;
        var ratio = delta.x / Screen.width;
        var yRatio = delta.y / Screen.width;
        ratio = SimpleInput.GetAxisRaw("Horizontal");
        yRatio = SimpleInput.GetAxisRaw("Vertical");
      //  _boomerangModel.DOShakeRotation(_airTime, new Vector3(0, 0, 25), 10, 15).SetId("Shake");
        transform.Rotate(ySpeed * Time.deltaTime * -yRatio, speed * Time.deltaTime * ratio, 0);
        isDragging = true;
    }
    private void ReturnBoomerangToHandWithArc()
    {
        if (isReturning)
        {
            if (Time.timeScale != 1)
                Time.timeScale = 1;
            if (_time < 1)
            {
                transform.position = getBQCPoint(_time, oldPosBeforeComingBack, curve_Point.position, prevPos);
                _time += Time.deltaTime;
                _boomerangModel.Rotate(Vector3.up * rotSpeed * Time.deltaTime);
                DOTween.Kill("Shake");
                transform.DOShakeRotation(1,new Vector3(0,0,25), 10, 15);
                if (Vector3.Distance(transform.position, prevPos) < .5f && shouldCatch)
                {
                    shouldCatch = false;
                    _handsAnim.SetTrigger("Catch");
                }
            }
            else
            {
                _boomerangModel.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                isReturning = false;
            }
        }
    }

    public IEnumerator MoveToPlatform()
    {
        if ( !GameManager.Instance.IsGameOver && GameManager.Instance.CanMoveToNextPlatform)
        {
            ReturnPositionsAndRotations();
            shouldFill = true;
            yield return new WaitForSeconds(.5f);
            ReturnBoomerangToHand();
            GameManager.Instance.ChangingPlatform = true;
            yield return new WaitForSeconds(.25f);
            StartCoroutine(GameManager.Instance.GoToNextPlatformIEnum());
        }
    }
    public IEnumerator ReturnWhenPointerUp()
    {
        if(GameManager.Instance.HasGameStarted && throwBoomerang && isDragging && !GameManager.Instance.IsGameOver)
        {
            ReturnPositionsAndRotations();
            shouldFill = true;
            yield return new WaitForSeconds(.5f);
            ReturnBoomerangToHand();
           // yield return new WaitForSeconds(.75f);
        }
    }
    private void ReturnBoomerangToHand()
    {
        if(_airTime > 0 && !GameManager.Instance.HitObstacle )
        {
            oldPosBeforeComingBack = transform.position;
            lookAtObject.transform.position = _lookAtObjPrevPos;
            lookAtObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            _handsGmodel.SetActive(true);
            _boomerangModel.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            throwBoomerang = false;
            rb.isKinematic = true;
            GameManager.Instance.HasGameStarted = false;
            isReturning = true;
        }
    }
    private void ReturnPositionsAndRotations()
    {
        if(_airTime > 0)
        {
            shouldFollowBoomerang = false;
            _cameraTransform.gameObject.SetActive(true);
            _boomerangCamera.SetActive(false);
            isDragging = false;
            //_cameraTransform.localPosition = _prevCameraTransform;
            //_cameraTransform.localRotation = Quaternion.Euler(new Vector3(22.118f, -180f, 0));
            //_cameraTransform.LookAt(null);
        }
    }
    private void ActivateCoolDownImage()
    {
        coolDownImageGo.SetActive(true);
    }
    private void CoolDownImageFilling()
    {
        if (_airTime <= _coolDownTime && _airTime >= 0)
        {
            _airTime -= Time.deltaTime;
            var percent = _airTime / _coolDownTime;
            coolDownImage.fillAmount = Mathf.Lerp(0,1, percent);
        }
    }
    private void FillAirTime()
    {
        if (_airTime <= _coolDownTime && _airTime >= 0 && shouldFill)
        {
            _airTime += Time.deltaTime;
            var percent = _airTime / _coolDownTime;
            coolDownImage.fillAmount = Mathf.Lerp(0,1 ,percent);
            if (_airTime > _coolDownTime)
                _airTime = _coolDownTime;
        }
    }
    private void Lose()
    {
        if (_airTime <= 0 && !GameManager.Instance.IsGameOver)
        {
            LoseCondition();
        }
    }
    private void LoseCondition()
    {
        GameManager.Instance.IsGameOver = true;
        throwBoomerang = false;
        _time = 0;
        rb.isKinematic = false;
        rb.useGravity = true;
        vCam.Follow = null;
        GetComponent<BoxCollider>().isTrigger = false;
        GameManager.Instance.HasGameStarted = false;    
        GameManager.Instance.ActivateLosePanel();
    }
    public IEnumerator WinPosition()
    {
        if(GameManager.Instance.IsGameOver && GameManager.Instance.HasKilledAll)
        {
            yield return new WaitForSeconds(.5f);
            ReturnPositionsAndRotations();
            yield return new WaitForSeconds(.9f);
            ReturnBoomerangToHand();
        }
    }
    //public void Testing()
    //{
    //    StartCoroutine(WinPosition());
    //}
    private IEnumerator CheckIfThereIsAnythingInfront()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position,transform.TransformDirection(Vector3.forward),out hit, distance))
        {
            if (hit.collider.CompareTag("Untagged") && shouldSlowDown)
            {
                Time.timeScale = Mathf.SmoothStep(Time.timeScale, minTimeScale, myDeltaTime * speedForTime);
                Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
                yield return new WaitForSeconds(.5f);
                Time.timeScale = Mathf.SmoothStep(Time.timeScale, maxtimeScale, myDeltaTime * speedForTime);
                Time.fixedDeltaTime = fixedDeltaTime;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //if(other.gameObject.layer == LayerMask.NameToLayer("Head"))
        //{
        //    SpawnBrain(other.transform);
        //}
        if (other.CompareTag(TagControl.Ground) || other.CompareTag(TagControl.Obstacle))
        {
            if (!GameManager.Instance.IsGameOver && !isReturning)
            {
                LoseCondition();
                shouldFollowBoomerang = false;
            }
        }
        if (other.GetComponent<HourGlass>())
        {
            if(_airTime <= _coolDownTime)
                _airTime += other.GetComponent<HourGlass>()._airTimeAdded;
            other.gameObject.SetActive(false);
        }
        if (!GameManager.Instance.IsGameOver && other.transform.root.CompareTag(TagControl.Friendly))
        {
            if(other.transform.root.GetComponent<CharacterSlicerSampleFast>().IsDead)
                LoseCondition();
        }
    }
    private void SpawnBrain(Transform head)
    {
        GameObject go = Instantiate(_brains);
        go.transform.position = head.position;
        go.transform.rotation = Quaternion.identity;

    }
    private void MakeAirTimeAlwaysEqualToCoolDown()
    {
        if (_airTime  > _coolDownTime)
        {
            _airTime = _coolDownTime;
        }
    }
    Vector3 getBQCPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }
}