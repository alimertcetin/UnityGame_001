using UnityEngine;

namespace PlayerSystems
{
    public abstract class BehaviourBase : MonoBehaviour
    {
        int[] states;
        
        protected virtual void Awake()
        {
            states = GetStates();
            MonoBehaviourManager.Add(this, states);
        }

        public virtual void Tick()
        {
            
        }

        protected virtual void OnDestroy()
        {
            MonoBehaviourManager.Remove(this, states);
        }

        protected abstract int[] GetStates();
    }
}