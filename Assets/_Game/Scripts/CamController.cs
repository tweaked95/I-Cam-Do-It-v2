using System;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public GameObject[] cameraList;
    public int currentCamera { get; private set; }

    public event Action<int> OnCameraChanged;

    Camera outputCamera;
    int lastAppliedCamera = -1;

    void Awake()
    {
        outputCamera = GetComponent<Camera>();
    }

    void Start()
    {
        currentCamera = 0;
        for (int i = 0; i < cameraList.Length; i++)
        {
            cameraList[i].SetActive(false);
        }

        ApplyCamera(currentCamera);
        OnCameraChanged?.Invoke(currentCamera);
    }

    public void SwitchToNext()
    {
        if (cameraList.Length == 0)
            return;

        int nextCamera = (currentCamera + 1) % cameraList.Length;
        SetCamera(nextCamera);
    }

    public void SetCamera(int index)
    {
        if (cameraList.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, cameraList.Length - 1);
        if (index == currentCamera && lastAppliedCamera == currentCamera)
            return;

        currentCamera = index;
        ApplyCamera(currentCamera);
        OnCameraChanged?.Invoke(currentCamera);
    }

    void ApplyCamera(int index)
    {
        if (cameraList.Length == 0 || index == lastAppliedCamera)
            return;

        for (int i = 0; i < cameraList.Length; i++)
        {
            cameraList[i].SetActive(i == index);
        }

        if (outputCamera != null)
            outputCamera.orthographic = index == 1;

        lastAppliedCamera = index;
    }
}
