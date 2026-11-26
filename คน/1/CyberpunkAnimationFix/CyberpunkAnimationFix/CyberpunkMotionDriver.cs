using UnityEngine;

[DefaultExecutionOrder(1000)]
public sealed class CyberpunkMotionDriver : MonoBehaviour
{
    public Animator animator;
    public Transform movementRoot;
    public CharacterController characterController;
    public float runThreshold = 3.4f;
    public float idleThreshold = 0.12f;
    public float crossFade = 0.12f;
    Vector3 previous;
    bool wasGrounded;
    float landingUntil;
    string active;
    void Start()
    {
        if (!movementRoot) movementRoot = transform;
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!characterController) characterController = movementRoot.GetComponent<CharacterController>();
        if (!animator || !animator.runtimeAnimatorController || !characterController)
        {
            Debug.LogError("Cyberpunk: assign Animator, controller asset and Player CharacterController.", this);
            enabled = false; return;
        }
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        foreach (string state in new[]{"Idle","Walk","Run","Jump","Land"})
            if (!animator.HasState(0, Animator.StringToHash("Base Layer." + state)))
            { Debug.LogError("Cyberpunk: missing state " + state, this); enabled = false; return; }
        previous = movementRoot.position;
        wasGrounded = characterController.isGrounded;
        Play("Idle");
    }
    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Vector3 velocity = (movementRoot.position - previous) / dt;
        previous = movementRoot.position;
        bool grounded = characterController.isGrounded;
        float speed = new Vector2(velocity.x, velocity.z).magnitude;
        if (grounded && !wasGrounded) landingUntil = Time.time + 0.25f;
        if (!grounded) { landingUntil = 0; Play("Jump"); }
        else if (Time.time < landingUntil) Play("Land");
        else Play(speed < idleThreshold ? "Idle" : speed < runThreshold ? "Walk" : "Run");
        wasGrounded = grounded;
    }
    void Play(string state)
    {
        if (active == state) return;
        active = state;
        animator.CrossFadeInFixedTime(Animator.StringToHash("Base Layer." + state), crossFade, 0, 0f);
    }
}
