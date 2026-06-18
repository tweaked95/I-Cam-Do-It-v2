using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;
    [SerializeField] float switchCooldown = 0.5f;

    public static int lives;

    public Camera cam;
    public GameObject youWinScreen;
    public SceneController sceneCont;

    CamController camController;
    Rigidbody rb;

    RaycastHit hit;
    bool valueChanging;
    Vector2 moveInput;
    GameObject offsetObject;

    void Start()
    {
        camController = cam.GetComponent<CamController>();
        rb = GetComponent<Rigidbody>();

        lives = 3;
        Cursor.visible = false;
    }

    void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Tab) && !valueChanging)
            TrySwitchCamera();
    }

    void FixedUpdate()
    {
        GroundCheck();
        Move();
        DeathCheck();
    }

    void TrySwitchCamera()
    {
        if (offsetObject == null || !offsetObject.CompareTag("Ground"))
            return;

        ObjectController objectController = offsetObject.GetComponent<ObjectController>();
        if (objectController == null)
            return;

        valueChanging = true;
        transform.position += objectController.offset;
        camController.SwitchToNext();
        StartCoroutine(SwitchCooldownRoutine());
    }

    void GroundCheck()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, 5f))
            return;

        if (hit.collider.CompareTag("KillVolume"))
        {
            transform.position = new Vector3(0f, 3f, 0f);
            lives--;
        }
        else if (hit.collider.CompareTag("Ground"))
        {
            offsetObject = hit.collider.gameObject;
        }
    }

    void Move()
    {
        Vector3 moveDirection = GetMoveDirection(camController.currentCamera, cam.transform, moveInput);
        ApplyMovement(moveDirection);
    }

    static Vector3 GetMoveDirection(int cameraIndex, Transform cameraTransform, Vector2 input)
    {
        Vector3 right = cameraTransform.right;
        Vector3 forward = cameraIndex == 2 ? cameraTransform.up : cameraTransform.forward;

        Vector3 direction = cameraIndex == 1
            ? right * input.x
            : right * input.x + forward * input.y;

        direction.y = 0f;
        return Vector3.ClampMagnitude(direction, 1f);
    }

    void ApplyMovement(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero)
            return;

        Vector3 delta = moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;

        if (rb != null)
            rb.MovePosition(rb.position + delta);
        else
            transform.position += delta;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime);
    }

    void DeathCheck()
    {
        if (lives == 0)
            SceneManager.LoadScene("EndGame");
    }

    IEnumerator SwitchCooldownRoutine()
    {
        yield return new WaitForSeconds(switchCooldown);
        valueChanging = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("FinalPickup"))
            return;

        if (youWinScreen != null)
            youWinScreen.SetActive(true);

        sceneCont.ChangeScene();
        transform.position = new Vector3(0f, 1f, 0f);
    }
}
