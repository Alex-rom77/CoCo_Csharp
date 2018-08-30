﻿using CoCo.Analyser;
using CoCo.Test.Common;
using NUnit.Framework;

namespace CoCo.Test.CSharpIdentifiers
{
    internal class Constructions : CSharpIdentifierTests
    {
        [Test]
        public void ConstructionTest_Catch_Variable()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\CatchVariable.cs".GetClassifications(ProjectInfo)
                .AssertContains(
                    CSharpNames.LocalVariableName.ClassifyAt(311, 9),
                    CSharpNames.PropertyName.ClassifyAt(321, 7));
        }

        [Test]
        public void ConstructionTest_ForForeach_Variable()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\ForForeachControlVariable.cs".GetClassifications(ProjectInfo)
                .AssertContains(
                    CSharpNames.LocalVariableName.ClassifyAt(242, 5),
                    CSharpNames.LocalVariableName.ClassifyAt(251, 5),
                    CSharpNames.LocalVariableName.ClassifyAt(387, 4),
                    CSharpNames.LocalVariableName.ClassifyAt(399, 4));
        }

        [Test]
        public void ConstructionTest_If_PatternVariable()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\IfPatternVariable.cs".GetClassifications(ProjectInfo)
                .AssertContains(CSharpNames.LocalVariableName.ClassifyAt(254, 5));
        }

        [Test]
        public void ConstructionTest_Switch_PatternVariable()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\SwitchPatternVariable.cs".GetClassifications(ProjectInfo)
                .AssertContains(
                    CSharpNames.LocalVariableName.ClassifyAt(279, 5),
                    CSharpNames.LocalVariableName.ClassifyAt(391, 4));
        }

        [Test]
        public void ConstructionTest_Using_Variable()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\UsingVariable.cs".GetClassifications(ProjectInfo)
                .AssertContains(CSharpNames.LocalVariableName.ClassifyAt(228, 6));
        }

        [Test]
        public void ConstructionTest_Nameof()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\Nameof.cs".GetClassifications(ProjectInfo)
                .AssertContains(
                    CSharpNames.LocalVariableName.ClassifyAt(229, 8),
                    CSharpNames.EnumFieldName.ClassifyAt(287, 9),
                    CSharpNames.PropertyName.ClassifyAt(342, 12),
                    CSharpNames.FieldName.ClassifyAt(389, 5));
        }

        [Test]
        public void ConstructionTest_Return()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\Return.cs".GetClassifications(ProjectInfo)
                .AssertContains(CSharpNames.LocalVariableName.ClassifyAt(172, 6));
        }

        [Test]
        public void ConstructionTest_Throw()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\Throw.cs".GetClassifications(ProjectInfo)
                .AssertContains(CSharpNames.LocalVariableName.ClassifyAt(204, 9));
        }

        [Test]
        public void ConstructionTest_YieldReturn()
        {
            @"Tests\Identifiers\CSharpIdentifiers\Constructions\YieldReturn.cs".GetClassifications(ProjectInfo)
                .AssertContains(
                    CSharpNames.LocalVariableName.ClassifyAt(235, 5),
                    CSharpNames.LocalVariableName.ClassifyAt(337, 4));
        }
    }
}