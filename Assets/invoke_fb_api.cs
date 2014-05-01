using UnityEngine;
using System.Collections;
using Facebook.MiniJSON;		// JSON Data Processor

public class invoke_fb_api : MonoBehaviour {

	private bool isInit = false;
	private string url = "";

	// FB init
	private void SetInit() {
		this.isInit = true;
		// show login ui or access token input ui
		if (FB.IsLoggedIn != true) {
			FB.Login ("publish_actions", this.AuthCallback);
		}
		else{
			this.do_my_work();
		}
	}

	void do_my_work(){
		if(this.gameObject.name == "Button1") {
			StartCoroutine(TakeScreenshot());
		}
		else{
			// get My Friends
			FB.API("me/friends?fields=picture.type(large),name", Facebook.HttpMethod.GET, Callback2);
		}
	}

	private void OnHideUnity(bool isGameShown) {
		if (!isGameShown) {
			// pause the game - we will need to hide
			Time.timeScale = 0;
		} else {
			// start the game back up - we're getting focus again
			Time.timeScale = 1;
		}
	}

	// FB Login
	void AuthCallback(FBResult result) {
		if(FB.IsLoggedIn) {
			Debug.Log(FB.UserId);

			this.do_my_work();

		} else {
			Debug.Log("User cancelled login");
		}
	}

	// FB.api
	void Callback(FBResult result)
	{
		Debug.Log ("Finished API call.");
	}

	// FB.api callback for friends
	void Callback2(FBResult result)
	{
		Debug.Log (result.Text);

		IDictionary data = (IDictionary) Json.Deserialize(result.Text);

		IList friends = (IList) data["data"];

		string name;
		string id;

		foreach (IDictionary friend in friends) {

			name = (string)friend["name"];
			id = (string)friend["id"];
			url = (string)((IDictionary)((IDictionary)friend["picture"])["data"])["url"];

			StartCoroutine(getTextureByURL());

			// fetch only one row
			break;
		}

		Debug.Log ("Finished API call.");
	}

	IEnumerator getTextureByURL() {
		WWW www = new WWW(url);
		yield return www;
		GameObject.Find ("Profile Picture").guiTexture.texture = www.texture;
	}

	private IEnumerator TakeScreenshot() 
	{
		yield return new WaitForEndOfFrame();
		
		var width = Screen.width;
		var height = Screen.height;
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		// Read screen contents into the texture
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		tex.Apply();
		byte[] screenshot = tex.EncodeToPNG();
		
		var wwwForm = new WWWForm();
		wwwForm.AddBinaryData("image", screenshot, "InteractiveConsole.png");
		
		FB.API("me/photos", Facebook.HttpMethod.POST, Callback, wwwForm);
	}

	// Use this for initialization
	void Start () {
		GUI.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseOver(){
		// Debug.Log ("over...");
		if (Input.GetMouseButtonDown(0)) {
			if(this.isInit!=true){
				// invoke init & login
				FB.Init(this.SetInit, this.OnHideUnity);
				Debug.Log ("invoke init & login if not init/login then call api.");
			}
			else{
				// only invoke login
				this.SetInit();
				Debug.Log ("only invoke login if not login then call api.");
			}

		}
	}
}
