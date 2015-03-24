using UnityEngine;
using System.Collections;

public class TwitterToken {
	private const string TWITTER_USER_ID           = "TwitterUserID";
	private const string TWITTER_USER_SCREEN_NAME  = "TwitterUserScreenName";
	private const string TWITTER_USER_TOKEN        = "TwitterUserToken";
	private const string TWITTER_USER_TOKEN_SECRET = "TwitterUserTokenSecret";

	public class TokenHolder{
		private static TokenHolder instance = new TokenHolder();
        public static TokenHolder Instance	{get{return TokenHolder.instance;}}

		private Request requestTokenResponse;
		public Request RequestTokenResponse	{get{return this.requestTokenResponse;}set{this.requestTokenResponse = value;}}
		private Access accessTokenResponse;
		public Access AccessTokenResponse	{get{return this.accessTokenResponse;}set{this.accessTokenResponse = value;}}

		private TokenHolder(){}
	}

	public class Request{
		private string token;
		public string Token			{get{return token;}}
		private string tokenSecret;
		public string TokenSecret	{get{return tokenSecret;}}
		public bool isExist()		{return (	!string.IsNullOrEmpty(token) &&
												!string.IsNullOrEmpty(tokenSecret));}

		public Request(string token,string tokenSecret){
			this.token = token;
			this.tokenSecret = tokenSecret;
		}
	}
	public class Access{
		private string token;
		public string Token			{get{return token;}}
		private string tokenSecret;
		public string TokenSecret	{get{return tokenSecret;}}
		private string userId;
		public string UserId		{get{return userId;}}
		private string screenName;
		public string ScreenName	{get{return screenName;}}
		public bool isExist()		{return (	!string.IsNullOrEmpty(token) &&
												!string.IsNullOrEmpty(tokenSecret) &&
												!string.IsNullOrEmpty(userId) &&
												!string.IsNullOrEmpty(screenName));}

		public Access(string token,string tokenSecret,string userId,string screenName){
			this.token = token;
			this.tokenSecret = tokenSecret;
			this.userId = userId;
			this.screenName = screenName;
		}
	}

	public static void saveAccessToken(){
		PlayerPrefs.SetString(TWITTER_USER_ID, TokenHolder.Instance.AccessTokenResponse.UserId);
		PlayerPrefs.SetString(TWITTER_USER_SCREEN_NAME, TokenHolder.Instance.AccessTokenResponse.ScreenName);
		PlayerPrefs.SetString(TWITTER_USER_TOKEN, TokenHolder.Instance.AccessTokenResponse.Token);
		PlayerPrefs.SetString(TWITTER_USER_TOKEN_SECRET, TokenHolder.Instance.AccessTokenResponse.TokenSecret);
	}

	public static bool loadAccessToken(){
		bool isExist = false;
		TwitterToken.Access token = new TwitterToken.Access(
			PlayerPrefs.GetString(TWITTER_USER_TOKEN),
			PlayerPrefs.GetString(TWITTER_USER_TOKEN_SECRET),
			PlayerPrefs.GetString(TWITTER_USER_ID),
			PlayerPrefs.GetString(TWITTER_USER_SCREEN_NAME)
		);
		
		if (token.isExist()) {
			TwitterToken.TokenHolder.Instance.AccessTokenResponse = token;
			isExist = true;
			Debug.Log("loadAccessToken - succeeded");
		}
		
		return isExist;
	}
}
