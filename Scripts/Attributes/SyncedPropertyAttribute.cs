using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    /// <summary>
    /// Route an inspector's field through a property
    /// </summary>
    public class SyncedPropertyAttribute : PropertyAttribute
    {
        public string propertyInfoName;

        /// <summary>
        /// Synced property's Type must match field, and must contain getter and setter
        /// </summary>
        /// <param name="propertyInfoName">tip: use nameof(property)</param>
        public SyncedPropertyAttribute(string propertyInfoName)
        {
            this.propertyInfoName = propertyInfoName;
            this.order = -1;
        }
    }
}