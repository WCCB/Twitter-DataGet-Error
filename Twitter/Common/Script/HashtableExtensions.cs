using System.Collections;
using System.Collections.Generic;

public static partial class HashtableExtensions{

	public static Dictionary<string, string> ToDictionary(this Hashtable self){
		Dictionary<string, string> result = new Dictionary<string, string>();
		foreach(DictionaryEntry n in self){
			result[ n.Key.ToString() ] = n.Value.ToString();
		}
		return result;
	}
}
