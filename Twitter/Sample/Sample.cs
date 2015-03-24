using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Sample : MonoBehaviour {
	public InputField pinField;

	public void login(){
		TwitterOAuthManager.Instance.twitterOAuthGetAccessToken (pinField.text);
	}
}
