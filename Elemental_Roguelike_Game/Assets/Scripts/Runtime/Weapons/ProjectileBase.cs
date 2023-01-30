using Data;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Weapons
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileBase: MonoBehaviour
    {
        #region Serialized Fields

        [Header("Info")]
        [SerializeField] private ProjectileInfo m_info; 

        #endregion

        #region Private Fields

        private Vector3 m_velocity;

        private Rigidbody m_rigidbody;

        private float m_life_time;

        #endregion
       
        
        #region Accessors

        public float moveSpeed => m_info.projectileSpeed;
        
        public float damage => m_info.projectileDamage;

        public ProjectileInfo projectileRef => m_info;

        public Rigidbody rb => CommonUtils.GetRequiredComponent(ref m_rigidbody, () =>
        {
            var r = GetComponent<Rigidbody>();
            return r;
        });

        #endregion

        #region Unity Events

        void Update()
        {
            m_velocity = transform.forward * (moveSpeed * Time.deltaTime);
            
            rb.MovePosition(rb.position + m_velocity);

            m_life_time -= Time.deltaTime;
            if (m_life_time <= 0)
            {
                DeleteObject();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Wall"))
            {
                DeleteObject();
            }
        }

        #endregion


        #region Class Implementation

        public void Initialize()
        {
            m_life_time = m_info.projectileLifetime;
        }

        public void DeleteObject()
        {
            ProjectileUtils.ReturnToPool(this);
        }

        #endregion

        
    }
}