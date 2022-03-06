using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleController : MonoBehaviour
{
    [SerializeField] float minReticleSize = 1;
    [SerializeField] float maxDefaultReticleSize = 5;
    [SerializeField] float maxSandReticleSize = 3;
    [SerializeField] float maxDefualtTerrainDistance = 10;
    [SerializeField] float reticleSpeed = 7;
    [SerializeField] float maxSandTerrainDistance = 5;  //Just add similar lines if you want to implement this for more terrains 
    [SerializeField] float cameraTransitionDurationToReticle = 1.0f;
    [SerializeField] GameObject player;
    [SerializeField] CameraFollow cf;

    GameObject goalHole;
    GolfBallController gbc;
    PlayerController pc;

    float maxReticleSize;
    float maxDistanceFromPlayer;

    bool focused = false;


    private void Start()
    {
        pc = player.GetComponent<PlayerController>();
        gbc = pc.GetCurrentGolfBall().GetComponent<GolfBallController>();
        //goalHole = gbc.GetGoalHole();
        gameObject.SetActive(false);
        maxDistanceFromPlayer = maxDefualtTerrainDistance;
        maxReticleSize = maxDefaultReticleSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (focused && (Input.GetKeyDown(KeyCode.X) || MobileControls.instance.GetMobileButtonDown("HitBall"))) {
            gbc.SetTargetPos(GenerateRandomPointInReticle());
            pc.SetState(2);
            gbc.TransitionToBallShadow();
            //cf.TransitionCamera(gbc.GetBallShadow());
            gbc.SetFoucsed(true);

            focused = false;
            gameObject.SetActive(false);
        }

        MoveReticle();
    }

    void MoveReticle() 
    {
        if (!cf.GetIfTransitioning()) {
            Vector2 movementVector = new Vector2((Input.GetAxis("Horizontal") + MobileControls.instance.GetJoystick("Joystick").x), (Input.GetAxis("Vertical") + MobileControls.instance.GetJoystick("Joystick").y)) * Time.deltaTime * reticleSpeed;
            Vector2 newPos = (Vector2)transform.position + movementVector;
            Vector2 offsetFromPlayer = newPos - (Vector2)player.transform.position;

            transform.position = (Vector2)player.transform.position + Vector2.ClampMagnitude(offsetFromPlayer, maxDistanceFromPlayer);
            float currentReticalScale = Mathf.Max(minReticleSize, maxReticleSize * (offsetFromPlayer.magnitude / maxDistanceFromPlayer)); ;
            transform.localScale = new Vector2(currentReticalScale, currentReticalScale);
        }
    }

    Vector2 GenerateRandomPointInReticle() 
    {
        CircleCollider2D reticleCollider = GetComponent<CircleCollider2D>();
        Vector2 randomPoint = new Vector2(Random.Range(-transform.localScale.x / 2, transform.localScale.x / 2), Random.Range(-transform.localScale.x / 2, transform.localScale.x / 2));
        //Debug.Log(randomPoint);
        //Debug.Log(randomPoint.normalized);
        float randValue = Random.Range(-transform.localScale.x/2, transform.localScale.x/2);
        //Debug.Log(randValue);
        //Debug.Log((randomPoint.normalized * (randValue)));
        randomPoint = (Vector2)transform.position + (randomPoint.normalized * (randValue));
        //Debug.Log("Random Point: " + randomPoint);
        //Debug.Log("Position: " + transform.position);
        return randomPoint;
    }

    public void TransitionToReticle() 
    {
        cf.TransitionCamera(this.gameObject, cameraTransitionDurationToReticle);
    }

    public void UpdateReticleTarget(GameObject gh) 
    {
        goalHole = gh;
    }

    public void SetDefualtGroundValues() 
    {
        maxDistanceFromPlayer = maxDefualtTerrainDistance;
        maxReticleSize = maxDefaultReticleSize;
    }

    public void SetSandTerrainValues() 
    {
        maxDistanceFromPlayer = maxSandTerrainDistance;
        maxReticleSize = maxSandReticleSize;
    }

    public void PushReticleInDirectionOfGoal() 
    {
        //Debug.Log("Player position: " + player.transform.position + "  Goal Position: " + goalHole.transform.position);
        Vector2 goalPlayerDistance = goalHole.transform.position - player.transform.position;
        //Debug.Log("Goal Player Distance: " + goalPlayerDistance);
        float distance = Mathf.Min(maxDistanceFromPlayer, goalPlayerDistance.magnitude);
        transform.position = player.transform.position;
        transform.position += (Vector3)goalPlayerDistance.normalized * (distance / 2.0f);
    }

    public float GetDistanceScalingFactor(float curDistance) 
    {
        return curDistance / maxDistanceFromPlayer;
    }

    public void SetFocused(bool f) 
    {
        focused = f;
    }
}
