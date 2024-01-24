using UnityEngine;

public class LevelCollectable : MonoBehaviour
{
    [Header("Spinning values")]
    [SerializeField] bool spin = true;
    [SerializeField] float spinSpeed = 100f;
    [SerializeField] Vector3 spinDirection = Vector3.forward;
    [Header("Bobbing values")]
    [SerializeField] bool bob = true;
    [SerializeField] float bobSpeed = 5f;
    [SerializeField] float bobAmplitude = 1f;
    float startYPosition;

    // Checks if this object collides with the player
    void OnTriggerEnter(Collider collision) { if (collision.gameObject.TryGetComponent(out Player player)) { LevelLoader.instance.levelCurrent.collectableCollected.Invoke(); gameObject.SetActive(false); } }
    
    void Start() { startYPosition = transform.position.y; } // Sets the start position for use with bobbing
    
    void Update() // Iterates on the position and rotation of the object to give a bobbing and spinning effect
    {
        if (bob) { transform.position = new Vector3 (transform.position.x, startYPosition + (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude), transform.position.z); }
        if (spin) { transform.Rotate(spinDirection, spinSpeed * Time.deltaTime, Space.Self); }
    }
}