using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// UnityBinder entry class. Use this class to setup any Unity Object that has any
/// Binder Attributes
/// </summary>
public static class UnityBinder 
{
	/// <summary>
	/// Inject an Object's field that have attributes.
	/// </summary>
	/// <param name="obj">The object to inject</param>
	/// <param name="recursive">Whether to recursively Inject all bound fields</param>
	public static void Inject(Object obj, bool recursive = false)
	{
		_Inject(obj, recursive, null);
	}

	private static void _Inject(Object obj, bool recursive = false, Stack<Object> currentStack = null)
	{
		if (recursive && currentStack == null)
			currentStack = new Stack<Object>();
		
		var bindingFlags = BindingFlags.Instance |
		                   BindingFlags.NonPublic |
		                   BindingFlags.Public;
		
		var fields = obj.GetType().GetFields(bindingFlags);

		foreach (var field in fields)
		{
			var injections = (Binder[])field.GetCustomAttributes(typeof(Binder), true);

			if (injections.Length > 0)
			{
				foreach (var inject in injections)
				{
					inject.InjectInto(obj, field);
				}

				if (recursive)
				{
					var val = field.GetValue(obj) as Object;
					if (val != null && !currentStack.Contains(val))
					{
						currentStack.Push(val);
						_Inject(val, true, currentStack);
						currentStack.Pop();
					}
				}
			}
		}

		var methods = obj.GetType().GetMethods(bindingFlags);
		foreach (var method in methods)
		{
			var injections = (BindOnClick[]) method.GetCustomAttributes(typeof(BindOnClick), true);

			if (injections.Length > 0)
			{
				foreach (var inject in injections)
				{
					inject.InjectInto(obj, method);
				}
			}
		}
	}

	private static GameObject DeepFind(string name)
	{
		if (name.StartsWith("/"))
		{
			string[] temp = name.Split('/');

			GameObject current = null;
			for (int i = 1; i < temp.Length; i++)
			{
				string n = temp[i];
				if (current == null)
				{
					current = GameObject.Find(n);
					if (current == null)
					{
						current = FindInActiveObjectByName(n);
					}
				}
				else
				{
					current = current.transform.Find(n).gameObject;
				}
			}

			return current;
		}
		
		return GameObject.Find(name);
	}

	internal static GameObject FindInActiveObjectByName(string name)
	{
		if (name.StartsWith("/"))
			return DeepFind(name);
		
		Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
		for (int i = 0; i < objs.Length; i++)
		{
			if (objs[i].hideFlags == HideFlags.None)
			{
				if (objs[i].name == name)
				{
					return objs[i].gameObject;
				}
			}
		}
		return null;
	}
}

/// <summary>
/// Abstract resource to represent any kind of Bind
/// </summary>
public abstract class Binder : Attribute
{
	public abstract void InjectInto(Object obj, FieldInfo field);
}

/// <summary>
/// Bind a method to an OnClick event that is triggered by a Button.
/// 
/// By default, the Button is searched on the gameObject attached to the script being bound
/// You may specify a GameObject to search in by supplying the Editor path in the constructor
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BindOnClick : Attribute
{
	private string buttonPath;

	public BindOnClick(string buttonPath = "")
	{
		this.buttonPath = buttonPath;
	}
	
	public void InjectInto(Object obj, MethodInfo method)
	{
		GameObject fromObj;
		if (string.IsNullOrEmpty(buttonPath))
		{
			var component = obj as Component;
			if (component != null)
			{
				fromObj = component.gameObject;
			}
			else
			{
				Debug.LogError("fromObject empty for field " + method.Name + ", and no default gameObject could be found!");
				return;
			}
		}
		else
		{
			fromObj = GameObject.Find(buttonPath);

			if (fromObj == null)
			{
				fromObj = UnityBinder.FindInActiveObjectByName(buttonPath);

				if (fromObj == null)
				{
					Debug.LogError("Could not find GameObject with name " + buttonPath + " for field " + method.Name);

					return;
				}
			}
		}

		var button = fromObj.GetComponent<Button>();
		if (button == null)
		{
			Debug.LogError("No Button Component found on GameObject @ " + buttonPath);
			return;
		}
		
		button.onClick.AddListener(delegate { method.Invoke(obj, new object[0]); });
	}
}

/// <summary>
/// Attribute to bind a resource at runtime
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class BindResource : Binder
{
	private static MethodInfo _cacheMethod;
	private static MethodInfo _cacheNotGeneric;

	public static MethodInfo GenericResourceLoad
	{
		get
		{
			if (_cacheMethod != null) return _cacheMethod;
			
			var methods = typeof(Resources).GetMethods();

			foreach (var method in methods)
			{
				if (method.Name != "Load" || !method.IsGenericMethod) continue;

				_cacheMethod = method;
				break;
			}

			return _cacheMethod;
		}
	}

	public static MethodInfo ResourceLoad
	{
		get
		{
			if (_cacheNotGeneric != null) return _cacheNotGeneric;

			_cacheNotGeneric = typeof(Resources).GetMethod("Load", new[] {typeof(string)});

			return _cacheNotGeneric;
		}
	}
	
	public string path;

	public BindResource(string path)
	{
		this.path = path;
	}


	public override void InjectInto(Object obj, FieldInfo field)
	{
		var injectType = field.FieldType;

		bool bindPrefab = false;
		object rawResult;
		if (injectType == typeof(GameObject))
		{
			bindPrefab = true;

			injectType = typeof(Object);

			rawResult = ResourceLoad.Invoke(null, new object[] {path});
		}
		else
		{
			var genericMethod = GenericResourceLoad.MakeGenericMethod(injectType);
			rawResult = genericMethod.Invoke(null, new object[] { path });
		}
		

		if (rawResult == null)
		{
			Debug.LogError("Could not find resource of type " + injectType + " for field " + field.Name);
		}
		else if (!injectType.IsInstanceOfType(rawResult))
		{
			Debug.LogError("Could not cast resource of type " + rawResult.GetType() + " to type of " + injectType + " for field " + field.Name);
		}
		else
		{
			if (bindPrefab)
			{
				var objResult = rawResult as Object;
				var instance = Object.Instantiate(objResult) as GameObject;
				
				field.SetValue(obj, instance);
			}
			else
			{
				field.SetValue(obj, rawResult);
			}
		}
	}
}

/// <summary>
/// Attribute to Bind a field to a component at runtime
/// </summary>
[AttributeUsage(AttributeTargets.Field)] 
public class BindComponents : Binder
{

	public string fromObject = "";
	public bool warnOnly = true;

	public BindComponents(int index = 0, string fromObject = "", bool warnOnly = true)
	{
		this.fromObject = fromObject;
		this.warnOnly = warnOnly;
	}

	public override void InjectInto(Object obj, FieldInfo field)
	{
		var injectType = field.FieldType;
					
		var unityCall = typeof(GameObject).GetMethod("GetComponents", new Type[0]);
		if (unityCall == null)
		{
			Debug.LogError("Could not find method GetComponents !!");
			return;
		}

		GameObject fromObj;
		if (string.IsNullOrEmpty(fromObject))
		{
			var component = obj as Component;
			if (component != null)
			{
				fromObj = component.gameObject;
			}
			else if (warnOnly)
			{
				Debug.LogWarning("fromObject empty for field " + field.Name + ", and no default gameObject could be found!");
				return;
			}
			else
			{
				Debug.LogError("fromObject empty for field " + field.Name + ", and no default gameObject could be found!");
				return;
			}
		}
		else
		{
			fromObj = GameObject.Find(fromObject);

			if (fromObj == null)
			{
				fromObj = UnityBinder.FindInActiveObjectByName(fromObject);

				if (fromObj == null)
				{
					if (warnOnly)
						Debug.LogWarning("Could not find GameObject with name " + fromObject + " for field " + field.Name);
					else
						Debug.LogError("Could not find GameObject with name " + fromObject + " for field " + field.Name);

					return;
				}
			}
		}

		if (!injectType.IsGenericList())
		{
			return;
		}

		var elementType = injectType.ListOfWhat();
		
		var listType = typeof(List<>);
		var constructedListType = listType.MakeGenericType(elementType);

		var resultedList = (IList)Activator.CreateInstance(constructedListType);	

		var genericMethod = unityCall.MakeGenericMethod(elementType);
		var rawResult = genericMethod.Invoke(fromObj, null);

		if (rawResult == null)
		{
			if (warnOnly)
				Debug.LogWarning("Could not find component of type " + elementType + " for field " + field.Name);
			else
				Debug.LogError("Could not find component of type " + elementType + " for field " + field.Name);
		} 
		else if (rawResult is object[])
		{
			var result = rawResult as object[];

			if (result.Length == 0)
			{
				if (warnOnly)
					Debug.LogWarning("Could not find component of type " + elementType + " for field " + field.Name);
				else
					Debug.LogError("Could not find component of type " + elementType + " for field " + field.Name);
			}
			
			//If above is true, then this is never executed
			for (int i = 0; i < result.Length; i++)
			{
				resultedList.Add(result[i]);
			}
		}

		field.SetValue(obj, resultedList);
	}
}

/// <summary>
/// Attribute to Bind a field to a component at runtime
/// </summary>
[AttributeUsage(AttributeTargets.Field)] 
public class BindComponent : Binder
{

	public int index = 0;
	public string fromObject = "";
	public bool warnOnly = true;
	public bool skipIfNonNull = false;

	public BindComponent(int index = 0, string fromObject = "", bool warnOnly = true, bool skipIfNonNull = false)
	{
		this.index = index;
		this.fromObject = fromObject;
		this.warnOnly = warnOnly;
		this.skipIfNonNull = skipIfNonNull;
	}

	public override void InjectInto(Object obj, FieldInfo field)
	{
		//Check if null
		if (field.GetValue(obj) != null && skipIfNonNull) return;

		var injectType = field.FieldType;
					
		var unityCall = typeof(GameObject).GetMethod("GetComponents", new Type[0]);
		if (unityCall == null)
		{
			Debug.LogError("Could not find method GetComponents !!");
			return;
		}

		GameObject fromObj;
		if (string.IsNullOrEmpty(fromObject))
		{
			var component = obj as Component;
			if (component != null)
			{
				fromObj = component.gameObject;
			}
			else if (warnOnly)
			{
				Debug.LogWarning("fromObject empty for field " + field.Name + ", and no default gameObject could be found!");
				return;
			}
			else
			{
				Debug.LogError("fromObject empty for field " + field.Name + ", and no default gameObject could be found!");
				return;
			}
		}
		else
		{
			fromObj = GameObject.Find(fromObject);

			if (fromObj == null)
			{
				fromObj = UnityBinder.FindInActiveObjectByName(fromObject);

				if (fromObj == null)
				{
					if (warnOnly)
						Debug.LogWarning("Could not find GameObject with name " + fromObject + " for field " + field.Name);
					else
						Debug.LogError("Could not find GameObject with name " + fromObject + " for field " + field.Name);

					return;
				}
			}
		}

		if (injectType == typeof(GameObject) && !string.IsNullOrEmpty(fromObject))
		{
			field.SetValue(obj, fromObj);
			return;
		}
					

		var genericMethod = unityCall.MakeGenericMethod(injectType);
		var rawResult = genericMethod.Invoke(fromObj, null);

		if (rawResult == null)
		{
			if (warnOnly)
				Debug.LogWarning("Could not find component of type " + injectType + " for field " + field.Name);
			else
				Debug.LogError("Could not find component of type " + injectType + " for field " + field.Name);
		} 
		else if (rawResult is object[])
		{
			var result = rawResult as object[];

			if (result.Length > 0)
			{
				if (index >= result.Length)
				{
					if (warnOnly)
						Debug.LogWarning("Could not find component of type " + injectType + " for field " + field.Name + " at index " + index);
					else
						Debug.LogError("Could not find component of type " + injectType + " for field " + field.Name + " at index " + index);
				}
				else
				{
					var found = result[index];
								
					field.SetValue(obj, found);
				}
			}
			else
			{
				if (warnOnly)
					Debug.LogWarning("Could not find component of type " + injectType + " for field " + field.Name);
				else
					Debug.LogError("Could not find component of type " + injectType + " for field " + field.Name);
			}
		}
	}
}

/// <summary>
/// A MonoBehavior that injects fields in the Awake() function
/// </summary>
public class BindableMonoBehavior : MonoBehaviour {

	/// <summary>
	/// The standard Unity Awake() function. This function will invoke UnityBinder.Inject.
	/// 
	/// If you override this Awake() function, be sure to call base.Awake()
	/// </summary>
	public virtual void Awake()
	{
		UnityBinder.Inject(this);
	}
}