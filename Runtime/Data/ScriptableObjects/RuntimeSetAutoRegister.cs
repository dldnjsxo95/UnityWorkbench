using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Auto-registers GameObject to a RuntimeSet on enable/disable.
    /// Attach to objects you want to track.
    /// </summary>
    public class RuntimeSetAutoRegister : MonoBehaviour
    {
        [SerializeField] private GameObjectSet _targetSet;

        private void OnEnable()
        {
            if (_targetSet != null)
            {
                _targetSet.Add(gameObject);
            }
        }

        private void OnDisable()
        {
            if (_targetSet != null)
            {
                _targetSet.Remove(gameObject);
            }
        }
    }

    /// <summary>
    /// Auto-registers Transform to a RuntimeSet.
    /// </summary>
    public class TransformSetAutoRegister : MonoBehaviour
    {
        [SerializeField] private TransformSet _targetSet;

        private void OnEnable()
        {
            if (_targetSet != null)
            {
                _targetSet.Add(transform);
            }
        }

        private void OnDisable()
        {
            if (_targetSet != null)
            {
                _targetSet.Remove(transform);
            }
        }
    }
}
