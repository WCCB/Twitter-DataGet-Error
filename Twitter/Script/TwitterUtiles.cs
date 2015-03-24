using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

public class TwitterUtiles : SingletonMonoBehaviour<TwitterUtiles> {

	[SerializeField]
	private string consumerKey = "KMPN9RxQ9ueyEMsYhsD14ELMi";
	public string ConsumerKey{get{return consumerKey;}}
	
	[SerializeField]
	private string consumerSecret = "rJC9IZgf9ed3Cy6dhu5QTyY5iRBKVtFicxkz2n3q04BF2fndAM";
	public string ConsumerSecret{get{return consumerSecret;}}

	private static readonly string[] OAUTH_HEADER_PARAMS = new[]{
		"oauth_version",
		"oauth_nonce",
		"oauth_timestamp",
		"oauth_signature_method",
		"oauth_consumer_key",
		"oauth_token",
		"oauth_verifier"
	};

	private static readonly string[] SECRET_PARAMS = new[]{
		"oauth_consumer_secret",
		"oauth_token_secret",
		"oauth_signature"
	};
	
	public void addDefaultParams(Dictionary<string,string> parameters){
		if (!string.IsNullOrEmpty (consumerKey) && !string.IsNullOrEmpty (consumerSecret)) {
			parameters.Add("oauth_version", "1.0");
			parameters.Add("oauth_nonce", generateNonce());
			parameters.Add("oauth_timestamp", generateTimeStamp());
			parameters.Add("oauth_signature_method", "HMAC-SHA1");
			parameters.Add("oauth_consumer_key", consumerKey);
			parameters.Add("oauth_consumer_secret", consumerSecret);
		}
	}

	public void addOAuthTokenParams(Dictionary<string,string> parameters){
		if (TwitterToken.TokenHolder.Instance.AccessTokenResponse.isExist()) {
			parameters.Add("screen_name", TwitterToken.TokenHolder.Instance.AccessTokenResponse.ScreenName);
			parameters.Add("user_id", TwitterToken.TokenHolder.Instance.AccessTokenResponse.UserId);
			parameters.Add("oauth_token", TwitterToken.TokenHolder.Instance.AccessTokenResponse.Token);
			parameters.Add("oauth_token_secret", TwitterToken.TokenHolder.Instance.AccessTokenResponse.TokenSecret);
		}
	}

	public string makeHeader(string HTTPRequestType, string URL, Dictionary<string, string> parameters){
		// Add the signature to the oauth parameters
		string signature = generateSignature(HTTPRequestType, URL, parameters);
		
		parameters.Add("oauth_signature", signature);

		StringBuilder headerBuilder = new StringBuilder();
		headerBuilder.AppendFormat("OAuth realm=\"{0}\"", "Twitter API");
		
		SortedDictionary<string, string> sortedParameters = sortParams (parameters);
		
		foreach (var item in sortedParameters){
			headerBuilder.AppendFormat(",{0}=\"{1}\"", urlEncode(item.Key), urlEncode(item.Value));
		}
		
		headerBuilder.AppendFormat(",oauth_signature=\"{0}\"", urlEncode(parameters["oauth_signature"]));

		return headerBuilder.ToString();
	}

	public string generateNonce(){
		// Just a simple implementation of a random number between 123400 and 9999999
		return new System.Random().Next(123400, int.MaxValue).ToString("X", CultureInfo.InvariantCulture);
	}
	
	public string generateTimeStamp(){
		// Default implementation of UNIX time of the current UTC time
		TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
	}
	
	public string generateSignature(string httpMethod, string url, Dictionary<string, string> parameters){
		Dictionary<string, string> nonSecretParameters = new Dictionary<string, string>();
		foreach(KeyValuePair<string, string> param in parameters){
			bool found = false;
			foreach(string secretParam in SECRET_PARAMS){
				if(secretParam == param.Key){
					found = true;
					break;
				}
			}
			if(!found)	nonSecretParameters.Add(param.Key, param.Value);
		}
		
		// Create the base string. This is the string that will be hashed for the signature.
		string signatureBaseString = string.Format(CultureInfo.InvariantCulture,
		                                           "{0}&{1}&{2}",
		                                           httpMethod,
		                                           urlEncode(normalizeUrl(new Uri(url))),
		                                           urlEncode(nonSecretParameters));
		Debug.Log (signatureBaseString);
		// Create our hash key (you might say this is a password)
		string key = string.Format(CultureInfo.InvariantCulture,
		                           "{0}&{1}",
		                           urlEncode(parameters["oauth_consumer_secret"]),
		                           parameters.ContainsKey("oauth_token_secret") ?  urlEncode(parameters["oauth_token_secret"]) : string.Empty);
		Debug.Log (key);
		// Generate the hash
		HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(key));
		byte[] signatureBytes = hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
		return Convert.ToBase64String(signatureBytes);
	}

	//パラメーターのソート
	private SortedDictionary<string, string> sortParams(Dictionary<string,string> parameters){
		SortedDictionary<string, string> sortedParameters = new SortedDictionary<string,string>();
		foreach(KeyValuePair<string, string> param in parameters){
			foreach(string oauth_header_param in OAUTH_HEADER_PARAMS){
				if(oauth_header_param.Contains(param.Key)){
					sortedParameters.Add(param.Key, param.Value);
				}
			}
		}
		return sortedParameters;
	}

	//URLのエンコード
	public string urlEncode(string value){
		if (string.IsNullOrEmpty(value)){
			return string.Empty;
		}
		
		value = Uri.EscapeDataString(value);
		
		value = Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper())
			.Replace("(", "%28")
				.Replace(")", "%29")
				.Replace("$", "%24")
				.Replace("!", "%21")
				.Replace("*", "%2A")
				.Replace("'", "%27")
				.Replace("%7E", "~");
		
		return value;
	}
	
	//URLのエンコード
	public string urlEncode(IEnumerable<KeyValuePair<string, string>> parameters){
		StringBuilder parameterString = new StringBuilder();
		
		var paramsSorted = from p in parameters
			orderby p.Key, p.Value
				select p;
		
		foreach (var item in paramsSorted){
			if (parameterString.Length > 0){
				parameterString.Append("&");
			}
			
			parameterString.Append(
				string.Format(
				CultureInfo.InvariantCulture,
				"{0}={1}",
				urlEncode(item.Key),
				urlEncode(item.Value)));
		}
		
		return urlEncode(parameterString.ToString());
	}
	
	//URLのノーマライズ
	public string normalizeUrl(Uri url){
		string normalizedUrl = string.Format(CultureInfo.InvariantCulture, "{0}://{1}", url.Scheme, url.Host);
		if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443))){
			normalizedUrl += ":" + url.Port;
		}
		
		normalizedUrl += url.AbsolutePath;
		return normalizedUrl;
	}
}
