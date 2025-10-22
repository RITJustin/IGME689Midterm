using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class TimeOfDayController : MonoBehaviour
{
    [SerializeField] private DirectionalLight sun;

    private double offset = -90;

    [Header("Time Variables")]
    [Range(0f, 1f)]
    [SerializeField] private float speed = 0.1f;

    [Range(0f, 24f)]
    [SerializeField] private double startTime = 6.0;

    [Range(0f, 24f)]
    [SerializeField] private double stopTime = 18.0;

    [Range(0f, 24f)]
    [SerializeField] private double sunRise;

    [Range(0f, 24f)]
    [SerializeField] private double sunSet;

    [Range(0f, 24f)]
    [SerializeField] private double time = 6.0;

    private void RotateSky()
    {
        var rotationCalculation = time / 24 * 360 + offset;
        time += speed;

        if (time == 24.0)
        {
            time = 0.0f;
        }
        else
        {
            transform.eulerAngles = new Vector3(
                transform.rotation.x,
                transform.rotation.y,
                (float)rotationCalculation
            );
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        RotateSky();
    }
}
