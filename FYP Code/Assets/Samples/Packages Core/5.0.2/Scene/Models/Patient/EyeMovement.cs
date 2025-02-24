namespace Packages.Scene
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Rotates the eyes of the character randomly.
    /// </summary>
    public class EyeMovement : MonoBehaviour
    {
        [SerializeField]
        private Transform leftEye;
        [SerializeField]
        private Transform rightEye;

        [SerializeField]
        private float maxXRotation = 15.0f; // Maximum rotation angle around X-axis in degrees
        [SerializeField]
        private float maxYRotation = 15.0f; // Maximum rotation angle around Y-axis in degrees
        [SerializeField]
        private float rotationSpeed = 1.0f; // Speed of eye movement

        private Vector3 initialRotation;

        private void Start()
        {
            // Store the initial rotation of the eyes
            initialRotation = leftEye.localRotation.eulerAngles;

            // Start the coroutine for random eye movements
            StartCoroutine(RandomEyeMovement());
        }

        private IEnumerator RandomEyeMovement()
        {
            while (true)
            {
                // Generate random rotation angles around X and Y axes
                float randomXRotation = UnityEngine.Random.Range(-maxXRotation, maxXRotation);
                float randomYRotation = UnityEngine.Random.Range(-maxYRotation, maxYRotation);

                // Calculate the target rotation
                Quaternion targetRotation = Quaternion.Euler(initialRotation.x + randomXRotation, initialRotation.y + randomYRotation, initialRotation.z);

                // Interpolate rotation smoothly
                float elapsedTime = 0;
                while (elapsedTime < rotationSpeed)
                {
                    leftEye.localRotation = Quaternion.Lerp(leftEye.localRotation, targetRotation, elapsedTime / rotationSpeed);
                    rightEye.localRotation = Quaternion.Lerp(rightEye.localRotation, targetRotation, elapsedTime / rotationSpeed);

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Wait for a random duration before the next movement
                float randomWaitTime = UnityEngine.Random.Range(1.0f, 5.0f);
                yield return new WaitForSeconds(randomWaitTime);
            }
        }
    }
}