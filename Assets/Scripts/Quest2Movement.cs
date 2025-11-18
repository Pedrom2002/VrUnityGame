using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR;
using UnityEngine.InputSystem;

/// <summary>
/// Movimento VR GARANTIDO para Quest 2
/// Adicione ao XR Origin para movimento funcionar
/// Usa Input System do Unity para Quest 2
/// </summary>
[RequireComponent(typeof(XROrigin))]
public class Quest2Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Velocidade de movimento")]
    public float moveSpeed = 3.0f;

    [Tooltip("Velocidade de rotação snap turn")]
    public float snapTurnAngle = 45.0f;

    [Tooltip("Usar smooth turn (rotação suave) em vez de snap")]
    public bool useSmoothTurn = true;

    [Tooltip("Velocidade de rotação suave")]
    public float smoothTurnSpeed = 60.0f;

    [Header("Input Actions")]
    [Tooltip("Input Action para movimento (Thumbstick Esquerdo)")]
    public InputActionReference moveAction;

    [Tooltip("Input Action para rotação (Thumbstick Direito)")]
    public InputActionReference turnAction;

    private XROrigin xrOrigin;
    private Camera vrCamera;
    private CharacterController characterController;
    private bool canSnapTurn = true;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        vrCamera = xrOrigin.Camera;

        // Criar ou obter Character Controller
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
            characterController.stepOffset = 0.3f;
            Debug.Log("✅ Character Controller adicionado automaticamente");
        }

        Debug.Log("🎮 Quest2Movement ativado!");
        Debug.Log("🕹️ Use thumbsticks para andar e rodar");

        if (moveAction == null || turnAction == null)
        {
            Debug.LogWarning("⚠️ Input Actions não configurados! Use XR Input nativo como fallback.");
        }
    }

    void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Enable();
        if (turnAction != null && turnAction.action != null)
            turnAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Disable();
        if (turnAction != null && turnAction.action != null)
            turnAction.action.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector2 moveInput = Vector2.zero;

        // Tentar obter via Input Action primeiro
        if (moveAction != null && moveAction.action != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            // Fallback: usar XR Input direto
            var leftHandDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);

            if (leftHandDevices.Count > 0)
            {
                leftHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out moveInput);
            }
        }

        if (moveInput.magnitude > 0.1f && vrCamera != null)
        {
            // Calcular direção baseada na câmera
            Vector3 forward = vrCamera.transform.forward;
            Vector3 right = vrCamera.transform.right;

            // Projetar no plano horizontal
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calcular movimento
            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x);

            // Aplicar movimento
            if (characterController != null)
            {
                characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                // Fallback: movimento direto
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
            }
        }

        // Aplicar gravidade
        if (characterController != null && !characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }

    void HandleRotation()
    {
        Vector2 turnInput = Vector2.zero;

        // Tentar obter via Input Action primeiro
        if (turnAction != null && turnAction.action != null)
        {
            turnInput = turnAction.action.ReadValue<Vector2>();
        }
        else
        {
            // Fallback: usar XR Input direto
            var rightHandDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

            if (rightHandDevices.Count > 0)
            {
                rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out turnInput);
            }
        }

        if (useSmoothTurn)
        {
            // Rotação suave
            if (Mathf.Abs(turnInput.x) > 0.1f)
            {
                float rotationAmount = turnInput.x * smoothTurnSpeed * Time.deltaTime;
                transform.Rotate(0, rotationAmount, 0);
            }
        }
        else
        {
            // Snap turn
            if (Mathf.Abs(turnInput.x) > 0.75f && canSnapTurn)
            {
                float direction = Mathf.Sign(turnInput.x);
                transform.Rotate(0, direction * snapTurnAngle, 0);
                canSnapTurn = false;
            }
            else if (Mathf.Abs(turnInput.x) < 0.3f)
            {
                canSnapTurn = true;
            }
        }
    }

    // Desenhar gizmos para debug
    void OnDrawGizmos()
    {
        if (vrCamera != null)
        {
            Gizmos.color = Color.green;
            Vector3 forward = vrCamera.transform.forward;
            forward.y = 0;
            Gizmos.DrawRay(transform.position + Vector3.up, forward.normalized * 2f);
        }
    }
}
