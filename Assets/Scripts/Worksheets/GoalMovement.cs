using System.Collections;
using UnityEngine;

public class GoalMovement : MonoBehaviour
{
	[SerializeField] Transform 
		player;
	[SerializeField] float 
		detectionDistance = 5f,
		moveSpeed = 15f,
		amplitude = 0.1f,
		rotationSpeed = 1000f,
		cubeColorSpeed = 1f;
	[SerializeField] Color cubeColorStart = Color.blue;
	Color cubeColorCurrent;
	MeshRenderer[] cubeRenderers;
	Vector3	startPos;
	int cubeColorDirection;
	void Start()
    {
		cubeRenderers = GetComponentsInChildren<MeshRenderer>();
		startPos = transform.position;
	}
    void Update()
    {
		GoalMove();
		GoalRotate();
		GoalDistanceToPlayer();
		if (Input.GetKeyDown(KeyCode.K))
		{
			cubeColorCurrent = cubeColorStart;
			StartCoroutine(CubeFade());
		}
	}
	void GoalMove()
	{
		transform.localPosition = (startPos + Vector3.up) * (Mathf.Sin(Time.time * moveSpeed) * amplitude);
	}
	void GoalRotate()
	{
		transform.Rotate(Vector3.left, rotationSpeed * Time.deltaTime);
	}
	void GoalDistanceToPlayer()
	{
		if (Vector3.Distance(transform.position, player.position) <= detectionDistance)
		{
			Debug.Log("Player is close enough");
		}
	}
	IEnumerator CubeFade()
	{
		do
		{
			cubeColorCurrent.a -= Time.deltaTime * cubeColorSpeed;
			CubeColorSet(cubeColorCurrent);
			yield return new WaitForEndOfFrame();
		}
		while (cubeColorCurrent.a > 0);
		cubeColorCurrent.a = 0;
		CubeColorSet(cubeColorCurrent);
	}
	void CubeColorSet(Color color)
	{
		foreach (MeshRenderer cube in cubeRenderers) { cube.material.color = color; }
	}

}