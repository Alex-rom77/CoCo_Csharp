﻿using CoCo.Analyser.Classifications.FSharp;
using CoCo.Test.Identifiers.Common;
using NUnit.Framework;

namespace CoCo.Test.Identifiers.FSharp.Access
{
    internal class Attributes : FSharpIdentifierTests
    {
        [Test]
        public void AttributeTest()
        {
            GetContext(@"Access\Attribute.fs").GetClassifications().AssertContains(
                FSharpNames.ClassName.ClassifyAt(62, 9),
                FSharpNames.ClassName.ClassifyAt(131, 9),
                FSharpNames.ClassName.ClassifyAt(142, 10));
        }

        [Test]
        public void AttributeAsModuleElementTest()
        {
            GetContext(@"Access\AttributeAsModuleElement.fs").GetClassifications().AssertContains(
                FSharpNames.ClassName.ClassifyAt(63, 22));
        }
    }
}