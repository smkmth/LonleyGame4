using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GhostState
{
    Patrolling,
    Chasing
}
public class Ghost : MonoBehaviour
{


    private AudioSource ghostAudio;
    public AudioClip ambiant;
    public AudioClip scare;
    public GameObject ghostMesh;
    public GameObject gameoverCanvas;

    [Header("Ghost Chase AI")]

    public Transform player;
    public Transform target;
    public bool ghostActive = false;
    public float currentMoveSpeed;
    public float minMoveSpeed;
    public float maxMoveSpeed;
    public float speedDropMod;
    public float stopDistance = 1.0f;
    public float chaseDistance = 10.0f;
    public float slowdownDistance = 5.0f;
    public GhostState currentGhostState;



    [Header("Ghost Patrol AI")]
    public Transform[] points;
    private int destPoint;
    public float rotSpeed;

    public float ghostSpotDistance;
    public float ghostSpotRadius;
    public LayerMask playerLayer;

    [Header("Ghost Audio")]
    public float timeBetweeIdleNoises;
    private float ghostIdleTimer;
    public float chanceOfIdleNoise;
    public AudioClip[] idleNoises;
    public AudioClip[] breathingNoises;
    public float breathingVol;
    public float timeBetweenBreath;
    private float breathTimer;

    public AudioClip[] spottedNoises;
    public float spottedVol;

    public AudioClip firstSpottedNoise;

    public GameObject[] targets;
    int targetIndex =1;

    [Header("Ghost Light")]
    public Light ghostLight;
    public float ghostLightInitIntensity;
    public float ghostLightDuration;

    [Header("Ghost Alpha")]
    [Range(0,1)]
    public float ghostInitAlpha;

    public float ghostFadeDuration;
    public float distanceToTarget;
    Renderer ghostRenderer;
    Coroutine fadeOut;
    Coroutine lightFade;
    public float targetAngle;
    private void Start()
    {
        player = GameObject.Find("Player").transform;
        ghostAudio = GetComponent<AudioSource>();
        target = points[0];
        
    }
    public void OnFirstActivateGhost()
    {
        ghostActive = true;
        ghostAudio.PlayOneShot(firstSpottedNoise, 1.0f);

    }

    public void ToggleGhost(bool ghostOn)
    {
        if (ghostOn)
        {
            if (fadeOut != null)
            {
                StopCoroutine(fadeOut);
            }
            if (lightFade != null)
            {
                StopCoroutine(lightFade);

            }
            //turn ghost on and move to correct position and rotation
            ghostMesh.SetActive(true);
            ghostMesh.transform.position = transform.position;
            ghostMesh.transform.rotation = transform.rotation;

            //set up light itencity and alpha
            ghostLight.intensity = ghostLightInitIntensity;
            ghostMesh.GetComponent<Renderer>().material.SetFloat("_Alpha", 0.7f);

            fadeOut = StartCoroutine(FadeTo(ghostMesh.GetComponent<Renderer>().material, 0f, ghostFadeDuration));
            lightFade = StartCoroutine(LightFade(0f, ghostLightDuration));


        }
        else
        {

            ghostMesh.SetActive(false);

        }
    }

  
    void Update()
    {

        GhostAudio();

     

        if (ghostActive)
        {
            Debug.DrawRay(transform.position, transform.forward * ghostSpotDistance, Color.red);
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, ghostSpotRadius, transform.forward,out hit, ghostSpotDistance))
            {
                if (hit.collider.tag == "Player")
                {
                    ChangeGhostState(GhostState.Chasing);
                }
            }
            distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            Vector3 pos = target.position;
            pos.y = 0f;

            switch (currentGhostState)
            {
                case GhostState.Chasing:

                    if (distanceToTarget > stopDistance)
                    {
                        float step = currentMoveSpeed * Time.deltaTime; // calculate distance to move
                        transform.position = Vector3.MoveTowards(transform.position, pos, step);
                    }
                    if (distanceToTarget > slowdownDistance)
                    {
                        if (currentMoveSpeed <= maxMoveSpeed)
                        {
                            currentMoveSpeed += Time.deltaTime * speedDropMod;
                        }
                    }
                    else
                    {
                        if (currentMoveSpeed >= minMoveSpeed)
                        {
                            currentMoveSpeed -= Time.deltaTime * speedDropMod;
                        }

                    }
                    if (distanceToTarget > chaseDistance)
                    {
                        ChangeGhostState(GhostState.Patrolling);
                    }
                    transform.LookAt(pos);

                    break;
                case GhostState.Patrolling:

                    // Rotate our transform a step closer to the target's.
                    float currentAngle = Vector3.Angle(transform.forward,(transform.position - target.transform.position));


                    var rotstep = rotSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, currentAngle, 0), rotstep);


                    if (distanceToTarget > stopDistance)
                    {
                        transform.position += (target.position - transform.position) * Time.deltaTime;

                    }
                    else
                    {
                        GotoNextPoint();

                    }

                    break;
            }
        
        }
        
    }

    public void ChangeGhostState(GhostState newGhostState)
    {
        currentMoveSpeed = maxMoveSpeed;

        switch (newGhostState)
        {
            case GhostState.Chasing:
                target = player;
                break;
            case GhostState.Patrolling:
                target = points[0];
                break;
        }
        currentGhostState = newGhostState;

    }


    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        target = points[destPoint].transform;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
        targetAngle = Vector3.Angle(transform.forward, (transform.position - target.transform.position));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ghostActive)
        {
       
            if (other.gameObject.tag == "Player")
            {
                HelperFunctions.PlayRandomNoiseInArray(spottedNoises, ghostAudio, spottedVol);

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ghostActive)
        {

            if (other.gameObject.tag == "Player")
            {

            }
        }
    }



    IEnumerator LightFade( float targetOpacity, float duration)
    {

        float startBrightness = ghostLight.intensity; 
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float blend = Mathf.Clamp(time, 0,  duration);
            ghostLight.intensity = Mathf.Lerp(startBrightness, targetOpacity, blend);
            yield return null;
        }
    }

    IEnumerator FadeTo(Material material, float targetOpacity, float duration)
    {

        // Cache the current color of the material, and its initiql opacity.
        float startOpacity = material.GetFloat("_Alpha");

        // Track how many seconds we've been fading.
        float t = 0;

        while (t < duration)
        {
            // Step the fade forward one frame.
            // Step the fade forward one frame.
            t += Time.deltaTime;
            // Turn the time into an interpolation factor between 0 and 1.
            float blend = Mathf.Clamp01(t / duration);

            // Blend to the corresponding opacity between start & target.
            material.SetFloat("_Alpha", Mathf.Lerp(startOpacity, targetOpacity, blend));



            // Wait one frame, and repeat.
            yield return null;
        }
        yield return null;
        
    }

    public void GhostAudio()
    {
        ghostIdleTimer -= Time.deltaTime;
        breathTimer -= Time.deltaTime;

        if (breathTimer <= 0)
        {
            breathTimer = timeBetweenBreath;
            HelperFunctions.PlayRandomNoiseInArray(breathingNoises, ghostAudio, breathingVol);
        }
        else
        { 

            if (ghostIdleTimer <= 0)
            {
                ghostIdleTimer = timeBetweeIdleNoises;

                if (Random.Range(0, 100) < chanceOfIdleNoise)
                {
                    HelperFunctions.PlayRandomNoiseInArray(idleNoises, ghostAudio, Random.Range(0.5f, 1f));
                }
            }
        
        }

    }
}
