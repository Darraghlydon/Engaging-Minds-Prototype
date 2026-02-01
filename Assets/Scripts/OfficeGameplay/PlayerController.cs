using UnityEngine;
using UnityEngine.InputSystem;

// Controls player movement and rotation.
public class PlayerController : MonoBehaviour
{
    public float speed;        // Set player movement speed.
    public float rotationSpeed; // Set player rotation speed.

    private Rigidbody rb; 

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }
    
    
    private void FixedUpdate()
    {
        float moveVertical = 0f;
        float turn = 0f;

        // Vertical movement (W / S)
        if (Keyboard.current.wKey.isPressed)
            moveVertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            moveVertical -= 1f;

        // Horizontal rotation (A / D)
        if (Keyboard.current.dKey.isPressed)
            turn += 1f;

        if (Keyboard.current.aKey.isPressed)
            turn -= 1f;

        // Move player forward/backward
        Vector3 movement = transform.forward * moveVertical * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // Rotate player left/right
        float rotationAmount = turn * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotationAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

}