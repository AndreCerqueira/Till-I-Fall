using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.General
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }
        
        [SerializeField] private Texture2D _defaultCursor;
        [SerializeField] private Texture2D _interactCursor;
        
        private Vector2 _cursorHotspot;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        private void Start()
        {
            _cursorHotspot = new Vector2(_defaultCursor.width / 2, _defaultCursor.height / 2);
            Cursor.SetCursor(_defaultCursor, _cursorHotspot, CursorMode.Auto);
            
            RegisterUIElements();
        }

        private void RegisterUIElements()
        {
            foreach (var selectable in Resources.FindObjectsOfTypeAll<Selectable>())
            {
                AddCursorEvents(selectable);
            }
        }

        public void RegisterNewUIElement(GameObject uiElement)
        {
            Selectable selectable = uiElement.GetComponent<Selectable>();
            if (selectable != null)
            {
                AddCursorEvents(selectable);
            }
        }

        private void AddCursorEvents(Selectable uiElement)
        {
            EventTrigger trigger = uiElement.gameObject.GetComponent<EventTrigger>() ?? uiElement.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((_) => { if (uiElement.interactable) SetInteractCursor(); });
            trigger.triggers.Add(entryEnter);
            
            EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((_) => SetDefaultCursor());
            trigger.triggers.Add(entryExit);
        }

        public void SetInteractCursor()
        {
            Cursor.SetCursor(_interactCursor, _cursorHotspot, CursorMode.Auto);
        }

        public void SetDefaultCursor()
        {
            Cursor.SetCursor(_defaultCursor, _cursorHotspot, CursorMode.Auto);
        }
    }
}
