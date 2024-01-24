using System.Text;
using UnityEngine;

public class Player : MonoBehaviour
{
    // this script controls everything about the player e.g. position, state, look, interact.

    // instancing
    public static Player instance { get; private set; }
    public LineRenderer lineRenderer { get; private set; }
    public Vector2 
        mouseRotation,
        mouseRotationMultiplier,
        lookSensitivity;
    public GameObject 
        worksheetObj;
    public Vector3 
        lineRendererOffset = new Vector3(0f, -0.1f, 0f),
        playerDimensions = Vector3.one;
    public float playerRadius;
    public Vector2 playerEulerAngles;
    public bool lookActive = true;
    [SerializeField] Light torch;
    [SerializeField] float torchIntensityMax = 300f;
    [SerializeField] float torchIntensityMin = 0.2f;
    [SerializeField] float torchIntensityDistance = 15f;
    bool torchActive;
    RaycastHit torchHit;

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 165;
        QualitySettings.vSyncCount = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lineRenderer = GetComponent<LineRenderer>();
        Debug.developerConsoleEnabled = true;
        playerDimensions = Vector3.Scale(playerDimensions, transform.localScale);
        playerRadius = playerDimensions.x / 2f;
    }
    void Start()
    {

    }
    void Update()
    {
        lineRenderer.textureScale = new Vector2(lineRenderer.positionCount, 1);
        //if (Input.GetKeyDown(KeyCode.F8)) { worksheetObj.SetActive(!worksheetObj.activeSelf); }
        if (Input.GetKeyDown(KeyCode.F)) { ToggleTorch(); }
    }
    void LateUpdate()
    {
        if (lookActive) { Look(); }
    }
    void FixedUpdate()
    {
        if (torch.gameObject.activeSelf)
        {
            torch.intensity = 
                Physics.Raycast(transform.position, transform.forward, out torchHit, torchIntensityDistance) ? 
                Mathf.Lerp(torchIntensityMin, torchIntensityMax, Vector3.Distance(transform.position, torchHit.point) / torchIntensityDistance) 
                : torchIntensityMax;
        }
    }
    /// <summary>
    /// Controls the camera view of the player - where they are looking
    /// </summary>
    void Look()
    {
        mouseRotation = (mouseRotation * mouseRotationMultiplier) * lookSensitivity;
        playerEulerAngles.x += mouseRotation.x * lookSensitivity.x;
        playerEulerAngles.y += mouseRotation.y * lookSensitivity.y;
        playerEulerAngles.y = Mathf.Clamp(playerEulerAngles.y, -90f, 90f);
        transform.localRotation = Quaternion.AngleAxis(playerEulerAngles.x, Vector3.up) 
                                * Quaternion.AngleAxis(playerEulerAngles.y, Vector3.left);
    }
    /// <summary>
    /// <para>Manually sets the rotation of the player</para>
    /// <para>Used instead of Player.instance.transform.eulerAngles = Vector3</para>
    /// </summary>
    /// <param name="eulerAngles"></param>
    public void LookSet(Vector3 eulerAngles)
    {
        playerEulerAngles.x = eulerAngles.x;
        playerEulerAngles.y = eulerAngles.y;
    }
    /// <summary>
    /// Instantly teleports the player to the given position and rotation
    /// </summary>
    /// <param name="worldSpacePosition"></param>
    /// <param name="worldSpaceEulerAngles"></param>
    public void TeleportInstant(Vector3 worldSpacePosition, Vector3 worldSpaceEulerAngles)
    {
        Grapple.instance.PlayerTeleported(worldSpacePosition, worldSpaceEulerAngles);
        transform.position = worldSpacePosition;

        playerEulerAngles.x = worldSpaceEulerAngles.y;
        float yAngle = 0;
        if (worldSpaceEulerAngles.x == 0) { yAngle = 0; }
        else if (worldSpaceEulerAngles.x > 0 && worldSpaceEulerAngles.x <= 90) { yAngle = 0 - worldSpaceEulerAngles.x; }
        else if (worldSpaceEulerAngles.x < 360 && worldSpaceEulerAngles.x >= 270) { yAngle = 360 - worldSpaceEulerAngles.x; }
        playerEulerAngles.y = yAngle;
        //Debug.Log("teleported to; pos: " + worldSpacePosition + ", inrot: " + worldSpaceEulerAngles + ", outrot: " + playerEulerAngles);
    }
    public void TorchSetActive(bool state)
    {
        torchActive = state;
    }
    public void ToggleTorch()
    {
        if (!torchActive) { return; }
        torch.gameObject.SetActive(!torch.gameObject.activeSelf);
    }
    public StringBuilder debugGetStats()
    {
        return new StringBuilder(uiDebug.str_playerTitle)
            .Append(uiDebug.str_targetFPS).Append(Application.targetFrameRate).Append(uiDebug.str_vSync).Append(QualitySettings.vSyncCount)
            //.Append(uiDebug.str_mouseRotation).Append(mouseRotation.ToStringBuilder()).Append(uiDebug.str_multiply).Append(mouseRotationMultiplier.ToStringBuilder())
            .Append(uiDebug.str_lookSensitivity).Append(lookSensitivity.ToStringBuilder())
            .Append(uiDebug.str_playerDimensions).Append(playerRadius);
    }
}