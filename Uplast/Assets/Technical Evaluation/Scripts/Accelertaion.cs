using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine;

public class Accelertaion : MonoBehaviour
{
    public Transform ElevatorTransform;
    public float ElevatorSpeed;

    public float TimeWindow;

    float timeCount;
    float minHeight;
    float maxHeight;


    List<Vector4> Data;
    List<Vector2> RecordData;
    float LastWriteDataTime;
    float LastRecordTime;
    float StartTime;

    bool DownTesting;

    public static Accelertaion instance;

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

    // Start is called before the first frame update
    void Start()
    {
        //TimeWindow = 0.5f;
        LastWriteDataTime = 0;
        LastRecordTime = 0;
        StartTime = 0;
    }

    void Update()
    {
        // Record Time
        if (Input.GetKey("r"))
        {
            if (Time.time - LastRecordTime > 1)
            {
                Debug.Log($"Record Time.");
                LastRecordTime = Time.time;
                RecordData.Add(new Vector2(ElevatorSpeed, Time.time - StartTime));
                PlatformController.instance.PlatformRotateStop();
                PlatformController.instance.PlatformStop();
                DownTesting = false;
            }
        }
    }

    private void FixedUpdate()
    {
        // Record Transform
        if (DownTesting)
        {
            Data.Add(new Vector4(ElevatorTransform.position.x, ElevatorTransform.position.y, ElevatorTransform.position.z, (Time.time - StartTime)));
        }

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

        
    }

    public void AccDown_1()
    {
        DownTesting = true;
        StartTime = Time.time;
        PlatformController.instance.PlatformKeepMovingDown();
        SpeedUp('2');
    }
    public void AccDown_2()
    {
        DownTesting = true;
        StartTime = Time.time;
        PlatformController.instance.PlatformKeepMovingDown();
        SpeedUp('3');
    }
    public void AccDown_3()
    {
        DownTesting = true;
        StartTime = Time.time;
        PlatformController.instance.PlatformKeepMovingDown();
        SpeedUp('6');
    }
    public void EqualAccDown()
    {
        DownTesting = true;
        StartTime = Time.time;
        StartCoroutine(Equalaccdown());
    }

    public IEnumerator Equalaccdown()
    {
        Debug.Log($"InDown4!");
        PlatformController.instance.PlatformKeepMovingDown();
        SpeedUp('1');
        yield return new WaitForSeconds(1);
        PlatformController.instance.PlatformRotateKeepRotate();
    }






    public void SpeedUp(char speed)
    {
        PlatformController.instance.PlatformRotatecounterclockwise(speed);
    }
 


    public void WriteData()
    {
        Debug.Log("Start writing file.");
        DateTime localTime = DateTime.Now;
        string fileName = localTime.ToString("MM dd yy H-mm-ss") + " elevator_log.csv";
        using (StreamWriter file = new StreamWriter(Application.dataPath + "/StreamingAssets/Data/Log/technical/" + fileName))
        {
            file.WriteLine("Stop Time");
            foreach (Vector2 V in RecordData)
            {
                file.Write(V.x.ToString("f6"));
                file.Write(",");
                file.WriteLine(V.y.ToString("f6"));
            }
            file.WriteLine("Routine begin.");
            foreach (Vector4 V in Data)
            {
                file.Write(V.x.ToString("f6"));
                file.Write(",");
                file.Write(V.y.ToString("f6"));
                file.Write(",");
                file.Write(V.z.ToString("f6"));
                file.Write(",");
                file.WriteLine(V.w.ToString("f6"));
            }
            Data.Clear();
            RecordData.Clear();
        }
    }

    public void WriteTestData()
    {
        Debug.Log("Start writing file.");
        DateTime localTime = DateTime.Now;
        string fileName = localTime.ToString("MM dd yy H-mm-ss") + " elevator_log.csv";
        using (StreamWriter file = new StreamWriter(Application.dataPath + "/StreamingAssets/Data/Log/technical_test/" + fileName))
        {
            file.WriteLine("Stop Time");
            foreach (Vector2 V in RecordData)
            {
                file.Write(V.x.ToString("f6"));
                file.Write(",");
                file.WriteLine(V.y.ToString("f6"));
            }
            file.WriteLine("Routine begin.");
            foreach (Vector4 V in Data)
            {
                file.Write(V.x.ToString("f6"));
                file.Write(",");
                file.Write(V.y.ToString("f6"));
                file.Write(",");
                file.Write(V.z.ToString("f6"));
                file.Write(",");
                file.WriteLine(V.w.ToString("f6"));
            }
            Data.Clear();
            RecordData.Clear();
        }
    }


}
