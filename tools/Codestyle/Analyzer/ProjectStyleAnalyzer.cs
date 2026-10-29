using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace MadSlime.CodeStyle
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProjectStyleAnalyzer : DiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> s_rules = ImmutableArray.Create(
            CreateRule("MS0001", "Prefer an explicit type over var", DiagnosticSeverity.Info),
            CreateRule("MS0002", "Compare with false instead of using logical negation"),
            CreateRule("MS0003", "Use an explicit if/else instead of a conditional expression"),
            CreateRule("MS0004", "Use an explicit null check instead of ?? or ??="),
            CreateRule("MS0005", "Use an auto-property or an expression-bodied property that returns a member"),
            CreateRule("MS0006", "Use a block body for methods"),
            CreateRule("MS0007", "Do not add comments to project code"),
            CreateRule("MS0008", "Subscribe using a named event handler"),
            CreateRule("MS0009", "Use a descriptive name; i and j are allowed only as for-loop counters"),
            CreateRule("MS0010", "Use a C# Action event instead of UnityEvent"),
            CreateRule("MS0011", "Project rules prohibit calling {0}"),
            CreateRule("MS0012", "Wrap this line at 120 characters"),
            CreateRule("MS0013", "The file name must match the main type name"),
            CreateRule("MS0014", "Use one class per file, except Serializable companions of a ScriptableObject"),
            CreateRule("MS0015", "Use System.Action or System.Action<T> as the event type"),
            CreateRule("MS0016", "Field {0} must follow the {1} naming convention"),
            CreateRule("MS0017", "Event handler names must start with On"),
            CreateRule("MS0018", "Separate these logical blocks with a blank line"));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => s_rules;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSemanticModelAction(Analyze);
        }

        private static DiagnosticDescriptor CreateRule(string identifier, string message,
            DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        {
            return new DiagnosticDescriptor(identifier, message, message, "MadSlime.CodeStyle", severity, true);
        }

        private static void Analyze(SemanticModelAnalysisContext context)
        {
            SyntaxTree tree = context.SemanticModel.SyntaxTree;
            string path = tree.FilePath.Replace('\\', '/');
            if (path.StartsWith("Assets/MadSlime/", StringComparison.Ordinal) == false
                && path.IndexOf("/Assets/MadSlime/", StringComparison.Ordinal) < 0)
            {
                return;
            }

            if (Path.GetFileName(path) == "PlayerInputActions.cs")
            {
                return;
            }

            SyntaxNode root = tree.GetRoot(context.CancellationToken);
            foreach (SyntaxTrivia trivia in root.DescendantTrivia())
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                    || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                    || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    Report(context, 7, trivia.GetLocation());
                }
            }

            SourceText text = tree.GetText(context.CancellationToken);
            foreach (TextLine line in text.Lines)
            {
                if (line.Span.Length > 120)
                {
                    Report(context, 12, Location.Create(tree, new TextSpan(line.Start + 120, line.Span.Length - 120)));
                }
            }

            AnalyzeBlankLines(context, root, text);

            List<ClassDeclarationSyntax> classes = new List<ClassDeclarationSyntax>();
            foreach (SyntaxNode node in root.DescendantNodes())
            {
                if (node is IdentifierNameSyntax identifier && identifier.IsVar
                    && (identifier.Parent is VariableDeclarationSyntax || identifier.Parent is ForEachStatementSyntax
                        || identifier.Parent is DeclarationExpressionSyntax))
                {
                    ITypeSymbol type = context.SemanticModel.GetTypeInfo(identifier, context.CancellationToken).Type;
                    if (type == null || type.Name != "var")
                    {
                        Report(context, 1, identifier.GetLocation());
                    }
                }

                switch (node.Kind())
                {
                    case SyntaxKind.LogicalNotExpression:
                        Report(context, 2, ((PrefixUnaryExpressionSyntax)node).OperatorToken.GetLocation());
                        break;
                    case SyntaxKind.ConditionalExpression:
                        Report(context, 3, node.GetLocation());
                        break;
                    case SyntaxKind.CoalesceExpression:
                    case SyntaxKind.CoalesceAssignmentExpression:
                        Report(context, 4, node.GetLocation());
                        break;
                    case SyntaxKind.PropertyDeclaration:
                        AnalyzeProperty(context, (PropertyDeclarationSyntax)node);
                        break;
                    case SyntaxKind.ArrowExpressionClause:
                        if (node.Parent is BaseMethodDeclarationSyntax || node.Parent is LocalFunctionStatementSyntax)
                        {
                            Report(context, 6, node.GetLocation());
                        }
                        break;
                    case SyntaxKind.AddAssignmentExpression:
                    case SyntaxKind.SubtractAssignmentExpression:
                        AnalyzeSubscription(context, (AssignmentExpressionSyntax)node);
                        break;
                    case SyntaxKind.VariableDeclarator:
                        VariableDeclaratorSyntax variable = (VariableDeclaratorSyntax)node;
                        bool counter = variable.Parent.Parent is ForStatementSyntax
                            && (variable.Identifier.ValueText == "i" || variable.Identifier.ValueText == "j");
                        if (counter == false && IsContractField(context, variable) == false)
                        {
                            CheckShortName(context, variable.Identifier);
                        }
                        AnalyzeField(context, variable);
                        break;
                    case SyntaxKind.Parameter:
                        CheckShortName(context, ((ParameterSyntax)node).Identifier);
                        break;
                    case SyntaxKind.ForEachStatement:
                        CheckShortName(context, ((ForEachStatementSyntax)node).Identifier);
                        break;
                    case SyntaxKind.CatchDeclaration:
                        CheckShortName(context, ((CatchDeclarationSyntax)node).Identifier);
                        break;
                    case SyntaxKind.SingleVariableDesignation:
                        CheckShortName(context, ((SingleVariableDesignationSyntax)node).Identifier);
                        break;
                    case SyntaxKind.IdentifierName:
                    case SyntaxKind.GenericName:
                        AnalyzeUnityEvent(context, node);
                        break;
                    case SyntaxKind.InvocationExpression:
                        AnalyzeInvocation(context, (InvocationExpressionSyntax)node, path);
                        break;
                    case SyntaxKind.ClassDeclaration:
                        ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)node;
                        if (declaration.Parent is NamespaceDeclarationSyntax || declaration.Parent is CompilationUnitSyntax)
                        {
                            classes.Add(declaration);
                        }
                        break;
                    case SyntaxKind.EventFieldDeclaration:
                        EventFieldDeclarationSyntax eventField = (EventFieldDeclarationSyntax)node;
                        CheckEventType(context, eventField.Declaration.Type);
                        break;
                    case SyntaxKind.EventDeclaration:
                        CheckEventType(context, ((EventDeclarationSyntax)node).Type);
                        break;
                }
            }

            AnalyzeClasses(context, classes, path);
        }

        private static void AnalyzeBlankLines(SemanticModelAnalysisContext context, SyntaxNode root, SourceText text)
        {
            foreach (SyntaxNode node in root.DescendantNodesAndSelf())
            {
                if (node is TypeDeclarationSyntax type)
                {
                    CheckMemberSpacing(context, text, type.Members);
                }
                else if (node is NamespaceDeclarationSyntax space)
                {
                    CheckMemberSpacing(context, text, space.Members);
                }
                else if (node is CompilationUnitSyntax unit)
                {
                    CheckMemberSpacing(context, text, unit.Members);
                }
                else if (node is BlockSyntax block)
                {
                    CheckStatementSpacing(context, text, block.Statements);
                }
                else if (node is SwitchStatementSyntax selection)
                {
                    for (int sectionIndex = 1; sectionIndex < selection.Sections.Count; sectionIndex++)
                    {
                        RequireBlankLine(context, text, selection.Sections[sectionIndex - 1], selection.Sections[sectionIndex]);
                    }
                }
                else if (node is SwitchSectionSyntax section)
                {
                    CheckStatementSpacing(context, text, section.Statements);
                }
            }
        }

        private static void CheckMemberSpacing(SemanticModelAnalysisContext context, SourceText text,
            SyntaxList<MemberDeclarationSyntax> members)
        {
            for (int memberIndex = 1; memberIndex < members.Count; memberIndex++)
            {
                MemberDeclarationSyntax previous = members[memberIndex - 1];
                MemberDeclarationSyntax current = members[memberIndex];
                bool groupedFields = previous is FieldDeclarationSyntax && current is FieldDeclarationSyntax;
                bool groupedEvents = previous is EventFieldDeclarationSyntax && current is EventFieldDeclarationSyntax;
                bool groupedProperties = previous is PropertyDeclarationSyntax && current is PropertyDeclarationSyntax
                    && IsSingleLine(text, previous) && IsSingleLine(text, current);
                if (groupedFields == false && groupedEvents == false && groupedProperties == false)
                {
                    RequireBlankLine(context, text, previous, current);
                }
            }
        }

        private static bool IsSingleLine(SourceText text, SyntaxNode node)
        {
            return text.Lines.GetLineFromPosition(node.SpanStart).LineNumber
                == text.Lines.GetLineFromPosition(node.Span.End - 1).LineNumber;
        }

        private static void CheckStatementSpacing(SemanticModelAnalysisContext context, SourceText text,
            SyntaxList<StatementSyntax> statements)
        {
            for (int statementIndex = 1; statementIndex < statements.Count; statementIndex++)
            {
                StatementSyntax previous = statements[statementIndex - 1];
                StatementSyntax current = statements[statementIndex];
                bool separate = IsControlBlock(previous) || IsControlBlock(current);
                if (previous is LocalDeclarationStatementSyntax previousDeclaration
                    && current is LocalDeclarationStatementSyntax currentDeclaration)
                {
                    separate |= previousDeclaration.IsConst != currentDeclaration.IsConst;
                }
                else
                {
                    separate |= previous is LocalDeclarationStatementSyntax || current is LocalDeclarationStatementSyntax;
                }

                if (separate)
                {
                    RequireBlankLine(context, text, previous, current);
                }
            }
        }

        private static bool IsControlBlock(StatementSyntax statement)
        {
            return statement is IfStatementSyntax || statement is ForStatementSyntax
                || statement is ForEachStatementSyntax || statement is ForEachVariableStatementSyntax
                || statement is WhileStatementSyntax || statement is DoStatementSyntax
                || statement is SwitchStatementSyntax || statement is TryStatementSyntax
                || statement is UsingStatementSyntax || statement is LockStatementSyntax
                || statement is FixedStatementSyntax || statement is LocalFunctionStatementSyntax
                || statement is BlockSyntax;
        }

        private static void RequireBlankLine(SemanticModelAnalysisContext context, SourceText text,
            SyntaxNode previous, SyntaxNode current)
        {
            if (previous.IsMissing || current.IsMissing || previous.Span.IsEmpty || current.Span.IsEmpty)
            {
                return;
            }

            int previousLine = text.Lines.GetLineFromPosition(previous.Span.End - 1).LineNumber;
            int currentLine = text.Lines.GetLineFromPosition(current.SpanStart).LineNumber;
            for (int lineIndex = previousLine + 1; lineIndex < currentLine; lineIndex++)
            {
                if (string.IsNullOrWhiteSpace(text.Lines[lineIndex].ToString()))
                {
                    return;
                }
            }

            Report(context, 18, current.GetFirstToken().GetLocation());
        }

        private static void AnalyzeProperty(SemanticModelAnalysisContext context, PropertyDeclarationSyntax property)
        {
            if (property.Parent is InterfaceDeclarationSyntax || property.Modifiers.Any(SyntaxKind.AbstractKeyword)
                || property.Modifiers.Any(SyntaxKind.ExternKeyword))
            {
                return;
            }

            if (property.AccessorList != null)
            {
                bool autoProperty = true;
                foreach (AccessorDeclarationSyntax accessor in property.AccessorList.Accessors)
                {
                    if (accessor.Body != null || accessor.ExpressionBody != null
                        || accessor.SemicolonToken.IsKind(SyntaxKind.SemicolonToken) == false)
                    {
                        autoProperty = false;
                    }
                }

                if (autoProperty)
                {
                    return;
                }
            }

            ExpressionSyntax expression = property.ExpressionBody?.Expression;
            ISymbol symbol = null;
            if (expression != null)
            {
                symbol = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;
            }

            if ((symbol is IFieldSymbol) == false && (symbol is IPropertySymbol) == false)
            {
                Report(context, 5, property.Identifier.GetLocation());
            }
        }

        private static void AnalyzeSubscription(SemanticModelAnalysisContext context, AssignmentExpressionSyntax assignment)
        {
            ITypeSymbol type = context.SemanticModel.GetTypeInfo(assignment.Left, context.CancellationToken).Type;
            if (type?.TypeKind != TypeKind.Delegate)
            {
                return;
            }

            ExpressionSyntax expression = assignment.Right;
            while (expression is ParenthesizedExpressionSyntax || expression is CastExpressionSyntax)
            {
                if (expression is ParenthesizedExpressionSyntax parenthesized)
                {
                    expression = parenthesized.Expression;
                }
                else
                {
                    expression = ((CastExpressionSyntax)expression).Expression;
                }
            }

            if (expression is AnonymousFunctionExpressionSyntax)
            {
                Report(context, 8, expression.GetLocation());
                return;
            }

            ISymbol method = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;
            if (method is IMethodSymbol && method.Name.StartsWith("On", StringComparison.Ordinal) == false)
            {
                Report(context, 17, expression.GetLocation());
            }
        }

        private static void CheckShortName(SemanticModelAnalysisContext context, SyntaxToken token)
        {
            if (token.ValueText.Length == 1 && token.ValueText != "_")
            {
                Report(context, 9, token.GetLocation());
            }
        }

        private static bool IsContractField(SemanticModelAnalysisContext context, VariableDeclaratorSyntax variable)
        {
            IFieldSymbol field = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;
            if (field == null)
            {
                return false;
            }

            string type = field.ContainingType.ToDisplayString();
            return type == "YG.SavesYG" || type.EndsWith("YandexAdsBridge.LeaderboardPayload", StringComparison.Ordinal)
                || type.EndsWith("YandexAdsBridge.LeaderboardEntry", StringComparison.Ordinal)
                || context.SemanticModel.SyntaxTree.FilePath.Replace('\\', '/').EndsWith("/SavesYG.cs", StringComparison.Ordinal);
        }

        private static void AnalyzeField(SemanticModelAnalysisContext context, VariableDeclaratorSyntax variable)
        {
            IFieldSymbol field = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;
            if (field == null || IsContractField(context, variable))
            {
                return;
            }

            string name = field.Name;
            string pattern = null;
            if (field.IsConst)
            {
                if (name.Length == 0 || char.IsUpper(name[0]) == false)
                {
                    pattern = "PascalCase";
                }
            }
            else if (field.DeclaredAccessibility == Accessibility.Protected
                || field.DeclaredAccessibility == Accessibility.ProtectedOrInternal
                || field.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
            {
                if (name.Length == 0 || char.IsUpper(name[0]) == false)
                {
                    pattern = "PascalCase";
                }
            }
            else if (field.IsStatic)
            {
                if (name.StartsWith("s_", StringComparison.Ordinal) == false || name.Length < 3 || char.IsLower(name[2]) == false)
                {
                    pattern = "s_camelCase";
                }
            }
            else if (field.DeclaredAccessibility == Accessibility.Private)
            {
                if (name.StartsWith("_", StringComparison.Ordinal) == false || name.Length < 2 || char.IsLower(name[1]) == false)
                {
                    pattern = "_camelCase";
                }
            }

            if (pattern != null)
            {
                Report(context, 16, variable.Identifier.GetLocation(), name, pattern);
            }
        }

        private static void AnalyzeUnityEvent(SemanticModelAnalysisContext context, SyntaxNode node)
        {
            ISymbol symbol = context.SemanticModel.GetSymbolInfo(node, context.CancellationToken).Symbol;
            if (symbol is INamedTypeSymbol type && type.Name == "UnityEvent"
                && type.ContainingNamespace.ToDisplayString() == "UnityEngine.Events")
            {
                Report(context, 10, node.GetLocation());
            }
        }

        private static void CheckEventType(SemanticModelAnalysisContext context, TypeSyntax syntax)
        {
            ITypeSymbol type = context.SemanticModel.GetTypeInfo(syntax, context.CancellationToken).Type;
            if (type == null || type.TypeKind == TypeKind.Error)
            {
                return;
            }

            if (type.Name != "Action" || type.ContainingNamespace.ToDisplayString() != "System")
            {
                Report(context, 15, syntax.GetLocation());
            }
        }

        private static void AnalyzeInvocation(SemanticModelAnalysisContext context, InvocationExpressionSyntax invocation, string path)
        {
            IMethodSymbol method = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
            if (method == null)
            {
                return;
            }

            string type = method.ContainingType.ToDisplayString();
            string space = method.ContainingNamespace.ToDisplayString();
            bool forbidden = (type == "UnityEngine.GameObject" || type == "UnityEngine.Transform") && method.Name == "Find";
            forbidden |= type == "UnityEngine.Object" && method.Name == "FindObjectOfType";
            forbidden |= type == "UnityEngine.Debug" && method.Name == "Log" && path.IndexOf("/Editor/", StringComparison.Ordinal) < 0;
            forbidden |= space == "System.Reflection" || space.StartsWith("System.Reflection.", StringComparison.Ordinal);
            forbidden |= type == "System.Type" && (method.Name.StartsWith("GetField", StringComparison.Ordinal)
                || method.Name.StartsWith("GetMethod", StringComparison.Ordinal)
                || method.Name.StartsWith("GetPropert", StringComparison.Ordinal)
                || method.Name.StartsWith("GetMember", StringComparison.Ordinal));
            if (forbidden)
            {
                Report(context, 11, invocation.GetLocation(), type + "." + method.Name);
            }
        }

        private static void AnalyzeClasses(SemanticModelAnalysisContext context, List<ClassDeclarationSyntax> classes, string path)
        {
            if (classes.Count == 0)
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);
            ClassDeclarationSyntax main = null;
            foreach (ClassDeclarationSyntax declaration in classes)
            {
                if (declaration.Identifier.ValueText == fileName
                    || (declaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                        && fileName.StartsWith(declaration.Identifier.ValueText + ".", StringComparison.Ordinal)))
                {
                    main = declaration;
                }
            }

            if (main == null)
            {
                Report(context, 13, classes[0].Identifier.GetLocation());
            }

            if (classes.Count == 1)
            {
                return;
            }

            bool scriptableObject = false;
            INamedTypeSymbol mainType = null;
            if (main != null)
            {
                mainType = context.SemanticModel.GetDeclaredSymbol(main, context.CancellationToken);
            }
            for (INamedTypeSymbol type = mainType; type != null; type = type.BaseType)
            {
                if (type.ToDisplayString() == "UnityEngine.ScriptableObject")
                {
                    scriptableObject = true;
                }
            }

            foreach (ClassDeclarationSyntax declaration in classes)
            {
                if (declaration == main)
                {
                    continue;
                }

                INamedTypeSymbol type = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
                bool serializable = false;
                if (type != null)
                {
                    foreach (AttributeData attribute in type.GetAttributes())
                    {
                        if (attribute.AttributeClass?.ToDisplayString() == "System.SerializableAttribute")
                        {
                            serializable = true;
                        }
                    }
                }

                if (scriptableObject == false || serializable == false)
                {
                    Report(context, 14, declaration.Identifier.GetLocation());
                }
            }
        }

        private static void Report(SemanticModelAnalysisContext context, int number, Location location, params object[] arguments)
        {
            context.ReportDiagnostic(Diagnostic.Create(s_rules[number - 1], location, arguments));
        }
    }
}
