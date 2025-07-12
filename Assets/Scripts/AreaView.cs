using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

public class AreaView : MonoBehaviour
{
    private Renderer _renderer;
    private Material _material;
    private bool isAnimating = false;

    private MMF_Player _onEnterFeedback;

    public void Setup(int tileIndex, MMF_Player onEnterFeedback)
    {
        _onEnterFeedback = onEnterFeedback;
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
        SetTile(tileIndex);
    }
    
        
    public void SetTile(int tileIndex)
    {
        _material.SetInt("_TileIndex", tileIndex);
    }
    
    public void AnimateFallAndDestroy()
    {
        transform.DOMoveY(transform.position.y - 5f, 0.5f)
            .SetEase(Ease.InBack);
    
        transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
    
    private void OnCollisionEnter(Collision collision)
        {
            if (isAnimating) return;
    
            if (collision.gameObject.CompareTag("Dice")) // Certifica-te que o dado tem a tag "Dice"
            {
                isAnimating = true;
    
                float downAmount = 0.15f;
                float duration = 0.1f;
    
                Vector3 originalPos = transform.position;
                Vector3 downPos = originalPos - new Vector3(0, downAmount, 0);
    
                _onEnterFeedback?.PlayFeedbacks(transform.position);
                
                transform.DOMoveY(downPos.y, duration).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    transform.DOMoveY(originalPos.y, duration).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        isAnimating = false;
                    });
                });
                
                if (_material.GetInt("_TileIndex") == 2)
                {
                    FindObjectOfType<BoardView>().HandleSpecialAreaTouched(this);
                }
            }
        }
}
