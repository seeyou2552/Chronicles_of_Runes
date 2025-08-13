using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Singleton<PlayerController>, IDamageable
{
    [Header("이동 속도")]
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody2D rb;
    public Vector2 moveInput;

    private PlayerInputActions inputActions;
    public bool canControl;
    public bool canUIInteraction;
    [SerializeField] public PlayerStatHandler statHandler;
    [SerializeField]private StatModifier statModifier;
    [SerializeField] private float DamageTimer;

    public bool isInvincible = false;
    public bool isDead = false;
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject); // 중복된 인스턴스 파괴
            return;
        }
        canControl = true;
        canUIInteraction = true;
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        EventBus.Subscribe(LoadState.LoadComplete, PlayerSpawnPosition);
        EventBus.Subscribe(EventType.BossSpawnCinematic, BossCinematicChangeControlFlag);
        EventBus.Subscribe(LoadState.LoadStart, OffCanControl);
        EventBus.Subscribe(LoadState.LoadComplete, param => OnCanControl(param));
        EventBus.Subscribe(LoadState.LoadComplete, param => MoveSpawnPoint(param));
        EventBus.Subscribe(GameState.GameOver, statHandler.ResetRerollCount);
        EventBus.Subscribe(GameState.Ending, statHandler.ResetRerollCount);
        
        if(SceneManager.GetActiveScene().name == "VillageScene")
        {
            isDead = false;
        }
    }

    private void OnDisable()
    {
        if (Instance != this) return;
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Disable();
        EventBus.Unsubscribe(EventType.BossSpawnCinematic, BossCinematicChangeControlFlag);
        EventBus.Unsubscribe(LoadState.LoadComplete, param => MoveSpawnPoint(param));
    }

    private void Update()
    {
        // Logger.Log(canUIInteraction.ToString());
        
        if (DamageTimer > 0)
        {
            DamageTimer -= Time.deltaTime;    
        }
        else
        {
            DamageTimer = 0;
        }
        
    }

    private void FixedUpdate()
    {
        if (canControl)
        {
            rb.velocity = moveInput.normalized * (moveSpeed + statModifier.MoveSpeed);    
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    public StatModifier GetStatModifier()
    {
        return statModifier;
    }
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    public void OnDamage(float value)
    {
        if (isDead) return;
        if (DamageTimer > 0) return;
        if (isInvincible) return;
        //statHandler.TakeDamage(value * statModifier.damageIncome);
        float damage = value * statModifier.DamageIncome;
        DamageFontManager.Instance.ShowDamage(transform.position, damage);
        statHandler.TakeDamage(damage);
    }

    public void PlayerSpawnPosition(object value)
    {
        gameObject.transform.position = Vector3.zero;
    }

    public float AttackPower()
    {
        //Debug.Log((statHandler.AttackPower + statModifier.AttackPower) * statModifier.DamageOutcome);
        return (statHandler.AttackPower + statModifier.AttackPower);
    }

    public float DamageOutcome()
    {
        return statModifier.DamageOutcome;
    }

    //이벤트 버스를 재사용하다보니 매개변수가 붙어있어야함
    public void BossCinematicChangeControlFlag(object obj)
    {
        canControl = !canControl;
        canUIInteraction = !canUIInteraction;

        if (canControl == false)
        {
            Invoke(nameof(BossCinematicChangeControlFlag), 4f);
        }
    }

    public void OnCanControl(object obj)
    {
        //Logger.Log("여기가 아닌가?");
        //canControl = true;


    }
    
    public void OffCanControl(object obj)
    {
        canControl = false;
    }

    //매개변수가 없는 오버로딩용
    public void BossCinematicChangeControlFlag()
    {
        canControl = true;
        canUIInteraction = true;
    }

    private void MoveSpawnPoint(object value)
    {
        Logger.Log("MoveSpawnPoint");
        Logger.Log(SceneManager.GetActiveScene().name);
        switch (SceneManager.GetActiveScene().name)
        {
            case "VillageScene":
                gameObject.transform.position = new Vector2(-10, 12);
                if (GameManager.Instance.state == GameState.Village)
                {
                    StaticNoticeManager.Instance.ShowMainNotice("우측 상단으로 이동해, 던전으로 진입해보자.");    
                }
                break;
        }
    }
}