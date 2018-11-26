using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

	public GameObject mainCamera;
	public GameObject videoCanvas;
	public MediaPlayerCtrl playerControl;
	public GameObject thumbnailCanvas;
	public AudioClip introSound;

	Vector3 mousePosition;

	enum State { Init, Shrinking, Shrinked, Expanding, Expanded }

	State state = State.Init;

	int videoIndex = 0;

	int thumbnailWidth = 320;
	int thumbnailHeight = 180;

	float distance = 1000f;
	float speed = 500f;

	List<GameObject> thumbnails = new List<GameObject>();
	List<int> vertices = new List<int>() { 15, 14, 14, 12, 12, 8, 8 };
	int total = 83;

	// Use this for initialization
	void Start () {

		AudioSource.PlayClipAtPoint (introSound, Vector3.zero);

		foreach(Texture2D thumbnail in Resources.LoadAll<Texture2D>("Textures/Thumbnails")) {
			GameObject obj = Instantiate (thumbnailCanvas);
			obj.SetActive (true);
			//GameObject obj = Instantiate (Resources.Load ("Prefabs/Thumbnail")) as GameObject;
			obj.GetComponent<RawImage>().texture = thumbnail;
			thumbnails.Add(obj);

			if (thumbnails.Count >= 83) {
				break;
			}
		}

		int index = 0;
		for (int i = 0; i < vertices.Count; i++) {
			for (int j = 0; j < vertices[i]; j++) {
				if (index >= thumbnails.Count) {
					break;
				}
				int row = (int)((i + 1) / 2);
				float modifier = 1.32f;
				if (i % 2 == 1) {
					modifier *= -1;
				}
				Transform transform = thumbnails [index++].transform;
				transform.position = new Vector3 (0,  row * thumbnailHeight * modifier, IVUtils.calculateRadius(thumbnailWidth, vertices[i]) * 1.18f);
				transform.RotateAround(Vector3.zero, Vector3.up, j*360/vertices[i]);
				transform.LookAt (Vector3.zero);
				transform.Rotate (new Vector3 (0, 180));
				shrinkThumbnails (distance, 1, false);
			}
		}
	}

	// Returns true if thumbnails are done shrinking, false otherwise.
	bool shrinkThumbnails(float step, float distance, bool reverse) {
		bool result = true;
		for (int i = 0; i < thumbnails.Count; i++) {
			Transform transform = thumbnails [i].transform;
			Vector3 newPosition = Vector3.MoveTowards (transform.position, Vector3.zero, step);
			if (newPosition.magnitude > distance) {
				transform.position = newPosition;
				result = false;
			} else {
				transform.position = transform.position.normalized * distance;
			}
			thumbnails [i].GetComponentInChildren<RawImage> ().color = IVUtils.calculateTransparency (transform.position.magnitude, distance, reverse);
		}
		return result;
	}

	// Returns true if thumbnails can be further expanded out, false otherwise.
	bool expandThumbnails(float step, float distance, bool reverse) {
		bool result = true;
		for (int i = 0; i < thumbnails.Count; i++) {
			Transform transform = thumbnails [i].transform;
			Vector3 newPosition = Vector3.MoveTowards (transform.position, Vector3.zero, -step);
			if (newPosition.magnitude < distance) {
				transform.position = newPosition;
				result = false;
			} else {
				transform.position = Vector3.ClampMagnitude(newPosition, distance);
			}
			thumbnails [i].GetComponentInChildren<RawImage> ().color = IVUtils.calculateTransparency (transform.position.magnitude, distance, reverse);
		}
		return result;
	}

	bool shrinkVideo(float step, float distance) {
		videoCanvas.transform.position = Vector3.MoveTowards (videoCanvas.transform.position, Vector3.zero, step);
		if (videoCanvas.transform.position.magnitude <= distance) {
			return true;
		}
		return false;
	}

	bool expandVideo(float step, float distance) {
		Vector3 towards = videoCanvas.transform.forward.normalized;
		videoCanvas.transform.position = Vector3.MoveTowards (videoCanvas.transform.position, towards * distance, step);
		if (videoCanvas.transform.position.magnitude >= distance) {
			return true;
		}
		return false;
	}
	
	// Update is called once per frame
	void Update () {
		if (state == State.Init) { 
			float step = 50f * Time.deltaTime;
			if (expandThumbnails (step, distance, false)){ //done expanding
				Debug.Log("Done init, new state = Shrinked");
				state = State.Shrinked;
			}
		}

		if(state == State.Expanding) {
			float step = speed * Time.deltaTime;
			if (expandThumbnails (step, distance*2, true) & shrinkVideo(step, distance)) { //done expanding
				Debug.Log("Done expanding, new state = Expanded");
				state = State.Expanded;
			}
		}

		if (state == State.Shrinking) {
			float step = speed * Time.deltaTime;
			if (shrinkThumbnails (step, distance, true) & expandVideo(step, distance*2)) { //done shrinking
				Debug.Log("Done Shrinking, new state = Shrinked");
				state = State.Shrinked;
				videoCanvas.SetActive (false);
			}
		}

		if(Input.GetKeyUp(KeyCode.Escape)) {
			Debug.Log ("Clicked Back Button.");
			playerStopped();
		}

		/*
		if (Input.GetMouseButtonDown (0)) {
			mousePosition = Input.mousePosition;
		} else if (Input.GetMouseButtonUp (0)) {
			mousePosition = Vector3.zero;
		}

		if(mousePosition != Vector3.zero)
		{
			Vector3 pos = Input.mousePosition - mousePosition;
			Vector3 rotate = new Vector3 (-pos.y, pos.x);
			mainCamera.transform.Rotate (rotate * .01f);
		}
		*/
	}

	public void thumbnailClicked() {
		Debug.Log ("Thumbnail clicked");
		if (state == State.Shrinked) {
			state = State.Expanding;

			videoCanvas.transform.LookAt (Vector3.zero);
			videoCanvas.transform.position = Vector3.zero;
			videoCanvas.transform.rotation = mainCamera.transform.rotation;
			expandVideo (distance * 2, distance * 2);

			videoCanvas.SetActive (true);
			if (videoIndex >= IVConstants.VIDEOS.Count) {
				videoIndex = 0;
			}
			playerControl.Load (IVConstants.VIDEOS [videoIndex++]);
		}
	}

	public void playerStopped() {
		Debug.Log ("Player stopped");
		if (state == State.Expanded) {
			state = State.Shrinking;

			playerControl.UnLoad();
		}
	}
}
