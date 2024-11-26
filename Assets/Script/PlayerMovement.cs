using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 2f; // Velocidade de movimento
    private float MIN_SPEED = 5f;
    private float MAX_SPEED = 8f;
    private float SPRINT_SPEED = 12f;
    private Vector2 moveInput; // Armazena o valor de movimento
    private Rigidbody rb;
    public bool Sprinting { get; private set; } = false;

    private void Awake() // Mudei OnEnable para Awake
    {
        // Certifique-se de que o Rigidbody está ativo
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() // Mudei Update para FixedUpdate
    {
        Move();
    }

    private void Move()
    {
        Vector3 moveInput = Vector3.zero;
        bool tmp_sprinting = Sprinting;
        Sprinting = Input.GetKey(KeyCode.LeftShift);

        if (tmp_sprinting && !Sprinting) // stopped sprinting
        {
            moveSpeed = MAX_SPEED;
        }
        else if (!tmp_sprinting && Sprinting) // started sprinting
        {
            moveSpeed = SPRINT_SPEED;
        }

        if (Input.GetKey(KeyCode.W)) moveInput += transform.forward; // Para frente
        if (Input.GetKey(KeyCode.S)) moveInput -= transform.forward; // Para trás
        if (Input.GetKey(KeyCode.A)) moveInput -= transform.right; // Para a esquerda
        if (Input.GetKey(KeyCode.D)) moveInput += transform.right; // Para a direita


        moveInput = moveInput.normalized;
        if (moveInput == Vector3.zero) moveSpeed = Mathf.Clamp(moveSpeed - Time.fixedDeltaTime * 2, MIN_SPEED, MAX_SPEED);
        else moveSpeed = Mathf.Clamp(moveSpeed + Time.fixedDeltaTime * 2, MIN_SPEED, MAX_SPEED);

        if (Sprinting) moveSpeed = SPRINT_SPEED;

        rb.velocity = moveInput * moveSpeed;

    }
}