using System;
using UnityEngine;

namespace Neeto
{
    public interface IIntComparison
    {
        public bool Evaluate(int a, int b);
    }
    public interface IFloatComparison
    {
        public bool Evaluate(float a, float b);
    }
    [Serializable]
    public class FloatComparison
    {
        [SerializeReference, Polymorphic]
        public IFloatComparison comparison = new Equal();

        public float rightHandSide;
        public FloatComparison() { }
        public FloatComparison(float rightHandSide) => this.rightHandSide = rightHandSide;
        public bool Evaluate(float leftHandSide) => comparison.Evaluate(leftHandSide, rightHandSide);
    }

    [Serializable]
    public struct Less : IIntComparison, IFloatComparison
    {
        public bool Evaluate(int a, int b) => a < b;
        public bool Evaluate(float a, float b) => a < b;
    }
    [Serializable]
    public struct LessOrEqual : IIntComparison, IFloatComparison
    {
        public bool Evaluate(int a, int b) => a <= b;
        public bool Evaluate(float a, float b) => a <= b;
    }
    [Serializable]
    public struct Equal : IIntComparison, IFloatComparison
    {
        public bool Evaluate(int a, int b) => a == b;
        public bool Evaluate(float a, float b) => a.RoughlyEquals(b);
    }
    [Serializable]
    public struct NotEqual : IIntComparison, IFloatComparison
    {
        public bool Evaluate(int a, int b) => a != b;
        public bool Evaluate(float a, float b) => !a.RoughlyEquals(b);
    }
    [Serializable]
    public struct GreaterOrEqual : IIntComparison, IFloatComparison
    {
        public bool Evaluate(int a, int b) => a >= b;
        public bool Evaluate(float a, float b) => a >= b;
    }
    [Serializable]
    public struct Greater : IIntComparison, IFloatComparison
    {
        public bool Evaluate(int a, int b) => a > b;
        public bool Evaluate(float a, float b) => a > b;
    }
}