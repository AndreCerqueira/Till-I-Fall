using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shaders
{
    public class ButtonFeedbackHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private MMF_Player _clickFeedback;
        [SerializeField] private MMF_Player _pointerEnterFeedback;
        [SerializeField] private MMF_Player _pointerExitFeedback;
        [SerializeField] private bool _autoPointerEnterFeedback;
        [SerializeField] private float _autoPointerEnterFeedbackDelay;
        [SerializeField] private bool _blockIfNotInteractable = true;
        
        private Button _button;
        private Toggle _toggle;

        private bool IsInteractable => !_blockIfNotInteractable || ((_button != null && _button.interactable) || (_toggle != null && _toggle.interactable));
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _toggle = GetComponent<Toggle>();
            
            if (_button != null)
            {
                _button.onClick.AddListener(() =>
                {
                    _clickFeedback?.PlayFeedbacks();
                });
            }
            else if (_toggle != null)
            {
                _toggle.onValueChanged.AddListener((value) =>
                {
                    _clickFeedback?.PlayFeedbacks();
                });
            }
        }
        
        
        private void OnEnable()
        {
            if (_autoPointerEnterFeedback)
                StartCoroutine(AutoPointerEnterFeedbackCoroutine());
        }
        
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable) return;
            _pointerEnterFeedback?.PlayFeedbacks();
        }

        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsInteractable) return;
            _pointerExitFeedback?.PlayFeedbacks();
        }
        

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator AutoPointerEnterFeedbackCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_autoPointerEnterFeedbackDelay);
                if (!IsInteractable) continue;
                _pointerEnterFeedback?.PlayFeedbacks(); 
            }
        }
    }
}
