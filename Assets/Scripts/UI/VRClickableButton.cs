using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRAimLab.UI
{
    /// Botão clicável em VR - pode ser acionado por:
    /// 1. Raycasting do controller (apontar e apertar trigger)
    /// 2. Tiro da arma (atirar no botão)
    /// 3. Click do mouse (para desktop/editor)
    [RequireComponent(typeof(Collider))]
    public class VRClickableButton : MonoBehaviour
    {
        [Header("Button Settings")]
        [SerializeField] private UnityEvent onButtonClicked;
        [SerializeField] private bool canBeShot = true; // Pode ser ativado atirando
        [SerializeField] private float cooldown = 0.5f; // Tempo entre cliques

        [Header("Visual Feedback")]
        [SerializeField] private Renderer buttonRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color clickColor = Color.green;
        [SerializeField] private float colorChangeDuration = 0.2f;

        [Header("Audio")]
        [SerializeField] private bool playClickSound = true;

        private Collider buttonCollider;
        private float lastClickTime = 0f;
        private Material buttonMaterial;
        private Color currentColor;

        void Awake()
        {
            buttonCollider = GetComponent<Collider>();

            if (buttonCollider != null)
            {
                // Garantir que é trigger para detecção de raios
                buttonCollider.isTrigger = true;
            }

            if (buttonRenderer != null)
            {
                buttonMaterial = buttonRenderer.material;
                currentColor = normalColor;
                buttonMaterial.color = normalColor;
            }
        }

        /// Detecta quando bala/raycast atinge o botão
        void OnTriggerEnter(Collider other)
        {
            if (!canBeShot) return;

            // Detectar se foi atingido por bala
            if (other.CompareTag("Bullet") || other.name.Contains("Bullet"))
            {
                Debug.Log($"[VRButton] Botão '{gameObject.name}' atingido por bala!");
                Click();
            }
        }

        /// Chamado quando botão é clicado (qualquer método)
        public void Click()
        {
            // Verificar cooldown
            if (Time.time - lastClickTime < cooldown)
            {
                Debug.Log("[VRButton] Cooldown ativo, ignorando click");
                return;
            }

            lastClickTime = Time.time;

            Debug.Log($"[VRButton] Botão '{gameObject.name}' clicado!");

            // Feedback visual
            if (buttonRenderer != null)
            {
                StartCoroutine(FlashColor(clickColor));
            }

            // Feedback audio
            if (playClickSound && VRAimLab.Core.AudioManager.Instance != null)
            {
                VRAimLab.Core.AudioManager.Instance.PlayButtonClick();
            }

            // Invocar evento
            onButtonClicked?.Invoke();
        }

        /// Flash de cor quando clicado
        private System.Collections.IEnumerator FlashColor(Color flashColor)
        {
            if (buttonMaterial == null) yield break;

            // Flash para cor de click
            buttonMaterial.color = flashColor;
            yield return new WaitForSeconds(colorChangeDuration);

            // Voltar para cor normal
            buttonMaterial.color = normalColor;
        }

        /// Detectar hover do mouse (para desktop)
        void OnMouseEnter()
        {
            if (buttonRenderer != null && buttonMaterial != null)
            {
                buttonMaterial.color = hoverColor;
            }
        }

        void OnMouseExit()
        {
            if (buttonRenderer != null && buttonMaterial != null)
            {
                buttonMaterial.color = normalColor;
            }
        }

        /// Click do mouse (para desktop)
        void OnMouseDown()
        {
            Click();
        }

        /// Adicionar listener programaticamente
        public void AddListener(UnityAction action)
        {
            onButtonClicked.AddListener(action);
        }
    }
}
