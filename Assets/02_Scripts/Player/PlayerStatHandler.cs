using _02_Scripts.UI;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using SmallScaleInc.TopDownPixelCharactersPack1;
using Cysharp.Threading.Tasks;
using System.Collections;

public class PlayerStatHandler : MonoBehaviour
{
    [Header("기본 스탯")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxMana = 50f;
    [SerializeField] private float attackPower = 10f;
    [SerializeField] private float recoverManaPerTic = 1;
    [SerializeField] private PlayerStatUIHandler playerStatUIHandler;
    [SerializeField] private ActivePotionSlotController[] activePotionSlotControllers;
    [SerializeField] private float manaTic = 1f;
    [SerializeField] private int rerollCount;

    [SerializeField] private Color hitColor;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private SpriteRenderer playerSprite;
    private PlayerInputActions inputActions;
    private int invincibilityLayer;
    private int playerLayer;

    private float manaTicTimer;
    [SerializeField] public float currentHealth;
    [SerializeField] private float currentMana;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    public float AttackPower => attackPower;

    private void OnEnable()
    {
        if (SaveManager.Instance.inputActions == null)
        {
            return;
        }
        inputActions = SaveManager.Instance.inputActions;
        inputActions.Player.Enable();
        inputActions.Player.Use_Potion_1.performed += UsePotion1Performed;
        inputActions.Player.Use_Potion_2.performed += UsePotion2Performed;

        EventBus.Subscribe("PlayerClearRoom", MpRefillEvent);
    }

    private void OnDisable()
    {
        if (inputActions == null) return;
        inputActions.Player.Use_Potion_1.performed -= UsePotion1Performed;
        inputActions.Player.Use_Potion_2.performed -= UsePotion2Performed;
        inputActions.Player.Disable();
        EventBus.Unsubscribe("PlayerClearRoom", MpRefillEvent);
    }


    private void Awake()
    {
        invincibilityLayer = LayerMask.NameToLayer("Invincibility");
        playerLayer = LayerMask.NameToLayer("Player");
        playerAnimationController = GetComponentInChildren<PlayerAnimationController>();
        currentHealth = maxHealth + PlayerController.Instance.GetStatModifier().MaxHealth();
        currentMana = maxMana;
        manaTicTimer = 1f;
        //inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        playerStatUIHandler.ChangeHP(maxHealth, maxHealth + PlayerController.Instance.GetStatModifier().MaxHealth());
        playerStatUIHandler.ChangeMP(maxMana, maxMana);
    }

    public void Update()
    {
        if (manaTicTimer <= 0)
        {
            RecoverMana(recoverManaPerTic);
            manaTicTimer = manaTic;
        }
        else
        {
            manaTicTimer -= Time.deltaTime;
        }
    }

    // 체력 처리
    public void TakeDamage(float damage)
    {
        GameManager.Instance.totalDamageTaken += (int)damage; // 누적 받은 데미지 저장

        currentHealth = Mathf.Max(currentHealth - (damage), 0);

        playerStatUIHandler.ChangeHP(currentHealth, maxHealth + PlayerController.Instance.GetStatModifier().MaxHealth());
        if (currentHealth <= 0.4f * (maxHealth + PlayerController.Instance.GetStatModifier().MaxHealth())) { playerStatUIHandler.OnOffAlert(true); }
        else { playerStatUIHandler.OnOffAlert(false); }

        if (currentHealth <= 0)
        {
            playerAnimationController.animator.SetTrigger("Die");
            StartCoroutine(playerAnimationController.PauseAnimatorAtEnd());
            PlayerController.Instance.canControl = false;
            PlayerController.Instance.isDead = true;
            GameManager.Instance.ChangeState(GameState.GameOver);
            return;
        }
        StartCoroutine(Invincibility());
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + (amount * PlayerController.Instance.GetStatModifier().HealEfficiency()), maxHealth + PlayerController.Instance.GetStatModifier().MaxHealth());
        playerStatUIHandler.ChangeHP(currentHealth, maxHealth + PlayerController.Instance.GetStatModifier().MaxHealth());
    }

    // 마나 처리
    public bool UseMana(float amount)
    {
        if (currentMana < amount) return false;

        currentMana -= amount;
        playerStatUIHandler.ChangeMP(currentMana, maxMana);
        return true;
    }

    public void RecoverMana(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        playerStatUIHandler.ChangeMP(currentMana, maxMana);
    }

    private void UsePotion1Performed(InputAction.CallbackContext context)
    {
        UsePotion(0);
    }

    private void UsePotion2Performed(InputAction.CallbackContext context)
    {
        UsePotion(1);
    }

    public void UsePotion(int index)
    {
        PotionData potionData = activePotionSlotControllers[index].GetPotion();
        int potionAmount = activePotionSlotControllers[index].potionAmount;
        if (potionData != null && potionAmount >= 1)
        {
            Heal(potionData.hpRecover);
            RecoverMana(potionData.mpRecover);
            activePotionSlotControllers[index].UseAmount(1);
        }
        else
        {
            StaticNoticeManager.Instance.ShowSideNotice("사용가능한 포션이 없습니다.", 1f);
        }
    }

    public int GetRerollCount()
    {
        return rerollCount;
    }

    public bool RerollCount()
    {
        return --rerollCount >= 0;
    }

    public void PlusRerollCount(int i)
    {
        rerollCount += i;
    }

    public void ResetRerollCount(object param)
    {
        rerollCount = 0;
    }

    public void Resurrection() // 최대 체력으로 부활 (회복)
    {
        playerAnimationController.animator.enabled = true;
        playerAnimationController.animator.speed = playerAnimationController.AnimationSpeed;
        currentHealth = MaxHealth + PlayerController.Instance.GetStatModifier().MaxHealth();
        currentMana = MaxMana;
        playerStatUIHandler.OnOffAlert(false);
        playerStatUIHandler.ChangeHP(currentHealth, maxHealth + PlayerController.Instance.GetStatModifier().MaxHealth());
    }

    public void MpRefill()
    {
        currentMana = MaxMana;
        playerStatUIHandler.ChangeMP(currentMana, maxMana);
    }

    public void MpRefillEvent(object obj)
    {
        MpRefill();
    }
    private IEnumerator Invincibility()
    {
        PlayerController.Instance.isInvincible = true;
        playerSprite.color = hitColor;
        yield return YieldCache.WaitForSeconds(0.5f);
        if (!gameObject.activeInHierarchy)
        {
            playerSprite.color = Color.white;
            gameObject.layer = playerLayer;
            yield break;
        }
        
        playerSprite.color = Color.white;
        PlayerController.Instance.isInvincible = false;

    }
}