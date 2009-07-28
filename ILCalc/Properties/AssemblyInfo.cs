using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;

[assembly: AssemblyTitle("ILCalc")]
[assembly: AssemblyDescription("Arithmetical expressions compiler and evaluator.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Pelmen Software")]
[assembly: AssemblyProduct("ILCalc")]
[assembly: AssemblyCopyright("Shvedov A. V. © 2008-2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: System.CLSCompliant(true)]

[assembly: AssemblyVersion("0.9.5.1")]

#if !CF

[assembly: AssemblyFileVersion("0.9.5.1")]
[assembly: AssemblyFlags(AssemblyNameFlags.EnableJITcompileOptimizer)]

[assembly: NeutralResourcesLanguage("en",
	UltimateResourceFallbackLocation.MainAssembly)]

#else

[assembly: NeutralResourcesLanguage("en")]

#endif
