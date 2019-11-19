using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlockManagement_GPULogic;

public class FlockBehaviourTest
{
    // Update is called once per frame
    public static void CPUTest()
    {
        //Simulate parralelisation
        for (int id = 0; id < boidsBuffer.Length; id++)
        {
            GPUBoid_Compute boid = boidsBuffer[id];

            //Average position
            Vector3 rule_cohesion = boid.position;
            //Average forward
            Vector3 rule_alignement = Vector3.zero;
            Vector3 rule_separation = Vector3.zero;

            //Normalize fucks up if try to normalize (0,0,0)
            Vector3 rule_target = (FlockManagement_GPULogic.Instance.target.position - boid.position).normalized;
            int neighboursCount = 1;

            for (int i = 0; i < FlockManagement_GPULogic.Instance.boidsCount; i++)
            {
                if (i != id)
                {
                    GPUBoid_Compute otherBoid = boidsBuffer[i];

                    //Distance
                    Vector3 diff = boid.position - otherBoid.position;
                    float diffLength = diff.magnitude;
                    int isNeighbour = Step(diffLength, FlockManagement_GPULogic.Instance.maxNeighbourDistance);
                    neighboursCount += isNeighbour;

                    //Separation
                    rule_separation += diff / diffLength * Inv(diffLength, FlockManagement_GPULogic.Instance.separationDistance) * isNeighbour;

                    //Cohesion
                    rule_cohesion += otherBoid.position * isNeighbour;

                    //Alignement
                    rule_alignement += otherBoid.direction * isNeighbour;
                }
            }

            rule_cohesion = (rule_cohesion / neighboursCount - boid.position * 0.99f).normalized * Step(2,neighboursCount);
            rule_alignement = rule_alignement/neighboursCount;

            Vector3 movement = rule_target * FlockManagement_GPULogic.Instance.weightTarget + rule_cohesion * FlockManagement_GPULogic.Instance.weightCohesion + rule_alignement * FlockManagement_GPULogic.Instance.weightAlignement + rule_separation * FlockManagement_GPULogic.Instance.weightSeparation;
            //Debug.DrawRay(boid.position, 3 *rule_target * FlockManagement.Instance.weightTarget, Color.black);
            //Debug.DrawRay(boid.position, 3*rule_cohesion * FlockManagement.Instance.weightCohesion, Color.blue);
            //Debug.DrawRay(boid.position, 3*rule_alignement * FlockManagement.Instance.weightAlignement, Color.yellow);
            //Debug.DrawRay(boid.position, 3*rule_separation * FlockManagement.Instance.weightSeparation, Color.red);

            //Direction
            //Separate the axis and give a little boost in x to avoid all parameters going the same speed
            //thus keeping the vector pretty mush the same after normalization
            float tx = Mathf.Exp(-FlockManagement_GPULogic.Instance.rotationSpeed * 1.1f * Time.deltaTime);
            float tyz = Mathf.Exp(-FlockManagement_GPULogic.Instance.rotationSpeed * Time.deltaTime);
            boid.direction.x = Mathf.Lerp(movement.x,boid.direction.x,tx);
            boid.direction.y = Mathf.Lerp(movement.y,boid.direction.y,tyz);
            boid.direction.z = Mathf.Lerp(movement.z,boid.direction.z,tyz);
            boid.direction = boid.direction.normalized;

            //Position
            boid.position += boid.direction * FlockManagement_GPULogic.Instance.moveSpeed * Time.deltaTime;

            boidsBuffer[id] = boid;
        }

        float Inv(float x, float s)
        {
            //Avoid dividing by zero using espilon
            float value = x / s + 0.00001f;

            return 1 / (value * value);
        }

        int Step(float y, float x)
        {
            if (x>=y)
                return 1;
            else
                return 0;
        }
    }
}
