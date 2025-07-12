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
    [SerializeField] private DirectionalPointer pointer;
    [SerializeField] private ParticleSystem particleSystemPrefab;

    private bool isRolling = false;
    
    [Header("Input Direction Settings")]
    public float torqueMultiplier = 1f; // de 0 a 1

    private bool hasFallen = false;
    
    [SerializeField] private MMF_Player _onRollFeedback;

    private void Update()
    {
        if (!hasFallen && transform.position.y < -2f)
        {
            hasFallen = true;
            Debug.Log("Dado caiu no abismo!");
            //StopCoroutine(WaitForDiceToSettle());
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
        isRolling = false;
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
        Vector2 direction2D = pointer.GetDirection();
        Vector3 direction = new Vector3(direction2D.x, 1f, direction2D.y).normalized;

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

    public void PlayParticles()
    {
        // play particleSystem
        if (particleSystemPrefab != null)
        {
            // rotation need to be -90 on x
            Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
            // position add 0.01 on y
            Vector3 position = transform.position + new Vector3(0f, 0.01f, 0f);
            ParticleSystem particleSystem = Instantiate(particleSystemPrefab, position, rotation);
            particleSystem.Play();
            Destroy(particleSystem.gameObject, particleSystem.main.duration);
        }
    }

    private IEnumerator WaitForDiceToSettle()
    {
        // Espera até o dado parar de se mover
        yield return new WaitForSeconds(1.5f);
        
        /*
        if (hasFallen) 
        {
            Debug.Log("Dado caiu no abismo durante a rolagem!");
            yield break; // Sai se o dado já tiver caído
        }*/

        while (rb.linearVelocity.magnitude > settleThreshold || rb.angularVelocity.magnitude > settleThreshold)
            yield return null;
        
        /*
        if (hasFallen) 
        {
            Debug.Log("Dado caiu no abismo durante a rolagem!");
            yield break; // Sai se o dado já tiver caído
        }*/
        
        int result = GetTopNumber();
        Debug.Log("Número obtido result: " + result);
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