using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR.WSA.Input;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Networking;

public class GazeGestureManager : MonoBehaviour {

    public static GazeGestureManager Instance { get; private set; }
    public GameObject FocusedObject { get; private set; }
    public GameObject TextViewPrefab;
    public AudioClip captureAudioClip;
    public AudioClip failedAudioClip;
    public GameObject model;
	public GameObject DUI;
    public GameObject MyGameObject;
	
		
	public List<string> myCollection = new List<string>();
	public List<string> myCollection2 = new List<string>();
	public List<string> myCollection3 = new List<string>();
	public List<string> myCollection4 = new List<string>();

    GestureRecognizer gestureRecognizer;
    PhotoInput photoInput;
    QrDecoder qrDecoder;
    AudioSource captureAudioSource;
    AudioSource failedAudioSource;

    public int width = 56;
    public int height = 56;

    void Awake () {
        Instance = this;
        photoInput = GetComponent<PhotoInput>();
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.TappedEvent += GestureRecognizer_TappedEvent;
        gestureRecognizer.StartCapturingGestures();
        qrDecoder = gameObject.AddComponent<QrDecoder>();
        
}

    void Start() {
        captureAudioSource = gameObject.AddComponent<AudioSource>();
        captureAudioSource.clip = captureAudioClip;
        captureAudioSource.playOnAwake = false;
        failedAudioSource = gameObject.AddComponent<AudioSource>();
        failedAudioSource.clip = failedAudioClip;
        failedAudioSource.playOnAwake = false;
    }

    private void Update() {
    }

    void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay) {
        photoInput.CapturePhotoAsync(onPhotoCaptured);
    }

    void onPhotoCaptured(List<byte> image, int width, int height) {
        string val = qrDecoder.Decode(image.ToArray(), width, height);
        Debug.Log(val);
        if(GameObject.Find ("QrSight").transform.localScale == new Vector3(0, 0, 0)){
			 StartCoroutine(GetTextAgain());
		}
		else if (val != null) {
            showText(val);
            captureAudioSource.Play();
        } else {
            failedAudioSource.Play();
        }
    }

    void showText(string text) {
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo)) {
			GameObject.Find ("QrSight").transform.localScale = new Vector3(0, 0, 0);
			model = Instantiate(Resources.Load("mazdaMC", typeof(GameObject))) as GameObject;
            //Material mat = Resources.Load("Models/Materials/Mazda_0-3") as Material;
            MyGameObject = model.transform.GetChild(0).GetChild(0).gameObject;
			MeshCollider MymeshCollider = MyGameObject.AddComponent<MeshCollider>();
			model.transform.Rotate(0, -90, 0);
			var temp = new Vector3(-0.25f, 0f, 0.0f);
			model.transform.position = temp;
            var obj = Instantiate(TextViewPrefab, hitInfo.point, Quaternion.identity);
            StartCoroutine(GetText(obj));
        }
    }
	
	void updateUIPos(){
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        RaycastHit hitInfo;
        //if (Physics.Raycast(headPosition, gazeDirection, out hitInfo)) {
		
		var pos = GameObject.Find("mazdaMC").transform.position;		
			
		var temp = new Vector3(pos.x/2, pos.y+1.0f, pos.z/2);
		DUI.transform.position = temp;
        //}
		
		RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(headPosition), out hit))
            return;

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            return;

        Texture2D tex = rend.material.mainTexture as Texture2D;
        Vector2 pixelUV = hit.textureCoord;
		
		var item0 = DUI.transform.Find("dt-00").gameObject;
		var textMesh = item0.GetComponent<TextMesh>();
		textMesh.text = pixelUV.x.ToString();
		
		
	}
	
		void showUI(int passedID){
		
		DUI = Instantiate(Resources.Load("DUI", typeof(GameObject))) as GameObject;
		
		var temp = new Vector3(0, 0, 1.25f);
        DUI.transform.position = temp;
		DUI.transform.localScale = new Vector3(0.22f, 0.13f, 0.01f);
		
		//Failed Item		
		var item0 = DUI.transform.Find("dt-00").gameObject;
		var textMesh = item0.GetComponent<TextMesh>();
		textMesh.text = myCollection[passedID];
		
		//Critical
		if(myCollection2[passedID]=="false"){
			myCollection2[passedID] = "N";
		}
		else{
			myCollection2[passedID] ="Y";
		}
		var item1 = DUI.transform.Find("dt-01").gameObject;
		var textMesh1 = item1.GetComponent<TextMesh>();
		textMesh1.text = myCollection2[passedID];
		
		//Complaints
		var item2 = DUI.transform.Find("dt-02").gameObject;
		var textMesh2 = item2.GetComponent<TextMesh>();
		textMesh2.text = myCollection3[passedID];
		
		//Comments 
		var item3 = DUI.transform.Find("dt-03").gameObject;
		var textMesh3 = item3.GetComponent<TextMesh>();
		textMesh3.text = myCollection4[passedID];
	}
	
	public IEnumerator GetText(GameObject obj)
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://rdu-rsnell.tmwsystems.com/AMSServerapi/api/inspections?activeCode=Y&defective=Y&unitId=159&appid=null"))
        {
            www.SetRequestHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InJzbmVsbCIsIm5iZiI6MTUwODQzOTkyMCwiZXhwIjoxNTExMTE4MzIwLCJpYXQiOjE1MDg0Mzk5MjB9.7anRXYIGKKPnKb9OOjKnVqzEVou3R-gcDAfSzG-5yEE");
            yield return www.Send();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                var N = JSON.Parse(www.downloadHandler.text);
                var texture = GetTexture();
				
				for(var i=0;i< N["data"].Count; i++)
                {
					myCollection.Add(N["data"][i]["inspectionFieldText"]);
					myCollection2.Add(N["data"][i]["critical"]);
					myCollection3.Add(N["data"][i]["complaint"]);
					myCollection4.Add(N["data"][i]["inspComment"]);
                }
				
                if (N["data"].Count >= 1)
                {
                    // set the default color GREEN for the vehicle
                    texture = SetDefaultColor(texture);
                    for (var i = 0; i < N["data"].Count; i++)
                    {
                        Debug.Log(N["data"][i]["inspectionFieldText"]);

                        if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Front Bumper")
                        {
                            texture = SetFrontBumperColorFailure(texture);
                        }
                        else if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Back Bumper")
                        {
                            texture = SetBackBumperColorFailure(texture);
                        }
                        else if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Front Windshield")
                        {
                            texture = SetFrontWindSheildColorFailure(texture);
                        }
                        else if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Rear Windshield")
                        {
                            texture = SetRearWindSheildColorFailure(texture);
                        }
                    }
                }
				else{
					SetDefaultColor(texture);
				}
                //Apply the texture
                texture.Apply();

                //Apply the texture to the main game object
                MyGameObject.GetComponent<Renderer>().material.mainTexture = texture;
                //showUI(0);
				
				var textMesh = obj.GetComponent<TextMesh>();
           		textMesh.transform.localScale = new Vector3(0, 0, 0);
				
            }
        }
    }
	
	
	public IEnumerator GetTextAgain()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://rdu-rsnell.tmwsystems.com/AMSServerapi/api/inspections?activeCode=Y&defective=Y&unitId=159&appid=null"))
        {
            www.SetRequestHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InJzbmVsbCIsIm5iZiI6MTUwODQzOTkyMCwiZXhwIjoxNTExMTE4MzIwLCJpYXQiOjE1MDg0Mzk5MjB9.7anRXYIGKKPnKb9OOjKnVqzEVou3R-gcDAfSzG-5yEE");
            yield return www.Send();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                var N = JSON.Parse(www.downloadHandler.text);
                var texture = GetTexture();
				
				for(var i=0;i< N["data"].Count; i++)
                {
					myCollection.Add(N["data"][i]["inspectionFieldText"]);
					myCollection2.Add(N["data"][i]["critical"]);
					myCollection3.Add(N["data"][i]["complaint"]);
					myCollection4.Add(N["data"][i]["inspComment"]);
                }
				
                if (N["data"].Count >= 1)
                {
                    // set the default color GREEN for the vehicle
                    texture = SetDefaultColor(texture);
                    for (var i = 0; i < N["data"].Count; i++)
                    {
                        Debug.Log(N["data"][i]["inspectionFieldText"]);

                        if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Front Bumper")
                        {
                            texture = SetFrontBumperColorFailure(texture);
                        }
                        else if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Back Bumper")
                        {
                            texture = SetBackBumperColorFailure(texture);
                        }
                        else if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Front Windshield")
                        {
                            texture = SetFrontWindSheildColorFailure(texture);
                        }
                        else if (!string.IsNullOrEmpty(N["data"][i]["inspectionFieldText"]) && N["data"][i]["inspectionFieldText"] == "Rear Windshield")
                        {
                            texture = SetRearWindSheildColorFailure(texture);
                        }
                    }
                }
				else{
					SetDefaultColor(texture);
				}
                //Apply the texture
                texture.Apply();

                //Apply the texture to the main game object
                MyGameObject.GetComponent<Renderer>().material.mainTexture = texture;
     			
            }
        }
    }
	

    public Texture2D SetFrontBumperColorFailure(Texture2D texture)
    {
        for (int i = 0; i < width; i++)
        {
            for (var m = 0; m < 5; m++)
            {
                texture.SetPixel(m, height - 1 - i, Color.red);
            }
        }

        return texture;

    }

    public Texture2D SetBackBumperColorFailure(Texture2D texture)
    {
        for (int i = 0; i < width; i++)
        {
            for (var k = 17; k < 23; k++)
            {
                texture.SetPixel(k, height - 1 - i, Color.red);
            }
        }
        return texture;
    }

    public Texture2D SetRearWindSheildColorFailure(Texture2D texture)
    {
        for (int i = 0; i < width; i++)
        {
            for (var k = 11; k < 17; k++)
            {
                texture.SetPixel(k, height - 1 - i, Color.red);
            }

        }
        return texture;
    }

    public Texture2D SetFrontWindSheildColorFailure(Texture2D texture)
    {
        for (int i = 0; i < width; i++)
        {
            for (var k = 5; k < 11; k++)
            {
                texture.SetPixel(k, height - 1 - i, Color.red);
            }
        }
        return texture;
    }

    public Texture2D SetDefaultColor(Texture2D texture)
    {
        //Default Green
        for (int i = 0; i < width; i++)
        {
            for (var k = 0; k < 56; k++)
            {
                texture.SetPixel(k, height - 1 - i, Color.green);
            }
        }
        return texture;
    }

    public Texture2D GetTexture()
    {
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        return texture;
    }
}
