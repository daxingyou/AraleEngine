using UnityEngine;
using System.Collections;

public class PhysicMove : Move
{
    float   mDistance;
    Rigidbody2D bd;
    Vector3 mPos;
    protected override void start(Unit unit)
    {
        mSpeed    = table.speed;
        mDistance = table.life;
        unit.dir = vTarget.normalized;
        bd = unit.GetComponent<Rigidbody2D>();
        bd.isKinematic = false;
        Vector3 speed = unit.dir * mSpeed;
        bd.velocity = new Vector2(speed.x, speed.y);
        mPos = unit.pos;
    }

    protected override void update(Unit unit)
    {
        if (mDistance <= 0)
        {//达到飞行距离
            stop(unit,true);
        }
        else
        {
            Vector3 d = unit.pos - mPos;
            mDistance -= d.magnitude;
            unit.dir = d.normalized;
            mPos = unit.pos;
        }
    }
}
