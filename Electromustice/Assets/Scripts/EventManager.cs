using System;
using System.Collections.Generic;

public enum EnumEvent
{
	OnMagnetUp,
	OnMagnetDown,
	OnMagnetStay
}

public delegate void EventHandler();
public delegate void EventHandler<T>(T para1);
public delegate void EventHandler<T, U>(T para1, U para2);

public static class EventManager{

	private static Dictionary<EnumEvent, Delegate> dic_eventList = new Dictionary<EnumEvent, Delegate> ();

	public static void AddEventFunction(EnumEvent _enum_eventType, EventHandler handler)
	{
		//Create a lock on the event list to keep this thread-safe
		lock(dic_eventList)
		{
			if(!dic_eventList.ContainsKey(_enum_eventType))
			{
				dic_eventList.Add(_enum_eventType, null);
			}

			//Update the value of the existing key in order to add a function to the existing list of event
			dic_eventList[_enum_eventType] = (EventHandler)dic_eventList[_enum_eventType] + handler;
		}
	}

	public static void RemoveEventFunction(EnumEvent _enum_eventType, EventHandler handler)
	{
		lock(dic_eventList)
		{
			if(dic_eventList.ContainsKey(_enum_eventType))
			{
				dic_eventList[_enum_eventType] = (EventHandler)dic_eventList[_enum_eventType] - handler;

				if(dic_eventList[_enum_eventType] == null)
				{
					dic_eventList.Remove (_enum_eventType);
				}
			}
		}
	}

	public static void callEventFunction(EnumEvent eventType)
	{
		Delegate del;
		// Raise the delegate only if the event type is in the dictionary.
		if(dic_eventList.TryGetValue(eventType, out del))
		{
			// Take a local copy to prevent a race condition if another thread
			// were to unsubscribe from this event.
			EventHandler handler = (EventHandler)del;

			// Raise the delegate if it's not null.
			if(handler != null)
			{
				handler();   
			}
		}
	}
}


//An EventHandler for events that have one parameter of type T.
public static class EventManager<T>
{
	private static Dictionary<EnumEvent, Delegate> dic_eventList = new Dictionary<EnumEvent, Delegate>();

	public static void AddEventFunction(EnumEvent _enum_eventType, EventHandler<T> handler)
	{
		lock(dic_eventList)
		{
			if(!dic_eventList.ContainsKey(_enum_eventType))
			{
				dic_eventList.Add(_enum_eventType, null);
			}

			dic_eventList[_enum_eventType] = (EventHandler<T>)dic_eventList[_enum_eventType] + handler;
		}
	}

	public static void RemoveEventFunction(EnumEvent _enum_eventType, EventHandler<T> handler)
	{
				lock (dic_eventList) {
						if (dic_eventList.ContainsKey (_enum_eventType)) {
								dic_eventList [_enum_eventType] = (EventHandler<T>)dic_eventList [_enum_eventType] - handler;

								if (dic_eventList [_enum_eventType] == null) {
										dic_eventList.Remove (_enum_eventType);
								}
						}
				}
		}
	public static void callEventFunction(EnumEvent _enum_eventType, T para1)
	{
		Delegate del;

		if(dic_eventList.TryGetValue(_enum_eventType, out del))
		{
			EventHandler<T> handler = (EventHandler<T>)del;

			if(handler != null)
			{
				handler(para1);
			}
		}
	}
}


//An EventHandler for events that have two parameters of types T and U

public static class EventManager<T, U>
{
	private static Dictionary<EnumEvent, Delegate> dic_eventList = new Dictionary<EnumEvent, Delegate>();

	public static void AddEventFunction(EnumEvent _enum_eventType, EventHandler<T, U> handler)
	{
		lock(dic_eventList)
		{
			if(!dic_eventList.ContainsKey(_enum_eventType))
			{
				dic_eventList.Add(_enum_eventType, null);							                  
			}

			dic_eventList[_enum_eventType] = (EventHandler<T, U>)dic_eventList[_enum_eventType] + handler;
		}
	}

	public static void RemoveEventFunction(EnumEvent _enum_eventType, EventHandler<T, U> handler)
	{
		lock(dic_eventList)
		{
			if(dic_eventList.ContainsKey(_enum_eventType))
			{
				dic_eventList[_enum_eventType] = (EventHandler<T, U>)dic_eventList[_enum_eventType] - handler;

				if(dic_eventList[_enum_eventType] == null)
				{
					dic_eventList.Remove(_enum_eventType);
				}
			}
		}
	}

	public static void callEventFunction(EnumEvent _enum_eventType, T para1, U para2)
	{
		Delegate del;

		if(dic_eventList.TryGetValue(_enum_eventType, out del))
		{
			EventHandler<T, U> handler = (EventHandler<T, U>)del;

			if(handler != null)
			{
				handler(para1, para2);
			}
		}
	}
}











