using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class XKXuNiPlayerCtrl : MonoBehaviour
{
    [HideInInspector]
    public float MoveSpeed = 0f;
    AiMark MarkCom;
    static XKXuNiPlayerCtrl Instance;
    public static XKXuNiPlayerCtrl GetInstance()
    {
        return Instance;
    }

    void Start()
    {
        Instance = this;
    }

    void FixedUpdate()
    {
        if (MoveSpeed <= 0)
        {
            return;
        }
        float disMove = MoveSpeed * Time.deltaTime;
        Vector3 dirMove = Vector3.Normalize(MarkCom.transform.position - transform.position);
        //transform.position += new Vector3(0f, 0f, disMove);
        transform.position += dirMove * disMove;

        if (Mathf.Abs(MarkCom.transform.position.z - transform.position.z) <= disMove * 1.25f)
        {
            //虚拟主角到达路径点.
            if (MarkCom.mNextMark != null)
            {
                MarkCom = MarkCom.mNextMark.GetComponent<AiMark>();
                MoveSpeed = MarkCom.MvSpeed;
            }
            else
            {
                //没有下一个路径点了.
                MoveSpeed = 0f;
            }
        }
    }

    public void InitXuNiPlayerInfo(AiMark mark)
    {
        MarkCom = mark;
        MoveSpeed = mark.MvSpeed;
    }
}
