/*
 * BaseGLStateMechine
 * 描述：游戏逻辑状态机基类
 * 时间：2016.9.26
 * 
 */

using UnityEngine;
using System.Collections;
using System;
public class BaseGLStateMachine{

    protected GameManager gm_instance;
    const float m_photonCircleTime = 4294967.295f;	//Photon服务器循环时间

    private double m_endTime;
    protected double m_countDown;
    private bool m_countDownFlag;
    //GameManager实例
    public GameManager GMInstance
    {
        get{
            return gm_instance; }
    }
    //有参构造函数
    public BaseGLStateMachine(GameManager gm)
    {
        gm_instance = gm;
    }
    public BaseGLStateMachine() { }
    //为无参构造函数准备的
    public void setGameManager(GameManager gm)
    {
        gm_instance = gm;
    }
    //fzy modify:解决时间不同步的问题
    public void startCountDown(double endTime)
    {
        m_endTime = endTime;
        m_countDown = endTime - PhotonNetwork.time; 
        m_countDownFlag = true;
    }
    //停止倒计时
    public void stopCountDown()
    {
        m_countDownFlag = false;
        m_endTime = -1;
        m_countDown = -1;
    }
    //暂停倒计时
    public void pauseCountDown()
    {
        m_countDownFlag = false;
    }
    //恢复倒计时 与pauseCountDown配对使用
    public void resumeCountDown()
    {
        m_endTime = PhotonNetwork.time + m_countDown;//恢复后 endtime里通过剩余时间与当前时间的和获取结束时间
        m_countDownFlag = true;
    }
    //状态机数据初始化 注意无论什么状态机都会在游戏开始的时候完成初始化
    virtual public void Init()
    {
    }
    //游戏状态机发生切换时 在上一个状态机的Exit函数执行后执行
    virtual public void Enter()
    {
    }
    //当前状态机更新 由对应继承自monoBehavior类的update函数调用 如果子类想使用倒计时，需要调用基类update
    virtual public void Update()
    {
        if (m_countDownFlag)
        {
            m_countDown = m_endTime - PhotonNetwork.time;
            m_countDown = (m_countDown >= m_photonCircleTime) ? m_countDown - m_photonCircleTime : m_countDown;
            if (m_countDown <= 0)
            {
                stopCountDown();
                onCountDownFinish();
            }
        }
    } 
    //游戏状态机发生切换时 在下一个状态机的Enter函数之前执行
    virtual public void Exit()
    {
    }
    //检测结束条件 为true则需要进行状态机替换 每帧会调用 注意效率
    virtual public bool checkEndCondition()
    {
        return false;
    }
    //当倒计时结束时触发该函数
    virtual public void onCountDownFinish()
    {
    }

}

