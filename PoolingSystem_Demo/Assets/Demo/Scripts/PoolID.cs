using UnityEngine;
using System.Collections;

public class PoolID : MonoBehaviour {
	[SerializeField]private int _id;
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
}
