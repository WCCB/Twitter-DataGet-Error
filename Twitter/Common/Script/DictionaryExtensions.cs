using System.Collections;
using System.Collections.Generic;

public static class DictionaryExtensions{

	public static Hashtable ToHashtable<TKey, TValue>(this Dictionary<TKey, TValue> self){
		Hashtable result = new Hashtable();
		foreach (var n in self){
			result[ n.Key ] = n.Value;
		}
		return result;
	}
}