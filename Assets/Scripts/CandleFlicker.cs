using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleFlicker : MonoBehaviour {

    Light candleLight;
    float currentTime;
    public float timeToMove;
    public float lowestVal;
    public float midVal;
    public float highestVal;

    public float lowMoveTime;
    public float highMoveTime;

    private float randomLow;
    private float randomHigh;
    public bool initial =true;

    // Use this for initialization
    void Start () {

        candleLight = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {

        if (currentTime <= timeToMove)
        {
            
            currentTime += Time.deltaTime * 10.0f;
            if (initial)
            {
                candleLight.intensity = Mathf.Lerp(randomLow, randomHigh, currentTime / timeToMove);
                initial = false;

            }
            else
            {
                candleLight.intensity = Mathf.Lerp(randomHigh, randomLow, currentTime / timeToMove);
            }

        }
        else
        {
            if (!initial)
            {
                initial = true;
                timeToMove = Random.Range(lowMoveTime, randomHigh);
                randomLow = Random.Range(lowestVal, midVal);
                randomHigh = Random.Range(midVal, randomHigh);

            }
            candleLight.intensity = midVal;
            currentTime = 0f;
        }


    }

}
