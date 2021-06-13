﻿using fNbt.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace fNbt.Test
{
	[TestFixture]
	public sealed class NbtSerializerTests
	{
		[Test]
		public void BuildFromTagTest()
		{
			CheckFromTag(false);
		}

		[Test]
		public void FillFromTagTest()
		{
			CheckFromTag(true);
		}

		[Test]
		public void FillTest()
		{
			var valuesSet = new List<string>() { "testVal1", "test_val_2", "TestVal3", "TEST_VAL_4" };

			var original = new FillTestClass();
			original.GetOnlyTestStringList.AddRange(valuesSet);
			original.GetOnlyTestClassProperty.EasyIntProperty = 321;
			original.GetOnlyTestClassProperty.EasyStringProperty = "ESP_TEST_FILL";

			var tag = NbtSerializer.SerializeObject(original);

			var result = NbtSerializer.DeserializeObject<FillTestClass>(tag);

			Assert.IsNotNull(result);

			Assert.AreEqual(original.GetOnlyTestClassProperty.EasyIntProperty, result.GetOnlyTestClassProperty.EasyIntProperty);
			Assert.AreEqual(original.GetOnlyTestClassProperty.EasyStringProperty, result.GetOnlyTestClassProperty.EasyStringProperty);
			CollectionAssert.AreEquivalent(original.GetOnlyTestStringList, result.GetOnlyTestStringList);
		}

		[Test]
		public void FieldsAndPropertiesTest()
		{
			var raw = new FieldsAndPropertiesTestClass();
			var tag = NbtSerializer.SerializeObject(raw);

			Assert.IsNotNull(tag);

			Assert.IsFalse(tag.Contains(nameof(raw.HidenTestIntFiled)));
			Assert.IsFalse(tag.Contains(nameof(raw.HidenDefaultTestIntFiled)));

			Assert.IsFalse(tag.Contains(nameof(raw.HidenTestIntProperty)));
			Assert.IsFalse(tag.Contains(nameof(raw.HidenDefaultTestIntProperty)));

			AssertTagValue<NbtInt>(tag, nameof(raw.DefaultTestIntFiled), raw.DefaultTestIntFiled, t => t.Value);
			AssertTagValue<NbtInt>(tag, nameof(raw.TestIntFiled), raw.TestIntFiled, t => t.Value);
			AssertTagValue<NbtInt>(tag, "PrivateTestIntFiled", 3, t => t.Value);
			AssertTagValue<NbtInt>(tag, FieldsAndPropertiesTestClass.NameForNamedTestIndFiled, raw.NamedTestIntFiled, t => t.Value);

			AssertTagValue<NbtInt>(tag, nameof(raw.DefaultTestIntProperty), raw.DefaultTestIntProperty, t => t.Value);
			AssertTagValue<NbtInt>(tag, nameof(raw.TestIntProperty), raw.TestIntProperty, t => t.Value);
			AssertTagValue<NbtInt>(tag, nameof(raw.OnlyGetTestIntProperty), raw.OnlyGetTestIntProperty, t => t.Value);
			AssertTagValue<NbtInt>(tag, "PrivateTestIntProperty", 8, t => t.Value);
			AssertTagValue<NbtInt>(tag, FieldsAndPropertiesTestClass.NameForNamedTestIndProperty, raw.NamedTestIntProperty, t => t.Value);
		}

		private void CheckFromTag(bool fill)
		{
			var stringValue = "test_string_value_rv";
			var intValue = 882882;

			var shadow = new EasyTestClass();

			var tag = new NbtCompound();
			tag.Add(new NbtString(nameof(shadow.EasyStringProperty), stringValue));
			tag.Add(new NbtInt(nameof(shadow.EasyIntProperty), intValue));

			EasyTestClass result = null;

			if (fill)
			{
				result = shadow;
				NbtSerializer.FillObject(result, tag);
			}
			else
			{
				result = NbtSerializer.DeserializeObject<EasyTestClass>(tag);
			}

			Assert.IsNotNull(result);

			Assert.AreEqual(stringValue, result.EasyStringProperty);
			Assert.AreEqual(intValue, result.EasyIntProperty);
		}

		private void AssertTagValue<TExpected>(NbtCompound parentTag, string expectedTagName, object expectedValue, Func<TExpected, object> getValue) where TExpected : NbtTag
		{
			Assert.True(parentTag.TryGet(expectedTagName, out NbtTag tag), 
				$"expected tag [{expectedTagName}] is not contains.");

			Assert.IsAssignableFrom<TExpected>(tag, 
				$"actual tag type is [{tag.GetType()}], but expected [{typeof(TExpected)}]");

			var actual = getValue((TExpected)tag);
			Assert.AreEqual(expectedValue, actual);
		}

		#region test classes

		public class FillTestClass
		{
			[NbtProperty] public List<string> GetOnlyTestStringList { get; } = new List<string>();
			[NbtProperty] public EasyTestClass GetOnlyTestClassProperty { get; } = new EasyTestClass();
		}

		public class EasyTestClass
		{
			[NbtProperty] public string EasyStringProperty { get; set; } = "easy property value";
			[NbtProperty] public int EasyIntProperty { get; set; } = 123456789;
		}

		public class FieldsAndPropertiesTestClass
		{
			public int HidenTestIntFiled = 1;
			[NbtProperty] public int HidenDefaultTestIntFiled = 0;
			[NbtProperty(hideDefault: false)] public int DefaultTestIntFiled = 0;
			[NbtProperty] public int TestIntFiled = 2;

			[NbtProperty] private int PrivateTestIntFiled = 3;

			public const string NameForNamedTestIndFiled = "named_test_int_field";
			[NbtProperty(NameForNamedTestIndFiled)] public int NamedTestIntFiled = 4;

			public int HidenTestIntProperty { get; set; } = 5;
			[NbtProperty] public int HidenDefaultTestIntProperty { get; set; } = 0;
			[NbtProperty(hideDefault: false)] public int DefaultTestIntProperty { get; set; } = 0;
			[NbtProperty] public int TestIntProperty { get; set; } = 6;
			[NbtProperty] public int OnlyGetTestIntProperty { get; } = 7;
			[NbtProperty] private int PrivateTestIntProperty { get; set; } = 8;

			public const string NameForNamedTestIndProperty = "named_test_int_property";
			[NbtProperty(NameForNamedTestIndProperty)] public int NamedTestIntProperty { get; set; } = 9;
		}

		#endregion
	}
}