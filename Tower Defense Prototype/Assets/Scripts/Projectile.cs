using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float damage;
    private Unit target;
    private Vector2 initialStart;
    [SerializeField]
    private float projectileSpeed = 0.01f;
    [SerializeField]
    private DamageType damageType;
    private float lerpProgress = 0f;
    private Vector2 lastPosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (lerpProgress < 0.9)
        { 
            lerpProgress += projectileSpeed;
            //Debug.Log(initialStart);
            //Debug.Log(target);
            Vector2 lerpTarget;
            if (target != null)
            {
                //lerpTarget = target.gameObject.transform.position;
                lerpTarget = target.AimTarget;
                lastPosition = lerpTarget;
            }
            else
            {
                lerpTarget = lastPosition;
            }
            transform.position = Vector2.Lerp(initialStart, lerpTarget, lerpProgress);

            Vector2 rotationDirection = (Vector2)transform.position - lerpTarget;
            float angle = Mathf.Atan2(rotationDirection.y, rotationDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = rotation;
                
        }
        else
        {
            if(target != null)
            {
                ApplyDamage(target);
            }
            Destroy(gameObject);
        }
    }

    public void ApplyDamage(Unit enemy)
    {
        enemy.TakeDamage(damageType, damage);
    }

    public void Initialize(Vector2 initialStart, Unit target)
    {
        this.target = target;
        this.initialStart = initialStart;
    }
}



