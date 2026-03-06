#region Using statements

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Bitgem.VFX.StylisedWater
{
    public class WateverVolumeFloater : MonoBehaviour
    {
        #region Public fields

        public WaterVolumeHelper WaterVolumeHelper = null;

        [Header("Float Settings")]
        public float heightOffset = 0.5f;

        #endregion

        #region MonoBehaviour events

        void Update()
        {
            var instance = WaterVolumeHelper ? WaterVolumeHelper : WaterVolumeHelper.Instance;
            if (!instance)
            {
                return;
            }

            float targetHeight = transform.position.y; 

            try
            {
                targetHeight = instance.GetHeight(transform.position) ?? transform.position.y;
            }
            catch (System.Exception)
            {
                return;
            }

            transform.position = new Vector3(transform.position.x, targetHeight + heightOffset, transform.position.z);
        }

        #endregion
    }
}