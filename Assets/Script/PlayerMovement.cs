using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool MovementLocked { get; private set; } = false;
    public float BodyRotationY { get; private set; } = 0;

    [SerializeField]
    float moveSpeed = 2f; // Velocidade inicial
    private const float MIN_SPEED = 3f;
    private const float MAX_SPEED = 8f;
    private const float SPRINT_SPEED = 12f;

    private Vector3 moveDirection = Vector3.zero; // Direção de movimento
    private Rigidbody rb; // Rigidbody 3D
    public bool Sprinting { get; private set; } = false;

    public float Speed
    {
        get => rb.velocity.magnitude;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Certifique-se de usar Rigidbody 3D
        if (rb == null)
        {
            Debug.LogError("Rigidbody não encontrado! Certifique-se de adicioná-lo ao objeto.");
        }
    }

    void Update()
    {
        if (MovementLocked)
            BodyRotationY = transform.rotation.eulerAngles.y; // turn body to interaction object
        else if (Speed > 0.01f)
            BodyRotationY = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

    }

    void FixedUpdate()
    {
        if (MovementLocked)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        Move();
    }

    private void Move()
    {
        moveDirection = Vector3.zero; // Reinicia a direção
        Sprinting = Input.GetKey(KeyCode.LeftShift); // Verifica sprint

        // Direções de movimento
        if (Input.GetKey(KeyCode.W))
            moveDirection += transform.forward; // Para frente
        if (Input.GetKey(KeyCode.S))
            moveDirection -= transform.forward; // Para trás
        if (Input.GetKey(KeyCode.A))
            moveDirection -= transform.right; // Para a esquerda
        if (Input.GetKey(KeyCode.D))
            moveDirection += transform.right; // Para a direita

        if (moveDirection != Vector3.zero)
        {
            moveDirection = moveDirection.normalized; // Normaliza a direção

            // Atualiza velocidade baseada em sprint
            if (Sprinting)
            {
                moveSpeed = Mathf.Clamp(
                    moveSpeed + Time.fixedDeltaTime * 7,
                    MIN_SPEED,
                    SPRINT_SPEED
                );
            }
            else
            {
                moveSpeed = Mathf.Clamp(moveSpeed + Time.fixedDeltaTime * 4, MIN_SPEED, MAX_SPEED);
            }

            // Move o Rigidbody com velocidade limitada
            Vector3 newVelocity = moveDirection * moveSpeed;
            newVelocity.y = rb.velocity.y; // Mantém o valor de gravidade no eixo Y
            rb.velocity = newVelocity;
        }
        else
        {
            // Desaceleração gradual quando o jogador para
            Vector3 velocity = rb.velocity;
            velocity.x *= 0.8f;
            velocity.z *= 0.8f;
            rb.velocity = velocity;

            moveSpeed = Mathf.Clamp(moveSpeed - Time.fixedDeltaTime * 7, MIN_SPEED, MAX_SPEED);
        }
    }

    public void LockMovement() => MovementLocked = true;

    public void UnlockMovement() => MovementLocked = false;

    public void SetBodyRotationY(float degreeAngle) => BodyRotationY = degreeAngle;
}
