using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DirectionalPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform _arrow;
    [SerializeField] private float _rotationSpeed = 90f;

    private bool _isLeftHolding = false;
    private bool _isRightHolding = false;

    private void Update()
    {
        if (_arrow == null) return;

        float direction = 0f;

        if (_isLeftHolding) direction -= 1f;
        if (_isRightHolding) direction += 1f;

        if (direction != 0f)
        {
            _arrow.Rotate(0, 0, direction * _rotationSpeed * Time.deltaTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _isRightHolding = true;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _isLeftHolding = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _isRightHolding = false;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _isLeftHolding = false;
        }
    }

    public Vector2 GetDirection()
    {
        float angleInRadians = _arrow.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(-Mathf.Cos(angleInRadians), -Mathf.Sin(angleInRadians)); // -X local
    }
}
