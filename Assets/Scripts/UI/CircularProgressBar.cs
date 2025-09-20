using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CircularProgressBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private float hideThreshold = 0.9f;
    
        private void Start()
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
        }
    
        public void SetFill(float amount)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
            fillImage.enabled = amount < hideThreshold;
        }
    }
}