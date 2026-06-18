using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectController : MonoBehaviour
{
    Vector3 originalPositions;
    Vector3 tempPositionsTop;
    Vector3 tempPositionsSide;

    public Vector3 offset;
    Vector3 offsetTop;
    Vector3 offsetSide;

    CamController camController;

    void Start()
    {
        originalPositions = transform.position;
        tempPositionsSide = new Vector3(0f, originalPositions.y, originalPositions.z);
        tempPositionsTop = new Vector3(originalPositions.x, 0f, originalPositions.z);

        offsetSide = originalPositions - tempPositionsSide;
        offsetTop = originalPositions - tempPositionsTop;

        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        camController = mainCamera.GetComponent<CamController>();
        camController.OnCameraChanged += ApplyCameraMode;
        ApplyCameraMode(camController.currentCamera);

        var objectRenderer = GetComponent<Renderer>();
        int seed = SceneManager.GetActiveScene().buildIndex;

        System.Random random = new System.Random(seed.GetHashCode());
        var initialColor = new Color(
            (float)random.Next(0, 255),
            (float)random.Next(0, 255),
            (float)random.Next(0, 255));

        Color.RGBToHSV(initialColor, out float h, out float s, out float v);
        objectRenderer.material.SetColor("objectColor", Random.ColorHSV(h, h + 0.2f, 0.9f, 1f, 0.5f, 1f));
    }

    void OnDestroy()
    {
        if (camController != null)
            camController.OnCameraChanged -= ApplyCameraMode;
    }

    void ApplyCameraMode(int cameraIndex)
    {
        switch (cameraIndex)
        {
            case 0:
                ChangeToMain();
                break;
            case 1:
                ChangeToSide();
                break;
            case 2:
                ChangeToTop();
                break;
        }
    }

    public void ChangeToMain()
    {
        offset = -offsetSide;
        transform.position = originalPositions;
    }

    public void ChangeToSide()
    {
        offset = offsetSide - offsetTop;
        transform.position = tempPositionsSide;
    }

    public void ChangeToTop()
    {
        offset = offsetTop;
        transform.position = tempPositionsTop;
    }
}
