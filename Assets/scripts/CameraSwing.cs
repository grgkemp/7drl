using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwing : MonoBehaviour
{

    public float rotationSpeed = 100.0f;
    public float volume = 0.5f;
    public AudioListener audioListener;

    // Update is called once per frame
    private void Start()
    {
        audioListener = GetComponentInChildren<AudioListener>();
        AdjustVolume(volume);
    }

    void Update()
    {
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        rotation *= Time.deltaTime;

        // Rotate around our y-axis
        transform.Rotate(0, rotation, 0);
    }

    public void AdjustVolume(float newVolume)
    {
        AudioListener.volume = newVolume;
    }

}
