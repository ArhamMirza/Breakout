using UnityEngine;

public class ArcGenerator : MonoBehaviour
{
    public float radius = 5f;           // Radius of the arc
    public float startAngle = 0f;       // Starting angle (in degrees)
    public float endAngle = 90f;        // Ending angle (in degrees)
    public int segments = 100;          // Number of segments (more = smoother arc)
    public Color arcColor = new Color(1f, 1f, 0f, 0.5f);  // Yellow translucent color
    
    private LineRenderer lineRenderer;

    void Start()
    {
        // Initialize LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;  // One extra point for center
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = arcColor;
        lineRenderer.endColor = arcColor;

        // Generate the arc
        GenerateArc();
    }

    void GenerateArc()
    {
        float angleStep = (endAngle - startAngle) / segments; // Angle step between each point

        // Loop through each segment and calculate the position
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (startAngle + i * angleStep);
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }

        // Connect the last point to the center to make it a filled sector
        lineRenderer.positionCount = segments + 2;
        lineRenderer.SetPosition(segments, Vector3.zero);  // Center of the arc
        lineRenderer.SetPosition(segments + 1, lineRenderer.GetPosition(0)); // Close the arc
    }
}
