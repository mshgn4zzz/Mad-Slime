using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

internal class Program
{
    private static int s_failures;

    private static async Task Main(string[] arguments)
    {
        string repository = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../"));
        string assemblyPath = arguments.Contains("--installed")
            ? Path.Combine(repository, "Assets/MadSlime/Scripts/Assemblies/MadSlime.Core/Analyzers/MadSlime.CodeStyle.dll")
            : Path.Combine(AppContext.BaseDirectory, "MadSlime.CodeStyle.dll");
        Assembly assembly = Assembly.LoadFrom(assemblyPath);
        Type analyzerType = assembly.GetType("MadSlime.CodeStyle.ProjectStyleAnalyzer");
        ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray<DiagnosticAnalyzer>.Empty;
        if (analyzerType != null)
        {
            analyzers = ImmutableArray.Create((DiagnosticAnalyzer)Activator.CreateInstance(analyzerType));
        }

        await Check(analyzers, "var is a suggestion", "class Sample { void Run() { var count = 1; } }", "MS0001", true, DiagnosticSeverity.Info);
        await Check(analyzers, "explicit types accepted", "class Sample { void Run() { int count = 1; } }", "MS0001", false);
        await Check(analyzers, "boolean negation rejected", "class Sample { bool Run(bool ready) { return !ready; } }", "MS0002", true);
        await Check(analyzers, "explicit false comparison accepted", "class Sample { bool Run(bool ready) { return ready == false; } }", "MS0002", false);
        await Check(analyzers, "explicit true comparison accepted", "class Sample { bool Run(bool ready) { return true == ready; } }", "MS0002", false);
        await Check(analyzers, "negated TryGetComponent rejected", "class Sample { bool TryGetComponent(out object component) { component = null; return false; } void Awake() { if (!TryGetComponent(out object component)) { return; } } }", "MS0002", true);
        await Check(analyzers, "explicit TryGetComponent comparison accepted", "class Sample { bool TryGetComponent(out object component) { component = null; return false; } void Awake() { if (TryGetComponent(out object component) == false) { return; } } }", "MS0002", false);
        await Check(analyzers, "private Unity serialized naming accepted", "namespace UnityEngine { class SerializeField : System.Attribute {} } class Sample { [UnityEngine.SerializeField] private float _fovReturnSpeed; }", "MS0016", false);
        await Check(analyzers, "private Unity serialized camelCase rejected", "namespace UnityEngine { class SerializeField : System.Attribute {} } class Sample { [UnityEngine.SerializeField] private float fovReturnSpeed; }", "MS0016", true);
        await Check(analyzers, "nullable false comparison accepted", "class Sample { bool Run(bool? ready) { return ready == false; } }", "MS0002", false);
        await Check(analyzers, "nullable true comparison accepted", "class Sample { bool Run(bool? ready) { return ready == true; } }", "MS0002", false);
        await Check(analyzers, "ternary rejected", "class Sample { int Run(bool ready) { return ready ? 1 : 0; } }", "MS0003", true);
        await Check(analyzers, "coalesce rejected", "class Sample { string Run(string text) { return text ?? string.Empty; } }", "MS0004", true);
        await Check(analyzers, "coalesce assignment rejected", "class Sample { void Run(string text) { text ??= string.Empty; } }", "MS0004", true);
        await Check(analyzers, "event null conditional accepted", "class Sample { event System.Action Changed; void Run() { Changed?.Invoke(); } }", "MS0004", false);
        await Check(analyzers, "private setter auto property accepted", "class Sample { public int Count { get; private set; } }", "MS0005", false);
        await Check(analyzers, "get only auto property accepted", "class Sample { public int Count { get; } }", "MS0005", false);
        await Check(analyzers, "public setter auto property accepted", "class Sample { public int Count { get; set; } }", "MS0005", false);
        await Check(analyzers, "initialized auto property accepted", "class Sample { public int Count { get; private set; } = 1; }", "MS0005", false);
        await Check(analyzers, "explicit setter body rejected", "class Sample { private int _count; public int Count { get { return _count; } private set { _count = value; } } }", "MS0005", true);
        await Check(analyzers, "computed property rejected", "class Sample { private int _count; public int Count => _count * 2; }", "MS0005", true);
        await Check(analyzers, "field property accepted", "class Sample { private int _count; public int Count => _count; }", "MS0005", false);
        await Check(analyzers, "interface property accepted", "interface ISample { int Count { get; } }", "MS0005", false);
        await Check(analyzers, "expression method rejected", "class Sample { int Run() => 1; }", "MS0006", true);
        await Check(analyzers, "comments rejected", "class Sample { // text\n }", "MS0007", true);
        await Check(analyzers, "documentation comments rejected", "/// <summary>text</summary>\nclass Sample { }", "MS0007", true);
        await Check(analyzers, "lambda event subscription rejected", "class Sample { event System.Action Changed; void Run() { Changed += () => Run(); } }", "MS0008", true);
        await Check(analyzers, "delegate field subscription rejected", "class Sample { System.Action _changed; void Run() { _changed += delegate { Run(); }; } }", "MS0008", true);
        await Check(analyzers, "ordinary lambda accepted", "class Sample { void Run() { System.Action callback = () => Run(); } }", "MS0008", false);
        await Check(analyzers, "one letter local rejected", "class Sample { void Run() { int x = 1; } }", "MS0009", true);
        await Check(analyzers, "for counter accepted", "class Sample { void Run() { for (int i = 0; i < 2; i++) {} } }", "MS0009", false);
        await Check(analyzers, "i outside for rejected", "class Sample { void Run() { int i = 0; } }", "MS0009", true);
        await Check(analyzers, "one letter argument rejected", "class Sample { void Run(int x) {} }", "MS0009", true);
        await Check(analyzers, "UnityEvent rejected", "namespace UnityEngine.Events { class UnityEvent {} } class Sample { UnityEngine.Events.UnityEvent _changed; }", "MS0010", true);
        await Check(analyzers, "unrelated UnityEvent accepted", "class UnityEvent {} class Sample { UnityEvent _changed; }", "MS0010", false);
        await Check(analyzers, "GameObject.Find rejected", "namespace UnityEngine { class GameObject { public static void Find(string name) {} } } class Sample { void Run() { UnityEngine.GameObject.Find(\"Item\"); } }", "MS0011", true);
        await Check(analyzers, "unrelated Find accepted", "class Sample { void Find() {} void Run() { Find(); } }", "MS0011", false);
        await Check(analyzers, "runtime log rejected", "namespace UnityEngine { class Debug { public static void Log(string text) {} } } class Sample { void Run() { UnityEngine.Debug.Log(\"text\"); } }", "MS0011", true);
        await Check(analyzers, "Editor log accepted", "namespace UnityEngine { class Debug { public static void Log(string text) {} } } class Sample { void Run() { UnityEngine.Debug.Log(\"text\"); } }", "MS0011", false, path: "Assets/MadSlime/Scripts/Editor/Sample.cs");
        await Check(analyzers, "sealed and readonly accepted", "sealed class Sample { private readonly int _count; }", "MS0005", false);
        await Check(analyzers, "third party excluded", "class Sample { void Run() { var count = 1; } }", "MS0001", false, path: "Assets/Plugins/Sample.cs");
        await Check(analyzers, "generated input excluded", "class Sample { void Run() { var count = 1; } }", "MS0001", false, path: "Assets/MadSlime/Scripts/PlayerInput/Data/PlayerInputActions.cs");
        await Check(analyzers, "serialized keys excluded from short names", "class Sample { public int x; }", "MS0009", false, path: "Assets/MadSlime/Scripts/Saves/Data/SavesYG.cs");

        await Check(analyzers, "matching class filename", "class Sample {}", "MS0013", false);
        await Check(analyzers, "mismatched class filename", "class Other {}", "MS0013", true);
        await Check(analyzers, "extra class rejected", "class Sample {} class Other {}", "MS0014", true);
        await Check(analyzers, "SO companion accepted", "class Sample : UnityEngine.ScriptableObject {} [System.Serializable] class Entry {}", "MS0014", false);
        await Check(analyzers, "nonserializable SO companion rejected", "class Sample : UnityEngine.ScriptableObject {} class Entry {}", "MS0014", true);
        await Check(analyzers, "Action event accepted", "class Sample { event System.Action Changed; }", "MS0015", false);
        await Check(analyzers, "custom event rejected", "delegate void Callback(); class Sample { event Callback Changed; }", "MS0015", true);
        await Check(analyzers, "private field naming", "class Sample { private int count; }", "MS0016", true);
        await Check(analyzers, "static field naming", "class Sample { private static int _count; }", "MS0016", true);
        await Check(analyzers, "static readonly naming accepted", "class Sample { private static readonly int s_count; }", "MS0016", false);
        await Check(analyzers, "const naming accepted", "class Sample { private const int Count = 1; }", "MS0016", false);
        await Check(analyzers, "private readonly naming accepted", "class Sample { private readonly int _count; }", "MS0016", false);
        await Check(analyzers, "event handler naming", "class Sample { event System.Action Changed; void Run() { Changed += Run; } }", "MS0017", true);
        await Check(analyzers, "named On handler accepted", "class Sample { event System.Action Changed; void OnChanged() {} void Run() { Changed += OnChanged; } }", "MS0017", false);
        await Check(analyzers, "120 character line accepted", "class Sample {}" + new string(' ', 105), "MS0012", false);
        await Check(analyzers, "121 character line rejected", "class Sample {}" + new string(' ', 106), "MS0012", true);
        await Check(analyzers, "line limit", "class Sample { string _text = \"" + new string('a', 141) + "\"; }", "MS0012", true);
        await Check(analyzers, "actual type called var accepted", "class var {} class Sample { void Run() { var instance = new var(); } }", "MS0001", false);
        await Check(analyzers, "DTO contract naming accepted", "class YandexAdsBridge { public class LeaderboardEntry { private int x; } }", "MS0016", false);
        await Check(analyzers, "reflection rejected", "class Sample { void Run(System.Type type) { type.GetFields(); } }", "MS0011", true);

        await Check(analyzers, "protected PascalCase accepted", "class Sample { protected int Value; }", "MS0016", false);
        await Check(analyzers, "protected underscore rejected", "class Sample { protected int _value; }", "MS0016", true);
        await Check(analyzers, "protected static PascalCase accepted", "class Sample { protected static int Value; }", "MS0016", false);
        await Check(analyzers, "protected internal PascalCase accepted", "class Sample { protected internal int Value; }", "MS0016", false);
        await Check(analyzers, "private protected PascalCase accepted", "class Sample { private protected int Value; }", "MS0016", false);
        await Check(analyzers, "forwarded property accepted", "class Data { public int Count { get; } } class Sample { private Data _data; public int Count => _data.Count; }", "MS0005", false);
        await Check(analyzers, "partial filename accepted", "partial class Sample {}", "MS0013", false, path: "Assets/MadSlime/Scripts/Sample.Part.cs");
        await Check(analyzers, "single partial type is not an extra class", "partial class Sample {}", "MS0014", false, path: "Assets/MadSlime/Scripts/Sample.Part.cs");
        foreach (DiagnosticAnalyzer analyzer in analyzers)
        {
            foreach (DiagnosticDescriptor rule in analyzer.SupportedDiagnostics)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(rule.Title.ToString() + rule.MessageFormat.ToString(), "[А-Яа-яЁё]"))
                {
                    s_failures++;
                    Console.WriteLine($"FAIL: {rule.Id} must have an English diagnostic message.");
                }
            }
        }

        ImmutableArray<DiagnosticAnalyzer> styleCop = Assembly.LoadFrom(Path.Combine(repository, "Assets/MadSlime/Scripts/Assemblies/MadSlime.Core/Analyzers/StyleCop.Analyzers.dll"))
            .GetTypes().Where(type => typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && type.IsAbstract == false)
            .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)).ToImmutableArray();
        var ruleOptions = System.Xml.Linq.XDocument.Load(Path.Combine(repository,
                "Assets/MadSlime/Scripts/Assemblies/MadSlime.Core/MadSlime.CodeStyle.ruleset"))
            .Descendants("Rule").ToImmutableDictionary(element => element.Attribute("Id").Value,
                element => element.Attribute("Action").Value switch
                {
                    "Warning" => ReportDiagnostic.Warn,
                    "Info" => ReportDiagnostic.Info,
                    _ => ReportDiagnostic.Suppress
                });
        await CheckStyleCop(styleCop, ruleOptions, "field after method rejected", "class Sample { private void Run() {} private int _count; }", "SA1201", true, DiagnosticSeverity.Info);
        await CheckStyleCop(styleCop, ruleOptions, "Unity method before public logic is only a suggestion", "class Sample { private void Awake() {} public void Run() {} }", "SA1202", true, DiagnosticSeverity.Info);
        await CheckStyleCop(styleCop, ruleOptions, "constant after field rejected", "class Sample { private int _count; private const int Limit = 2; }", "SA1203", true, DiagnosticSeverity.Info);
        await CheckStyleCop(styleCop, ruleOptions, "static after instance field rejected", "class Sample { private int _count; private static int s_count; }", "SA1204", true, DiagnosticSeverity.Info);
        await CheckStyleCop(styleCop, ruleOptions, "valid member order accepted", "class Sample { private const int Limit = 2; private static int s_count; private int _count; public int Count => _count; public void Run() {} private void Stop() {} }", "SA1201", false);
        await CheckStyleCop(styleCop, ruleOptions, "private underscores accepted", "class Sample { private int _count; }", "SA1309", false);
        await CheckStyleCop(styleCop, ruleOptions, "static prefix accepted", "class Sample { private static readonly int s_count; }", "SA1311", false);
        await CheckStyleCop(styleCop, ruleOptions, "no documentation required", "public class Sample {}", "SA1600", false);
        await CheckStyleCop(styleCop, ruleOptions, "SO extra types allowed", "class Sample {} [System.Serializable] class Entry {}", "SA1402", false);
        await CheckStyleCop(styleCop, ruleOptions, "braces required", "class Sample { void Run(bool ready) { if (ready) return; } }", "SA1503", true);
        await CheckStyleCop(styleCop, ruleOptions, "multiline method required", "class Sample { void Run() {} }", "SA1502", true);

        await CheckStyleCop(styleCop, ruleOptions, "auto property can remain on one line", "public class Sample\n{\n    public int Value { get; private set; }\n}", "SA1502", false);

        await Check(analyzers, "adjacent fields form one block", "class Sample\n{\n    private int _first;\n    private string _second;\n}", "MS0018", false);
        await Check(analyzers, "adjacent single-line properties form one block", "class Sample\n{\n    public int First { get; private set; }\n    public int Second { get; private set; }\n}", "MS0018", false);
        await Check(analyzers, "field and property blocks need separation", "class Sample\n{\n    private int _count;\n    public int Count => _count;\n}", "MS0018", true);
        await Check(analyzers, "member blocks separated correctly", "class Sample\n{\n    private int _count;\n\n    public int Count => _count;\n\n    public void Run()\n    {\n    }\n}", "MS0018", false);
        await Check(analyzers, "methods need separation", "class Sample\n{\n    void First()\n    {\n    }\n    void Second()\n    {\n    }\n}", "MS0018", true);
        await Check(analyzers, "multiline property needs separation", "class Sample\n{\n    public int First { get; }\n    public int Second\n    {\n        get;\n        private set;\n    }\n}", "MS0018", true);
        await Check(analyzers, "constructor needs separation", "class Sample\n{\n    private int _count;\n    public Sample()\n    {\n    }\n}", "MS0018", true);
        await Check(analyzers, "classes need separation", "class Sample\n{\n}\nclass Other\n{\n}", "MS0018", true);
        await Check(analyzers, "if block needs leading separation", "class Sample\n{\n    void Run(bool ready)\n    {\n        int count = 0;\n        if (ready)\n        {\n            count++;\n        }\n    }\n}", "MS0018", true);
        await Check(analyzers, "if block needs trailing separation", "class Sample\n{\n    void Run(bool ready)\n    {\n        if (ready)\n        {\n            return;\n        }\n        Run(true);\n    }\n}", "MS0018", true);
        await Check(analyzers, "if else is one block", "class Sample\n{\n    void Run(bool ready)\n    {\n        if (ready)\n        {\n            Run(false);\n        }\n        else\n        {\n            Run(true);\n        }\n    }\n}", "MS0018", false);
        await Check(analyzers, "local declarations form one block", "class Sample\n{\n    void Run()\n    {\n        int first = 1;\n        int second = 2;\n    }\n}", "MS0018", false);
        await Check(analyzers, "constants and variables are separate blocks", "class Sample\n{\n    void Run()\n    {\n        const int Limit = 1;\n        int count = Limit;\n    }\n}", "MS0018", true);
        await Check(analyzers, "variables and operations are separate blocks", "class Sample\n{\n    void Run()\n    {\n        int count = 1;\n        Run();\n    }\n}", "MS0018", true);
        await Check(analyzers, "related calls can be grouped", "class Sample\n{\n    void Run()\n    {\n        Run();\n        Run();\n    }\n}", "MS0018", false);
        await Check(analyzers, "for block needs separation", "class Sample\n{\n    void Run()\n    {\n        Run();\n        for (int i = 0; i < 2; i++)\n        {\n            Run();\n        }\n    }\n}", "MS0018", true);
        await Check(analyzers, "while block needs separation", "class Sample\n{\n    void Run(bool ready)\n    {\n        Run(false);\n        while (ready)\n        {\n            return;\n        }\n    }\n}", "MS0018", true);
        await Check(analyzers, "switch sections need separation", "class Sample\n{\n    void Run(int value)\n    {\n        switch (value)\n        {\n            case 1:\n                break;\n            default:\n                break;\n        }\n    }\n}", "MS0018", true);
        await Check(analyzers, "stacked switch labels are one section", "class Sample\n{\n    void Run(int value)\n    {\n        switch (value)\n        {\n            case 1:\n            case 2:\n                break;\n\n            default:\n                break;\n        }\n    }\n}", "MS0018", false);
        await CheckStyleCop(styleCop, ruleOptions, "single-line property group is allowed", "public class Sample\n{\n    public int First { get; }\n    public int Second { get; }\n}", "SA1516", false);
        await CheckStyleCop(styleCop, ruleOptions, "multiple blank lines rejected", "public class Sample\n{\n    private int _first;\n\n\n    private int _second;\n}", "SA1507", true);
        await CheckStyleCop(styleCop, ruleOptions, "blank after opening brace rejected", "public class Sample\n{\n\n    private int _count;\n}", "SA1505", true);
        await CheckStyleCop(styleCop, ruleOptions, "blank before closing brace rejected", "public class Sample\n{\n    private int _count;\n\n}", "SA1508", true);

        Console.WriteLine($"Analyzer and StyleCop checks: {s_failures} failed.");
        Environment.ExitCode = s_failures == 0 ? 0 : 1;
    }

    private static async Task CheckStyleCop(ImmutableArray<DiagnosticAnalyzer> analyzers,
        ImmutableDictionary<string, ReportDiagnostic> options, string name, string source, string id, bool expected, DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, path: "Assets/MadSlime/Scripts/Sample.cs");
        CSharpCompilation compilation = CSharpCompilation.Create("Checks", new[] { tree, CSharpSyntaxTree.ParseText("namespace UnityEngine { public class ScriptableObject {} }", path: "Assets/Plugins/Stub.cs") },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithSpecificDiagnosticOptions(options));
        ImmutableArray<Diagnostic> diagnostics = await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
        if (diagnostics.Any(diagnostic => diagnostic.Id == id && diagnostic.Severity == severity
                && diagnostic.Location.SourceTree == tree) != expected
            || diagnostics.Any(diagnostic => diagnostic.Id == "AD0001"))
        {
            s_failures++;
            Console.WriteLine($"FAIL: {name}; expected {id}={expected}; got {string.Join(", ", diagnostics)}");
        }
    }

    private static async Task Check(ImmutableArray<DiagnosticAnalyzer> analyzers, string name, string source,
        string id, bool expected, DiagnosticSeverity severity = DiagnosticSeverity.Warning,
        string path = "Assets/MadSlime/Scripts/Sample.cs")
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, path: path);
        CSharpCompilation compilation = CSharpCompilation.Create("Checks", new[] { tree, CSharpSyntaxTree.ParseText("namespace UnityEngine { public class ScriptableObject {} }", path: "Assets/Plugins/Stub.cs") },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        ImmutableArray<Diagnostic> diagnostics = analyzers.IsEmpty
            ? ImmutableArray<Diagnostic>.Empty
            : await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
        Diagnostic match = diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == id);
        bool passed = expected ? match != null && match.Severity == severity : match == null;
        if (diagnostics.Any(diagnostic => diagnostic.Id == "AD0001"))
        {
            passed = false;
        }

        if (passed == false)
        {
            s_failures++;
            Console.WriteLine($"FAIL: {name}; expected {id}={expected}/{severity}; got {string.Join(", ", diagnostics)}");
        }
    }
}
