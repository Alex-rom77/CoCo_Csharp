using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoCo.Analyser.Classifications;
using CoCo.Analyser.Classifications.CSharp;
using CoCo.Analyser.Classifications.VisualBasic;
using CoCo.Analyser.Editor;
using CoCo.Logging;
using CoCo.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Project = CoCo.MsBuild.ProjectInfo;

namespace CoCo.Test.Common
{
    public static class ClassificationHelper
    {
        private static readonly List<SimplifiedClassificationSpan> _empty = new List<SimplifiedClassificationSpan>();

        public static SimplifiedClassificationSpan ClassifyAt(this string name, int start, int length) => IsUnknownClassification(name)
            ? throw new ArgumentOutOfRangeException(nameof(name), "Argument must be one of constant names")
            : new SimplifiedClassificationSpan(new Span(start, length), new ClassificationType(name));

        public static SimplifiedClassificationInfo EnableInEditor(this string name)
        {
            if (IsUnknownClassification(name)) throw new ArgumentOutOfRangeException(nameof(name), "Argument must be one of constant names");
            SimplifiedClassificationInfo info = name;
            return info.EnableInEditor();
        }

        public static SimplifiedClassificationInfo DisableInEditor(this string name)
        {
            if (IsUnknownClassification(name)) throw new ArgumentOutOfRangeException(nameof(name), "Argument must be one of constant names");
            SimplifiedClassificationInfo info = name;
            return info.DisableInEditor();
        }

        public static SimplifiedClassificationInfo DisableInXml(this string name)
        {
            if (IsUnknownClassification(name)) throw new ArgumentOutOfRangeException(nameof(name), "Argument must be one of constant names");
            SimplifiedClassificationInfo info = name;
            return info.DisableInXml();
        }

        public static List<SimplifiedClassificationSpan> GetClassifications(
            string path, Project project, IReadOnlyList<SimplifiedClassificationInfo> infos = null)
        {
            using (var logger = LogManager.GetLogger("Test execution"))
            {
                path = Path.Combine(project.ProjectPath.GetDirectoryName(), path);
                if (!File.Exists(path))
                {
                    logger.Warn("File {0} doesn't exist.", path);
                    return _empty;
                }

                var compilationUnits = ExtractCompilationUnits(project);

                SemanticModel semanticModel = null;
                ProgrammingLanguage language = default;
                foreach (var unit in compilationUnits)
                {
                    var roslynCompilation = unit.Compilation;
                    var syntaxTree = roslynCompilation.SyntaxTrees.FirstOrDefault(x => x.FilePath.EqualsNoCase(path));
                    if (!(syntaxTree is null))
                    {
                        semanticModel = roslynCompilation.GetSemanticModel(syntaxTree, true);
                        language = unit.Language;
                        break;
                    }
                }

                if (semanticModel is null)
                {
                    logger.Warn("Project {0} doesn't have the file {1}. Check that it's included.", project.ProjectPath, path);
                    return _empty;
                }

                List<ClassificationSpan> actualSpans = null;
                // TODO: cache workspaces by project
                using (var workspace = new AdhocWorkspace())
                {
                    var buffer = new TextBuffer(GetContentType(language), new StringOperand(semanticModel.SyntaxTree.ToString()));
                    var snapshotSpan = new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length);

                    var newProject = workspace.AddProject(project.ProjectName, LanguageNames.CSharp);
                    var newDocument = workspace.AddDocument(newProject.Id, Path.GetFileName(path), snapshotSpan.Snapshot.AsText());

                    var classifier = GetClassifier(language, infos);
                    actualSpans = classifier.GetClassificationSpans(workspace, semanticModel, snapshotSpan);
                }
                return actualSpans.Select(x => new SimplifiedClassificationSpan(x.Span.Span, x.ClassificationType)).ToList();
            }
        }

        private static RoslynTextBufferClassifier GetClassifier(
            ProgrammingLanguage language, IReadOnlyList<SimplifiedClassificationInfo> infos)
        {
            var dictionary = infos?.ToDictionary(x => x.Name);
            var classificationTypes = new Dictionary<string, ClassificationInfo>(32);
            var names = language == ProgrammingLanguage.VisualBasic ? VisualBasicNames.All : CSharpNames.All;
            foreach (var name in names)
            {
                var option = dictionary is null || !dictionary.TryGetValue(name, out var simplifiedInfo)
                    ? ClassificationService.GetDefaultOption(name)
                    : simplifiedInfo;
                classificationTypes.Add(name, new ClassificationInfo(new ClassificationType(name), option));
            }

            if (language == ProgrammingLanguage.VisualBasic)
            {
                VisualBasicClassifierService.Reset();
                return new VisualBasicTextBufferClassifier(classificationTypes);
            }

            CSharpClassifierService.Reset();
            return new CSharpTextBufferClassifier(classificationTypes);
        }

        private static ContentType GetContentType(ProgrammingLanguage language) =>
            new ContentType(language == ProgrammingLanguage.VisualBasic ? "basic" : "csharp");

        private static CompilationUnit[] ExtractCompilationUnits(Project project)
        {
            using (var logger = LogManager.GetLogger("Test execution"))
            {
                var csharpTrees = new List<SyntaxTree>(project.CompileItems.Length);
                var visualBasicTrees = new List<SyntaxTree>(project.CompileItems.Length);
                foreach (var item in project.CompileItems)
                {
                    if (!File.Exists(item))
                    {
                        logger.Error($"File {item} doesn't exist");
                        continue;
                    }

                    var code = File.ReadAllText(item);
                    if (Path.GetExtension(item).EqualsNoCase(".vb"))
                    {
                        visualBasicTrees.Add(VisualBasicSyntaxTree.ParseText(code, VisualBasicParseOptions.Default, item));
                    }
                    else
                    {
                        // NOTE: currently is assumed that all this files is C#
                        // TODO: fix it in the future
                        csharpTrees.Add(CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default, item));
                    }
                }

                var references = new List<MetadataReference>(project.AssemblyReferences.Length + project.ProjectReferences.Length);
                foreach (var item in project.AssemblyReferences)
                {
                    references.Add(MetadataReference.CreateFromFile(item));
                }
                foreach (var item in project.ProjectReferences)
                {
                    foreach (var unit in ExtractCompilationUnits(item))
                    {
                        references.Add(unit.Compilation.ToMetadataReference());
                    }
                }

                var visualBasicOptions = new VisualBasicCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    rootNamespace: project.RootNamespace,
                    globalImports: project.Imports.Select(GlobalImport.Parse),
                    optionCompareText: project.OptionCompare,
                    optionExplicit: project.OptionExplicit,
                    optionInfer: project.OptionInfer,
                    optionStrict: project.OptionStrict ? OptionStrict.On : OptionStrict.Off);
                return new CompilationUnit[]
                {
                    CSharpCompilation.Create($"{project.ProjectName}_{LanguageNames.CSharp}")
                        .AddSyntaxTrees(csharpTrees)
                        .AddReferences(references),

                    VisualBasicCompilation.Create($"{project.ProjectName}_{LanguageNames.VisualBasic}", options: visualBasicOptions)
                        .AddSyntaxTrees(visualBasicTrees)
                        .AddReferences(references)
                };
            }
        }

        private static bool IsUnknownClassification(string name) =>
            !CSharpNames.All.Contains(name) && !VisualBasicNames.All.Contains(name);
    }
}