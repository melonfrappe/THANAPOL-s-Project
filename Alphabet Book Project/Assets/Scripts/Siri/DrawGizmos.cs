using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siri
{
    class DrawGizmos : MonoBehaviour
    {
        public bool isDeawSelected = false;
        public Color m_color = Color.yellow;
        public float radius = 1;
        private void OnDrawGizmos()
        {
            if(!isDeawSelected)
                Draw();
        }

        private void OnDrawGizmosSelected()
        {
             if(isDeawSelected)
            Draw();
        }

        private void Draw()
        {
            Gizmos.color = m_color;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
