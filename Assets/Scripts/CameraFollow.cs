using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] Transform objectTransform;
    [SerializeField] BoxCollider2D mapBounds;
    //[SerializeField] float transitionDuration = 2.5f;

    Camera cameraProperty;

    bool transitioning = false;

    float camOrthographicOffset;
    float camHorizontalOffset;
    float camX;
    float camY;

    // Start is called before the first frame update testing
    void Start()
    {
        cameraProperty = GetComponent<Camera>();

        camOrthographicOffset = cameraProperty.orthographicSize;
        camHorizontalOffset = cameraProperty.orthographicSize * cameraProperty.aspect;

        //Debug.Log(camHorizontalOffset);
    }


    private void LateUpdate()
    {
        //transform.position = new Vector3(camX, camY, transform.position.z);
        if (transitioning) {
            return;
        }

        camX = Mathf.Clamp(objectTransform.position.x, mapBounds.bounds.min.x + camHorizontalOffset, mapBounds.bounds.max.x - camHorizontalOffset);
        camY = Mathf.Clamp(objectTransform.position.y, mapBounds.bounds.min.y + camOrthographicOffset, mapBounds.bounds.max.y - camOrthographicOffset);

        transform.position = new Vector3(camX, camY, transform.position.z);
    }


    public void TransitionCamera(GameObject gb, float td) 
    {
        objectTransform = gb.transform;
        transitioning = true;
        StartCoroutine(Transition(td));
    }

    IEnumerator Transition(float transitionDuration) 
    {
        float t = 0.0f;
        Vector3 startingPos = transform.position;
        Vector3 endingPos = new Vector3(objectTransform.position.x, objectTransform.position.y, transform.position.z);

        while (t < 1.0f) {
            t += Time.deltaTime * (Time.timeScale / transitionDuration);
            transform.position = Vector3.Lerp(startingPos, endingPos, t);
            yield return 0;
        }

        transitioning = false;
    }

    public bool GetIfTransitioning() 
    {
        return transitioning;
    }
}
