using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of the player movement
    public float runSpeed = 10f; // Speed of the player when running
    public float jumpForce = 5f; // Force applied when the player jumps
    public float gravity = -9.81f; // Gravity applied to the player
    public float maxFallSpeed = -20f; // Maximum fall speed to prevent excessive falling speed
    public float maxHorizontalSpeed = 10f; // Maximum horizontal speed to prevent excessive horizontal speed
    public float drag = 0.1f; // Drag applied to the player to slow down movement over time
    public GameObject groundCheckObject; // Object used to check if the player is grounded, we'll start just by making it work if it collides with any object
    
    private Rigidbody rb; // Reference to the Rigidbody component, if we decide to use one later
    public bool isGrounded = false; // Flag to check if the player is on the ground
    private Vector3 velocity; // Current velocity of the player, so we can hopefully avoid using a rigidbody. I feel better not using one because it gives me more freedom to control the player movement and physics directly.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component if it exists

    }

    bool CheckGrounded()
    {
        // Check if the groundCheckObject is colliding with any object
        Collider[] colliders = Physics.OverlapSphere(groundCheckObject.transform.position, 0.25f);

        foreach (Collider collider in colliders)
        {
            Component[] components = gameObject.GetComponentsInChildren<Component>();
            if (collider.gameObject == gameObject) // Ignore self
            {
                continue;
            }
            bool isChild = false;
            foreach (Component component in components)
            {
                if (component.gameObject == collider.gameObject)
                {
                    isChild = true; // Ignore children
                }
            }
            if (isChild)
            {
                continue;
            }
            return true;
        }
        return false; // Player is not grounded
    }


    private void FixedUpdate()
    {

        //Handle player movement, and keep the rigidbody standing up
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Reset horizontal velocity to zero
        }
        else
        {
            velocity.x = 0; // Reset horizontal velocity to zero
        }
        // Get input for horizontal movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized; // Normalize the direction vector
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize(); // Ensure the direction vector has a magnitude of 1
        }
        // Calculate the target speed based on input
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed; // Use run speed if Left Shift is pressed, otherwise use normal speed
        // Calculate the desired velocity
        Vector3 desiredVelocity = moveDirection * targetSpeed;
        // Apply drag to the velocity
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(Mathf.Clamp(desiredVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed), rb.linearVelocity.y, Mathf.Clamp(desiredVelocity.z, -maxHorizontalSpeed, maxHorizontalSpeed));
        }
        else
        {
            velocity.x = Mathf.Clamp(desiredVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
            velocity.z = Mathf.Clamp(desiredVelocity.z, -maxHorizontalSpeed, maxHorizontalSpeed);
        }
        // Apply gravity
        if (rb != null)
        {
            rb.AddForce(new Vector3(0, gravity, 0), ForceMode.Acceleration); // Apply gravity force
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, maxFallSpeed), rb.linearVelocity.z); // Limit fall speed
        }
        else
        {
            velocity.y += gravity * Time.fixedDeltaTime; // Apply gravity to the vertical velocity
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed); // Limit fall speed
        }
        // Handle jumping
        if (rb != null)
        {
            if (!isGrounded)
            {
                rb.AddForce(new Vector3(0, gravity, 0), ForceMode.Acceleration); // Apply gravity force
            }
        }
        else
        {
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpForce; // Set vertical velocity to jump force
            }
            else
            {
                velocity.y += gravity * Time.fixedDeltaTime; // Apply gravity to the vertical velocity

            }
        }
        // Apply the velocity to the player
        if (isGrounded) {
            velocity.y = 0;
        }
        if (velocity.y < 0) {
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed); // Limit fall speed
        }
        if (rb != null) {
            rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime); // Move the player using Rigidbody
        }
        else
        {
            transform.position += velocity * Time.fixedDeltaTime; // Move the player directly
        }
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = CheckGrounded(); // Update grounded state

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (rb != null)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Apply jump force using Rigidbody
            }
            else
            {
                velocity.y = jumpForce; // Set vertical velocity to jump force
            }
        }

        //Ensure the player is always upright
        if (rb != null)
        {
            rb.rotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0); // Keep the player upright
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); // Keep the player upright
        }
    }
}
