namespace MAGES.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using MAGES;
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// Tests various aspects of the MAGES.OverridableProperty system.
    /// </summary>
    public class TestOverridableProperty
    {
        /// <summary>
        /// Tests that the MAGES.OverridableProperty system works correctly for primitive types.
        /// </summary>
        [Test]
        public void TestOverridablePropertyPrimitiveType()
        {
            Data data = new Data();
            Assert.AreEqual(data.IntegerProperty, 1);
        }

        /// <summary>
        /// Tests that the OverridableProperty system works correctly for compound types.
        /// </summary>
        [Test]
        public void TestOverridablePropertyCompoundType()
        {
            Data data = new Data();
            var c = data.CompoundProperty;
            Assert.AreEqual(c.Integer, 2);
            Assert.AreEqual(c.Single, 10.0f);
        }

        /// <summary>
        /// Tests that the MAGES.OverridableProperty system works correctly by accesing values through indexers.
        /// </summary>
        [Test]
        public void TestOverridablePropertyIndexer()
        {
            Data data = new Data();
            Assert.AreEqual(data["integerProperty"], 1);
        }

        /// <summary>
        /// Tests that the MAGES.OverridableProperty system works correctly when modifying values through indexers.
        /// </summary>
        [Test]
        public void TestOverridablePropertyIndexerModify()
        {
            Data data = new Data();
            data["integerProperty"] = 2;

            Assert.AreEqual(data["integerProperty"], 2);
            Assert.AreEqual(data.IntegerProperty, 2);
        }

        private class Compound : ICloneable
        {
            [SerializeField]
            private int integer;

            [SerializeField]
            private float single;

            public int Integer { get => integer; set => integer = value; }

            public float Single { get => single; set => single = value; }

            public object Clone()
            {
                return new Compound()
                {
                    Integer = Integer,
                    Single = Single,
                };
            }
        }

        [System.Serializable]
        private class Data : PropertyCollection
        {
            [DefaultValueLocator(typeof(int), typeof(Defaults), "k-integer")]
            [SerializeField]
            private OverridableProperty<int> integerProperty = new OverridableProperty<int>();

            [DefaultValueLocator(typeof(Compound), typeof(Defaults), "k-compound")]
            [SerializeField]
            private OverridableProperty<Compound> compoundProperty = new OverridableProperty<Compound>();

            public int IntegerProperty => integerProperty.GetEffectiveValue();

            public Compound CompoundProperty => compoundProperty.GetEffectiveValue();
        }

        private class Defaults : ILocator
        {
            private Dictionary<string, System.Func<object>> dict = new Dictionary<string, System.Func<object>>()
        {
            { "k-integer", () => 1 },
            { "k-compound", () => new Compound() { Integer = 2, Single = 10.0f } },
        };

            public object Locate(string key)
            {
                return dict[key]();
            }
        }
    }
}