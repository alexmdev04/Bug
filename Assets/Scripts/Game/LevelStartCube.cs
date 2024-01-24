using System.Collections;
using UnityEngine;

public class LevelStartCube : MonoBehaviour
{
    [SerializeField] float playerMoveSpeed = 2f;
    //[SerializeField] float addYRotation = 180f;
    [SerializeField] string levelAssetKey = "Level0";
    /// <summary>
    /// If the player enters the trigger box start LevelStartSequence
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player)) { StartCoroutine(LevelStartSequence()); }
    }
    /// <summary>
    /// Brings the player to the center of this cube, when at the center start the defined level and override the rotation to create the seamless transition
    /// </summary>
    /// <returns></returns>
    IEnumerator LevelStartSequence()
    {
        Player.instance.lookActive = false;
        while (transform.InverseTransformPoint(Player.instance.transform.position) != Vector3.zero) 
        {
            Player.instance.transform.position = Vector3.MoveTowards(Player.instance.transform.position, transform.position, Time.deltaTime * playerMoveSpeed);
            yield return new WaitForEndOfFrame();
        }

        // prevents negative eulerAngles
        Quaternion overrideStartRotation = Quaternion.Euler(new Vector3(
            Player.instance.transform.eulerAngles.x,
            Player.instance.transform.eulerAngles.y - transform.localEulerAngles.y, // flips to match the direction the level is facing in the world
            Player.instance.transform.eulerAngles.z));

        // starts the defined level
        //LevelLoader.instance.ChangeLevel(levelAssetKey, Level.levelDifficultiesEnum.normal, false, overrideStartRotation.eulerAngles);
        LevelLoader.instance.LoadLevel(new LevelLoader.levelLoadData()
        {
            levelAssetKey = levelAssetKey,
            levelDifficulty = Level.levelDifficultiesEnum.normal,
            useFade = false,
            overrideStartRotation = overrideStartRotation.eulerAngles
        });
        Player.instance.lookActive = true;
    }
}
