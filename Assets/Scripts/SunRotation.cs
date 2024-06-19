using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axegen
{
    public class SunRotation : MonoBehaviour
    {
        [SerializeField] Transform sunTransform;
        [SerializeField] Transform cameraLight;
        
        [Range(0, 24)]
        [SerializeField] float timeOfDay = 12f;

        [SerializeField] float dayScale = 1f;
        [SerializeField] float nightScale = 1f;

        public static float timeOfDayStatic = 12f;

        private bool isLightOpen = false;

        private void OnValidate()
        {
            SetTime(timeOfDay);
        }

        private void Awake()
        {
            SetTime(timeOfDay);
        }

        private void Update()
        {
            // If day
            if (timeOfDay >= 5 && timeOfDay < 19)
            {
                timeOfDay += Time.deltaTime / 60f * dayScale;
            }
            else
            {
                timeOfDay += Time.deltaTime / 60f * nightScale;
            }

            if(Input.GetKeyDown(KeyCode.L))
            {
                isLightOpen = !isLightOpen;
                cameraLight.gameObject.SetActive(isLightOpen);
            }


            SetTime(timeOfDay);
        }

        public void SetTime(float time)
        {
            timeOfDay = time;
            if (timeOfDay >= 23.9f)
            {
                timeOfDay = 0.1f;
            }
            timeOfDayStatic = timeOfDay;
            float rotation = timeOfDay / 24f * 360f - 90f; // Because the sun is at 6:00 at 0 degrees
            sunTransform.localRotation = Quaternion.Euler(rotation, 0, 0);
        }
    }
}
