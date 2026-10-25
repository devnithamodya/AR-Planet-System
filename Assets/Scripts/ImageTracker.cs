using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker : MonoBehaviour
{
    // one row in the Inspector: "when you see THIS image, show THIS prefab"
    [System.Serializable]
    public class ImagePrefab
    {
        public string imageName;      // must match the name in the Reference Image Library
        public GameObject prefab;
    }

    public ARTrackedImageManager trackedImageManager;   // drag XR Origin here
    public List<ImagePrefab> imagePrefabs = new List<ImagePrefab>();
    public float heightAboveImage = 0.05f;               // 5 cm above the card

    [Header("Sound (optional)")]
    public AudioSource audioSource;
    public AudioClip foundSound;

    // remember which object we made for which image
    private Dictionary<TrackableId, GameObject> spawned = new Dictionary<TrackableId, GameObject>();

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnImagesChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnImagesChanged);
    }

    // AR Foundation calls this whenever images are found or move
    void OnImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (ARTrackedImage image in args.added)
        {
            SpawnFor(image);
        }

        foreach (ARTrackedImage image in args.updated)
        {
            // show the object only while the camera can really see the image
            if (spawned.TryGetValue(image.trackableId, out GameObject obj))
            {
                obj.SetActive(image.trackingState == TrackingState.Tracking);
            }
        }

        // FIX: clean up when an image is removed
        foreach (var removed in args.removed)
        {
            if (spawned.TryGetValue(removed.Key, out GameObject obj))
            {
                if (obj != null) Destroy(obj);
                spawned.Remove(removed.Key);
            }
        }
    }

    void SpawnFor(ARTrackedImage image)
    {
        // FIX: renamed from "name" so it doesn't hide MonoBehaviour.name
        string imgName = image.referenceImage.name;   // e.g. "Earth"

        // FIX: don't spawn twice for the same image
        if (spawned.ContainsKey(image.trackableId)) return;

        foreach (ImagePrefab item in imagePrefabs)
        {
            if (item.imageName == imgName)
            {
                // make it a CHILD of the image, so it moves with the card
                GameObject obj = Instantiate(item.prefab, image.transform);
                obj.transform.localPosition = new Vector3(0f, heightAboveImage, 0f);
                spawned[image.trackableId] = obj;

                if (audioSource != null && foundSound != null)
                    audioSource.PlayOneShot(foundSound);
                return;
            }
        }

        Debug.Log("No prefab for image: " + imgName);
    }
}