using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

/// Script simples para movimento VR usando thumbsticks
/// Anexe este script ao XR Origin para movimento básico funcionar
[RequireComponent(typeof(CharacterController))]
public class SimpleVRMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Velocidade de movimento em metros por segundo")]
    public float moveSpeed = 3.0f;

    [Tooltip("Velocidade de rotação em graus por segundo")]
    public float turnSpeed = 60.0f;

    [Tooltip("Usar movimento contínuo ou snap turn")]
    public bool smoothTurning = true;

    [Tooltip("Ângulo de rotação para snap turn")]
    public float snapTurnAngle = 45.0f;

    [Header("References")]
    [Tooltip("Referência para a câmera (será encontrada automaticamente se vazio)")]
    public Camera playerCamera;

    private CharacterController characterController;
    private XROrigin xrOrigin;
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;
    private bool snapTurnUsed = false;

    void Start()
    {
        // Obter componentes
        characterController = GetComponent<CharacterController>();
        xrOrigin = GetComponent<XROrigin>();

        // Encontrar câmera se não foi atribuída
        if (playerCamera == null)
        {
            playerCamera = xrOrigin != null ? xrOrigin.Camera : Camera.main;
        }

        // Inicializar controllers
        InitializeControllers();

        Debug.Log("🎮 SimpleVRMovement inicializado!");
        Debug.Log("🕹️ Use o thumbstick esquerdo para andar");
        Debug.Log("🔄 Use o thumbstick direito para rodar");
    }

    void Update()
    {
        // Re-inicializar controllers se necessário
        if (!leftController.isValid || !rightController.isValid)
        {
            InitializeControllers();
        }

        // Processar movimento
        HandleMovement();
        HandleRotation();
    }

    private void InitializeControllers()
    {
        var leftHandDevices = new System.Collections.Generic.List<InputDevice>();
        var rightHandDevices = new System.Collections.Generic.List<InputDevice>();

        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        if (leftHandDevices.Count > 0)
            leftController = leftHandDevices[0];

        if (rightHandDevices.Count > 0)
            rightController = rightHandDevices[0];
    }

    private void HandleMovement()
    {
        Vector2 inputAxis = Vector2.zero;

        // Obter input do thumbstick esquerdo
        if (leftController.isValid)
        {
            if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftThumbstick))
            {
                inputAxis = leftThumbstick;
            }
        }

        // Calcular direção de movimento baseado na câmera
        if (inputAxis.magnitude > 0.1f && playerCamera != null)
        {
            // Obter direção forward e right da câmera (apenas no plano horizontal)
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;

            // Projetar no plano horizontal
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calcular direção de movimento
            Vector3 moveDirection = (forward * inputAxis.y + right * inputAxis.x);

            // Aplicar movimento
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // Aplicar gravidade
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }

    private void HandleRotation()
    {
        Vector2 inputAxis = Vector2.zero;

        // Obter input do thumbstick direito
        if (rightController.isValid)
        {
            if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightThumbstick))
            {
                inputAxis = rightThumbstick;
            }
        }

        if (smoothTurning)
        {
            // Rotação contínua suave
            if (Mathf.Abs(inputAxis.x) > 0.1f)
            {
                transform.Rotate(0, inputAxis.x * turnSpeed * Time.deltaTime, 0);
            }
        }
        else
        {
            // Snap turn (rotação por cliques)
            if (Mathf.Abs(inputAxis.x) > 0.75f && !snapTurnUsed)
            {
                float turnDirection = Mathf.Sign(inputAxis.x);
                transform.Rotate(0, turnDirection * snapTurnAngle, 0);
                snapTurnUsed = true;
            }
            else if (Mathf.Abs(inputAxis.x) < 0.1f)
            {
                snapTurnUsed = false;
            }
        }
    }

    // Gizmos para debug
    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.blue;
            Vector3 forward = playerCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();
            Gizmos.DrawRay(transform.position, forward * 2f);

            Gizmos.color = Color.red;
            Vector3 right = playerCamera.transform.right;
            right.y = 0;
            right.Normalize();
            Gizmos.DrawRay(transform.position, right * 2f);
        }
    }
}
