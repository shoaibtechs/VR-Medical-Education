namespace Packages.Scene
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Class to control eye blinking on a character.
    /// Just attach this script to the character's head where the blink blend shape is located.
    /// </summary>
    public class EyeBlinker : MonoBehaviour
    {
        private SkinnedMeshRenderer eyeRenderer;

        [SerializeField]
        private string blinkBlendShapeName;

        [SerializeField]
        private float minBlinkDuration = 0.2f;

        [SerializeField]
        private float maxBlinkDuration = 0.4f;

        [SerializeField]
        private float minTimeBetweenBlinks = 2.0f;

        [SerializeField]
        private float maxTimeBetweenBlinks = 5.0f;

        [SerializeField]
        private float doubleBlinkChance = 0.2f;

        private bool isBlinking = false;
        private float timeSinceLastBlink = 0.0f;

        private void Start()
        {
            eyeRenderer = GetComponent<SkinnedMeshRenderer>();

            timeSinceLastBlink = Random.Range(minTimeBetweenBlinks, maxTimeBetweenBlinks);

            if (!eyeRenderer)
            {
                Debug.LogError("CharacterEyeBlinker: No SkinnedMeshRenderer found on " + gameObject.name);
                Destroy(this);
            }

            if (string.IsNullOrEmpty(blinkBlendShapeName))
            {
                Debug.LogError("CharacterEyeBlinker: No blink blend shape name set on " + gameObject.name);
                Destroy(this);
            }
        }

        private void Update()
        {
            if (!isBlinking)
            {
                timeSinceLastBlink -= Time.deltaTime;

                if (timeSinceLastBlink <= 0)
                {
                    StartCoroutine(PerformBlink());
                    timeSinceLastBlink = Random.Range(minTimeBetweenBlinks, maxTimeBetweenBlinks);
                }
            }
        }

        private IEnumerator PerformBlink()
        {
            isBlinking = true;

            float blinkSpeed = Random.Range(1.0f / maxBlinkDuration, 1.0f / minBlinkDuration); // Randomized speed
            float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration); // Randomized duration
            float startTime = Time.time;
            float elapsedTime = 0.0f;

            // Eye open
            while (elapsedTime < blinkDuration)
            {
                float blendValue = Mathf.Lerp(0, 100, elapsedTime / blinkDuration);
                eyeRenderer.SetBlendShapeWeight(eyeRenderer.sharedMesh.GetBlendShapeIndex(blinkBlendShapeName), blendValue);

                elapsedTime = (Time.time - startTime) * blinkSpeed;
                yield return null;
            }

            // Eye close
            while (elapsedTime > 0)
            {
                float blendValue = Mathf.Lerp(0, 100, elapsedTime / blinkDuration);
                eyeRenderer.SetBlendShapeWeight(eyeRenderer.sharedMesh.GetBlendShapeIndex(blinkBlendShapeName), blendValue);

                elapsedTime = (blinkDuration - (Time.time - startTime)) * blinkSpeed;
                yield return null;
            }

            eyeRenderer.SetBlendShapeWeight(eyeRenderer.sharedMesh.GetBlendShapeIndex(blinkBlendShapeName), 0);

            // Implement double blinks with a chance
            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f)); // Delay between blinks
                StartCoroutine(PerformBlink());
            }

            isBlinking = false;
        }
    }
}