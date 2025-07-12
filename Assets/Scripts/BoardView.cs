using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [SerializeField] private AreaView areaViewPrefab;
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [SerializeField] private float spacing = 0f;
    [SerializeField] private float delayBetweenCells = 0.025f;

    private List<AreaView> _areaViews = new();
    private AreaView _specialArea;
    
    [SerializeField] private GameObject boundaryPrefab;
    [SerializeField] private float boundaryThickness = 1f;
    [SerializeField] private float boundaryHeight = 2f;

    [SerializeField] private MMF_Player _onAreaEnterFeedback;
    [SerializeField] private MMF_Player _onAreaDropFeedback;
    [SerializeField] private MMF_Player _onAreaSpawnFeedback;
    
    private void Start()
    {
        StartCoroutine(SpawnBoardCoroutine());
    }

    private IEnumerator SpawnBoardCoroutine()
    {
        // set width and height to random
        width = Random.Range(3, 15);
        height = Random.Range(3, 10);
        
        var allPositions = GenerateAllPositions();
        var center = new Vector2Int(width / 2, height / 2);
        var spiralPositions = GenerateSpiralPositions(allPositions, center);

        float cellSizeWithSpacing = 1f + spacing;
        float offsetX = (width - 1) * cellSizeWithSpacing / 2f;
        float offsetZ = (height - 1) * cellSizeWithSpacing / 2f;

        Vector2Int specialPos = spiralPositions[Random.Range(0, spiralPositions.Count)];

        foreach (var pos in spiralPositions)
        {
            Vector3 worldPos = new Vector3(
                pos.x * cellSizeWithSpacing - offsetX,
                0,
                pos.y * cellSizeWithSpacing - offsetZ
            );

            var areaView = Instantiate(areaViewPrefab, worldPos, Quaternion.identity, transform);
            areaView.name = $"Area_{pos.x}_{pos.y}";

            var tileIndex = (pos.x + pos.y) % 2 == 0 ? 0 : 1;

            if (pos == specialPos)
            {
                tileIndex = 2;
                _specialArea = areaView;
            }

            areaView.Setup(tileIndex, _onAreaEnterFeedback);
            _areaViews.Add(areaView);
            AnimateArea(areaView.gameObject);
            _onAreaSpawnFeedback?.PlayFeedbacks();

            yield return new WaitForSeconds(delayBetweenCells);
        }
        
        SpawnBoundaries();
    }
    
    public void RemoveAreasExceptAroundDice(Vector3 dicePosition, int amountToRemove)
    {
        var protectedAreas = new List<AreaView>();
        float protectionRadius = 1.1f;

        foreach (var area in _areaViews)
        {
            if (Vector3.Distance(area.transform.position, dicePosition) <= protectionRadius)
            {
                protectedAreas.Add(area);
            }
        }
        
        if (_specialArea != null && !protectedAreas.Contains(_specialArea))
        {
            protectedAreas.Add(_specialArea);
        }

        var candidates = new List<AreaView>(_areaViews);
        candidates.RemoveAll(a => protectedAreas.Contains(a));

        Shuffle(candidates);
        int toRemove = Mathf.Min(amountToRemove, candidates.Count);

        for (int i = 0; i < toRemove; i++)
        {
            var area = candidates[i];
            _areaViews.Remove(area);
            StartCoroutine(AnimateAndRemove(area));
        }
    }

    private IEnumerator AnimateAndRemove(AreaView area)
    {
        float fallDuration = 0.6f;

        _onAreaDropFeedback?.PlayFeedbacks();
        area.transform.DOScale(Vector3.zero, fallDuration).SetEase(Ease.InBack);
        area.transform.DOMoveY(area.transform.position.y - 2f, fallDuration).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(fallDuration);

        try
        {
            Destroy(area.gameObject);
        }
        catch
        {
        }
    }

    private static void AnimateArea(GameObject go)
    {
        go.transform.localScale = Vector3.zero;
        go.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    private List<Vector2Int> GenerateAllPositions()
    {
        var positions = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                positions.Add(new Vector2Int(x, y));
        return positions;
    }

    private List<Vector2Int> GenerateSpiralPositions(List<Vector2Int> availablePositions, Vector2Int center)
    {
        var spiral = new List<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        int[] dx = { 1, 0, -1, 0 };
        int[] dy = { 0, 1, 0, -1 };

        int x = center.x;
        int y = center.y;
        int step = 1;
        int direction = 0;

        if (availablePositions.Contains(center))
        {
            spiral.Add(center);
            visited.Add(center);
        }

        while (spiral.Count < availablePositions.Count)
        {
            for (int side = 0; side < 2; side++)
            {
                for (int i = 0; i < step; i++)
                {
                    x += dx[direction];
                    y += dy[direction];
                    var pos = new Vector2Int(x, y);
                    if (availablePositions.Contains(pos) && !visited.Contains(pos))
                    {
                        spiral.Add(pos);
                        visited.Add(pos);
                    }

                    if (spiral.Count >= availablePositions.Count)
                        break;
                }

                direction = (direction + 1) % 4;
            }

            step++;
        }

        return spiral;
    }
    
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
    private void SpawnBoundaries()
    {
        float cellSizeWithSpacing = 1f + spacing;
        float totalWidth = width * cellSizeWithSpacing;
        float totalHeight = height * cellSizeWithSpacing;

        float offsetX = (width - 1) * cellSizeWithSpacing / 2f;
        float offsetZ = (height - 1) * cellSizeWithSpacing / 2f;

        Vector3 center = transform.position;

        // Lado Esquerdo
        SpawnBoundary(new Vector3((-offsetX - boundaryThickness / 2f) -0.5f, boundaryHeight / 2f, 0), new Vector3(boundaryThickness, 20f, totalHeight));

        // Lado Direito
        SpawnBoundary(new Vector3(offsetX + boundaryThickness / 2f +0.5f, boundaryHeight / 2f, 0), new Vector3(boundaryThickness, 20f, totalHeight));

        // Topo
        SpawnBoundary(new Vector3(0, boundaryHeight / 2f, offsetZ + boundaryThickness / 2f +0.5f), new Vector3(totalWidth, 20f, boundaryThickness));

        // Fundo
        SpawnBoundary(new Vector3(0, boundaryHeight / 2f, -offsetZ - boundaryThickness / 2f -0.5f), new Vector3(totalWidth, 20f, boundaryThickness));
    }

    private void SpawnBoundary(Vector3 localPosition, Vector3 scale)
    {
        var go = Instantiate(boundaryPrefab, transform);
        go.transform.localPosition = localPosition;
        go.transform.localScale = scale;
    }
    
    public void StartBoardDestruction()
    {
        StartCoroutine(DestroyBoardCoroutine());
    }

    private IEnumerator DestroyBoardCoroutine()
    {
        float delay = 0.01f;

        var areasToDestroy = new List<AreaView>(_areaViews);
    
        while (areasToDestroy.Count > 0)
        {
            int index = Random.Range(0, areasToDestroy.Count);
            AreaView area = areasToDestroy[index];
            areasToDestroy.RemoveAt(index);
            _areaViews.Remove(area);

            area.AnimateFallAndDestroy(); // método que anima e destrói
            _onAreaDropFeedback?.PlayFeedbacks();
            yield return new WaitForSeconds(delay);
        }
        
        // Destroy every child 
        yield return new WaitForSeconds(1f);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        StartCoroutine(SpawnBoardCoroutine());
    }
    
    public void HandleSpecialAreaTouched(AreaView touchedArea)
    {
        if (_specialArea == null || touchedArea != _specialArea) return;

        // Reconstroi áreas que desapareceram
        var allPositions = GenerateAllPositions();
        float cellSizeWithSpacing = 1f + spacing;
        float offsetX = (width - 1) * cellSizeWithSpacing / 2f;
        float offsetZ = (height - 1) * cellSizeWithSpacing / 2f;

        HashSet<Vector2Int> existingPositions = new();

        foreach (var area in _areaViews)
        {
            Vector3 pos = area.transform.position;
            int x = Mathf.RoundToInt(pos.x + offsetX);
            int y = Mathf.RoundToInt(pos.z + offsetZ);
            existingPositions.Add(new Vector2Int(x, y));
        }

        foreach (var pos in allPositions)
        {
            if (existingPositions.Contains(pos)) continue;

            Vector3 worldPos = new Vector3(
                pos.x * cellSizeWithSpacing - offsetX,
                0,
                pos.y * cellSizeWithSpacing - offsetZ
            );

            var newArea = Instantiate(areaViewPrefab, worldPos, Quaternion.identity, transform);
            newArea.name = $"Area_{pos.x}_{pos.y}";
            int tileIndex = (pos.x + pos.y) % 2 == 0 ? 0 : 1;
            newArea.Setup(tileIndex, _onAreaEnterFeedback);
            _areaViews.Add(newArea);
            AnimateArea(newArea.gameObject);
            _onAreaSpawnFeedback?.PlayFeedbacks();
        }

        // Muda o especial para outro
        Vector3 specialPos = _specialArea.transform.position;
        int specialX = Mathf.RoundToInt(specialPos.x + offsetX);
        int specialY = Mathf.RoundToInt(specialPos.z + offsetZ);
        int normalTileIndex = (specialX + specialY) % 2 == 0 ? 0 : 1;
        _specialArea.SetTile(normalTileIndex);

        var candidates = new List<AreaView>(_areaViews);
        candidates.Remove(_specialArea);

        AreaView newSpecial = candidates[Random.Range(0, candidates.Count)];
        newSpecial.SetTile(2);
        _specialArea = newSpecial;
    }
}