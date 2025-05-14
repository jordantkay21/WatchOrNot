using UnityEngine;

namespace KayosMedia.WatchOrNot.Prototype
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Popups")]
        public ConfirmationPopup confirmationPopup;

        [Header("Overlays")]
        public GameObject loadingOverlay;
        public GameObject errorBanner;
        public GameObject toastPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}