using UnityEngine;
using VRAimLab.Gameplay;

namespace VRAimLab.UI
{
    /// <summary>
    /// Botão para repetir o tutorial - VR READY
    /// Setup: Adicione este script + VRClickableButton ao botão
    /// O botão pode ser ativado atirando nele ou clicando!
    /// </summary>
    [RequireComponent(typeof(VRClickableButton))]
    public class RepeatTutorialButton : MonoBehaviour
    {
        private VRClickableButton vrButton;

        void Awake()
        {
            // Pegar ou adicionar VRClickableButton
            vrButton = GetComponent<VRClickableButton>();
            if (vrButton == null)
            {
                vrButton = gameObject.AddComponent<VRClickableButton>();
            }

            // Registrar o OnClick
            vrButton.AddListener(OnClick);

            Debug.Log("[RepeatButton] Botão VR configurado! Pode atirar no botão para clicar.");
        }

        /// <summary>
        /// Chamado automaticamente quando o botão é clicado
        /// </summary>
        public void OnClick()
        {
            Debug.Log("[RepeatButton] Botão clicado - procurando tutorial...");

            // Encontrar o ShootingRangeTutorial na cena
            ShootingRangeTutorial tutorial = FindObjectOfType<ShootingRangeTutorial>();

            if (tutorial != null)
            {
                Debug.Log("[RepeatButton] Tutorial encontrado! Reiniciando...");
                tutorial.OnRepeatButtonClicked();
            }
            else
            {
                Debug.LogError("[RepeatButton] ShootingRangeTutorial não encontrado na cena!");
            }
        }
    }
}
