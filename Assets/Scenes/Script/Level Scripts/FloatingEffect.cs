using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    [Header("Floating Parameters")]
    [Tooltip("Maximum distance from original position")]
    public float maxFloatDistance = 10f;

    [Tooltip("Speed of floating movement")]
    public float floatSpeed = 2f;

    [Tooltip("Intensity of floating motion")]
    public float floatIntensity = 5f;

    private Vector3 startPosition;
    private float uniqueSeed;

    void Start()
    {
        // Store the original position
        startPosition = transform.localPosition;

        // Generate a unique random seed for this button
        uniqueSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Use the unique seed to create independent movement
        float time = Time.time * floatSpeed + uniqueSeed;

        // Calculate smooth floating movement around the start position
        float xOffset = Mathf.Sin(time) * floatIntensity;
        float yOffset = Mathf.Cos(time * 0.7f) * floatIntensity;

        // Clamp the offset to prevent moving too far from start position
        Vector3 newPosition = startPosition + new Vector3(xOffset, yOffset, 0);
        newPosition = Vector3.ClampMagnitude(newPosition - startPosition, maxFloatDistance) + startPosition;

        // Update the local position
        transform.localPosition = newPosition;
    }
}