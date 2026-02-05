using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Integer runtime variable.
    /// </summary>
    [CreateAssetMenu(fileName = "IntVariable", menuName = "LWT/Data/Variables/Int")]
    public class IntVariable : RuntimeValue<int>
    {
        public void Add(int amount) => Value += amount;
        public void Subtract(int amount) => Value -= amount;
        public void Multiply(int multiplier) => Value *= multiplier;
        public void Clamp(int min, int max) => Value = Mathf.Clamp(Value, min, max);
    }

    /// <summary>
    /// Float runtime variable.
    /// </summary>
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "LWT/Data/Variables/Float")]
    public class FloatVariable : RuntimeValue<float>
    {
        public void Add(float amount) => Value += amount;
        public void Subtract(float amount) => Value -= amount;
        public void Multiply(float multiplier) => Value *= multiplier;
        public void Clamp(float min, float max) => Value = Mathf.Clamp(Value, min, max);
        public void Lerp(float target, float t) => Value = Mathf.Lerp(Value, target, t);
    }

    /// <summary>
    /// Boolean runtime variable.
    /// </summary>
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "LWT/Data/Variables/Bool")]
    public class BoolVariable : RuntimeValue<bool>
    {
        public void Toggle() => Value = !Value;
        public void SetTrue() => Value = true;
        public void SetFalse() => Value = false;
    }

    /// <summary>
    /// String runtime variable.
    /// </summary>
    [CreateAssetMenu(fileName = "StringVariable", menuName = "LWT/Data/Variables/String")]
    public class StringVariable : RuntimeValue<string>
    {
        public void Append(string text) => Value += text;
        public void Clear() => Value = string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(Value);
    }

    /// <summary>
    /// Vector3 runtime variable.
    /// </summary>
    [CreateAssetMenu(fileName = "Vector3Variable", menuName = "LWT/Data/Variables/Vector3")]
    public class Vector3Variable : RuntimeValue<Vector3>
    {
        public void Add(Vector3 amount) => Value += amount;
        public void Normalize() => Value = Value.normalized;
        public float Magnitude => Value.magnitude;
    }

    /// <summary>
    /// Color runtime variable.
    /// </summary>
    [CreateAssetMenu(fileName = "ColorVariable", menuName = "LWT/Data/Variables/Color")]
    public class ColorVariable : RuntimeValue<Color>
    {
        public void SetAlpha(float alpha)
        {
            var c = Value;
            c.a = alpha;
            Value = c;
        }

        public void Lerp(Color target, float t) => Value = Color.Lerp(Value, target, t);
    }

    /// <summary>
    /// GameObject reference (useful for player, camera, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "GameObjectVariable", menuName = "LWT/Data/Variables/GameObject")]
    public class GameObjectVariable : RuntimeValue<GameObject>
    {
        public Transform Transform => Value != null ? Value.transform : null;
        public bool IsValid => Value != null;

        public T GetComponent<T>() where T : Component
        {
            return Value != null ? Value.GetComponent<T>() : null;
        }
    }

    /// <summary>
    /// Transform reference variable.
    /// </summary>
    [CreateAssetMenu(fileName = "TransformVariable", menuName = "LWT/Data/Variables/Transform")]
    public class TransformVariable : RuntimeValue<Transform>
    {
        public Vector3 Position => Value != null ? Value.position : Vector3.zero;
        public Quaternion Rotation => Value != null ? Value.rotation : Quaternion.identity;
        public bool IsValid => Value != null;
    }
}
