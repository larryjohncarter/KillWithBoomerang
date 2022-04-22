using UnityEngine;
using BzKovSoft.ObjectSlicer.Samples;

public class WaterMelon : MonoBehaviour
{
    [SerializeField] private ObjectSlicerSample _waterMelon;
    public bool isSliced;

    private void Awake()
    {
        _waterMelon = GetComponent<ObjectSlicerSample>();
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_waterMelon.isSliced)
        {
            isSliced = true;
        }
    }

}
