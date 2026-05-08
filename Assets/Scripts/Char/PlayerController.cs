using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseDeadzoneRadius = 0.5f;

    [Header("Dash & Mana Settings")]
    public float maxMana = 100f;
    public float dashCost = 30f;
    public float manaRegenRate = 10f; 
    public float manaPerKill = 15f; 
    public float dashSpeed = 25f; 
    public float dashDuration = 0.15f; 

    [Header("Core & Mirror Settings")]
    public Transform coreTransform;
    public MirrorCharacterManager mirrorPrefab;

    [Header("Input Bounds")]
    public Collider2D mouseBoundsCollider;
    public Collider2D movementBoundsCollider;

    [Header("Orbit Settings")]
    public GameObject orbitPrefab; 
    private List<OrbitingProjectile> activeOrbiters = new List<OrbitingProjectile>();

    private int mirrorCharacterCount = 1;
    private List<MirrorCharacterManager> activeMirrors = new List<MirrorCharacterManager>();

    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private Animator anim;

    private Vector2 targetMousePos;
    private bool isMoving = false;
    private bool isMouseListening = true;
    private bool isDragging = false;

    private float currentMana;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector2 dashDirection;

    private float stepTimer = 0f;
    public float stepInterval = 0.35f;

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
        lineRenderer.startColor = new Color(1f, 0.5f, 0f, 0.15f);
        lineRenderer.endColor = new Color(1f, 0.5f, 0f, 0.01f);
    }

    private void OnEnable()
    {
        GameEvents.OnMirrorCountChanged += SetMirrorCount;
        GameEvents.OnEnemyKilled += AddManaFromKill; 
    }

    private void OnDisable()
    {
        GameEvents.OnMirrorCountChanged -= SetMirrorCount;
        GameEvents.OnEnemyKilled -= AddManaFromKill;
    }

    private void Start()
    {
        SetMirrorCount(1);
        currentMana = maxMana;
        GameEvents.OnDashManaChanged?.Invoke(currentMana, maxMana);
    }

    public void AddOrbitingProjectile()
    {
        if (orbitPrefab == null) return;

        GameObject orbObj = Instantiate(orbitPrefab, transform.position, Quaternion.identity);
        OrbitingProjectile orb = orbObj.GetComponent<OrbitingProjectile>();

        activeOrbiters.Add(orb);

        float angleStep = 360f / activeOrbiters.Count;
        for (int i = 0; i < activeOrbiters.Count; i++)
        {
            activeOrbiters[i].Initialize(this.transform, i * angleStep);
        }

        GameEvents.OnShowFloatingText?.Invoke(transform.position, "+Orbital Shield", Color.yellow);
    }
    public void UpgradeMaxMana(float amount)
    {
        maxMana += amount;
        currentMana = maxMana;
        GameEvents.OnDashManaChanged?.Invoke(currentMana, maxMana);

        GameEvents.OnShowFloatingText?.Invoke(transform.position, "+Max Mana", Color.cyan);
    }
    public void IncreaseMirrorCount()
    {
        SetMirrorCount(mirrorCharacterCount + 1);
    }

    private void AddManaFromKill(int soulValue)
    {
        currentMana += manaPerKill;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        GameEvents.OnDashManaChanged?.Invoke(currentMana, maxMana);
        GameEvents.OnShowFloatingText?.Invoke(transform.position, "+" + manaPerKill.ToString(), Color.cyan);
    }

    private void Update()
    {
        if (!isDashing) 
        {
            HandleInputAndRotation();
            HandleAttackInput();
            HandleDashInput();
        }

        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            GameEvents.OnDashManaChanged?.Invoke(currentMana, maxMana);
        }

        if (anim != null) anim.SetBool("isMoving", isMoving && !isDashing);

        if (isMoving && !isDashing)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                GameEvents.OnPlaySound?.Invoke(GameEvents.SoundType.WalkStep);
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; 
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            Vector2 newPosition = rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime;

            if (movementBoundsCollider != null)
            {
                newPosition = movementBoundsCollider.ClosestPoint(newPosition);
            }
            rb.MovePosition(newPosition);

            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                targetMousePos = movementBoundsCollider != null ? movementBoundsCollider.ClosestPoint(mousePos) : mousePos;
            }
        }
        else if (isMoving)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetMousePos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
    }

    private void LateUpdate()
    {
        if (coreTransform != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, coreTransform.position);
        }
    }

    private void HandleInputAndRotation()
    {
        if (!isMouseListening)
        {
            isMoving = false;
            isDragging = false;
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        bool isInsideBounds = mouseBoundsCollider == null || mouseBoundsCollider.OverlapPoint(mousePos);

        if (Input.GetMouseButtonDown(0))
        {
            if (isInsideBounds) isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isInsideBounds && !isDragging)
        {
            isMoving = false;
            return;
        }

        targetMousePos = movementBoundsCollider != null ? movementBoundsCollider.ClosestPoint(mousePos) : mousePos;

        Vector3 lookDir = targetMousePos - (Vector2)transform.position;
        if (lookDir.magnitude > mouseDeadzoneRadius)
        {
            float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, targetAngle - 90f);
            isMoving = isDragging;
        }
        else
        {
            isMoving = false;
        }
    }

    private void HandleAttackInput()
    {
        if (!isMouseListening) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameEvents.OnPlayerAttack?.Invoke();
        }
    }

    private void HandleDashInput()
    {
        if (!isMouseListening) return;

        if (Input.GetKeyDown(KeyCode.LeftShift) && currentMana >= dashCost)
        {
            currentMana -= dashCost;
            GameEvents.OnDashManaChanged?.Invoke(currentMana, maxMana);

            GameEvents.OnShowFloatingText?.Invoke(transform.position, "-" + dashCost.ToString(), Color.blue);

            GameEvents.OnPlaySound?.Invoke(GameEvents.SoundType.Dash);

            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = transform.up; 

            GameEvents.OnPlayerDash?.Invoke();
            if (anim != null) anim.SetTrigger("dash");
        }
    }

    public void StopMouseListening()
    {
        isMouseListening = false;
        isMoving = false;
        isDragging = false;
    }
    public void StartMouseListening()
    {
        isMouseListening = true;
    }
    public void SetMirrorCount(int newCount)
    {
        mirrorCharacterCount = newCount;
        foreach (var mirror in activeMirrors)
        {
            if (mirror != null) Destroy(mirror.gameObject);
        }
        activeMirrors.Clear();

        if (mirrorCharacterCount <= 0) return;
        float angleStep = 360f / (mirrorCharacterCount + 1);

        for (int i = 1; i <= mirrorCharacterCount; i++)
        {
            float currentAngleOffset = angleStep * i;
            MirrorCharacterManager newMirror = Instantiate(mirrorPrefab, transform.position, Quaternion.identity);
            newMirror.Initialize(this.transform, coreTransform, currentAngleOffset, movementBoundsCollider);
            activeMirrors.Add(newMirror);
        }
    }
}