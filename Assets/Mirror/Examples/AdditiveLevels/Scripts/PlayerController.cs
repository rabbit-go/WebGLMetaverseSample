using UnityEngine;

namespace Mirror.Examples.AdditiveLevels
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : NetworkBehaviour
    {
        public CharacterController characterController;
        public Animator animator;

        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float turnSensitivity = 5f;
        public float maxTurnSpeed = 100f;

        [Header("Diagnostics")]
        public float horizontal;
        public float vertical;
        public float turn;
        public float jumpSpeed;
        public bool isGrounded = true;
        public bool isFalling;
        public Vector3 velocity;

        void OnValidate()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            if (animator == null)
                animator = GetComponent<Animator>();
            characterController.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
        }

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;
        }

        void Update()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            // Q and E cancel each other out, reducing the turn to zero
            if (Input.GetKey(KeyCode.Q))
                turn = Mathf.MoveTowards(turn, -maxTurnSpeed, turnSensitivity);
            if (Input.GetKey(KeyCode.E))
                turn = Mathf.MoveTowards(turn, maxTurnSpeed, turnSensitivity);
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);

            if (isGrounded)
                isFalling = false;

            if ((isGrounded || !isFalling) && jumpSpeed < 1f && Input.GetKey(KeyCode.Space))
            {
                jumpSpeed = 3f;
                animator.SetTrigger("Jump");
            }
            else if (!isGrounded)
            {
                isFalling = true;
            }
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);
            if (jumpSpeed > 0)
            {
                jumpSpeed = jumpSpeed - (9.8f * Time.deltaTime);
            }
                Vector3 direction = new Vector3(horizontal, jumpSpeed, vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;

            if (jumpSpeed > 0)
            {
                
                characterController.Move(direction * Time.fixedDeltaTime);
            }
            else
                characterController.SimpleMove(direction);

            isGrounded = characterController.isGrounded;
            velocity = characterController.velocity;
            animator.SetFloat("Direction", velocity.x);
            animator.SetFloat("Speed", velocity.magnitude);
        }
    }
}
