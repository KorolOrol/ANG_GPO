using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float verticalRotationLimit = 80f;

    [Header("Movement Settings")]
    public float movementSpeed = 100f;
    public float verticalMovementSpeed = 100f;

    private float verticalRotation = 5f;
    private bool isCursorLocked = true;

    public Camera CityCamera;

    void Start()
    {
    }

    void Update()
    {
        if (CityCamera.enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            HandleMouseLook();
            HandleMovement();
            HandleCursorLock();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMouseLook()
    {
        // Получаем движение мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Применяем вращение по горизонтали к самой камере
        transform.Rotate(Vector3.up, mouseX);

        // Применяем вращение по вертикали
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);

        // Устанавливаем вращение по вертикали
        transform.localRotation = Quaternion.Euler(verticalRotation, transform.localEulerAngles.y, 0f);
    }

    void HandleMovement()
    {
        // Определение скорости движения (быстрая при зажатом Shift)
        float currentSpeed = movementSpeed;

        // Перемещение вперед/назад
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * currentSpeed * Time.deltaTime;
        }

        // Перемещение влево/вправо
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * currentSpeed * Time.deltaTime;
        }

        // Вертикальное перемещение
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.up * verticalMovementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.position -= transform.up * verticalMovementSpeed * Time.deltaTime;
        }
    }

    void HandleCursorLock()
    {
        // Нажатие ESC для разблокировки курсора
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked;
            Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isCursorLocked;
        }
    }
}