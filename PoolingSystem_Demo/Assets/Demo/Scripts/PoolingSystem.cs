using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolingSystem : MonoBehaviour 
{
	// Global instance of the pooling system
	private static PoolingSystem _instance;
	
	// Root of the object to start putting objects into as a container
	private Transform _root;
	
	public bool object_hierarchy;
	
	#region Pooled Object class
	private class Pooled_Object
	{
		// Main object to copy and pool
		private GameObject _main;
		
		// Queue of objects. Front are available. Back are unavailable
		private LinkedList<GameObject> _pool;
		
		// Root for the pooled objects for the specified object
		private GameObject _pool_root;
		
		public Pooled_Object(GameObject obj)
		{
			_main      = obj;
			_pool     = new LinkedList<GameObject>();
			
			// Add in our little tag to keep track
			obj.AddComponent<PoolID>();
			obj.GetComponent<PoolID>().GenerateID(obj);
			
			if(PoolingSystem.instance.object_hierarchy)
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
			
			/*
			// Go through our list of pooled objects
			for(int i=0;i<_pool.Count;++i)
			{
				// Current object is not being used
				if(!_pool[i].activeInHierarchy)
				{
					obj = _pool[i]; 
					// stop for loop. Way to prevent branch with break or return
					i   = _pool.Count; 
					obj.SetActive(true);
					if(PoolingSystem.instance.object_hierarchy)
					{
						obj.transform.parent = _pool_root.transform;
					}
				}
			}
			*/
			
			// First node has an available object, this is how the list is organized
			LinkedListNode<GameObject> first = _pool.First;
			if(first != null)
			{
				if(!first.Value.activeInHierarchy)
				{
					obj = first.Value;
					obj.SetActive(true);
					
					// Move to end
					_pool.Remove(obj);
					_pool.AddLast(obj);
					
					/*
					if(PoolingSystem.instance.object_hierarchy)
					{
						obj.transform.parent = _pool_root.transform;
					}
					*/
				}
			}
			
			// Allow to expand pool, if no more in pool
			if(obj == null)
			{
				obj = Instantiate(_main, Vector3.zero, Quaternion.identity) as GameObject;
				obj.SetActive(true);
				
				if(PoolingSystem.instance.object_hierarchy)
				{
					obj.transform.parent = _pool_root.transform;
				}
				
				//_pool.Add (obj);
				
				// Move to end
				_pool.AddLast(obj);
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
			
			/*
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
						obj        = null;
						i          = _pool.Count;
					}
				}
			}
			*/
			
			// Validation
			if(_main.GetComponent<PoolID>().id == obj.GetComponent<PoolID>().id)
			{
				// See if it exist, if so remove it
				if(_pool.Remove(obj))
				{
					// Setup for availability
					obj.SetActive(false);
					
					// Put to front
					_pool.AddFirst(obj);
					
					recieved = true;
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
			get { return (PoolingSystem.instance.object_hierarchy)? _pool_root : null; }
		}
	}
	#endregion
	
	// Pool of Pooled object classes, which facilitates the pool
	private Hashtable _objects;
	
	void Awake()
	{
		_instance = this;
		_objects = new Hashtable();
		_root     = (object_hierarchy)? this.transform : null;
	}
	
	/// <summary>
	/// Gets the static instance of this script, only one instance.
	/// </summary>
	/// <value>The instance.</value>
	public static PoolingSystem instance
	{
		get 
		{ 
			if(_instance == null)
			{
				_instance = new GameObject("Pooling System").AddComponent<PoolingSystem>();
			}
			
			return _instance; 
		}
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
		PoolID   obj_pid = obj.GetComponent<PoolID>();
		
		if(obj_pid != null && _objects.Count == 0)
		{
			Debug.LogError("PoolID is already contained on the prefabs. Make sure to remove them before start.");
			return null;
		}
		
		// Cahced already, search for the correct pool
		if(obj_pid != null)
		{
			Pooled_Object po = (Pooled_Object)_objects[obj_pid.id];
			if(po.main_obj.GetComponent<PoolID>().id == obj_pid.id)
			{
				o  = po.GetObject();
			}
		}
		// No cache of the object
		else
		{
			// Create new object and have it generate a pool id
			Pooled_Object po = new Pooled_Object(obj);
			
			// Get pool id for this object now
			obj_pid = obj.GetComponent<PoolID>();
			
			// Add object into our table of pooled objects
			_objects.Add(obj_pid.id, po);
			
			// Get the object of the current pooled object
			o = po.GetObject();
			
			if(object_hierarchy)
			{
				// Root the pooled objects into a true root object for the entire pooling system
				po.pool_root.transform.parent = _root.transform;
			}
		}
		
		return o;
	}
	
	/// <summary>
	/// This Instantiate takes a position and rotation. Pretend to instantiate the specified object, put it in a pool. 
	/// If the object is not in the existing cache pool
	/// put it in and reuse. If the object already exist, reuse the exisiting pool objects. If there is no more objects
	/// in the pool, expanded the pool to add more.
	/// </summary>
	/// <param name="obj">Object.</param>
	public GameObject PS_Instantiate(GameObject obj, Vector3 position, Quaternion rotation)
	{
		GameObject o = PS_Instantiate(obj);
		
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
		PoolID obj_pid = obj.GetComponent<PoolID>();
		
		// Object is in our pool
		if(obj_pid != null)
		{
			Pooled_Object po = (Pooled_Object)_objects[obj_pid.id];
			po.ReturnObject(obj);
		}
		else
			Debug.LogWarning(string.Format("{0} does not belong in the pool system since it was not Instantiated by the pooling system"));
	}
	
	// This ensures objects that we are caching don't get polluted with our tag script, so destroy on exit
	void OnApplicationQuit()
	{
		foreach (DictionaryEntry entry in _objects)
		{
			Pooled_Object po = (Pooled_Object)entry.Value;
			Component.DestroyImmediate(po.main_obj.GetComponent<PoolID>(), true);
		}
	}
}
