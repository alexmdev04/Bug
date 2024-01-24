using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] bool 
        detectionEnabled = true;
    [SerializeField] float
        detectionRadius = 5f,
        detectionAngle = 45f;
    [SerializeField] LayerMask
        playerMask,
        obstacleMask;
    [SerializeField] GameObject 
        detectionOrigin;
    [SerializeField] bool 
        cubeOverlap;
    [Header("Detection Light")]
    [SerializeField] Light 
        detectionLight;
    [SerializeField] Color
        hiddenColor = Color.green,
        spottedColor = Color.yellow,
        fullAlertColor = Color.red;
    void FixedUpdate()
    {
        if (detectionEnabled) { ConeOfVisionUpdate(); }
        DetectionLight();
    }
    void ConeOfVisionUpdate()
    {
        Collider[] colliders = cubeOverlap ?
            Physics.OverlapBox(detectionOrigin.transform.position, new Vector3(detectionRadius, detectionRadius, detectionRadius), detectionOrigin.transform.rotation, playerMask) :
            Physics.OverlapSphere(detectionOrigin.transform.position, detectionRadius, playerMask);

        bool playerFound = colliders.Length != 0;
        if (uiDebug.instance.debugLines) 
        {
            if (cubeOverlap) { Popcron.Gizmos.Cube(detectionOrigin.transform.position, detectionOrigin.transform.rotation, new Vector3(detectionRadius, detectionRadius, detectionRadius) * 2, playerFound ? Color.green : Color.red); }
            else { Popcron.Gizmos.Sphere(detectionOrigin.transform.position, detectionRadius, playerFound ? Color.green : Color.red); }
            //Vector3 fovLeft = Extensions.DirectionFromAngleY(detectionOrigin.transform.eulerAngles.y, -detectionAngle / 2),
            //Popcron.Gizmos.Line(detectionOrigin.transform.position, detectionOrigin.transform.position + fovLeft * detectionRadius, Color.yellow);
            Popcron.Gizmos.Cone(detectionOrigin.transform.position, detectionOrigin.transform.rotation, detectionRadius, detectionAngle, Color.magenta);
        }
        if (!playerFound) { StealthHandler.instance.SetPlayerVisible(false); return; }

        Vector3 directionToTarget = (colliders[0].transform.position - detectionOrigin.transform.position).normalized;
        // checks if the player is within the detectionAngle and not obstrcuted
        StealthHandler.instance.SetPlayerVisible(Vector3.Angle(detectionOrigin.transform.forward, directionToTarget) < detectionAngle / 2
            && !Physics.Raycast(detectionOrigin.transform.position, directionToTarget, Vector3.Distance(detectionOrigin.transform.position, colliders[0].transform.position), obstacleMask));

        if (uiDebug.instance.debugLines) 
        {
            Popcron.Gizmos.Line(detectionOrigin.transform.position, colliders[0].transform.position, StealthHandler.instance.playerVisible ? Color.green : Color.red);
        }
    }
    void DetectionLight()
    {
        detectionLight.range = detectionRadius;
        detectionLight.spotAngle = detectionAngle;
        detectionLight.color = StealthHandler.instance.stealthLevel switch
        {
            StealthHandler.stealthLevelEnum.hidden => hiddenColor,
            StealthHandler.stealthLevelEnum.spotted => spottedColor,
            StealthHandler.stealthLevelEnum.fullAlert => fullAlertColor,
            _ => Color.magenta
        };
    }
}
