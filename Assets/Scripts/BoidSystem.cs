using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSystem : MonoBehaviour
{
    public Boid[] boids;

    [Space(10)]

    public Boid prefabBoid;


    [Header("Input")]

    public float inputScroll;
    public bool inputMouseActive;


    [Header("Values")]

    public int boidTotal = 40;
    public float boidSize;
    Vector3 tendencyPlace;


    [Header("References")]

    public Camera cam;


    void Start()
    {
        boids = new Boid[boidTotal];

        for (int i = 0; i < boidTotal; ++i)
        {
            boids[i] = Instantiate(prefabBoid);
            boids[i].position = Random.insideUnitSphere.normalized;
        }
    }

    void Update()
    {
        //Set the amount you scrolled up to a limit.
        inputScroll = Mathf.Clamp(inputScroll + Input.mouseScrollDelta.y, -15, 5);
    }

    void FixedUpdate()
    {
        InputTendencyPlace();

        UpdateBoids();
    }

    void UpdateBoids()
    {
        if (boids.Length <= 1) return;

        //Rule vectors.
        Vector3 r1, r2, r3, rb, rpt;

        //Get the center of all boids.
        //See GetRule1(b, center) description for why.
        Vector3 center = Vector3.zero;
        foreach (Boid b in boids)
        {
            center += b.position;
        }
        center /= boids.Length - 1;

        //Update each boid according to rules.
        foreach (Boid b in boids)
        {
            r1 = GetRule1(b, center);
            r2 = GetRule2(b);
            r3 = GetRule3(b);

            rb = GetRuleBound(b);
            rpt = GetRulePlaceTendency(b);

            b.velocity = b.velocity + r1 + r2 + r3 + rb + rpt;
            b.position = b.position + b.velocity;

            RuleLimitVelocity(b);
        }
    }

    //Instead of getting the perceived center by cycling through each boid for every single boid,
    //we just get the center once globally that includes each boid and then remove the boid in their own perceived rule.
    Vector3 GetRule1(Boid ruleBoid, Vector3 center)
    {
        Vector3 perceivedCenter = center - (ruleBoid.position / (boids.Length - 1));

        return (perceivedCenter - ruleBoid.position) / 100;
    }

    Vector3 GetRule2(Boid ruleBoid)
    {
        Vector3 distance = Vector3.zero;

        foreach (Boid b in boids)
        {
            if (b != ruleBoid)
            {
                //Minor optimization to use square magnitude for comparison.
                if ((b.position - ruleBoid.position).sqrMagnitude < boidSize * boidSize)
                {
                    //Reversed from the example in pseudocode.
                    distance -= b.position - ruleBoid.position;
                }
            }
        }

        return distance;
    }

    Vector3 GetRule3(Boid ruleBoid)
    {
        Vector3 perceivedVelocity = Vector3.zero;

        foreach (Boid b in boids)
        {
            if (b != ruleBoid)
            {
                perceivedVelocity += b.velocity;
            }
        }

        perceivedVelocity /= boids.Length - 1;

        return (perceivedVelocity - ruleBoid.velocity) / 8;
    }

    Vector3 GetRuleBound(Boid ruleBoid)
    {
        float xMax = 20, xMin = -20, yMax = 12, yMin = -12, zMax = 13, zMin = -13;
        Vector3 bound = Vector3.zero;

        if (ruleBoid.position.x < xMin) bound.x = .1f;
        else if (ruleBoid.position.x > xMax) bound.x = -.1f;

        if (ruleBoid.position.y < yMin) bound.y = .1f;
        else if (ruleBoid.position.y > yMax) bound.y = -.1f;

        if (ruleBoid.position.z < zMin) bound.z = .1f;
        else if (ruleBoid.position.z > zMax) bound.z = -.1f;

        return bound;
    }

    Vector3 GetRulePlaceTendency(Boid ruleBoid)
    {
        //If mouse isn't on-screen, ignore.
        if (!inputMouseActive) return Vector3.zero;

        Vector3 place = tendencyPlace;

        return (place - ruleBoid.position) / 200;
    }

    void RuleLimitVelocity(Boid ruleBoid)
    {
        float maxVelocity = .35f;

        float velocityMagnitude = ruleBoid.velocity.magnitude;

        if (velocityMagnitude > maxVelocity)
        {
            ruleBoid.velocity = (ruleBoid.velocity / velocityMagnitude) * maxVelocity;
        }
    }

    void InputTendencyPlace()
    {
        Ray screenRay = cam.ScreenPointToRay(Input.mousePosition);
        tendencyPlace = Vector3.forward * 6;

        if (Physics.Raycast(screenRay, out RaycastHit ray))
        {
            inputMouseActive = true;

            tendencyPlace += ray.point + Vector3.forward * inputScroll;

            return;
        }

        inputMouseActive = false;
    }
}
