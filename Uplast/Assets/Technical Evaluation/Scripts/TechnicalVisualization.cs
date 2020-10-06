using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechnicalVisualization : MonoBehaviour
{

    public static TechnicalVisualization instance;
    public VirtualElevator virtualLiftingPlatformManager;

    bool HaveEnterElevator;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnEnterElevator()
    {
        Debug.Log("Enter elevator");
        if (HaveEnterElevator) return;
        virtualLiftingPlatformManager.OnEnter();
        LevelLoader.instance.GoNext();
        PlatformUpAndDown.instance.UpAndDown();
        LevelLoader.instance.OnElevatorGoDown();
        HaveEnterElevator = true;

    }



}
