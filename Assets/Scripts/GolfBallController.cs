using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GolfBallController : MonoBehaviour
{
    [SerializeField] float maxBallHeight = 4;
    [SerializeField] float minBallHeight = 0.5f;
    [SerializeField] float maxShotDuration = 4.0f;
    [SerializeField] float minShotDuration = 0.1f;
    [SerializeField] int maxBounceNumber = 3;
    [SerializeField] int minBounceNumber = 1;
    [SerializeField] float bounceDistanceTravelFactor = 0.25f;
    [SerializeField] float cameraTransitionDurationToBall = 1.0f;
    [SerializeField] Vector2[] spawnPoints = new Vector2[8];
    [SerializeField] int[] parValues = new int[8];
    [SerializeField] Text[] scoreTextList = new Text[8];
    [SerializeField] GameObject[] holeList = new GameObject[8];
    [SerializeField] Text totalScoreText;
    [SerializeField] GameObject totalScorePanel;
    [SerializeField] GameObject scorePanel;
    [SerializeField] GameObject player;
    [SerializeField] GameObject shadow;
    [SerializeField] CameraFollow cf;
    [SerializeField] ReticleController rc;
    [SerializeField] PowerBarController pbc;

    Collider2D ballCollider;
    PlayerController pc;
    GameObject prompt;
    GameObject goalHole;

    Vector2 previousLandingCoordinates;
    Vector2 nextSpawnPoint;
    Vector2 targetPos;

    float shotDuration;
    float greenAreaSize = 0;
    float puttingDistance = 0;
    int currentIndex = 0;
    int currentHoleScore = 0;
    int totalScore = 0;

    bool focused = false;
    bool ballInGreen = false;

    // Start is called before the first frame update
    void Start()
    {
        prompt = this.gameObject.transform.GetChild(0).gameObject;
        prompt.SetActive(false);
        scorePanel.SetActive(false);
        totalScorePanel.SetActive(false); 
        ballCollider = this.gameObject.GetComponent<Collider2D>();
        pc = player.GetComponent<PlayerController>();
        goalHole = holeList[currentIndex];
        rc.UpdateReticleTarget(goalHole);
        nextSpawnPoint = spawnPoints[currentIndex];
        previousLandingCoordinates = transform.position;
        shadow.transform.position = transform.position;
    }

    private void Update()
    {
        if (prompt.activeSelf && (Input.GetKeyDown(KeyCode.X) || MobileControls.instance.GetMobileButtonDown("HitBall"))) {
            prompt.SetActive(false);
            currentHoleScore++;
            if (ballInGreen) {
                pbc.DisplayPowerBar();
                //prompt.SetActive(false);
                pc.SetState(3);
                
                //focused = true;

                return;
            }

            pc.SetState(1);
            return;
        }

        
        if (focused && !cf.GetIfTransitioning()) {
            previousLandingCoordinates = transform.position;
            focused = false;

            if (ballInGreen) {
                StartCoroutine(PuntBall());
                return;
            }
            
            StartCoroutine(BallArc());
            //StartCoroutine(MoveShadow());
        }

        if (totalScorePanel.activeSelf && (Input.GetKeyDown(KeyCode.X) || MobileControls.instance.GetMobileButtonDown("HitBall"))) 
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    IEnumerator BallArc()
    {
        Vector2 midpoint = CalculateMidPoint();
        int numberOfBounces = CalculateNumberOfBounces();
        float travelDistance = CalculateTravelDistance();
        Vector2 directionVector = CalculateDirectionvector();
        
        CalculateShotDuration();

        float t = 0.0f;
        Vector2 startPos = transform.position;

        do {
            StartCoroutine(MoveShadow());

            while (t < 1.0f) {
                t += Time.deltaTime * (Time.timeScale / shotDuration);
                transform.position = CalculateQuadraticBezierCurve(startPos, midpoint, targetPos, t);
                yield return 0;
            }

            StopCoroutine(MoveShadow());
            
            if (!IsBounceableTerrain())
            {
                break;
            }

            startPos = transform.position;
            travelDistance = travelDistance * bounceDistanceTravelFactor;
            targetPos = startPos + directionVector * travelDistance;
            midpoint = CalculateMidPoint();
            
            CalculateShotDuration();
            
            //midpoint = startPos + StartEndMidPoint();
            //midpoint.y += ReturnMaxElement(minBallHeight, maxBallHeight); //Mathf.Max(minBallHeight, maxBallHeight * rc.GetDistanceScalingFactor((targetPos - startPos).magnitude));
            //shotDuration = ReturnMaxElement(minShotDuration, maxShotDuration);
            
            t = 0.0f;

            numberOfBounces--;

        } while (numberOfBounces > 0);


        pc.TransitionToPlayer();
        //cf.TransitionCamera(player);
        
        pc.SetState(0);
        pc.SetFocused(true);

        shadow.transform.position = this.transform.position;
        shadow.SetActive(false);
    }
    IEnumerator PuntBall() 
    {
        Vector2 startPosition = transform.position;
        Vector2 distanceVectorToHole = goalHole.transform.position - transform.position;
        Vector2 targetPosition = (Vector2)transform.position + (puttingDistance * distanceVectorToHole.normalized);
        //Debug.Log(distanceVectorToHole.magnitude);
        //Debug.Log(transform.position);
        //Debug.Log(distanceVectorToHole);
        //Debug.Log(targetPosition);
        //bool curvingEnabled = (targetPosition.magnitude > distanceVectorToHole.magnitude) ? true : false;

        float putDuration = Mathf.Max(minShotDuration, maxShotDuration * (targetPosition.magnitude / greenAreaSize)); 
        float t = 0.0f;

        while (t < 1.0f) 
        {
            t += Time.deltaTime * (Time.timeScale / putDuration);
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            yield return 0;
        }

        IsBounceableTerrain();

        pc.TransitionToPlayer();

        pc.SetState(0);
        pc.SetFocused(true);

    }

    IEnumerator MoveShadow()
    {
        shadow.SetActive(true);
        float t = 0.0f;
        Vector2 startPos = transform.position;

        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / shotDuration);
            shadow.transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return 0;
        }

        //shadow.SetActive(false);
    }
    
    Vector2 CalculateQuadraticBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float tt = t * t;

        Vector2 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    bool IsBounceableTerrain()
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Water", "Sand", "Forest", "Hole", "Putting", "Default"));
        
        List<Collider2D> results = new List<Collider2D>();

        int overlaps = ballCollider.OverlapCollider(contactFilter, results);
        //Debug.Log(results[0].gameObject.name);
        rc.SetDefualtGroundValues();

        ballInGreen = false;

        if (results[0].gameObject.layer == LayerMask.NameToLayer("Forest") || results[0].gameObject.layer == LayerMask.NameToLayer("Water")) {
            ResetGolfBallToLastPosition();
            return false;
        }

        else if (results[0].gameObject.layer == LayerMask.NameToLayer("Sand")) {
            rc.SetSandTerrainValues();
            return false;
        }

        else if (results[0].gameObject.layer == LayerMask.NameToLayer("Hole")) {
            results[0].enabled = false;
            //Debug.Log("Here");

            UpdateScore();
            
            currentIndex++;
            if (currentIndex >= holeList.Length) {
                totalScoreText.text = totalScore.ToString();
                totalScorePanel.SetActive(true);
                GetComponent<SpriteRenderer>().enabled = false;
                //gameObject.SetActive(false);
                return false;
            }

            BallInHole();

            return false;
        }

        else if (results[0].gameObject.layer == LayerMask.NameToLayer("Default")) {
            ballInGreen = false;
            
            return true;
        }

        greenAreaSize = Mathf.Max(results[0].bounds.size.x, results[0].bounds.size.y);
        pbc.SetDistance(((Vector2)goalHole.transform.position - (Vector2)transform.position).magnitude);
        pbc.SetDistanceRatio(Mathf.Clamp((goalHole.transform.position - transform.position).magnitude / greenAreaSize, 0.0f, 1.0f));
        ballInGreen = true;

        return true;
    }

    void UpdateScore() 
    {
        scoreTextList[currentIndex].text = currentHoleScore.ToString();
        totalScore += currentHoleScore;
        currentHoleScore = 0;

        StartCoroutine(UpdateScoreCoroutine());
    }

    IEnumerator UpdateScoreCoroutine() 
    {
        scorePanel.SetActive(true);

        yield return new WaitForSeconds(5);

        scorePanel.SetActive(false);
    }

    void BallInHole() 
    {
        transform.position = spawnPoints[currentIndex];
        shadow.transform.position = transform.position;
        previousLandingCoordinates = transform.position;
        goalHole = holeList[currentIndex];
        goalHole.GetComponent<Collider2D>().enabled = true;
        rc.UpdateReticleTarget(goalHole);
        rc.SetDefualtGroundValues();
    }

    void ResetGolfBallToLastPosition() 
    {
        transform.position = previousLandingCoordinates;
    }

    Vector2 StartEndMidPoint()
    {
        return (targetPos - (Vector2)transform.position) / 2.0f;
    }

    float ReturnMaxElement(float minElement, float elementToScale)
    {
        return Mathf.Max(minElement, elementToScale * rc.GetDistanceScalingFactor((targetPos - (Vector2)transform.position).magnitude));
    }

    Vector2 CalculateMidPoint()
    {
        Vector2 mp = (Vector2)transform.position + StartEndMidPoint();
        mp.y += ReturnMaxElement(minBallHeight, maxBallHeight);  //Mathf.Max(minBallHeight, maxBallHeight * (currentDistance / maxDistance));
        return mp;
    }

    void CalculateShotDuration()
    {
        shotDuration = ReturnMaxElement(minShotDuration, maxShotDuration);
    }

    int CalculateNumberOfBounces()
    {
        return Mathf.RoundToInt(ReturnMaxElement(minBounceNumber, maxBounceNumber));
    }

    float CalculateTravelDistance()
    {
        return (targetPos - (Vector2)transform.position).magnitude;
    }

    Vector2 CalculateDirectionvector() 
    {
        return (targetPos - (Vector2)transform.position).normalized;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        prompt.SetActive(true);
       
        //promptEnabled = true;
        //mp = collision.gameObject.GetComponent<PlayerController>();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        prompt.SetActive(false);
        //promptEnabled = false;
        //pc = null;
    }

    public void SetPuttingDistance(float pd) 
    {
        puttingDistance = pd;
    }

    public void TransitionToBall() 
    {
        cf.TransitionCamera(this.gameObject, cameraTransitionDurationToBall);
    }

    public void TransitionToBallShadow() 
    {
        cf.TransitionCamera(shadow, cameraTransitionDurationToBall);    
    }

    public void SetTargetPos(Vector2 tp) 
    {
        targetPos = tp;
        /*
        midpoint = (Vector2)transform.position + StartEndMidPoint();
        midpoint.y += Mathf.Max(minBallHeight, maxBallHeight * (currentDistance / maxDistance));
        //Debug.Log("Height: " + maxBallHeight * (currentDistance / maxDistance) + " " + "Mid Point: " + midpoint);
        
        shotDuration = Mathf.Max(minShotDuration, maxShotDuration * (currentDistance / maxDistance));
        
        numberOfBounces = Mathf.Max(minBounceNumber, Mathf.RoundToInt(maxBounceNumber * (currentDistance/maxDistance)));
        Debug.Log("Bounces: " + numberOfBounces);
        directionVector = (targetPos - (Vector2)transform.position).normalized;
        travelDistance = currentDistance;
         */
    }

    public GameObject GetBallShadow() 
    {
        return shadow;
    }

    public GameObject GetGoalHole() 
    {
        return this.goalHole;
    }

    public void SetFoucsed(bool f) 
    {
        focused = f;
    }
}
