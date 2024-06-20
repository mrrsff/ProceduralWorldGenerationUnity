using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Axegen
{
    public class GUISettingsSlider : MonoBehaviour
    {
        public Slider slider;
        public TextMeshProUGUI valueText;
        public string settingName;
        public int minValue;
        public int maxValue;
        public int defaultValue;
        public int currentValue;

        public UnityEvent<int> onValueChanged;

        private void Start()
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            currentValue = defaultValue;
            valueText.text = $"{settingName}: {currentValue}";
        }

        public void OnSliderValueChanged()
        {
            currentValue = (int)slider.value;
            valueText.text = $"{settingName}: {currentValue}";

            onValueChanged.Invoke(currentValue);
        }
    }
}
