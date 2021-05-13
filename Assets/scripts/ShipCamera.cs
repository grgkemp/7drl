using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ShipCamera : MonoBehaviour
{
    public Camera shipCamera;
    public float m_SmoothTime;
    private Camera oldCamera;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    public float targetSize;
    public ShipAnimator shipAnimator;
    // Use this for initialization
    void Start()
    {
        oldCamera = Camera.main;
        targetPosition = shipCamera.transform.position;
        targetRotation = shipCamera.transform.rotation;

        transform.position = oldCamera.transform.position;
        shipCamera.orthographicSize = oldCamera.orthographicSize;

        StartCoroutine(CameraPan(oldCamera, shipCamera));
        //Camera.main.enabled = false;
    }

    IEnumerator CameraPan(Camera SceneCamera, Camera SecondCamera)
    {
        Vector3 refVelocity = Vector3.zero;
        float journey = 0f;
        Quaternion originRotation = SceneCamera.transform.rotation;
        float originSize = SceneCamera.orthographicSize;
        bool animating = false;

        while (Vector3.Distance(SecondCamera.transform.position, targetPosition) > 0.02f)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / m_SmoothTime);

            //Position & rotation
            SecondCamera.transform.position = Vector3.SmoothDamp(SecondCamera.transform.position, targetPosition, ref refVelocity, m_SmoothTime);
            SecondCamera.transform.rotation = Quaternion.Slerp(originRotation, targetRotation, percent);

            oldCamera.transform.rotation = SecondCamera.transform.rotation;

            //Camera size
            SecondCamera.orthographicSize = Mathf.Lerp(originSize, targetSize, percent);

            if (percent > 0.9f & !animating)
            {
                shipAnimator.AnimateTransition();
                animating = true;
            }

            yield return null;
        }

        SceneCamera.enabled = false;
        //SecondCamera.enabled = true;
        //SecondCamera.orthographicSize = 5;
    }
}
