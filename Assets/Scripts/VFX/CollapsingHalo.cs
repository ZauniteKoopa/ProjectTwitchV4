using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CollapsingHalo : MonoBehaviour
{
    [SerializeField]
    private float radius = 2f;
    [SerializeField]
    private float circleStepSize = 0.02f;
    [SerializeField]
    private LineRenderer circleBorderRender = null;
    [SerializeField]
    private LineRenderer circleProgressRender = null;
    [SerializeField]
    private AnimationCurve collapsingCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    private Transform character = null;
    private AudioSource speaker = null;

    private const float CIRCLE_RADS = 2f * Mathf.PI;
    private const float COLLAPSING_TIME = 0.25f;


    // If using update
    private bool constantRender = false;
    private Color renderProgressColor = Color.black;


    // Start is called before the first frame update
    void Awake()
    {
        // Error check
        if (circleBorderRender == null || circleProgressRender == null) {
            Debug.LogError("LineRenderers are not connected to collapsing halo");
        }

        if (radius <= 0.0f || circleStepSize <= 0.0f || circleStepSize >= CIRCLE_RADS) {
            Debug.LogError("Invalid radius or invalid step size assigned for collapsing halo");
        }

        speaker = GetComponent<AudioSource>();
        if (speaker == null) {
            Debug.LogError("No speaker found in this collapsing halo object", transform);
        }
        // else if (speaker.clip == null)
        // {
        //     Debug.LogWarning("No clip found within collapsing halo speaker, this will not make any sound", transform);
        // }
    }


    // Main function to activate collapsing halo sequence
    //  Pre: duration > COLLAPSING_TIME && only 1 halo sequence is running at a time && c != null
    //  Post: running collapsing halo sequence
    public void runCollapsingHalo(float duration, Transform c, Color progressColor) {
        Debug.Assert(duration > COLLAPSING_TIME && c != null);

        StopAllCoroutines();
        constantRender = false;
        character = c;
        StartCoroutine(collapsingHaloSequence(duration, progressColor));
    }


    // Main function to clear halo
    //  Pre: none
    //  Post: halo routine has been interrupted and you cannot see halos anymore
    public void clearHalo() {
        StopAllCoroutines();
        circleBorderRender.enabled = false;
        circleProgressRender.enabled = false;

        constantRender = false;

        if (speaker == null) {
            speaker = GetComponent<AudioSource>();
        }
        
        speaker.Stop();
    }


    // Main function to just show the halo
    public void showHalo(Color lineColor) {
        StopAllCoroutines();

        circleBorderRender.enabled = true;
        circleProgressRender.enabled = true;

        renderProgressColor = lineColor;
        character = GetComponent<Transform>();
        constantRender = true;

        if (speaker == null) {
            speaker = GetComponent<AudioSource>();
        }
        
        speaker.Stop();
    }


    // Main sequence for just displaying halo
    private void Update() {
        character = GetComponent<Transform>();

        if (constantRender) {
            drawCircle(radius, circleProgressRender, 1f, renderProgressColor);
            drawCircle(radius, circleBorderRender, 1f, Color.black);
        }
    }


    // Main Sequence to do collapsing circle
    //  Pre: duration > COLLAPSING_TIME, character != null
    private IEnumerator collapsingHaloSequence(float duration, Color progressColor) {
        Debug.Assert(duration > 0.0f && character != null);

        // Enable renderers
        circleBorderRender.enabled = true;
        circleProgressRender.enabled = true;

        // Main loop for drawing the circle
        float timer = 0f;
        float drawDuration = duration - COLLAPSING_TIME;

        while (timer < drawDuration) {
            yield return null;

            timer += Time.deltaTime;
            float progress = Mathf.Min(timer / drawDuration, 1f);
            drawCircle(radius, circleProgressRender, progress, progressColor);
            drawCircle(radius, circleBorderRender, 1f, Color.black);
        }

        // Main loop for collapsing the circle
        timer = 0f;
        while (timer < COLLAPSING_TIME) {
            yield return null;

            // Calculate radius depending on the collapsing curve
            timer += Time.deltaTime;
            float collapseCurveX = Mathf.Min(timer / COLLAPSING_TIME, 1f);
            float curRadius = radius * collapsingCurve.Evaluate(collapseCurveX);

            // Draw the circles
            drawCircle(curRadius, circleBorderRender, 1f, Color.black);
            drawCircle(curRadius, circleProgressRender, 1f, progressColor);
        }

        // Disable renderers
        circleBorderRender.enabled = false;
        circleProgressRender.enabled = false;

        speaker.Play();
        yield return new WaitForSeconds(1.0f);
        speaker.Stop();

    }


    // Main function to draw a circle
    //  Pre: r >= 0.0f, circleRender != null, 0 <= progress <= 1f, character != null
    //  Post: draws a circle revolving around character
    private void drawCircle(float r, LineRenderer circleRender, float progress, Color lineColor) {
        Debug.Assert(r >= 0.0f);
        Debug.Assert(progress >= 0f && progress <= 1f);
        Debug.Assert(circleRender != null && character != null);

        Vector3 circleCenter = character.position;
        float radsProgressed = Mathf.Lerp(0f, CIRCLE_RADS, progress);
        int numSteps = (int)Mathf.Ceil(radsProgressed / circleStepSize);
        Vector3[] circlePositions = new Vector3[numSteps];

        for (int i = 0; i < numSteps; i++) {
            // Calculate what rads we're on
            float curRads = Mathf.Min(circleStepSize * i, CIRCLE_RADS);

            // Get the X and Z distance vectors individual
            float zDistance = r * Mathf.Cos(curRads);
            float xDistance = r * Mathf.Sin(curRads);
            Vector3 distVector = new Vector3(xDistance, 0f, zDistance);

            // Add the vector to the array
            circlePositions[i] = circleCenter + distVector;
        }

        // Set lineRenderer positions to circle positions
        circleRender.positionCount = numSteps;
        circleRender.startColor = lineColor;
        circleRender.endColor = lineColor;
        circleRender.SetPositions(circlePositions);
    }

}
