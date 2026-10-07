using UnityEngine;

namespace StudyGame.Managers
{
    /// <summary>
    /// 모든 코어 시스템과 UI 뷰에 접근하기 위한 단일 레지스트리 (Single Source of Truth)
    /// 스파게티 의존성 및 잦은 GameObject.Find 호출을 방지합니다.
    /// </summary>
    public class ModuleHub : MonoBehaviour
    {
        private static ModuleHub _instance;
        public static ModuleHub Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ModuleHub>();
                }
                return _instance;
            }
        }

        [Header("UI Controllers")]
        [SerializeField] private PhoneUIDocumentController _phoneUI;
        // 다른 UI나 매니저들도 필요해지면 여기에 추가합니다.
        
        public PhoneUIDocumentController PhoneUI => _phoneUI;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 씬에 인스펙터로 할당되어 있지 않다면, Awake 시점에만 Find를 제한적으로 허용합니다.
            if (_phoneUI == null)
            {
                _phoneUI = FindFirstObjectByType<PhoneUIDocumentController>(FindObjectsInactive.Include);
            }
        }
    }
}
