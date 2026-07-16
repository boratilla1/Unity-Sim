using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Mouse / Kamera Ayarlarý")]
    public float mouseSensitivity = 2f;
    public Transform cameraHolder;
    public Camera playerCamera;
    public AudioListener audioListener;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch = 0f;

    [Header("Görsel Ayarlar")]
    public MeshRenderer playerMesh; // Kapsülün MeshRenderer'ýný buraya sürükleyeceðiz

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Debug.Log($"<color=cyan>[PlayerController - Awake]</color> Obje uyandý. Obje Adý: {gameObject.name}");
    }

    private void Start()
    {
        Debug.Log($"<color=orange>[PlayerController - Start]</color> Çalýþtý. isLocalPlayer þu an: {isLocalPlayer}, netId: {netId}");

        if (!isLocalPlayer)
        {
            Debug.Log($"<color=orange>[PlayerController - Start]</color> Bu benim karakterim DEÐÝL (veya Mirror henüz yetkiyi atamadý). Kamerayý kapatýyorum.");
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
        }
        else
        {
            Debug.Log($"<color=orange>[PlayerController - Start]</color> Bu BENÝM karakterim. Start metodunda kameraya dokunmuyorum.");
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"<color=green>[PlayerController - OnStartLocalPlayer]</color> Çalýþtý! Ýþte otorite bende. netId: {netId}");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraHolder != null)
        {
            cameraHolder.gameObject.SetActive(true);
            Debug.Log("<color=green>[PlayerController - OnStartLocalPlayer]</color> CameraHolder baþarýyla aktif edildi.");
        }
        else
        {
            Debug.LogError("<color=red>[HATA]</color> cameraHolder referansý ÝNSPECTOR'DA BOÞ BIRAKILMIÞ!");
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            Debug.Log("<color=green>[PlayerController - OnStartLocalPlayer]</color> PlayerCamera baþarýyla aktif edildi.");
        }
        else
        {
            Debug.LogError("<color=red>[HATA]</color> playerCamera referansý ÝNSPECTOR'DA BOÞ BIRAKILMIÞ!");
        }

        if (audioListener != null)
        {
            audioListener.enabled = true;
        }
        else
        {
            Debug.LogWarning("<color=yellow>[UYARI]</color> AudioListener referansý boþ, seslerde sorun olabilir.");
        }
        if (playerMesh != null)
        {
            playerMesh.enabled = false;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        if (cameraHolder != null)
            cameraHolder.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}