using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float maxPlayerSpeed = 10;
    [SerializeField] float cameraTransitionDurationToPlayer = 1.0f;
    [SerializeField] GameObject golfBall;
    [SerializeField] CameraFollow cf;
    [SerializeField] GameObject reticle;

    ReticleController reticleController;

    bool focused = true;
    //delegate void Statefunction();
    //Statefunction[] stateDelegates = { new Statefunction(PlayerAim) };
    
    enum PlayerStates { NOT_GOLFING, AIMING, SHOOTING, PUTTING }
    PlayerStates currentPlayerState = PlayerStates.NOT_GOLFING;

    private void Start()
    {
        reticleController = reticle.GetComponent<ReticleController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPlayerState == PlayerStates.AIMING) {
            if (focused) {
                focused = false;
                PlayerAim();
            }

            return;
        }

        if (currentPlayerState == PlayerStates.SHOOTING) {
            return;
        }

        if (currentPlayerState == PlayerStates.PUTTING)
        {
            return;
        }


        PlayerMove();
        /*
        float x = Input.GetAxis("Horizontal") * Time.deltaTime;
        float y = Input.GetAxis("Vertical") * Time.deltaTime;

        //Vector2 translation = new Vector2(x, y);
        //translation.Normalize();
        transform.Translate(new Vector2(x, y) * maxPlayerSpeed);
         */

    }

    void PlayerMove() 
    {
        if (!cf.GetIfTransitioning()) {
            float x = (Input.GetAxis("Horizontal") + MobileControls.instance.GetJoystick("Joystick").x) * Time.deltaTime;
            float y = (Input.GetAxis("Vertical") + MobileControls.instance.GetJoystick("Joystick").y) * Time.deltaTime;

            //Vector2 translation = new Vector2(x, y);
            //translation.Normalize();
            transform.Translate(new Vector2(x, y) * maxPlayerSpeed);
        }
    }

    void PlayerAim() 
    {
        reticleController.PushReticleInDirectionOfGoal();
        reticle.SetActive(true);
        reticleController.TransitionToReticle();
        //cf.TransitionCamera(reticle);
        reticleController.SetFocused(true);
    }


    public void TransitionToPlayer() 
    {
        cf.TransitionCamera(this.gameObject, cameraTransitionDurationToPlayer);
    }

    public int GetState() 
    {
        return (int)currentPlayerState;
    }

    public void SetState(int stateNum) 
    {
        currentPlayerState = (PlayerStates)stateNum;
    }

    public GameObject GetCurrentGolfBall() 
    {
        return golfBall;
    }

    public void SetFocused(bool f) 
    {
        focused = f;
    }
}
