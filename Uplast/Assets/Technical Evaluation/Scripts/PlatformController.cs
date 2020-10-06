using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public static PlatformController instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(instance);
        }

        instance = this;
    }


    public void PlatformUp(){
        StartCoroutine(PlatformManager.SendRequestToServer("Run", "dir=1"));
    }

    public void PlatformKeepMovingUp(){
        Debug.Log($"UP!");
        StartCoroutine(PlatformManager.SendRequestToServer("Run", "dir=2"));
        Debug.Log($"After Up!");
    }

    public void PlatformStop(){
        Debug.Log($"STOP!");
        StartCoroutine(PlatformManager.SendRequestToServer("Run", "dir=0"));
        Debug.Log($"After STOP!");
    }

    public void PlatformDown()
    {
        StartCoroutine(PlatformManager.SendRequestToServer("Run", "dir=-1"));
    }

    public void PlatformKeepMovingDown()
    {
        StartCoroutine(PlatformManager.SendRequestToServer("Run", "dir=-2"));
    }

    public void PlatformRotateStop()
    {
        StartCoroutine(PlatformManager.SendRequestToServer("SetPlatformSpeed", "speed=0"));
    }

    public void PlatformRotateClockwise()
    {
        StartCoroutine(PlatformManager.SendRequestToServer("SetPlatformSpeed", "speed=1"));
    }

    public void PlatformRotateCounterClockwise()
    {
        PlatformRotatecounterclockwise('2');
    }

    public void PlatformRotatecounterclockwise(char Speed)
    {
        string str = "speed=-" + Speed;
        Debug.Log(str);
        StartCoroutine(PlatformManager.SendRequestToServer("SetPlatformSpeed", str));
    }

    public void PlatformRotateKeepRotate()
    {
        StartCoroutine(PlatformManager.SendRequestToServer("SetPlatformSpeed", "speed=2"));
    }
}
