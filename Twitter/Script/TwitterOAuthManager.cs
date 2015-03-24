using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

[RequireComponent(typeof (TwitterUtiles))]
public class TwitterOAuthManager : SingletonMonoBehaviour<TwitterOAuthManager> {
	private readonly string REQUEST_TOKEN_URL = "https://api.twitter.com/oauth/request_token";
	private readonly string AUTHORIZATION_URL = "https://api.twitter.com/oauth/authenticate?oauth_token={0}";
	private readonly string ACCESS_TOKEN_URL = "https://api.twitter.com/oauth/access_token";

	[SerializeField]
	private UnityEvent _onGetRequestTokenSucceeded;
	public UnityEvent onGetRequestTokenSucceeded   {set{_onGetRequestTokenSucceeded = value;}}
	
	[SerializeField]
	private UnityEvent _onGetRequestTokenFailed;
	public UnityEvent onGetRequestTokenFailed      {set{_onGetRequestTokenFailed = value;}}

	[SerializeField]
	private UnityEvent _onGetAccessTokenSucceeded;
	public UnityEvent onGetAccessTokenSucceeded   {set{_onGetAccessTokenSucceeded = value;}}
	
	[SerializeField]
	private UnityEvent _onGetAccessTokenFailed;
	public UnityEvent onGetAccessTokenFailed      {set{_onGetAccessTokenFailed = value;}}

	//PIN取得メソッド
	public void twitterOAuthGetPIN(){
		StartCoroutine(coGetRequestToken());
	}

	//AccessToken取得メソッド
	public void twitterOAuthGetAccessToken(string pin){
		StartCoroutine (coGetAccessToken (TwitterToken.TokenHolder.Instance.RequestTokenResponse.Token, pin));
	}

	//リクエストトークン取得コルーチン
	private IEnumerator coGetRequestToken(){
		WWW web = wwwRequestToken();
		
		yield return web;
		
		if (!string.IsNullOrEmpty(web.error)){
			Debug.Log(string.Format("GetRequestToken - failed. error : {0}", web.error));
			getRequestTokenCallback(false);
		}
		else{
			TwitterToken.Request response = new TwitterToken.Request(
				Regex.Match(web.text, @"oauth_token=([^&]+)").Groups[1].Value,
				Regex.Match(web.text, @"oauth_token_secret=([^&]+)").Groups[1].Value
			);
			
			if (response.isExist()){
				TwitterToken.TokenHolder.Instance.RequestTokenResponse = response;
				Application.OpenURL(string.Format(AUTHORIZATION_URL, response.Token));
				getRequestTokenCallback(true);
			}
			else{
				Debug.Log(string.Format("GetRequestToken - failed. response : {0}", web.text));
				getRequestTokenCallback(false);
			}
		}
	}

	private void getRequestTokenCallback(bool isSuccess){
		if (isSuccess) {
			if (_onGetRequestTokenSucceeded != null){
				_onGetRequestTokenSucceeded.Invoke();
			}
		}
		else {
			if (_onGetRequestTokenFailed != null){
				_onGetRequestTokenFailed.Invoke();
			}
		}
	}

	//アクセストークン取得コルーチン
	private IEnumerator coGetAccessToken(string requestToken, string pin){
		WWW web = wwwAccessToken(requestToken, pin);
		
		yield return web;
		
		if (!string.IsNullOrEmpty(web.error)){
			Debug.Log(string.Format("GetAccessToken - failed. error : {0}", web.error));
			getAccessTokenCallback(false);
		}
		else{
			TwitterToken.Access response = new TwitterToken.Access(
				Regex.Match(web.text, @"oauth_token=([^&]+)").Groups[1].Value,
				Regex.Match(web.text, @"oauth_token_secret=([^&]+)").Groups[1].Value,
				Regex.Match(web.text, @"user_id=([^&]+)").Groups[1].Value,
				Regex.Match(web.text, @"screen_name=([^&]+)").Groups[1].Value
			);
			
			if (response.isExist()){
				TwitterToken.TokenHolder.Instance.AccessTokenResponse = response;
				getAccessTokenCallback(true);
			}
			else{
				Debug.Log(string.Format("GetAccessToken - failed. response : {0}", web.text));	
				getAccessTokenCallback(false);
			}
		}
	}

	private void getAccessTokenCallback(bool isSuccess){
		if (isSuccess) {
			if (_onGetAccessTokenSucceeded != null){
				_onGetAccessTokenSucceeded.Invoke();
			}
		}
		else {
			if (_onGetAccessTokenFailed != null){
				_onGetAccessTokenFailed.Invoke();
			}
		}
	}

	private WWW wwwRequestToken(){
		// Add data to the form to post.
		WWWForm form = new WWWForm();
		form.AddField("oauth_callback", "oob");
		
		// HTTP header
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		TwitterUtiles.Instance.addDefaultParams(parameters);
		parameters.Add("oauth_callback", "oob");
		
		var headers = new Hashtable();
		headers["Authorization"] = TwitterUtiles.Instance.makeHeader("POST", REQUEST_TOKEN_URL, parameters);
		
		return new WWW(REQUEST_TOKEN_URL, form.data, headers.ToDictionary());
	}

	private WWW wwwAccessToken(string requestToken, string pin){
		// Need to fill body since Unity doesn't like an empty request body.
		byte[] dummmy = new byte[1];
		dummmy[0] = 0;
		
		// HTTP header
		var headers = new Hashtable();
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		TwitterUtiles.Instance.addDefaultParams(parameters);
		parameters.Add("oauth_token", requestToken);
		parameters.Add("oauth_verifier", pin);
		
		headers["Authorization"] = TwitterUtiles.Instance.makeHeader("POST", ACCESS_TOKEN_URL, parameters);

		return new WWW(ACCESS_TOKEN_URL, dummmy, headers.ToDictionary());
	}
}
