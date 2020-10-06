using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine;
using Valve.Newtonsoft.Json.Bson;

public enum UpAndDownStat
{
    Rise,
    Down,
    DownBeforeSpeedDown,
    SpeedUp,
    SpeedDown,
    Stop,
    SpeedUpToEnd,
    DownBeforeSpeedUpToEnd
};

public enum UpAndDownPattern
{
    NoStartNoEnd,
    StartNoEnd,
    NoStartEnd,
    StartEnd
}


public class PlatformUpAndDown : MonoBehaviour
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

    public float HeightOffset = 0.045f;

    List<Vector4> Data;
    List<Vector2> RecordData;

    public float TimeWindow;

    float timeCount;
    float minHeight;
    float maxHeight;

    float LastRecordTime;
    float LastWriteDataTime;
    float LastClockwiseTime;
    float LastCounterClockwiseTime;
    float LastStopRotateTime;
    float LastZeroTime;

    bool DownTesting;



    public UpAndDownPattern Pattern;
    public UpAndDownStat MovingStat;

    public static PlatformUpAndDown instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
        Data = new List<Vector4>();
        RecordData = new List<Vector2>();
    }

    private void Start()
    {
        MovingStat = UpAndDownStat.Stop;
        Data = new List<Vector4>();
        minHeight = Single.MaxValue;
        maxHeight = Single.MinValue;
        StopSpeedUpSpeed = -0.035f;
        StopSpeedDownSpeed = -0.005f;
        TimeWindow = 0.5f;
        //LastRecordTime = 0;
        //LastWriteDataTime = 0;
        //LastClockwiseTime = 0;
        //LastCounterClockwiseTime = 0;
        //LastStopRotateTime = 0;
        LastZeroTime = 0;
        DownTesting = false;
    }



    private void Update()
    {
        // Fall
        if (ElevatorSpeed < StopSpeedUpSpeed && MovingStat == UpAndDownStat.SpeedUp)
        {
            PlatformController.instance.PlatformRotateStop();
            SpeedUpTime = Time.time - StartSpeedUpTime;
            SpeedUpHeight = ElevatorTop - ElevatorTransform.position.y;
            if (Pattern == UpAndDownPattern.NoStartEnd || Pattern == UpAndDownPattern.StartEnd)
            {
                MovingStat = UpAndDownStat.DownBeforeSpeedUpToEnd;
                Debug.Log($"Stat: DownBeforeSpeedUpToEnd");
            }
            else
            {
                MovingStat = UpAndDownStat.DownBeforeSpeedDown;
                Debug.Log($"Stat: DownBeforeSpeedDown");
            }

            Debug.Log($"Speedup Speed: {StopSpeedUpSpeed}, Elevator Speed: {ElevatorSpeed}");
            Debug.Log($" Elevator Speed: {ElevatorSpeed}, Elevator Go Down Time: {ElevatorGoDownTime}");
            Data.Add(new Vector2(ElevatorSpeed, ElevatorGoDownTime));
            Debug.Log($"Bottom: {ElevatorBottom}, Stop height: {ElevatorStopHeight}");
        }
        
        if (ElevatorTransform.position.y < ElevatorStopHeight && TimeToGround < 2f && MovingStat == UpAndDownStat.DownBeforeSpeedDown)
        {
            {
                SpeedDown();
                MovingStat = UpAndDownStat.SpeedDown;
                Debug.Log($"Stat: SpeedDown");
            }
        }

        if (ElevatorTransform.position.y < ElevatorStopHeight && TimeToGround < 3f && MovingStat == UpAndDownStat.DownBeforeSpeedUpToEnd)
        {
            SpeedUpToEnd();
            MovingStat = UpAndDownStat.SpeedUpToEnd;
            Debug.Log($"Stat: SpeedUpToEnd");
        }


        if (MovingStat == UpAndDownStat.SpeedUpToEnd && ElevatorTransform.position.y < ElevatorBottom)
        {
            PlatformController.instance.PlatformRotateStop();
            PlatformController.instance.PlatformStop();
            MovingStat = UpAndDownStat.Stop;
            Debug.Log($"Stat: Stop");
            DownTesting = false;
        }


        if (ElevatorSpeed > StopSpeedDownSpeed && MovingStat == UpAndDownStat.SpeedDown)
        {
            StopSpeedDown();
            MovingStat = UpAndDownStat.Down;
            Debug.Log($"Stat: Down");
        }

        if (ElevatorTransform.position.y < ElevatorBottom && MovingStat == UpAndDownStat.Down)
        {
            PlatformController.instance.PlatformStop();
            ElevatorGoDownTime = Time.time - StartSpeedUpTime;
            MovingStat = UpAndDownStat.Stop;
            Debug.Log($"Stat: Stop");
            DownTesting = false;
        }

        // Rise
        if (ElevatorTransform.position.y > ElevatorTop && MovingStat == UpAndDownStat.Rise)
        {
            PlatformController.instance.PlatformStop();
            MovingStat = UpAndDownStat.Stop;
            Debug.Log($"Stat: Stop");
        }

        // Speed to zero
        //if (Input.GetKey("z"))
        //{
        //    StartCoroutine(zero());
        //}

        // Write Data
        //if (Input.GetKey("p"))
        //{
        //    if (Time.time - LastWriteDataTime > 1)
        //    {
        //        Debug.Log($"Write data in file.");
        //        WriteData();
        //        LastWriteDataTime = Time.time;
        //    }
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
            ElevatorSpeed = (minHeight - maxHeight) / TimeWindow;
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
        else if (MovingStat == UpAndDownStat.Rise)
        {
            TimeToTop = (ElevatorTop - ElevatorTransform.position.y) / Mathf.Abs(ElevatorSpeed);
            TimeToGround = -1;
        }
        else if (MovingStat == UpAndDownStat.SpeedUp)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == UpAndDownStat.DownBeforeSpeedDown)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == UpAndDownStat.SpeedDown)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == UpAndDownStat.Down)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == UpAndDownStat.DownBeforeSpeedUpToEnd)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }
        else if (MovingStat == UpAndDownStat.SpeedUpToEnd)
        {
            TimeToTop = -1;
            TimeToGround = (ElevatorTransform.position.y - ElevatorBottom) / Mathf.Abs(ElevatorSpeed);
        }

        // Record Transform
        if (DownTesting)
        {
            Data.Add(new Vector4(ElevatorTransform.position.x, ElevatorTransform.position.y, ElevatorTransform.position.z, Time.time));
        }


    }

    void SpeedUpToEnd()
    {
        StartCoroutine(Speeduptoend());
    }
    IEnumerator Speeduptoend()
    {
        Rotate('6');
        yield return new WaitForSeconds(2);
        //無感的話得轉更快，就要改ardiuno code了
        //或是要更早開始加速，不確定要不要減速
    }


    // Initial calibration
    public void CalibrateBottom()
    {
        ElevatorBottom = ElevatorTransform.position.y + 0.01f;
        ElevatorStopHeight = ElevatorBottom + 0.2f;
        Debug.Log($"Bottom: {ElevatorBottom}, Stop height: {ElevatorStopHeight}");
    }

    public void CalibrateTop()
    {
        ElevatorTop = ElevatorTransform.position.y - 0.01f;
        Debug.Log($"Bottom: {ElevatorBottom}, Stop height: {ElevatorStopHeight}");
    }



    public void SpeedToZero()
    { 
        StartCoroutine(zero());
    }



    public void Rotate(char speed)
    {
        PlatformController.instance.PlatformRotatecounterclockwise(speed);
    }

    
    // Four virtual elevator pattern 
    public void NoStartNoEnd()
    {
        Pattern = UpAndDownPattern.NoStartNoEnd;
        UpAndDown();
        DownTesting = true;
    }
    public void NoStartEnd()
    {
        Pattern = UpAndDownPattern.NoStartEnd;
        UpAndDown();
        DownTesting = true;
    }
    public void StartNoEnd()
    {
        Pattern = UpAndDownPattern.StartNoEnd;
        UpAndDown();
        DownTesting = true;
    }
    public void StartEnd()
    {
        Pattern = UpAndDownPattern.StartEnd;
        UpAndDown();
        DownTesting = true;
    }

   





    void SpeedDown()
    {
        Debug.Log($"SpeedDown!");
        PlatformController.instance.PlatformRotateClockwise();
    }

    void StopSpeedDown()
    {
        Debug.Log($"StopSpeedDown!");
        PlatformController.instance.PlatformRotateStop();
    }

    public IEnumerator zero()
    {
        if (Time.time - LastZeroTime > 1)
        {
            LastZeroTime = Time.time;
            Debug.Log($"Zero!");
            PlatformController.instance.PlatformKeepMovingDown();
            MovingStat = UpAndDownStat.DownBeforeSpeedDown;
            yield return new WaitForSeconds(1);
            Debug.Log($"movingstat: {MovingStat}");
            SpeedDown();
            MovingStat = UpAndDownStat.SpeedDown;
            Debug.Log($"movingstat: {MovingStat}");
        }
    }

    public void UpAndDown()
    {
        StartCoroutine(Upanddown());
    }
    public IEnumerator Upanddown()
    {
        if (Pattern == UpAndDownPattern.StartEnd || Pattern == UpAndDownPattern.StartNoEnd)
        {
            PlatformController.instance.PlatformKeepMovingUp();
            yield return new WaitForSeconds(1);
            PlatformController.instance.PlatformStop();
            yield return new WaitForSeconds(2);
        }
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
        yield return new WaitForSeconds(1);
        PlatformController.instance.PlatformRotateKeepRotate();
        //platform speed up
        MovingStat = UpAndDownStat.SpeedUp;
        Debug.Log($"Platform Speed Up");
    }

    public void PlatformUp_T()
    {
        MovingStat = UpAndDownStat.Rise;
        PlatformController.instance.PlatformKeepMovingUp();
    }


    // Recording
    public void RecordTime()
    {
        if (Time.time - LastRecordTime > 1)
        {
            RecordData.Add(new Vector2(ElevatorSpeed, Time.time));
            LastRecordTime = Time.time;
        }
    }


    public void WriteData()
    {
        Debug.Log("Start writing file.");
        DateTime localTime = DateTime.Now;
        string fileName = localTime.ToString("MM dd yy H-mm-ss") + " elevator_log.txt";
        using (StreamWriter file = new StreamWriter(Application.dataPath + "/StreamingAssets/Data/Log/" + fileName))
        {
            file.WriteLine("Routine begin.");
            foreach (Vector4 V in Data)
            {
                file.WriteLine(V);
            }
            file.WriteLine("Stop Time");
            foreach (Vector2 V in RecordData)
            {
                file.WriteLine(V);
            }
            file.WriteLine("Routine end.");
        }
    }




}
