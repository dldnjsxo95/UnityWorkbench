using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Reference that can be either a constant value or a RuntimeValue.
    /// Allows flexibility between hardcoded and data-driven values.
    /// </summary>
    [Serializable]
    public class IntReference
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private int _constantValue;
        [SerializeField] private IntVariable _variable;

        public IntReference() { }
        public IntReference(int value)
        {
            _useConstant = true;
            _constantValue = value;
        }

        public int Value
        {
            get => _useConstant ? _constantValue : (_variable != null ? _variable.Value : 0);
            set
            {
                if (_useConstant)
                    _constantValue = value;
                else if (_variable != null)
                    _variable.Value = value;
            }
        }

        public static implicit operator int(IntReference reference) => reference.Value;
    }

    [Serializable]
    public class FloatReference
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private float _constantValue;
        [SerializeField] private FloatVariable _variable;

        public FloatReference() { }
        public FloatReference(float value)
        {
            _useConstant = true;
            _constantValue = value;
        }

        public float Value
        {
            get => _useConstant ? _constantValue : (_variable != null ? _variable.Value : 0f);
            set
            {
                if (_useConstant)
                    _constantValue = value;
                else if (_variable != null)
                    _variable.Value = value;
            }
        }

        public static implicit operator float(FloatReference reference) => reference.Value;
    }

    [Serializable]
    public class BoolReference
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private bool _constantValue;
        [SerializeField] private BoolVariable _variable;

        public BoolReference() { }
        public BoolReference(bool value)
        {
            _useConstant = true;
            _constantValue = value;
        }

        public bool Value
        {
            get => _useConstant ? _constantValue : (_variable != null ? _variable.Value : false);
            set
            {
                if (_useConstant)
                    _constantValue = value;
                else if (_variable != null)
                    _variable.Value = value;
            }
        }

        public static implicit operator bool(BoolReference reference) => reference.Value;
    }

    [Serializable]
    public class StringReference
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private string _constantValue;
        [SerializeField] private StringVariable _variable;

        public StringReference() { }
        public StringReference(string value)
        {
            _useConstant = true;
            _constantValue = value;
        }

        public string Value
        {
            get => _useConstant ? _constantValue : (_variable != null ? _variable.Value : string.Empty);
            set
            {
                if (_useConstant)
                    _constantValue = value;
                else if (_variable != null)
                    _variable.Value = value;
            }
        }

        public static implicit operator string(StringReference reference) => reference.Value;
    }

    [Serializable]
    public class Vector3Reference
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private Vector3 _constantValue;
        [SerializeField] private Vector3Variable _variable;

        public Vector3Reference() { }
        public Vector3Reference(Vector3 value)
        {
            _useConstant = true;
            _constantValue = value;
        }

        public Vector3 Value
        {
            get => _useConstant ? _constantValue : (_variable != null ? _variable.Value : Vector3.zero);
            set
            {
                if (_useConstant)
                    _constantValue = value;
                else if (_variable != null)
                    _variable.Value = value;
            }
        }

        public static implicit operator Vector3(Vector3Reference reference) => reference.Value;
    }
}
