using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace GSUnity.MonoBehaviours
{
    public class FpsDisplay : MonoBehaviour
    {
        private const int TargetFPS = 244;
        private const float UpdateFrequency = 0.25f;

        private int _intervalFrameCount;
        private float _lastUpdated;

        [SerializeField] private TextMeshProUGUI _tmpFps;

        public void Update()
        {
            _intervalFrameCount++;
            if (_lastUpdated < UpdateFrequency)
            {
                _lastUpdated += Time.deltaTime;
                return;
            }

            var fps = math.round(_intervalFrameCount / UpdateFrequency);

            if (fps >= TargetFPS)
                _tmpFps.text = $"<color #0CA900>{fps}</color> FPS";
            else if (fps >= TargetFPS / 2f)
                _tmpFps.text = $"<color #6CFF62>{fps}</color> FPS";
            else if (fps >= TargetFPS / 4f)
                _tmpFps.text = $"<color #F5F100>{fps}</color> FPS";
            else if (fps >= TargetFPS / 8f)
                _tmpFps.text = $"<color #F5AA00>{fps}</color> FPS";
            else
                _tmpFps.text = $"<color #FF2F00>{fps}</color> FPS";

            _lastUpdated = 0;
            _intervalFrameCount = 0;
        }
    }
}