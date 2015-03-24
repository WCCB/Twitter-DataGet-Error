
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public class TwitterUserDataGetter : SingletonMonoBehaviour<TwitterUserDataGetter> {

	private const string SHOW_USER_DATA_URL = "https://api.twitter.com/1.1/users/show.json";

	private string userName;

	public void getData(){
		StartCoroutine(coGetData());
	}

	private IEnumerator coGetData(){

		//WWWForm form = new WWWForm();
		Hashtable hash = new Hashtable();
		//hash = form.headers.ToHashtable();

		//byte[] dummmy = new byte[1];
		//dummmy[0] = 0;
		
		WWW web = null;
		hash["Authorization"] = makeUserHeader();
		web = new WWW(SHOW_USER_DATA_URL, null, hash.ToDictionary());
		
		yield return web;

		if (!string.IsNullOrEmpty(web.error)){
			Debug.Log(string.Format("GetData - failed. {0}\n{1}", web.error, web.text));
			Debug.Log(string.Format(web.url));
			getDataCallback(false);
		}
		else{
			string error = Regex.Match(web.text, @"<error>([^&]+)</error>").Groups[1].Value;

			if (!string.IsNullOrEmpty(error)){
				Debug.Log(string.Format("GetData - failed. {0}", error));
				getDataCallback(false);
			}
			else{
				getDataCallback(true);
			}
		}
	}

	private string makeUserHeader(){		
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		TwitterUtiles.Instance.addDefaultParams(parameters);
		TwitterUtiles.Instance.addOAuthTokenParams(parameters);
		//parameters.Add("include_entities", "true");
		return TwitterUtiles.Instance.makeHeader("GET",SHOW_USER_DATA_URL,parameters);
	}
	
	//コールバック
	private void getDataCallback(bool isSuccess){
		if (isSuccess) {

		}
		else {

		}
	}
}