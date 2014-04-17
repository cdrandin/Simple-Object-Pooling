using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolingSystem : MonoBehaviour 
{
	// Global instance of the pooling system
	private static PoolingSystem _instance;

	// Root of the object to start putting objects into as a container
	private Transform _root;

	public bool objects_in_hierarchy;

	// Pooled Object class
	//
	private class Pooled_Object
	{
		// Main object to copy and pool
		private GameObject _main;

		// Pool of objects
		private List<GameObject> _pool;

		// Root for the pooled objects for the specified object
		private GameObject _pool_root;

		public Pooled_Object(GameObject obj)
		{
			_main      = obj;
			_pool      = new List<GameObject>();

			// Add in our little tag to keep track
			obj.AddComponent<PoolID>();
			obj.GetComponent<PoolID>().GenerateID(obj);

			if(PoolingSystem.instance.objects_in_hierarchy)
			{
				_pool_root = new GameObject(string.Format("pooled object: {0}", _main.name));
			}
		}
			
		/// <summary>
		/// Gets an object of the desired type. If there is an object to spare return the object, else return null.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="location">Location.</param>
		public GameObject GetObject()
		{
			GameObject obj = null; 
			
			for(int i=0;i<_pool.Count;++i)
			{
				// Current object is not being used
				if(!_pool[i].activeInHierarchy)
				{
					obj = _pool[i]; 
					// stop for loop. Way to prevent branch with break or return
					i   = _pool.Count; 
					obj.SetActive(true);
					if(PoolingSystem.instance.objects_in_hierarchy)
					{
						obj.transform.parent = _pool_root.transform;
					}
				}
			}

			// Allow to expand pool, if no more in pool
			if(obj == null)
			{
				obj = Instantiate(_main, Vector3.zero, Quaternion.identity) as GameObject;
				obj.SetActive(true);
				if(PoolingSystem.instance.objects_in_hierarchy)
				{
					obj.transform.parent = _pool_root.transform;
				}

				_pool.Add (obj);
			}
		
			return obj;
		}

		/// <summary>
		/// Returns the object back to the pooling system. Returns true if it was sucessful in putting it back, else false.
		/// </summary>
		/// <returns><c>true</c>, if object was returned, <c>false</c> otherwise.</returns>
		/// <param name="obj">Object.</param>
		public bool ReturnObject(GameObject obj)
		{
			bool recieved = false;
			
			if(obj.tag == this._main.tag)
			{
				for(int i=0;i<_pool.Count;++i)
				{
					if(_pool[i] == obj)
					{
						//_pool[i]   = obj; 
						// stop for loop. Way to prevent branch with break or return
						recieved   = true;
						_pool[i].SetActive(false);
						obj = null;
						i          = _pool.Count;
					}
				}
			}
			
			return recieved;
		}

		/// <summary>
		/// Gets the main object in which to clone.
		/// </summary>
		/// <value>The main_obj.</value>
		public GameObject main_obj
		{
			get { return _main; }
		}

		/// <summary>
		/// Gets the object that is a container for the pooled objects for this type.
		/// </summary>
		/// <value>The pool_root.</value>
		public GameObject pool_root
		{
			get { return (PoolingSystem.instance.objects_in_hierarchy)? _pool_root : null; }
		}
	}
	//
	// End of Pooled_Object

	// Pool of Pooled object classes, which facilitates the pool
	private List<Pooled_Object> _objects; 

	void Awake()
	{
		_instance = this;
		_objects  = new List<Pooled_Object>();
		_root     = (objects_in_hierarchy)? this.transform : null;
	}
	
	/// <summary>
	/// Gets the static instance of this script, only one instance.
	/// </summary>
	/// <value>The instance.</value>
	public static PoolingSystem instance
	{
		get { return _instance; }
	}

	/// <summary>
	/// Pretend to instantiate the specified object, put it in a pool. If the object is not in the existing cache pool
	/// put it in and reuse. If the object already exist, reuse the exisiting pool objects. If there is no more objects
	/// in the pool, expanded the pool to add more.
	/// </summary>
	/// <param name="obj">Object.</param>
	public GameObject PS_Instantiate(GameObject obj)
	{
		GameObject o = null;

		// Cahced already, search for the correct pool
		if(obj.GetComponent<PoolID>() != null)
		{
			for(int i=0;i<_objects.Count;++i)
			{
				// The obejct to get is already in pool
				if(_objects[i].main_obj.GetComponent<PoolID>().id == obj.GetComponent<PoolID>().id)
				{
					o  = _objects[i].GetObject();
					i  = _objects.Count;
				}
			}
		}

		// No cache of the object
		else
		{
			// Add object into our list of pool
			_objects.Add(new Pooled_Object(obj));

			// Count - 1, because Add puts object to the end
			o = _objects[_objects.Count-1].GetObject();

			if(objects_in_hierarchy)
			{
				// Root the pooled objects into a true root object for the entire pooling system
				_objects[_objects.Count-1].pool_root.transform.parent = _root.transform;
			}
		}

		return o;
	}

	public GameObject PS_Instantiate(GameObject obj, Vector3 position, Quaternion rotation)
	{
		GameObject o = this.PS_Instantiate(obj);

		o.transform.position = position;
		o.transform.rotation = rotation;

		return o;
	}

	/// <summary>
	/// Destroy the specified object by removing the reference of the passed object, disabling the major component
	/// </summary>
	/// <param name="obj">Object.</param>
	public void PS_Destroy(GameObject obj)
	{
		// Object is in our pool
		if(obj.GetComponent<PoolID>() != null)
		{
			for(int i=0;i<_objects.Count;++i)
			{
				if(_objects[i].main_obj.GetComponent<PoolID>().id == obj.GetComponent<PoolID>().id)
				{
					_objects[i].ReturnObject(obj);
					i = _objects.Count;
				}
			}
		}
		else
			Debug.LogWarning(string.Format("{0} does not belong in the pool system since it was not Instantiated by the pooling system"));
	}

	// This ensures objects that we are caching don't get polluted with our tag script, so destroy on exit
	void OnApplicationQuit()
	{
		foreach(Pooled_Object po in _objects)
		{
			Component.DestroyImmediate(po.main_obj.GetComponent<PoolID>(), true);
		}
	}
}
