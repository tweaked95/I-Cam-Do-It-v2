using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    int camValue;
    Camera cam;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        originalPositions = transform.position;
        tempPositionsSide = new Vector3(0, originalPositions.y, originalPositions.z);
        tempPositionsTop = new Vector3(originalPositions.x, 0, originalPositions.z);

        offsetSide = originalPositions - tempPositionsSide;
        offsetTop = originalPositions - tempPositionsTop;

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player");

        var objectRenderer = gameObject.GetComponent<Renderer>();

        int seed = SceneManager.GetActiveScene().buildIndex;

        System.Random random = new System.Random(seed.GetHashCode());
        var initialColor = new Color(
            (float)random.Next(0, 255),
            (float)random.Next(0, 255),
            (float)random.Next(0, 255)
            );
        Color.RGBToHSV(initialColor,out float H, out float S, out float V);
        objectRenderer.material.SetColor("objectColor", Random.ColorHSV(H,H+0.2f,0.9f,1f,0.5f,1f));
    }

    private void Update()
    {
        camValue = cam.GetComponent<CamController>().currentCamera;
        if (camValue == 0)
        {
            ChangeToMain();
        }
        else if (camValue == 1)
        {
            ChangeToSide();
        }
        else if (camValue == 2)
        {
            ChangeToTop();
        }
    }
    public void ChangeToMain()
    {
        offset = -offsetSide; //-offsetTop
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
