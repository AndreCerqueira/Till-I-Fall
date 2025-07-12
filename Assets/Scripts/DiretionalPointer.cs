using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DirectionalPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform _arrow;
    [SerializeField] private float _rotationSpeed = 90f;

    private bool _isHolding = false;

    private void Update()
    {
        if (_isHolding && _arrow != null)
        {
            _arrow.Rotate(0, 0, -_rotationSpeed * Time.deltaTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isHolding = false;
    }

    public Vector2 GetDirection()
    {
        float angleInRadians = _arrow.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(-Mathf.Cos(angleInRadians), -Mathf.Sin(angleInRadians)); // -X local
    }
}
