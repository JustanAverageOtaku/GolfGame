using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBarController : MonoBehaviour
{
    [SerializeField] Vector2 offsetFromCameraCentre;
    [SerializeField] GameObject pointer;
    [SerializeField] GameObject goalArea;
    [SerializeField] GameObject camera;
    [SerializeField] GolfBallController gbc;

    Collider2D powerBarRect;
    Collider2D goalAreaRect;
    Collider2D pointerRect;

    float requiredDistanceRatio = 0;
    float distance = 0;

    bool focused = false;
    // Start is called before the first frame update
    void Start()
    {
        //Collider2D powerBarCollider = gameObject.GetComponent<Collider2D>();
        powerBarRect = gameObject.GetComponent<Collider2D>(); //new Rect(powerBarCollider.bounds.min.x, powerBarCollider.bounds.min.y, powerBarCollider.bounds.size.x, powerBarCollider.bounds.size.y);

        //Collider2D goalAreaCollider = goalArea.GetComponent<Collider2D>();
        goalAreaRect = goalArea.GetComponent<Collider2D>(); //new Rect(goalAreaCollider.bounds.min.x, goalAreaCollider.bounds.min.y, goalAreaCollider.bounds.size.x, goalAreaCollider.bounds.size.y);

        //Collider2D pointerCollider = pointer.GetComponent<Collider2D>();
        pointerRect = pointer.GetComponent<Collider2D>(); //new Rect(pointerCollider.bounds.min.x, pointerCollider.bounds.min.y, pointerCollider.bounds.size.x, pointerCollider.bounds.size.y);

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (focused && (Input.GetKeyDown(KeyCode.X) || MobileControls.instance.GetMobileButtonDown("HitBall"))) {
            float pd; //= (goalAreaRect.bounds.min.x < pointerRect.bounds.max.x ^ goalAreaRect.bounds.max.x < pointerRect.bounds.min.x) ? distance : distance * ((pointer.transform.localPosition.x + 0.5f) / (goalArea.transform.position.x + 0.5f)); //(goalArea.transform.localPosition.x + pointer.transform.localPosition.x) + requiredDistanceRatio;  //Mathf.Max(goalAreaRect.bounds.center.x - pointerRect.bounds.min.x, goalAreaRect.bounds.center.x - pointerRect.bounds.max.x);
            Debug.Log((goalAreaRect.bounds.max.x > pointerRect.bounds.center.x) && (goalAreaRect.bounds.min.x < pointerRect.bounds.center.x)); // (Mathf.Abs(goalAreaRect.bounds.max.x) > Mathf.Abs(pointerRect.bounds.center.x)) && (Mathf.Abs(goalAreaRect.bounds.min.x) < Mathf.Abs(pointerRect.bounds.center.x))
            if ((goalAreaRect.bounds.max.x > pointerRect.bounds.center.x) && (goalAreaRect.bounds.min.x < pointerRect.bounds.center.x))
            {
                Debug.Log("If");
                pd = distance;
            }
            else 
            {
                Debug.Log("else");
                pd = distance * ((pointer.transform.localPosition.x + 0.5f) / (goalArea.transform.localPosition.x + 0.5f));
            }
            Debug.Log(distance);
            Debug.Log((goalAreaRect.bounds.max.x < pointerRect.bounds.min.x) ^ (goalAreaRect.bounds.min.x > pointerRect.bounds.max.x)); //distance * ((pointer.transform.localPosition.x + 0.5f) / (goalArea.transform.position.x + 0.5f)));
            //Debug.Log(goalAreaRect.bounds.min.x > pointerRect.bounds.max.x);
            Debug.Log("Goal Max X: " + goalAreaRect.bounds.max.x);
            Debug.Log("Goal Min X: " + goalAreaRect.bounds.min.x);
            Debug.Log("Pointer Min X: " + pointerRect.bounds.center.x);
            Debug.Log("Pointer Max X: " + pointerRect.bounds.max.x);
            Debug.Log(distance * ((pointer.transform.localPosition.x + 0.5f) / (goalArea.transform.localPosition.x + 0.5f)));
            Debug.Log(pd);
            gbc.SetPuttingDistance(pd);
            focused = false;
            gbc.SetFoucsed(true);
            //gbc.TransitionToBall();
            gameObject.SetActive(false); ;
            return;
        }

        MovePointer();
    }

    void MovePointer() 
    {
        //Debug.Log(Mathf.Sin(Time.time));
        pointer.transform.position = new Vector3(Mathf.Abs(powerBarRect.bounds.size.x * Mathf.Sin(Time.time)) + powerBarRect.bounds.min.x, transform.position.y, transform.position.z);
    }

    public void DisplayPowerBar()
    {
        transform.position = new Vector3(camera.transform.position.x + offsetFromCameraCentre.x, camera.transform.position.y + offsetFromCameraCentre.y, transform.position.z);

        //powerBarRect.center = transform.position;
        pointer.transform.position = new Vector3(powerBarRect.bounds.min.x, transform.position.y, transform.position.z);
        //goalArea.transform.position = new Vector3((powerBarRect.bounds.size.x * requiredDistanceRatio) + powerBarRect.bounds.min.x, transform.position.y, transform.position.z);
        goalArea.transform.localPosition = new Vector3(Mathf.Clamp(requiredDistanceRatio - 0.5f, -0.5f, 0.5f), 0, 0);
        //Debug.Log(goalArea.transform.position);
        //Debug.Log(requiredDistanceRatio);
        focused = true;
        
        gameObject.SetActive(true);
    }

    public void SetDistance(float d) 
    {
        distance = d;
    }

    public void SetDistanceRatio(float dr)
    {
        requiredDistanceRatio = dr;
    }

    public void DeactivatePowerBar() 
    {
        gameObject.SetActive(false);
    }

    public void SetFocused(bool f) 
    {
        focused = f;
    }

}
