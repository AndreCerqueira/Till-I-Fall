using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoardView boardView;
    [SerializeField] private float torqueAmount = 10f;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float settleThreshold = 0.1f;

    private bool isRolling = false;
    
    [Header("Input Direction Settings")]
    public float directionX = 0f; // de -1 a 1
    public float directionZ = 1f; // de -1 a 1
    public float torqueMultiplier = 1f; // de 0 a 1

    private bool hasFallen = false;
    
    [SerializeField] private MMF_Player _onRollFeedback;

    private void Update()
    {
        if (!hasFallen && transform.position.y < -2f)
        {
            hasFallen = true;
            Debug.Log("Dado caiu no abismo!");
            boardView.StartBoardDestruction(); // Chama o método no BoardView
            StartCoroutine(RespawnDiceCoroutine());
        }
    }

    public IEnumerator RespawnDiceCoroutine()
    {
        this.GetComponent<TrailRenderer>().enabled = false;
        yield return new WaitForSeconds(1f);
        this.transform.position = new Vector3(0f, 20f, 0f);
        this.transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        hasFallen = false;
        this.GetComponent<TrailRenderer>().enabled = true;
        
        ScoreManager.Instance.ResetScore();
    }
    
    public void Roll()
    {
        if (isRolling) return;

        isRolling = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Normaliza o vetor direção
        Vector3 direction = new Vector3(directionX, 1f, directionZ).normalized;

        // Aplica força na direção desejada
        rb.AddForce(direction * throwForce, ForceMode.Impulse);

        // Torque baseado em direção + intensidade
        Vector3 torque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * torqueAmount * torqueMultiplier;

        rb.AddTorque(torque, ForceMode.Impulse);
        
        _onRollFeedback?.PlayFeedbacks();

        StartCoroutine(WaitForDiceToSettle());
    }

    private IEnumerator WaitForDiceToSettle()
    {
        // Espera até o dado parar de se mover
        yield return new WaitForSeconds(1.5f);

        while (rb.linearVelocity.magnitude > settleThreshold || rb.angularVelocity.magnitude > settleThreshold)
            yield return null;

        int result = GetTopNumber();
        Debug.Log("Número obtido: " + result);
        boardView.RemoveAreasExceptAroundDice(transform.position, result);
        ScoreManager.Instance.AddScore(result);

        isRolling = false;
    }

    private int GetTopNumber()
    {
        Vector3 up = Vector3.up;
        float maxDot = -1f;
        int topNumber = -1;

        // Assume que as faces estão orientadas com normais nos eixos, como (+/-X, +/-Y, +/-Z)
        var faceNormals = new (Vector3 normal, int number)[]
        {
            (Vector3.up, 1),
            (Vector3.down, 6),
            (Vector3.forward, 2),
            (Vector3.back, 5),
            (Vector3.right, 3),
            (Vector3.left, 4),
        };

        foreach (var (normal, number) in faceNormals)
        {
            Vector3 worldNormal = transform.TransformDirection(normal);
            float dot = Vector3.Dot(worldNormal, up);
            if (dot > maxDot)
            {
                maxDot = dot;
                topNumber = number;
            }
        }

        return topNumber;
    }
}