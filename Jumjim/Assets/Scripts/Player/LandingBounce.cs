using UnityEngine;

public class LandingBounce : MonoBehaviour
{
    public float landingBounceAmount = 0.2f;
    public float landingBounceSpeed = 12f;
    public float minAirTime = 0.1f; // Minimum time in air before landing bounce

    private Vector3 startLocalPos;
    private float bounceOffset = 0f;

    private bool wasGroundedLastFrame = true;
    private float airTime = 0f;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        bounceOffset = Mathf.Lerp(bounceOffset, 0f, landingBounceSpeed * Time.deltaTime);
        Vector3 targetPos = startLocalPos + new Vector3(0, bounceOffset, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, landingBounceSpeed * Time.deltaTime);
    }

    public void SetGrounded(bool grounded)
    {
        if (!grounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            if (!wasGroundedLastFrame && airTime >= minAirTime)
            {
                // Only trigger bounce if in air longer than minAirTime
                bounceOffset = -landingBounceAmount;
            }
            airTime = 0f;
        }
        wasGroundedLastFrame = grounded;
    }
}
