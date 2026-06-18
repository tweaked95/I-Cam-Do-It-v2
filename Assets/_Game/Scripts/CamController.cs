using System;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

public class CamController : MonoBehaviour
{
    const int ActivePriority = 10;
    const int InactivePriority = 0;

    public GameObject[] cameraList;
    public float blendDuration = 0.5f;

    [Header("Side View")]
    [SerializeField] Vector3 sideViewOffset = new Vector3(20f, 0f, 0f);
    [SerializeField] float sideOrthographicSize = 14f;

    [Header("Top View")]
    [SerializeField] Vector3 topViewOffset = new Vector3(0f, 16f, 0f);
    [SerializeField] float topOrthographicSize = 20f;

    public int currentCamera { get; private set; }

    public event Action<int> OnCameraChanged;

    CinemachineVirtualCameraBase[] virtualCameras;
    CinemachineBrain brain;
    Camera outputCamera;
    int lastAppliedCamera = -1;

    void Awake()
    {
        outputCamera = GetComponent<Camera>();
        brain = GetComponent<CinemachineBrain>();
        if (brain != null)
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);

        virtualCameras = new CinemachineVirtualCameraBase[cameraList.Length];
        for (int i = 0; i < cameraList.Length; i++)
        {
            virtualCameras[i] = cameraList[i].GetComponent<CinemachineVirtualCameraBase>();
            cameraList[i].SetActive(true);
            ConfigureVirtualCamera(i);
        }

        currentCamera = 0;
        ApplyCamera(currentCamera);
    }

    void Start()
    {
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

    void ConfigureVirtualCamera(int index)
    {
        GameObject cameraObject = cameraList[index];
        CinemachineVirtualCameraBase vcam = virtualCameras[index];
        if (vcam == null)
            return;

        if (index == 0)
            ConfigureMainCamera(cameraObject);
        else if (index == 1)
            ConfigureFixedCamera(cameraObject, vcam, sideViewOffset, sideOrthographicSize);
        else
            ConfigureFixedCamera(cameraObject, vcam, topViewOffset, topOrthographicSize);
    }

    static void ConfigureMainCamera(GameObject cameraObject)
    {
        CinemachineFreeLook freeLook = cameraObject.GetComponent<CinemachineFreeLook>();
        if (freeLook == null)
            return;

        SetLensMode(freeLook, false, 40f);
    }

    void ConfigureFixedCamera(
        GameObject cameraObject,
        CinemachineVirtualCameraBase vcam,
        Vector3 worldOffset,
        float orthographicSize)
    {
        ClearLookAt(vcam);
        DisableFramingTransposers(cameraObject);
        SetupWorldSpaceFollow(cameraObject, worldOffset);
        SetBlendHint(cameraObject, CinemachineCore.BlendHints.SphericalPosition);
        SetLensMode(vcam, true, orthographicSize);
    }

    void ApplyCamera(int index)
    {
        if (cameraList.Length == 0 || index == lastAppliedCamera)
            return;

        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if (virtualCameras[i] != null)
                virtualCameras[i].Priority = i == index ? ActivePriority : InactivePriority;
        }

        if (outputCamera != null)
            outputCamera.orthographic = index == 1 || index == 2;

        lastAppliedCamera = index;
    }

    static void ClearLookAt(CinemachineVirtualCameraBase vcam)
    {
        CinemachineVirtualCamera legacyCamera = vcam as CinemachineVirtualCamera;
        if (legacyCamera != null)
        {
            legacyCamera.m_LookAt = null;
            return;
        }

        CinemachineCamera camera = vcam as CinemachineCamera;
        if (camera != null)
            camera.LookAt = null;
    }

    static void DisableFramingTransposers(GameObject cameraObject)
    {
        CinemachineFramingTransposer[] framingTransposers =
            cameraObject.GetComponentsInChildren<CinemachineFramingTransposer>(true);

        foreach (CinemachineFramingTransposer framingTransposer in framingTransposers)
            framingTransposer.enabled = false;
    }

    static void SetupWorldSpaceFollow(GameObject cameraObject, Vector3 worldOffset)
    {
        foreach (Transform child in cameraObject.GetComponentsInChildren<Transform>(true))
        {
            if (!child.name.Equals("cm", StringComparison.Ordinal))
                continue;

            CinemachineFollow followBody = child.GetComponent<CinemachineFollow>();
            if (followBody == null)
                followBody = child.gameObject.AddComponent<CinemachineFollow>();

            followBody.enabled = true;
            followBody.FollowOffset = worldOffset;
            followBody.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            followBody.TrackerSettings.PositionDamping = Vector3.zero;
        }
    }

    static void SetBlendHint(GameObject cameraObject, CinemachineCore.BlendHints blendHint)
    {
        CinemachineVirtualCamera legacyCamera = cameraObject.GetComponent<CinemachineVirtualCamera>();
        if (legacyCamera != null)
        {
            legacyCamera.BlendHint = blendHint;
            return;
        }

        CinemachineFreeLook freeLook = cameraObject.GetComponent<CinemachineFreeLook>();
        if (freeLook != null)
        {
            freeLook.BlendHint = blendHint;
            return;
        }

        CinemachineCamera camera = cameraObject.GetComponent<CinemachineCamera>();
        if (camera != null)
            camera.BlendHint = blendHint;
    }

    static void SetLensMode(CinemachineVirtualCameraBase vcam, bool orthographic, float orthographicSize)
    {
        LensSettings.OverrideModes mode = orthographic
            ? LensSettings.OverrideModes.Orthographic
            : LensSettings.OverrideModes.Perspective;

        CinemachineVirtualCamera legacyCamera = vcam as CinemachineVirtualCamera;
        if (legacyCamera != null)
        {
            LegacyLensSettings lens = legacyCamera.m_Lens;
            lens.ModeOverride = mode;
            lens.OrthographicSize = orthographicSize;
            legacyCamera.m_Lens = lens;
            return;
        }

        CinemachineFreeLook freeLook = vcam as CinemachineFreeLook;
        if (freeLook != null)
        {
            LegacyLensSettings lens = freeLook.m_Lens;
            lens.ModeOverride = mode;
            lens.OrthographicSize = orthographicSize;
            freeLook.m_Lens = lens;
            return;
        }

        CinemachineCamera camera = vcam as CinemachineCamera;
        if (camera != null)
        {
            LensSettings lens = camera.Lens;
            lens.ModeOverride = mode;
            lens.OrthographicSize = orthographicSize;
            camera.Lens = lens;
        }
    }
}
