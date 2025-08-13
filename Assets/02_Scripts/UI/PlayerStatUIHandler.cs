using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _02_Scripts.UI
{
    public class PlayerStatUIHandler : MonoBehaviour
    {
        //보스는 BgSlider 넣을예정
        [SerializeField] public Slider HPSlider;
        [SerializeField] Slider MPSlider;
        [SerializeField] TMP_Text HPSText;
        [SerializeField] TMP_Text MPSText;
        [SerializeField] Image Alert;
        
        //보스만 사용할 예정
        [HideInInspector] public Slider BossSlider;

        float targetHP;
        float targetMP;
        float lerpSpeed = 1f;

        public void ChangeBossHP(float value, float MaxHP)
        {
            BossSlider.value = value / MaxHP;
            targetHP = value / MaxHP;
        }

        public void ChangeHP(float value, float MaxHP)
        {
            targetHP = value/MaxHP;
            HPSText.text = Mathf.RoundToInt(value)+"/"+Mathf.RoundToInt(MaxHP);
        }

        public void ChangeMP(float value, float MaxMP)
        {
            targetMP = value/MaxMP;
            MPSText.text = Mathf.RoundToInt(value)+"/"+Mathf.RoundToInt(MaxMP);
        }

        public void OnOffAlert(bool bol)
        {
            if(Alert != null)
                Alert.gameObject.SetActive(bol);
        }
        
        void FixedUpdate()
        {
            if (HPSlider != null)
            {
                if (Mathf.Abs(HPSlider.value - targetHP) > 0.001f)
                {
                    HPSlider.value = Mathf.Lerp(HPSlider.value, targetHP, Time.deltaTime * lerpSpeed);
                }
            }

            if (MPSlider != null)
            {
                if (Mathf.Abs(MPSlider.value - targetMP) > 0.001f)
                {
                    MPSlider.value = Mathf.Lerp(MPSlider.value, targetMP, Time.deltaTime * lerpSpeed);
                }
            }
        }
    }
}