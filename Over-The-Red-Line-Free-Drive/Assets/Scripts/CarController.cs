using UnityEngine;

public class CarController : MonoBehaviour
{
    public float acceleration = 8f;
    public float deceleration = 3f;
    public float brakeStrength = 10f;
    public float turnSpeed = 100f;

    public int currentGear = 0; // -1 = R, 0 = N, 1..5 = forward

    public float reverseMaxSpeed = 5f;
    public float[] gearMaxSpeeds = { 0f, 8f, 12f, 16f, 20f, 25f };

    private float moveInput;
    private float turnInput;
    private float currentSpeed;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        HandleGearInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleTurning();
    }

    void HandleGearInput()
    {
        if(Input.GetKeyDown(KeyCode.R)){
            currentGear = -1;
        }

        if(Input.GetKeyDown(KeyCode.N)){
            currentGear = 0;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)){
            currentGear = 1;
        }

        if(Input.GetKeyDown(KeyCode.Alpha2)){
            currentGear = 2;
        }

        if(Input.GetKeyDown(KeyCode.Alpha3)){
            currentGear = 3;
        }

        if(Input.GetKeyDown(KeyCode.Alpha4)){
            currentGear = 4;
        }

        if(Input.GetKeyDown(KeyCode.Alpha5)){
            currentGear = 5;
        }

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E)){
            if(currentGear < 5){
                currentGear++;
            }
        }

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Q)){
            if(currentGear > -1){
                currentGear--;
            }
        }
    }

    void HandleMovement()
    {
        float targetSpeed = 0f;

        if(currentGear > 0){
            if(moveInput > 0f){
                targetSpeed = gearMaxSpeeds[currentGear];
            }
        }
        else if(currentGear == -1){
            if(moveInput > 0f){
                targetSpeed = -reverseMaxSpeed;
            }
        }

        if(Input.GetKey(KeyCode.S)){
            if(currentSpeed > 0f){
                currentSpeed -= brakeStrength * Time.fixedDeltaTime;
                if(currentSpeed < 0f){
                    currentSpeed = 0f;
                }
            }
            else if(currentSpeed < 0f){
                currentSpeed += brakeStrength * Time.fixedDeltaTime;
                if(currentSpeed > 0f){
                    currentSpeed = 0f;
                }
            }
        }
        else{
            if(currentSpeed < targetSpeed){
                currentSpeed += acceleration * Time.fixedDeltaTime;
                if(currentSpeed > targetSpeed){
                    currentSpeed = targetSpeed;
                }
            }
            else if(currentSpeed > targetSpeed){
                currentSpeed -= deceleration * Time.fixedDeltaTime;
                if(currentSpeed < targetSpeed){
                    currentSpeed = targetSpeed;
                }
            }
        }

        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    void HandleTurning()
    {
        if(currentGear == 0){
            return;
        }

        if(Mathf.Abs(currentSpeed) < 0.1f){
            return;
        }

        if(Mathf.Abs(turnInput) < 0.01f){
            return;
        }

        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;

        if(currentSpeed < 0f){
            turn *= -1f;
        }

        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }
}