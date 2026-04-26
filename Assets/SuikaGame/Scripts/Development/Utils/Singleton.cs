using UnityEngine;

namespace Development.Utils
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }

                return _instance;
            }
            private set => _instance = value;
        }

        [SerializeField] private bool dontDestroyOnLoad;

        public virtual void Awake()
        {
            if (Instance != null && !ReferenceEquals(Instance, this))
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
