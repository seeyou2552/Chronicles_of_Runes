using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using _02_Scripts;
using Cysharp.Threading.Tasks;
using SmallScaleInc.TopDownPixelCharactersPack1;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    public enum MovementMode { RelativeToMouse, ClickToMove, Cardinal, WASDOnly }
    public enum AttackMode { Melee, Ranged }
    public enum IdleVariant { Idle1 = 1, Idle2, Idle3, Idle4 }

    [Header("Settings")]
    public MovementMode movementMode = MovementMode.RelativeToMouse;
    public AttackMode attackMode = AttackMode.Melee;
    public IdleVariant idleVariant = IdleVariant.Idle1;
    public float basePlaybackSpeed = 1f;
    public float alternatePlaybackSpeed = 2f;
    public bool allowPlaybackSpeedToggle = true;
    public float AnimationSpeed = 2f;

    public event Action<string> OnAnimationTriggerSent;

    public Animator animator;
    private Camera mainCamera;
    private PlayerInputActions inputActions;
    SpriteRenderer sr;

    private Vector2 moveInput;
    private Vector3 clickTarget;
    private bool hasClickTarget = false;
    private bool isMounted = false;
    private bool isUsingAlternateSpeed = false;
    private Vector2 lastFacingDir = Vector2.right;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
    void LateUpdate()
    {
        // y 좌표가 낮을수록 앞에 오도록 음수로 변환
        //최소 : 0 최대 : 100
        int rawOrder = Mathf.RoundToInt((-transform.position.y + 50));
        int clamped = Mathf.Clamp(rawOrder, 0, 100);
        sr.sortingOrder = clamped;
    }
    private void Update()
    {
        Vector2 aimDir = GetMouseDirection();
        Vector2 forwardDir = aimDir; // 마우스 방향 기준
        if (forwardDir != Vector2.zero) lastFacingDir = forwardDir;

        Vector2 rightDir = new Vector2(forwardDir.y, -forwardDir.x);
        int dirIdx = GetDirectionIndex(forwardDir);
        if (!PlayerController.Instance.canControl)
        {
            OnIdle(dirIdx);
            return;
        }
        HandleMoveAnimation(forwardDir, rightDir, dirIdx);
        ApplyAnimatorPlaybackSpeed();
    }

    private void HandleMoveAnimation(Vector2 forwardDir, Vector2 rightDir, int dirIdx)
    {
        OnIdle(dirIdx);
        SetIdleVariant();

        // 기존 dot 값 계산
        float dotF = Vector2.Dot(moveInput.normalized, forwardDir);
        float dotB = Vector2.Dot(moveInput.normalized, -forwardDir);
        float dotR = Vector2.Dot(moveInput.normalized, rightDir);
        float dotL = Vector2.Dot(moveInput.normalized, -rightDir);

        // 방향별 임계값
        const float THRESHOLD = 0.5f;
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool("IsRun", isMoving && dotF >= THRESHOLD);
        animator.SetBool("IsWalk", false); // 필요시 따로 처리
        animator.SetBool("IsRunBackwards", isMoving && dotB >= THRESHOLD);
        animator.SetBool("IsStrafeLeft", isMoving && dotL >= THRESHOLD);
        animator.SetBool("IsStrafeRight", isMoving && dotR >= THRESHOLD);

        // 예외 처리: dotF가 거의 1에 가까울 때 명시적으로 Run 실행
        if (isMoving && dotF > 0.95f)
        {
            animator.SetBool("IsRun", true);
        }
    }

    public void HandleAttack(string attackAnimation)
    {
        animator.SetTrigger(attackAnimation);
        OnAnimationTriggerSent?.Invoke(attackAnimation);
    }

    private void ApplyAnimatorPlaybackSpeed()
    {
        animator.speed = 2f;
    }

    private Vector2 GetMouseDirection()
    {
        Vector3 mouseWorld = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0;
        return ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
    }

    private void SetIdleVariant()
    {
        animator.SetBool("UseIdle2", idleVariant == IdleVariant.Idle2);
        animator.SetBool("UseIdle3", idleVariant == IdleVariant.Idle3);
        animator.SetBool("UseIdle4", idleVariant == IdleVariant.Idle4);
    }

    private int GetDirectionIndex(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        if (angle >= 337.5f || angle < 22.5f) return 0;
        if (angle < 67.5f) return 4;
        if (angle < 112.5f) return 3;
        if (angle < 157.5f) return 5;
        if (angle < 202.5f) return 1;
        if (angle < 247.5f) return 7;
        if (angle < 292.5f) return 2;
        return 6;
    }

    private void OnIdle(int dirIdx)
    {
        animator.SetInteger("DirIndex", dirIdx);
        animator.SetFloat("Direction", dirIdx);
        animator.SetBool("IsRun", false);
        animator.SetBool("IsWalk", false); // 필요시 따로 처리
        animator.SetBool("IsRunBackwards", false);
        animator.SetBool("IsStrafeLeft", false);
        animator.SetBool("IsStrafeRight", false);
    }

    //사망 애니메이션용도
    public IEnumerator PauseAnimatorAtEnd()
    {

        animator.speed = 1.5f;
        // 일단 1프레임 대기: GetCurrentAnimatorStateInfo는 재생 요청 직후엔 반영 안 됨
        yield return null;

        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 대충 마지막 프레임정도
            if (stateInfo.normalizedTime >= 0.85f)
            {

                // 애니메이션 정지
                animator.speed = 0f;
                animator.enabled = false;
                break;


            }

            yield return null;
        }
        /*
        float alpha = sr.color.a;
        DoTweenExtensions.TweenFloat(alpha, 0f, 2f, (alpha) =>
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        },
        async () =>
        {
            await UniTask.Delay(2000);
            StateInit();
            //ObjectPoolManager.Instance.Return(targetPos.gameObject, "TargetPos");
            ReturnPool(null);
            if (enemyStat.isBoss)
            {
                BossHpbar.Instance.HPbar.SetActive(false);
            }
        }

        );*/
    }
}
