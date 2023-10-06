using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace Chronicle
{
    public static class TableScripts
    {
        public static Assembly Assembly = null;
        public static ObservableCollection<TableScriptSet> ScriptSets = new ObservableCollection<TableScriptSet>();

        static ObservableCollection<TableScript> currentGameScripts = new ObservableCollection<TableScript>();
        public static ObservableCollection<TableScript> CurrentGameScripts
        {
            get
            {
                return currentGameScripts;
            }
            set
            {
                lock (currentGameScriptsLock)
                {
                    currentGameScripts = value;
                }
                //OnPropertyChanged("CurrentGameScripts");
            }
        }
        static object currentGameScriptsLock = new object();

        static Dictionary<string, string> scriptsDict = new Dictionary<string, string>();
        static List<string> classNames = new List<string>();
        static string source = "";

        static bool initialized = false;

        public static void Initialize()
        {
            try
            {
                ScriptSets.Clear();
                classNames.Clear();
                scriptsDict.Clear();
                source = "";

                // Find all files
                var files = Directory.GetFiles("Script\\", "*.cs", SearchOption.AllDirectories);

                // Create script sets list
                foreach (var file in files)
                {
                    var setname = Path.GetFileName(Path.GetDirectoryName(file));
                    scriptsDict.Add(file, setname);
                    var currentScript = ScriptSets.FirstOrDefault(x => x.Name == setname);
                    if (currentScript == null)
                    {
                        currentScript = new TableScriptSet(setname, Path.GetDirectoryName(file));
                        ScriptSets.Add(currentScript);
                    }
                    currentScript.Files.Add(file);
                }

                // Check if the scripts should be enabled
                foreach (var script in ScriptSets)
                {
                    if (Core.Settings.DisabledScriptSets.Exists(x => x == script.Name))
                    {
                        script.IsEnabled = false;
                    }
                }

                OrderSetsList();

                foreach (var scriptset in ScriptSets)
                {
                    if (!scriptset.IsEnabled) continue;

                    foreach (var file in scriptset.Files)
                    {
                        try
                        {
                            string src = File.ReadAllText(file);

                            #region Remove usings
                            while (src.StartsWith("using"))
                            {
                                src = src.Remove(0, src.IndexOf("\n") + 1);
                            }
                            #endregion

                            #region Remove comments
                            int commentStart = -1;
                            while ((commentStart = src.IndexOf("//")) > 0)
                            {
                                src = src.Remove(commentStart, src.IndexOf("\n", commentStart) - commentStart);
                            }
                            #endregion

                            int classStart = 0;
                            string className = "";
                            string classType = "";
                            string classCode = "";
                            string parseCode = "";

                            while ((classStart = src.IndexOf("public class", classStart)) > -1)
                            {
                                var classNameLength = src.IndexOf(":", classStart + "public class".Length) - classStart - "public class".Length;
                                className = src.Substring(classStart + "public class".Length, classNameLength).Trim();
                                var classTypeLength = src.IndexOf("\n", classStart) - src.IndexOf(":", classStart) - 1;
                                classType = src.Substring(src.IndexOf(":", classStart) + 1, classTypeLength).Trim();

                                #region Save code
                                classCode = "";
                                var bodystart = src.IndexOf("public override void Parse", classStart);
                                if (bodystart < 1) continue;
                                int depth = 1;
                                bool str = false;
                                bool cmt = false;
                                int i = 0;

                                for (i = src.IndexOf("{", bodystart); i < src.Length && depth > 0; i++)
                                {
                                    if (src[i] == '\"' && !cmt) str = !str;
                                    if (str) continue;
                                    if (src[i] == '/' && src[i - 1] == '/') { cmt = true; continue; }
                                    if (src[i] == '\n') cmt = false;
                                    if (cmt) continue;
                                    if (src[i] == '{') depth++;
                                    if (src[i] == '}') depth--;
                                }

                                classCode = src.Substring(classStart, i - classStart + 1);
                                parseCode = src.Substring(bodystart, i - bodystart + 1);

                                #endregion

                                // At this point the following are set:
                                // className - name of the class
                                // classType - type of the class
                                // classCode - the class code body

                                if (classNames.Contains(className))
                                {
                                    Core.Log.Write("Duplicate table script omitted: <" + className + ">.");
                                    classStart += Math.Max(1, classCode.Length);
                                    continue;
                                }

                                var ts = new TableScript(className)
                                {
                                    Set = Path.GetFileName(Path.GetDirectoryName(file)),
                                    Type = classType,
                                    AssemblyPath = file,
                                    Code = parseCode,
                                    Game = Path.GetFileName(file).Substring(0, Path.GetFileName(file).IndexOf('.')).ToLowerInvariant()
                                };
                                classNames.Add(className);
                                Core.Log.Write("Table script imported: <" + className + ">.");

                                ts.Status = "Compiled";
                                scriptset.Scripts.Add(ts);
                                source += "[Serializable]\r\n" + classCode + "\r\n\r\n";
                                classStart += Math.Max(1, classCode.Length);
                            }

                            Core.Log.Write("Finished importing table scripts from: <" + file.Replace(Core.Paths.Application, "") + ">.");

                        }
                        catch (Exception ex)
                        {
                            Core.Log.ReportError("Error interpreting table scripts file: <" + file + ">.", ex);
                        }
                    }
                }

                initialized = true;
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error initializing table scripts.", ex);
            }
        }

        public static void RefreshStats()
        {
            Core.Dispatch.Run(() => Core.UI_Developer.lCompiledScripts.InvalidateVisual());
        }

        public static void SavePerformanceData(string set, string name, long time)
        {
            var script = CurrentGameScripts.FirstOrDefault(x => x.Set == set && x.Name == name);
            if (script == null)
            {
                if (set == "(built-in)") return;
                Core.Log.WriteWarning("Could not locate table script: " + name + ". Performance data could not be saved.");
                return;
            }
            script.Status = "Parsed";
            script.LastRunTime = time;

        }

        public static void SaveExceptionData(string set, string name, Exception ex)
        {
            var script = CurrentGameScripts.FirstOrDefault(x => x.Set == set && x.Name == name);
            if (script == null)
            {
                if (set == "(built-in)") return;
                Core.Log.WriteWarning("Could not locate table script: " + name + ". Exception data could not be saved.");
                return;
            }
            script.Errors.Add(ex);
        }

        static void OrderSetsList()
        {
            for (int i = 0; i < ScriptSets.Count; i++)
            {
                ScriptSets[i].PreferredOrder = Core.Settings.PreferredScriptSetsOrder.FindIndex(x => x == ScriptSets[i].Name);
            }
            ScriptSets.OrderBy(x => x.PreferredOrder);
        }

        public static void SaveOrder()
        {
            for (int i = 0; i < ScriptSets.Count; i++)
            {
                ScriptSets[i].PreferredOrder = i;
            }
        }

        public static void Compile()
        {
            try
            {
                if (!initialized) return;

                Directory.CreateDirectory(Core.Paths.DynamicCompile);

                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();

                parameters.ReferencedAssemblies.Add("Chronicle.exe");
                parameters.ReferencedAssemblies.Add("CEParser.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.GenerateInMemory = false;
                parameters.GenerateExecutable = false;

                // Set output DLL name
                parameters.OutputAssembly = Path.Combine(Core.Paths.DynamicCompile, "Chronicle.Dynamic.dll");
                int idx = 1;
                while (File.Exists(parameters.OutputAssembly))
                {
                    parameters.OutputAssembly = Path.Combine(Core.Paths.DynamicCompile, "Chronicle.Dynamic_" + idx.ToString() + ".dll");
                    idx++;
                }

                // Prepare source
                source = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nnamespace Chronicle\r\n{" + source + "\r\n}";

                CompilerResults results = provider.CompileAssemblyFromSource(parameters, source);

                #region Errors
                if (results.Errors.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (CompilerError error in results.Errors)
                    {
                        sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                    }

                    throw new SystemException(sb.ToString());
                }
                #endregion

                Assembly = results.CompiledAssembly;
            }
            catch (Exception ex)
            {
                Core.Log.ReportError("Error compiling table scripts.", ex);
            }
        }

        public static IEnumerable<TableScript> Get(string gameToken)
        {
            List<TableScript> output = new List<TableScript>();

            if (!initialized) return output;

            foreach (var ss in ScriptSets)
            {
                if (!ss.IsEnabled) continue;
                output.AddRange(ss.Scripts.FindAll(x => x.Game == gameToken));
            }

            return output;
        }

        public static void RefreshCurrentGameScripts(string token)
        {
            Core.Dispatch.Run(() => { CurrentGameScripts.Clear(); });
            var sets = ScriptSets.Where(x => x.IsEnabled);
            foreach (var set in sets)
            {
                foreach (var script in set.Scripts)
                {
                    if (script.Game == token)
                    {
                        Core.Dispatch.Run(() => { CurrentGameScripts.Add(script); });
                    }
                }
            }
            Core.Dispatch.Run(() => { Core.UI_Developer.lCompiledScripts.ItemsSource = CurrentGameScripts; });
        }
    }
}
