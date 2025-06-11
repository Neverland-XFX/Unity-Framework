using System.Collections;
using GameLogic;
using UnityFramework;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityEnemy : MonoBehaviour
{
	public void InitEntity()
	{
		
	}

	void Awake()
	{
	}
	
	void OnTriggerEnter(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("player_bullet"))
		{
			GameEvent.Send(ActorEventDefine.EnemyDead,this.transform.position, this.transform.rotation);
			PoolManager.Instance.PushGameObject(this.gameObject);
		}
	}
	void OnTriggerExit(Collider other)
	{
		var objectName = other.gameObject.name;
		if (objectName.StartsWith("Boundary"))
		{
			PoolManager.Instance.PushGameObject(this.gameObject);
		}
	}

}