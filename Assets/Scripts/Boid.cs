using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Rigidbody rigid;
    private Neighborhood neighborhood;
    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
    private void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigid.velocity = vel;
        LookAhead();
        CreateRandomColor();    
    }
    private void FixedUpdate()
    {
        Vector3 vel = rigid.velocity;
        Spawner spn = Spawner.S;
        Vector3 velAvoid = Vector3.zero;
        Vector3 toClosePos = neighborhood.avgClosePos;
        if (toClosePos != Vector3.zero)
        {
            velAvoid = pos - toClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }
        Vector3 velAlign = neighborhood.avgVel;
        if (velAlign != Vector3.zero)
        {
            velAlign.Normalize();
            velAlign *= spn.velocity;
        }
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }
        Vector3 delta = Attractor.POS - pos;
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 velAttract = delta.normalized * spn.velocity;

        float allVelocity = Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid * allVelocity);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * allVelocity);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.flockCentring * allVelocity);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * allVelocity);
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * allVelocity);
                }
            }
        }
        vel = vel.normalized * spn.velocity;
        rigid.velocity = vel;
        LookAhead();
    }

    void LookAhead()
    {
        transform.LookAt(pos + rigid.velocity);
    }
    void CreateRandomColor()
    {
        Color rColor = Color.black;
        while (rColor.r + rColor.g + rColor.b < 1.0f)
        {
            rColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = rColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", rColor);
    }
}
