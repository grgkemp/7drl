using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    IEnumerator AnimateBounce(Vector3 origin, Vector3 source, float strength, float duration)
    {
        Vector3 target = Vector3.MoveTowards(origin, source, -strength);
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);

            if (journey < duration / 2) // react
            {
                transform.position = Vector3.Lerp(origin, target, percent);
            }
            else // recover
            {
                transform.position = Vector3.Lerp(target, origin, percent);
            }

            yield return null;
        }
    }

    public void DamageResponse(Vector3 source, float strength, float duration)
    {
        StartCoroutine(AnimateBounce(transform.position, source, strength, duration));
    }
}
