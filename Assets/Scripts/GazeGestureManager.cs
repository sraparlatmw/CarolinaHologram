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
    public GameObject MyGameObject;
	
		
	public List<string> myCollection = new List<string>();

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
        if (val != null) {
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

            model = Instantiate(Resources.Load("mazdaMC", typeof(GameObject))) as GameObject;
            //Material mat = Resources.Load("Models/Materials/Mazda_0-3") as Material;
            MyGameObject = model.transform.GetChild(0).GetChild(0).gameObject;

            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;

            //FRONT BUMPER
            for (int i = 0; i < width; i++)
            {
                for (var m = 0; m < 5; m++)
                    texture.SetPixel(m, height - 1 - i, Color.red);
            }

            //FRONT WINDSHIELD
            for (int i = 0; i < width; i++)
            {
                for (var m = 5; m < 11; m++)
                    texture.SetPixel(m, height - 1 - i, Color.green);
            }

            //REAR WINDSHEILD
            for (int i = 0; i < width; i++)
            {
                for (var k = 11; k < 17; k++)
                    texture.SetPixel(k, height - 1 - i, Color.red);
            }

            //BACK BUMPER
            for (int i = 0; i < width; i++)
            {
                for (var k = 17; k < 23; k++)
                    texture.SetPixel(k, height - 1 - i, Color.red);
            }

            for (int i = 0; i < width; i++)
            {
                for (var k = 23; k < 56; k++)
                    texture.SetPixel(k, height - 1 - i, Color.green);
            }
            texture.Apply();

            MyGameObject.GetComponent<Renderer>().material.mainTexture = texture;

            var obj = Instantiate(TextViewPrefab, hitInfo.point, Quaternion.identity);

            StartCoroutine(GetText(obj));
        }
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
                for(var i=0;i< N["data"].Count; i++)
                {
                    Debug.Log(N["data"][i]["inspectionFieldText"]);
					myCollection.Add(N["data"][i]["inspectionFieldText"]);
                }
                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
				
				
           		var textMesh = obj.GetComponent<TextMesh>();
           		textMesh.text = N["data"][0]["inspectionFieldText"];
				
            }
        }
    }
}
