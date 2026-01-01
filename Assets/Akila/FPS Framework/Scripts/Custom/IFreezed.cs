using UnityEngine;

namespace Akila.FPSFramework
{
    public interface IFreezed
    {
        public Vector3 Freeze();
        public void UnFreeze(Vector3 velocity);
    }
}