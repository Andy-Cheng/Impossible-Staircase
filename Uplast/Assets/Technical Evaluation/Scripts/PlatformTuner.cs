using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine;

public enum ElevatorMovingStat{
    Rise,
    Down,
    DownBeforeSpeedDown,
    SpeedUp,
    SpeedDown,
    Stop,
};


public class PlatformTuner : MonoBehaviour
{
    public Transform ElevatorTransform;

    [Space]
    public float ElevatorBottom;
    public float ElevatorTop;
    //public float ElevatorTop_R;
    public float ElevatorStopHeight;
    [Space]
    float ElevatorLastY = 0;
    public float ElevatorSpeed;

    public float StartSpeedUpTime;
    public float SpeedUpTime;
    public float SpeedUpHeight;

    public float TimeToTop;
    public float TimeToGround;

    public float ElevatorGoDownTime;
    
    public float StopSpeedUpSpeed;
    public float StopSpeedDownSpeed;

    public float HeightOffset = 0.008f;

    List<Vector2> Data;
    List<float> AccData;

    public float TimeWindow;

    float timeCount;
    float minHeight;
    float maxHeight;

    public ElevatorMovingStat MovingStat;

    public static PlatformTuner instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
        AccData = new List<float>();
    }

    private void Start()
    {
        MovingStat = ElevatorMovingStat.Stop;
        Data = new List<Vector2>();
        minHeight = Single.MaxValue;
        maxHeight = Single.MinValue;
        StopSpeedUpSpeed = -0.03f;
        StopSpeedDownSpeed = -0.005f;
        TimeWindow = 0.5f;
    }
    


    private void Update()
    {
        // Initialize
        //if (Input.GetKey("b"))
        //{
        //    ElevatorBottom = ElevatorTransform.position.y + 0.01f;
        //    ElevatorStopHeight = ElevatorBottom + 0.2f;
        //    Debug.Log($"Bottom: {ElevatorBottom}, Stop height: {ElevatorStopHeight}");
        //}
        //if (Input.GetKey("t"))
        //{


        //}

        // Fall
        if (ElevatorSpeed < StopSpeedUpSpeed && MovingStat == ElevatorMovingStat.SpeedUp)
        {
            PlatformController.instance.PlatformRotateStop();
            SpeedUpTime = Time.time - StartSpeedUpTime;
            SpeedUpHeight = ElevatorTop - ElevatorTransform.position.y;
            MovingStat = ElevatorMovingStat.DownBeforeSpeedDown;

            Debug.Log($"Speedup Speed: {StopSpeedUpSpeed}, Elevator Speed: {ElevatorSpeed}");
            Debug.Log($" Elevator Speed: {ElevatorSpeed}, Elevator Go Down Time: {ElevatorGoDownTime}");
            Data.Add(new Vector2(ElevatorSpeed, ElevatorGoDownTime));
            Debug.Log($"Bottom: {ElevatorBottom}, Stop height: {ElevatorStopHeight}");
        }

        if (ElevatorTransform.position.y < ElevatorStopHeight && TimeToGround < 2f && MovingStat == ElevatorMovingStat.DownBeforeSpeedDown)
        {
            SpeedDown();
            MovingStat = ElevatorMovingStat.SpeedDown;
            Debug.Log($"SpeedDown!");
            Debug.Log($"Height: {ElevatorTransform.position.y}, SpeedDown Height: {ElevatorStopHeight}");
        }

        if (ElevatorSpeed > StopSpeedDownSpeed && MovingStat == ElevatorMovingStat.SpeedDown)
        {
            StopSpeedDown();
            MovingStat = ElevatorMovingStat.Down;
            Debug.Log($"StopSpeedDown!");
            Debug.Log($"SpeedDown Speed: {StopSpeedDownSpeed}, Elevator Speed: {ElevatorSpeed}");
        }

        if (ElevatorTransform.position.y < ElevatorBottom && MovingStat == ElevatorMovingStat.Down)
        {
            PlatformController.instance.PlatformStop();
            ElevatorGoDownTime = Time.time - StartSpeedUpTime;
            MovingStat = ElevatorMovingStat.Stop;
            Debug.Log($"Stop!");
        }

        // Rise
        if (ElevatorTransform.position.y > ElevatorTop && MovingStat == ElevatorMovingStat.Rise)
        {
            PlatformController.instance.PlatformStop();
            MovingStat = ElevatorMovingStat.Stop;
        }

        //// Record acceleration & speed
        //if (Input.GetKey("c"))
        //{
        //    Debug.Log($"Before record");
        //}

        //// Write Data
        //if (Input.GetKey("p"))
        //{
        //    Debug.Log("Write Data in file.");
        //    WirteData();
        //}
    }

    private void FixedUpdate()
    {
        if (timeCount < TimeWindow)
        {
            minHeight = ElevatorTransform.position.y < minHeight ? ElevatorTransform.position.y : minHeight;
            maxHeight = ElevatorTransform.position.y > maxHeight ? ElevatorTransform.position.y : maxHeight;
            timeCount += Time.fixedDeltaTime;
            return;
        }
        else
        {
            //Debug.Log($"Before,{ElevatorSpeed}");
            ElevatorSpeed = (minHeight - maxHeight) / TimeWindow;
            //Debug.Log($"After,{ElecatorSpeed}");
            minHeight = Single.MaxValue;
            maxHeight = Single.MinValue;
            timeCount = 0f;
        }
        if (ElevatorTransform.position.y - ElevatorBottom < 0.02)
        {
            TimeToTop = -1;
            TimeToGround = 0;
        }
        else if (ElevatorTransform.position.y - ElevatorTop > -0.02)
        {
            TimeToTop = 0;
            TimeToGround = -1;
        }
        else if (MovingStat == ElevatorMovingStat.Rise)
        {
            TimeToTop = (ElevatorTop - ElevatorTransform.position.y) / Mathf.Abs(ElevatorSpeed);
            TimeToGround = -1;
        }
        else if (MovingStat == ElevatorMovingStat.SpeedUp)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == ElevatorMovingStat.DownBeforeSpeedDown)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == ElevatorMovingStat.SpeedDown)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == ElevatorMovingStat.Down)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
    }


    public void CalibrateBottom()
    {
        ElevatorBottom = ElevatorTransform.position.y + 0.01f;
        ElevatorStopHeight = ElevatorBottom + 0.2f;
        Debug.Log($"Bottom: {ElevatorBottom}, Stop height: {ElevatorStopHeight}");
    }


    public void Initialize()
    {
        ElevatorTop = ElevatorTransform.position.y - 0.01f;
        //ElevatorBottom = ElevatorTop - 1.39f;
        //ElevatorStopHeight = ElevatorBottom + 0.2f;
        Debug.Log($"Bottom: {ElevatorBottom}, Stop height: {ElevatorStopHeight}");

        LevelLoader.instance.Initialize(ElevatorTop - ElevatorBottom + HeightOffset);
    }


    public void AddData()
    {
        AccData.Add(ElevatorSpeed);
        Debug.Log(AccData.Count);

    }

    void SpeedDown()
    {
        StartCoroutine(speeddown());
    }

    IEnumerator speeddown()
    {
        PlatformController.instance.PlatformRotateClockwise();
        yield return new WaitForSeconds(2);
        PlatformController.instance.PlatformRotateKeepRotate();
    }

    void StopSpeedDown()
    {
        PlatformController.instance.PlatformRotateStop();
    }


    public void UpAndDown()
    {
        StartCoroutine(Upanddown());
    }

    public IEnumerator Upanddown()
    {
        PlatformController.instance.PlatformKeepMovingUp();
        yield return new WaitForSeconds(1);
        PlatformController.instance.PlatformStop();
        yield return new WaitForSeconds(2);
        PlatformToGround();
    }

    public void PlatformToGround()
    {
        Debug.Log($"platform to ground");
        StartCoroutine(Platformtoground());
    }

    public IEnumerator Platformtoground()
    {
        PlatformController.instance.PlatformKeepMovingDown();
        StartSpeedUpTime = Time.time;
        PlatformController.instance.PlatformRotatecounterclockwise('2');
        yield return new WaitForSeconds(2);
        PlatformController.instance.PlatformRotateKeepRotate();
        //platform speed up
        MovingStat = ElevatorMovingStat.SpeedUp;
        Debug.Log($"Platform Speed Up");
    }

    public void PlatformUp_T()
    {
        MovingStat = ElevatorMovingStat.Rise;
        PlatformController.instance.PlatformKeepMovingUp();
    }

    public void WirteData()
    {
        Debug.Log("Start writing file.");
        DateTime localTime = DateTime.Now;
        string fileName = localTime.ToString("MM dd yy H-mm-ss") + " elevator_log.txt";
        using (StreamWriter file = new StreamWriter(Application.dataPath + "/StreamingAssets/Data/Log/" + fileName))
        {
            foreach (float d in AccData)
            {
                file.WriteLine(d);
            }
        }
    }
    
}
