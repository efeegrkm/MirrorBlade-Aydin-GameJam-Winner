using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Animator))] // Animator eklendi
public class MirrorCharacterManager : MonoBehaviour
{
    private Transform playerTransform;
    private Transform coreTransform;
    private float angleOffset;
    private Collider2D boundsCollider;

    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private Animator anim; 

    private Vector2 lastPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        anim = GetComponent<Animator>();

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(0.6f, 0.2f, 0.8f, 0.35f);
        lineRenderer.endColor = new Color(0.6f, 0.2f, 0.8f, 0.05f);
    }

    public void Initialize(Transform player, Transform core, float angle, Collider2D bounds)
    {
        playerTransform = player;
        coreTransform = core;
        angleOffset = angle;
        boundsCollider = bounds;

        lastPosition = transform.position;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerAttack += Attack;
        GameEvents.OnPlayerDash += Dash; 
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerAttack -= Attack;
        GameEvents.OnPlayerDash -= Dash;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || coreTransform == null) return;

        Vector3 coreToPlayerDir = playerTransform.position - coreTransform.position;
        Vector3 rotatedVector = Quaternion.Euler(0, 0, angleOffset) * coreToPlayerDir;

        Vector2 idealPosition = coreTransform.position + rotatedVector;
        Vector2 targetPosition = idealPosition;

        if (boundsCollider != null)
        {
            targetPosition = boundsCollider.ClosestPoint(idealPosition);
        }

        rb.MovePosition(targetPosition);
        transform.rotation = playerTransform.rotation * Quaternion.Euler(0, 0, angleOffset);

        if (anim != null)
        {
            float moveDelta = Vector2.Distance(lastPosition, rb.position);
            anim.SetBool("isMoving", moveDelta > 0.01f);
        }
        lastPosition = rb.position;
    }

    private void LateUpdate()
    {
        if (coreTransform != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, coreTransform.position);
        }
    }

    private void Attack()
    {
        Debug.Log($"Yansýma (Açý: {angleOffset}) hasar vurdu!");
    }

    private void Dash()
    {
        if (anim != null) anim.SetTrigger("dash");
    }
}