using UnityEngine;

// Attach to existing player root. Existing controller remains responsible for movement and jumping.
public sealed class CharacterAnimationBridge : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;
    public float runSpeed = 5f;
    public float walkSpeed = 2f;
    public float damping = 0.12f;
    private Vector3 lastPosition;
    private bool wasGrounded;
    private bool initialized;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Land = Animator.StringToHash("Land");
    void Awake() {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!controller) controller = GetComponent<CharacterController>();
        if (animator) animator.applyRootMotion = false;
        lastPosition = transform.position;
    }
    void LateUpdate() {
        float dt = Time.deltaTime;
        if (!animator || !controller || dt <= 0f) return;
        Vector3 v = (transform.position - lastPosition) / dt;
        lastPosition = transform.position;
        Drive(v, controller.isGrounded, dt);
    }
    // For a custom movement controller, leave controller empty and call this once per frame.
    public void Drive(Vector3 velocity, bool grounded, float dt) {
        if (!animator) return;
        float horizontal = new Vector2(velocity.x, velocity.z).magnitude;
        float speed = horizontal <= walkSpeed ? horizontal / Mathf.Max(walkSpeed, .01f) : 1f + Mathf.InverseLerp(walkSpeed, Mathf.Max(runSpeed, walkSpeed + .01f), horizontal);
        animator.SetFloat(Speed, speed, damping, dt);
        animator.SetFloat(VerticalSpeed, velocity.y);
        animator.SetBool(Grounded, grounded);
        if (initialized && wasGrounded && !grounded && velocity.y > .1f) animator.SetTrigger(Jump);
        if (initialized && !wasGrounded && grounded) { animator.ResetTrigger(Jump); animator.SetTrigger(Land); }
        wasGrounded = grounded;
        initialized = true;
    }
}
