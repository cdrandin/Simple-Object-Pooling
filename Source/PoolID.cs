using UnityEngine;
using System.Collections;

public class PoolID : MonoBehaviour 
{
	private int _id;
	private bool _init;

	public int id
	{
		get { return _id; }
	}

	void Awake()
	{
		_init = false;
	}

	public void GenerateID(GameObject obj)
	{
		if(_init) return; // called only once
		_id = obj.transform.GetInstanceID();
		_init = true;
	}

	// This ensures objects that we are caching don't get polluted with our tag script, so destroy on exit
	// Also, in case PoolingSystem misses removing it
	void OnApplicationQuit()
	{
		Component.DestroyImmediate(this, true);
	}
}
