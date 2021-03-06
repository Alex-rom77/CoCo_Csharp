using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CoCo.Analyser.QuickInfo
{
    /// <summary>
    /// It's a abstract provider for full description of symbols in quick info (tooltip)
    /// </summary>
    internal abstract partial class SymbolDescriptionProvider
    {
        [Flags]
        protected enum PrefixKind
        {
            None = 0,
            Awaitable = 1 << 0,
            Extension = 1 << 1,
            All = Awaitable | Extension,
        }

        /// <summary>
        /// Contains collected classified texts for input symbols
        /// </summary>
        private Dictionary<SymbolDescriptionKind, ImmutableArray<TaggedText>.Builder> _description;

        private ImageKind _image;
        private Dictionary<ISymbol, string> _anonymousNames;

        private readonly SymbolDisplayPartConverter _displayConverter;
        private readonly int _position;

        protected readonly SemanticModel SemanticModel;
        protected readonly CancellationToken CancellationToken;

        protected SymbolDescriptionProvider(
            SymbolDisplayPartConverter displayConverter,
            SemanticModel semanticModel,
            int position,
            CancellationToken cancellationToken)
        {
            _displayConverter = displayConverter;
            _position = position;

            SemanticModel = semanticModel;
            CancellationToken = cancellationToken;
        }

        public async Task<SymbolDescriptionInfo> GetDescriptionAsync(ImmutableArray<ISymbol> symbols)
        {
            if (symbols.IsDefaultOrEmpty) return default;

            if (_description is null)
            {
                _description = new Dictionary<SymbolDescriptionKind, ImmutableArray<TaggedText>.Builder>();
            }

            await AppendPartsAsync(symbols);
            return BuildDescription();
        }

        /// <remarks>
        /// C# and VB has a different style for braces and for the word's case
        /// </remarks>
        protected abstract void AppenDeprecatedParts();

        /// <remarks>
        /// C# and VB has a different style for braces and for the word's case
        /// </remarks>
        protected abstract void AppendPrefixParts(PrefixKind prefix);

        protected abstract ImmutableArray<SymbolDisplayPart>.Builder GetAnonymousTypeParts(
            SymbolDisplayPart part, ITypeSymbol anonymousType);

        protected abstract Task<ImmutableArray<SymbolDisplayPart>> GetInitializerPartsAsync(ISymbol symbol);

        protected SymbolDescriptionInfo BuildDescription()
        {
            var result = new Dictionary<SymbolDescriptionKind, ImmutableArray<TaggedText>>();
            foreach (var item in _description)
            {
                result[item.Key] = item.Value.TryMoveToImmutable();
            }

            return new SymbolDescriptionInfo(result, _image);
        }

        protected void SetImage(ImageKind image) => _image = image;

        protected async Task AppendPartsAsync(ImmutableArray<ISymbol> symbols)
        {
            var main = GetRelevantSymbol(symbols[0]);

            AppendDeprecatedParts(main);
            await AppendDescriptionPartsAsync(main);
            AppendOverloadCountParts(symbols);
            AppendCaptureParts(main);
            XmlDocumentParser.Parse(this, main);
        }

        protected void AppendDeprecatedParts(ISymbol symbol)
        {
            foreach (var item in symbol.GetAttributes())
            {
                if (item.AttributeClass.MetadataName.Equals("ObsoleteAttribute"))
                {
                    AppenDeprecatedParts();
                    return;
                }
            }
        }

        protected async Task AppendDescriptionPartsAsync(ISymbol symbol)
        {
            void AppendImageKind(int startPosition, Accessibility accessibility = Accessibility.NotApplicable)
            {
                var delta =
                    accessibility == Accessibility.Public ? 0 :
                    accessibility == Accessibility.Internal ? 1 :
                    accessibility == Accessibility.Protected ? 2 :
                    accessibility == Accessibility.Private ? 3 :
                    0;
                SetImage((ImageKind)startPosition + delta);
            }

            // TODO: miss something?
            switch (symbol)
            {
                case IDynamicTypeSymbol _:
                    AppendDynamicTypeParts();
                    break;

                case IEventSymbol _:
                    AppendSymbolParts(symbol);
                    AppendImageKind(21, symbol.DeclaredAccessibility);
                    break;

                case IFieldSymbol field:
                    await AppendFieldPartsAsync(field);

                    var fieldStart =
                        field.ContainingType.TypeKind == TypeKind.Enum ? 17 :
                        field.IsConst ? 5 :
                        29;
                    AppendImageKind(fieldStart, field.DeclaredAccessibility);
                    break;

                case ILabelSymbol label:
                    AppendLabelParts(label);
                    AppendImageKind(53);
                    break;

                case ILocalSymbol local:
                    await AppendLocalPartsAsync(local);
                    AppendImageKind(54);
                    break;

                case IMethodSymbol method:
                    AppendMethodParts(method);

                    var methodStart = method.IsExtensionMethod() ? 25 : 37;
                    AppendImageKind(methodStart, method.DeclaredAccessibility);
                    break;

                case INamedTypeSymbol namedType:
                    AppendNamedTypeParts(namedType);

                    var (start, accessibility) =
                        namedType.TypeKind == TypeKind.Class ? (1, namedType.DeclaredAccessibility) :
                        namedType.TypeKind == TypeKind.Struct ? (49, namedType.DeclaredAccessibility) :
                        namedType.TypeKind == TypeKind.Enum ? (13, namedType.DeclaredAccessibility) :
                        namedType.TypeKind == TypeKind.Delegate ? (9, namedType.DeclaredAccessibility) :
                        namedType.TypeKind == TypeKind.Error ? (100, Accessibility.NotApplicable) :
                        namedType.TypeKind == TypeKind.Interface ? (33, namedType.DeclaredAccessibility) :
                        namedType.TypeKind == TypeKind.Module ? (41, namedType.DeclaredAccessibility) :
                        namedType.TypeKind == TypeKind.TypeParameter ? (57, Accessibility.NotApplicable) :
                        (0, Accessibility.NotApplicable);
                    AppendImageKind(start, accessibility);
                    break;

                case INamespaceSymbol @namespace:
                    AppendNamespaceParts(@namespace);
                    AppendImageKind(55);
                    break;

                case IParameterSymbol parameter:
                    await AppendParameterPartsAsync(parameter);
                    AppendImageKind(56);
                    break;

                case IPropertySymbol property:
                    AppendPropertyParts(property);
                    AppendImageKind(45, property.DeclaredAccessibility);
                    break;

                case IRangeVariableSymbol rangeVariable:
                    AppendRangeVariableParts(rangeVariable);
                    AppendImageKind(58);
                    break;

                case ITypeParameterSymbol typeParameter:
                    AppendTypeParameterParts(typeParameter);
                    AppendImageKind(57);
                    break;

                default:
                    AppendSymbolParts(symbol);
                    break;
            }
        }

        protected void AppendOverloadCountParts(ImmutableArray<ISymbol> symbols)
        {
            int GetOverloadCount()
            {
                if (symbols.IsDefaultOrEmpty) return 0;

                var main = symbols[0].OriginalDefinition;
                var overloadCount = 0;
                foreach (var symbol in symbols)
                {
                    var original = symbol.OriginalDefinition;
                    if (!original.Equals(main) &&
                        (original is IMethodSymbol || original is IPropertySymbol property && property.IsIndexer))
                    {
                        ++overloadCount;
                    }
                }
                return overloadCount;
            }

            var count = GetOverloadCount();
            if (count >= 1)
            {
                AppendParts(SymbolDescriptionKind.Main,
                    CreateSpaces(), CreatePunctuation("("),
                    CreatePunctuation("+"), CreateSpaces(),
                    CreateText($"{count}"), CreateSpaces(),
                    count == 1 ? CreateText("overload") : CreateText("overloads"),
                    CreatePunctuation(")"));
            }
        }

        /// <summary>
        /// Adds captured variables
        /// </summary>
        protected void AppendCaptureParts(ISymbol symbol)
        {
            // NOTE: lambda initializer in type cannot reference to a non static fields
            if (symbol is IMethodSymbol method && method.ContainingSymbol.Kind == SymbolKind.Method)
            {
                if (method.DeclaringSyntaxReferences.IsDefaultOrEmpty) return;

                var syntax = method.DeclaringSyntaxReferences[0].GetSyntax();
                // TODO: should directly check that syntax is lambda or delegate?
                if (!(syntax is null))
                {
                    var dataFlow = GetSemanticModel(syntax.SyntaxTree)?.AnalyzeDataFlow(syntax);
                    if (dataFlow is null) return;

                    var declaredVariables = new HashSet<ISymbol>(dataFlow.VariablesDeclared);

                    List<SymbolDisplayPart> parts = null;
                    foreach (var captureVariable in dataFlow.Captured)
                    {
                        if (declaredVariables.Contains(captureVariable)) continue;

                        if (parts is null)
                        {
                            parts = new List<SymbolDisplayPart> { CreateText("\r\nVariables captured:") };
                        }
                        else
                        {
                            parts.Add(CreatePunctuation(","));
                        }
                        parts.Add(CreateSpaces());
                        parts.AddRange(ToMinimalDisplayParts(captureVariable, _capturesFormat));
                    }
                    if (!(parts is null))
                    {
                        AppendParts(SymbolDescriptionKind.Captures, parts);
                    }
                }
            }
        }

        protected void AppendPropertyParts(IPropertySymbol symbol) =>
            AppendParts(SymbolDescriptionKind.Main, ToMinimalDisplayParts(symbol, _memberFormat));

        protected void AppendNamedTypeParts(INamedTypeSymbol symbol)
        {
            if (symbol.IsAwaitable())
            {
                AppendPrefixParts(PrefixKind.Awaitable);
            }

            var format = symbol.TypeKind == TypeKind.Delegate ? _delegateFormat : _descriptionFormat;
            AppendParts(SymbolDescriptionKind.Main, symbol.OriginalDefinition.ToDisplayParts(format));
            AppendTypeParameterParts(symbol);
        }

        protected void AppendNamespaceParts(INamespaceSymbol symbol)
        {
            var parts = symbol.IsGlobalNamespace
                ? symbol.ToDisplayParts(_globalNamespaceFormat)
                : symbol.ToDisplayParts(_descriptionFormat);
            AppendParts(SymbolDescriptionKind.Main, parts);
        }

        protected async Task AppendFieldPartsAsync(IFieldSymbol symbol)
        {
            var parts = await GetDeclarationPartsAsync(symbol, symbol.IsConst);

            // NOTE: don't show redundant info for enum fields
            if (symbol.ContainingType.TypeKind != TypeKind.Enum)
            {
                parts.InsertRange(0, symbol.IsConst ? CreateDescription("constant") : CreateDescription("field"));
            }
            AppendParts(SymbolDescriptionKind.Main, parts);
        }

        protected async Task AppendLocalPartsAsync(ILocalSymbol symbol)
        {
            var parts = await GetDeclarationPartsAsync(symbol, symbol.IsConst);
            var prefixParts =
                symbol.IsConst ? CreateDescription("local constant") :
                symbol.IsFunctionValue ? CreateDescription("function variable") :
                CreateDescription("local variable");
            parts.InsertRange(0, prefixParts);
            AppendParts(SymbolDescriptionKind.Main, parts);
        }

        protected void AppendLabelParts(ILabelSymbol symbol)
        {
            var parts = CreateDescription("label");
            parts.AddRange(ToMinimalDisplayParts(symbol));
            AppendParts(SymbolDescriptionKind.Main, parts);
        }

        protected void AppendMethodParts(IMethodSymbol method)
        {
            // NOTE: VS shows special prefix: (awaitable), (extension) and (awaitable, extension)
            // for the corresponding method's type and doesn't show anything else for regular methods
            var isAwaitable = method.IsAwaitable();
            var isExtension = method.IsExtensionMethod();

            var prefix =
                isAwaitable && isExtension ? PrefixKind.All :
                isAwaitable ? PrefixKind.Awaitable :
                isExtension ? PrefixKind.Extension :
                PrefixKind.None;

            if (prefix != PrefixKind.None)
            {
                AppendPrefixParts(prefix);
            }
            AppendParts(SymbolDescriptionKind.Main, ToMinimalDisplayParts(method, _memberFormat));
        }

        protected async Task AppendParameterPartsAsync(IParameterSymbol symbol)
        {
            var parts = await GetDeclarationPartsAsync(symbol, symbol.IsOptional);
            parts.InsertRange(0, CreateDescription("parameter"));
            AppendParts(SymbolDescriptionKind.Main, parts);
        }

        protected void AppendTypeParameterParts(ITypeParameterSymbol symbol)
        {
            var parts = ToMinimalDisplayParts(symbol);
            parts.Add(CreateSpaces());
            parts.Add(CreateText("in"));
            parts.Add(CreateSpaces());
            parts.AddRange(ToMinimalDisplayParts(symbol.ContainingSymbol, _typeParameterFormat));
            AppendParts(SymbolDescriptionKind.Main, parts);
        }

        protected void AppendDynamicTypeParts()
        {
            AppendParts(SymbolDescriptionKind.Main, CreateKeyword("dynamic"));
            AppendParts(SymbolDescriptionKind.Additional, CreateText("Represents an object whose operations will be resolved at runtime."));
        }

        protected void AppendRangeVariableParts(IRangeVariableSymbol symbol)
        {
            var parts = CreateDescription("range variable");
            parts.AddRange(ToMinimalDisplayParts(symbol));
            AppendParts(SymbolDescriptionKind.Main, parts);
        }

        protected void AppendTypeParameterParts(INamedTypeSymbol symbol)
        {
            if (symbol.IsUnboundGenericType) return;

            var containingTypes = symbol.AncestorsContainingTypesAndSelf();
            var typeParameters = new List<ITypeParameterSymbol>();
            foreach (var item in containingTypes)
            {
                typeParameters.AddRange(item.TypeParameters);
            }

            var typeArguments = new List<ITypeSymbol>();
            foreach (var item in containingTypes)
            {
                typeArguments.AddRange(item.TypeArguments);
            }

            List<SymbolDisplayPart> parts = null;
            for (int i = 0; i < typeParameters.Count; ++i)
            {
                if (i >= typeArguments.Count) continue;

                var typeParameter = typeParameters[i];
                var typeArgument = typeArguments[i];
                if (typeParameter.Equals(typeArgument) || typeParameter.Name.Equals(typeArgument.Name)) continue;

                if (parts is null)
                {
                    parts = new List<SymbolDisplayPart>();
                    parts.Add(CreatePart(SymbolDisplayPartKind.LineBreak, "\r\n"));
                }

                parts.AddRange(ToMinimalDisplayParts(typeParameters[i]));
                parts.Add(CreateSpaces());
                parts.Add(CreateText("is"));
                parts.Add(CreateSpaces());
                parts.AddRange(ToMinimalDisplayParts(typeArguments[i]));
                if (i < typeParameters.Count - 1)
                {
                    parts.Add(CreatePart(SymbolDisplayPartKind.LineBreak, "\r\n"));
                }
            }

            if (!(parts is null))
            {
                AppendParts(SymbolDescriptionKind.TypeParameter, parts);
            }
        }

        protected void AppendSymbolParts(ISymbol symbol) =>
            AppendParts(SymbolDescriptionKind.Main, ToMinimalDisplayParts(symbol));

        protected async Task<ImmutableArray<SymbolDisplayPart>.Builder> GetDeclarationPartsAsync(ISymbol symbol, bool isConst)
        {
            if (isConst)
            {
                var initializerParts = await GetInitializerPartsAsync(symbol);
                if (!initializerParts.IsDefaultOrEmpty)
                {
                    var parts = ToMinimalDisplayParts(symbol, _minimallyQualifiedFormat);
                    parts.Add(CreateSpaces());
                    parts.Add(CreatePunctuation("="));
                    parts.Add(CreateSpaces());
                    parts.AddRange(initializerParts);
                    return parts;
                }
            }
            return ToMinimalDisplayParts(symbol, _minimallyQualifiedFormatWithConstants);
        }

        protected SymbolDisplayPart CreateText(string text) => CreatePart(SymbolDisplayPartKind.Text, text);

        protected SymbolDisplayPart CreatePunctuation(string text) => CreatePart(SymbolDisplayPartKind.Punctuation, text);

        protected SymbolDisplayPart CreateSpaces(int count = 1)
        {
            var text =
                count == 1 ? " " :
                count == 2 ? "  " :
                count == 3 ? "   " :
                count == 4 ? "    " :
                new string(' ', count);

            return CreatePart(SymbolDisplayPartKind.Space, text);
        }

        protected SymbolDisplayPart CreateKeyword(string text) => CreatePart(SymbolDisplayPartKind.Keyword, text);

        protected List<SymbolDisplayPart> CreateDescription(string description) => new List<SymbolDisplayPart>
        {
            CreatePunctuation("("),
            CreateText(description),
            CreatePunctuation(")"),
            CreateSpaces(),
        };

        protected SymbolDisplayPart CreatePart(SymbolDisplayPartKind kind, string text) => new SymbolDisplayPart(kind, null, text);

        protected void AppendParts(SymbolDescriptionKind kind, params SymbolDisplayPart[] partsArray) =>
            AppendParts<SymbolDisplayPart[]>(kind, partsArray);

        protected SymbolDisplayPart ProcessAnonymousType(SymbolDisplayPart part)
        {
            if (part.Symbol is ITypeSymbol type && type.IsAnonymousType)
            {
                if (_anonymousNames is null)
                {
                    _anonymousNames = new Dictionary<ISymbol, string>();
                }

                if (!_anonymousNames.TryGetValue(part.Symbol, out var anonymousName))
                {
                    var count = _anonymousNames.Count;
                    var div = count >= 26 ? count / 26 : 0;
                    var mod = count - div * 26;
                    var @char = (char)(mod + 97);
                    anonymousName = div > 0 ? $"'{@char}{div}" : $"'{@char}";
                    _anonymousNames[part.Symbol] = anonymousName;
                    part = new SymbolDisplayPart(part.Kind, part.Symbol, anonymousName);

                    if (!_description.TryGetValue(SymbolDescriptionKind.AnonymousTypes, out var anonymousParts))
                    {
                        anonymousParts = ImmutableArray.CreateBuilder<TaggedText>();
                        _description.Add(SymbolDescriptionKind.AnonymousTypes, anonymousParts);

                        AppendParts(SymbolDescriptionKind.AnonymousTypes, CreateText("\r\nAnonymous Types:").Enumerate());
                        AppendParts(SymbolDescriptionKind.AnonymousTypes, CreatePart(SymbolDisplayPartKind.LineBreak, "\r\n"));
                    }

                    AppendParts(SymbolDescriptionKind.AnonymousTypes, GetAnonymousTypeParts(part, type));
                }
                part = new SymbolDisplayPart(part.Kind, part.Symbol, anonymousName);
            }
            return part;
        }

        protected void AppendParts<T>(SymbolDescriptionKind descriptionKind, T parts) where T : IEnumerable<SymbolDisplayPart>
        {
            if (_description is null)
            {
                _description = new Dictionary<SymbolDescriptionKind, ImmutableArray<TaggedText>.Builder>();
            }

            if (!_description.TryGetValue(descriptionKind, out var builder))
            {
                builder = ImmutableArray.CreateBuilder<TaggedText>();
                _description.Add(descriptionKind, builder);
            }

            foreach (var item in parts)
            {
                var part = ProcessAnonymousType(item);
                var tag = _displayConverter.ToTag(part);
                if (!tag.IsDefault)
                {
                    builder.Add(tag);
                }
            }
        }

        protected ImmutableArray<SymbolDisplayPart>.Builder ToMinimalDisplayParts(ISymbol symbol, SymbolDisplayFormat format = null)
        {
            format = format ?? _minimallyQualifiedFormat;
            return symbol.ToMinimalDisplayParts(SemanticModel, _position, format).ToBuilder();
        }

        /// <remarks>
        /// For the couple of symbol, such <see cref="IAliasSymbol"/>, we should collect description from a relevant symbol
        /// </remarks>
        private ISymbol GetRelevantSymbol(ISymbol symbol) => symbol.Kind == SymbolKind.Alias ? (symbol as IAliasSymbol)?.Target : symbol;

        private SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
        {
            if (SemanticModel.SyntaxTree.Equals(syntaxTree)) return SemanticModel;

            var compilation = SemanticModel.Compilation;
            if (compilation.ContainsSyntaxTree(syntaxTree)) return compilation.GetSemanticModel(syntaxTree);

            // NOTE: try to find from referenced projects
            foreach (var referencedCompilation in compilation.GetReferencedCompilations())
            {
                if (referencedCompilation.ContainsSyntaxTree(syntaxTree)) return referencedCompilation.GetSemanticModel(syntaxTree);
            }
            return null;
        }
    }
}