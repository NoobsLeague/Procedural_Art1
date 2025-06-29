﻿// Version 2023
//  Updates:
//   (1) localScale=(1,1,1) to work correctly with scaled roots,
//   (2) buildDelay included and passed here, though used only in subclasses

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Demo {
	/// <summary>
	/// This is a superclass that you can use for any custom grammar.
	/// Note: To create grammars, it's not necessary to understand the implementation details here,
	///  you just need to know what the key methods do (see comments + lecture slides).
	/// Also: you probably shouldn't make changes in this class, unless you really know what you're doing.
	/// </summary>
	public abstract class Shape : MonoBehaviour {
		public float buildDelay = 0.1f; // Added here for convenience, since it's a common parameter.

		/// <summary>
		/// Returns the total number of generated game objects 
		/// Note: make sure that you only spawn game objects using the SpawnPrefab method, if you want
		///  them to be cleaned up properly.
		/// </summary>
		public int NumberOfGeneratedObjects {
			get {
				if (generatedObjects!=null)
					return generatedObjects.Count;
				else
					return 0;
			}
		}
		List<GameObject> generatedObjects = null;

		/// <summary>
		/// In any child game object / symbol of this grammar, Root will give a reference to the root game object 
		///  in the scene.
		/// This means that you can e.g. add custom global parameter components to the root game object, and 
		///  retrieve them anywhere using Root.GetComponent.
		/// </summary>
		public GameObject Root {
			get {
				if (root==null) {
					return gameObject;
				} else {
					return root;
				}
			}
		}
		GameObject root = null;

		/// <summary>
		/// A utility method for creating new grammar symbols, or in Unity terms: (child) game objects with a 
		///  Shape component.		
		/// For [T], pass the symbol name (a subclass of Shape). 
		/// [name] is the name of the resulting game object in the hierarchy.
		/// Optionally, you can pass in a (local) position and rotation for the new shape, and a parent transform.
		///  (By default, the parent is the game object of the current grammar symbol.)
		/// Returns the new Symbol (a.k.a. Shape component).
		/// </summary>
		protected T CreateSymbol<T>(string name, Vector3 localPosition=new Vector3(), Quaternion localRotation=new Quaternion(), Transform parent=null) where T : Shape {
			if (parent==null) {
				parent=transform; // default: add as child game object
			}
			GameObject newObj = new GameObject(name);
			newObj.transform.parent=parent;
			newObj.transform.localPosition=localPosition;
			newObj.transform.localRotation=localRotation;
			newObj.transform.localScale = new Vector3(1, 1, 1);
			AddGenerated(newObj);
			T component = newObj.AddComponent<T>();
			component.root = Root;
			component.buildDelay = buildDelay;
			return component;
		}

		
		/// <summary>
		/// A utility method for spawning prefabs.
		/// Use this to spawn prefabs (=terminal symbols in the grammar) cleanly, such that they will be destroyed
		///  when calling DeleteGenerated.
		/// Optionally, you can pass in a position and rotation for the new shape, and a parent transform.
		///  (By default, the parent is the game object of the current grammar symbol.)
		/// Returns the generated game object.
		/// </summary>
		protected GameObject SpawnPrefab(GameObject prefab, Vector3 localPosition=new Vector3(), Quaternion localRotation=new Quaternion(), Transform parent=null) {
			if (parent==null) {
				parent=transform; // default: add as child game object
			}
			GameObject copy = Instantiate(prefab, parent);
			copy.transform.localPosition=localPosition;
			copy.transform.localRotation=localRotation;
			copy.transform.localScale = new Vector3(1, 1, 1);
			AddGenerated(copy);
			return copy;
		}


		protected int RandomInt(int maxValue) {
			RandomGenerator rnd = Root.GetComponent<RandomGenerator>();
			if (rnd!=null) {
				return rnd.Next(maxValue);
			} else { // use Unity's random
				return Random.Range(0, maxValue);
			}
		}

		protected float RandomFloat() {
			RandomGenerator rnd = Root.GetComponent<RandomGenerator>();
			if (rnd!=null) {
				return (float)(rnd.Rand.NextDouble());
			} else { // use Unity's random
				return Random.value;
			}
		}
				
		public T SelectRandom<T>(T[] objectArray) {
			return objectArray[RandomInt(objectArray.Length)];
		}

		protected GameObject AddGenerated(GameObject newObject) {
			if (generatedObjects==null) {
				generatedObjects=new List<GameObject>();
			}
			generatedObjects.Add(newObject);
			return newObject;
		}

		public void Generate(float delaySeconds = 0) {
			DeleteGenerated();
			if (delaySeconds==0 || !Application.isPlaying) {
				Execute();
			} else {
				StartCoroutine(DelayedExecute(delaySeconds));
			}
		}

		IEnumerator DelayedExecute(float delay) {
			yield return new WaitForSeconds(delay);
			Execute();
		}

		public void DeleteGenerated() {
			if (generatedObjects==null)
				return;
			foreach (GameObject gen in generatedObjects) {
				if (gen==null)
					continue;
				// Delete recursively: (needed for when it's not a child of this game object)
				Shape shapeComp = gen.GetComponent<Shape>();
				if (shapeComp!=null)
					shapeComp.DeleteGenerated();

				DestroyImmediate(gen);
			}
			generatedObjects.Clear();
		}

		protected abstract void Execute();		
	}
}