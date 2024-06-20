using System.Collections;
using TMPro;
using UnityEngine;

namespace Axegen
{
    public class DebugGUI : MonoBehaviour
    {
        [SerializeField] Canvas debugCanvas;
        [SerializeField] TextMeshProUGUI avgFPSText;
        [SerializeField] TextMeshProUGUI coordsText;
        [SerializeField] TextMeshProUGUI humidityText;
        [SerializeField] TextMeshProUGUI temperatureText;
        [SerializeField] TextMeshProUGUI currentBiomeText;
        [SerializeField] TextMeshProUGUI queueCountText;

        [SerializeField] bool isOpen;

        private void Start()
        {
            debugCanvas.enabled = isOpen;
            StartCoroutine(UpdateFPS());
        }
        IEnumerator UpdateFPS()
        {
            var wait = new WaitForSeconds(.5f);
            while (true)
            {
                float avgFPS = 1.0f / Time.deltaTime;
                UpdateFPS(avgFPS);

                queueCountText.text = "In Queue: " + MapGenerator.instance.meshDataThreadInfoQueue.Count;

                yield return wait;
            }
        }
        public void UpdateHumidity(float humidity)
        {
            humidityText.text = "H: " + humidity.ToString("F3");
        }

        public void UpdateTemperature(float temperature)
        {
            temperatureText.text = "T: " + temperature.ToString("F3");
        }
        public void UpdateCoords(Vector2Int coords)
        {
            coordsText.text = "C: " + coords.x + ", " + coords.y;
        }
        public void UpdateBiome(BiomeData biome)
        {
            currentBiomeText.text = "Biome: " + biome.name;
        }
        public void UpdateFPS(float avgFPS)
        {
            avgFPSText.text = "FPS: " + avgFPS.ToString("F0");
        }

        private void Update()
        {
            // On ESC press, toggle cursor lock
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = Cursor.lockState == CursorLockMode.None;
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                isOpen = !isOpen;
                debugCanvas.enabled = isOpen;
                Debug.Log("Debug GUI: " + (isOpen ? "Enabled" : "Disabled"));
            }

            if (isOpen) UpdateGUI();
        }

        void UpdateGUI()
        {
            var playerPos = EndlessTerrain.instance.viewer.position;
            Vector2Int coord = MapGenerator.instance.GetCoord(playerPos);
            float humidity = BiomeValuesGenerator.instance.GetHumidity(playerPos.x, playerPos.z);
            float temperature = BiomeValuesGenerator.instance.GetTemperature(playerPos.x, playerPos.z);
            BiomeData biome = MapGenerator.instance.biomeCollection.ChooseBiome((int)playerPos.x, (int)playerPos.z, Vector2Int.zero);
            
            UpdateCoords(coord);
            UpdateHumidity(humidity);
            UpdateTemperature(temperature);
            UpdateBiome(biome);
        }
    }
}
