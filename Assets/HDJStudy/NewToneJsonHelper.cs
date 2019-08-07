using Newtonsoft.Json;
using JsonC = Newtonsoft.Json.JsonConvert;
using System.Collections.Generic;
using UnityEngine;

public static class NewToneJsonHelper  {


	private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings {
		DefaultValueHandling = DefaultValueHandling.Ignore,
		Converters = new List<JsonConverter>(){new UnityVector3JsonConverter()},
		TypeNameHandling = TypeNameHandling.Auto,
	};

	public static T DeepCopy<T>(this T data){
		var jsonStr = JsonC.SerializeObject (data, _jsonSerializerSettings);
		return JsonC.DeserializeObject<T> (jsonStr);
	}
}

public class UnityVector3JsonConverter:JsonConverter{

	public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer){
		
	}

	public override object ReadJson (JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new System.NotImplementedException ();
	}

	public override bool CanRead {
		get {
			return false;
		}
	}

	public override bool CanWrite {
		get {
			return false;
		}
	}

	public override bool CanConvert (System.Type objectType)
	{
		return typeof(UnityEngine.Vector3) == objectType;
	}


	public void WriteFloat(JsonWriter writer,string name,float value){
		if (Mathf.Abs (value) < float.Epsilon)
			return;
		writer.WritePropertyName (name);
		writer.WriteValue (value);
	}
}
