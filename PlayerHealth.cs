using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

    public float MaxPlayerHP = 100.0f;
    public float MaxPlayerMP = 100.0f;
    public Slider HPSlider;
    public Slider MPSlider;
    public Image HPSliderFillimage;
    public float MPRecoverNum = 10.0f;

    private float currentHP;
    private float currentMP;
    private Animator playerAnimator;

    private float MPRecoverTimer = 0.0f;

    private float timer=0.0f;

    public void TakePlayerDamage(float damage)
    {
        currentHP -= damage;
        //HPSlider.value = currentHP;
        if (currentHP < 0)
            currentHP = 0;
        HPSliderFillimage.fillAmount = currentHP / 100;
        playerAnimator.SetBool("isHurt",true);
        playerAnimator.SetBool("isHurt", false);
        //Debug.Log(currentHP+"====="+ HPSliderFillimage.fillAmount);
    }

    private void MpRecoverInterval()
    {
        currentMP += MPRecoverNum/60;
        if (currentMP > 100.0f)
            currentMP = 100.0f;
        MPSlider.value = currentMP;
    }


    public void TakePlayerMP(float MPvalue)
    {
        currentMP -= MPvalue;
        if (currentMP < 0)
            currentMP = 0;
        MPSlider.value = currentMP;
    }

	void Start () {
        //HPSlider.maxValue = MaxPlayerHP;
        //HPSlider.minValue = 0.0f;
        //HPSlider.value = currentHP = 100.0f;
        currentHP = 100.0f;
        HPSliderFillimage.fillAmount = MaxPlayerHP/100;

        MPSlider.maxValue = MaxPlayerMP;
        MPSlider.minValue = 0.0f;
        MPSlider.value = currentMP = 50.0f;
        playerAnimator = GetComponent<Animator>();

        
    }
	
    void playerDead()
    {
        if (currentHP <= 0)
        {
            playerAnimator.SetBool("isAlive", false);
            GetComponent<PlayerMove>().enabled = false;
        }
    }

	void Update () {
        playerDead();
        if (MPRecoverTimer > 1.0f/60)
        {
            MpRecoverInterval();
            MPRecoverTimer = 0.0f;
        }
        MPRecoverTimer += Time.deltaTime;

        if (timer > 3.0f)
        {
            TakePlayerDamage(20.0f);
            timer = 0.0f;
        }
        timer += Time.deltaTime;

    }
}
