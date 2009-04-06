﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ILCalc {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ILCalc.Misc.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ambiguous match between.
        /// </summary>
        internal static string errAmbiguousMatch {
            get {
                return ResourceManager.GetString("errAmbiguousMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument &quot;{0}&quot; already exist in the list..
        /// </summary>
        internal static string errArgumentExist {
            get {
                return ResourceManager.GetString("errArgumentExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Constant &quot;{0}&quot; already exist in the list..
        /// </summary>
        internal static string errConstantExist {
            get {
                return ResourceManager.GetString("errConstantExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Constant &quot;{0}&quot; does not exist in collection..
        /// </summary>
        internal static string errConstantNotExist {
            get {
                return ResourceManager.GetString("errConstantNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t extract culture-specified separators chars..
        /// </summary>
        internal static string errCultureExtract {
            get {
                return ResourceManager.GetString("errCultureExtract", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Brace disbalance (not closed)..
        /// </summary>
        internal static string errDisbalanceClose {
            get {
                return ResourceManager.GetString("errDisbalanceClose", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Brace disbalance (not opened)..
        /// </summary>
        internal static string errDisbalanceOpen {
            get {
                return ResourceManager.GetString("errDisbalanceOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong range step: endless loop..
        /// </summary>
        internal static string errEndlessLoop {
            get {
                return ResourceManager.GetString("errEndlessLoop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is overload(s) with {0} argument(s)..
        /// </summary>
        internal static string errExistOverload {
            get {
                return ResourceManager.GetString("errExistOverload", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Opening brace should be after function name.
        /// </summary>
        internal static string errFunctionNoBrace {
            get {
                return ResourceManager.GetString("errFunctionNoBrace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Empty or null identifier name..
        /// </summary>
        internal static string errIdentifierEmpty {
            get {
                return ResourceManager.GetString("errIdentifierEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incorrect identifier name: &quot;{0}&quot;. Name should begin with a letter or underscore..
        /// </summary>
        internal static string errIdentifierStartsWith {
            get {
                return ResourceManager.GetString("errIdentifierStartsWith", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incorrect symbol &apos;{0}&apos; in identifier name &quot;{1}&quot;. Only letters, digits and underscore symbol are allowed..
        /// </summary>
        internal static string errIdentifierSymbol {
            get {
                return ResourceManager.GetString("errIdentifierSymbol", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incorrect construction.
        /// </summary>
        internal static string errIncorrectConstr {
            get {
                return ResourceManager.GetString("errIncorrectConstr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid (separator) usage..
        /// </summary>
        internal static string errInvalidSeparator {
            get {
                return ResourceManager.GetString("errInvalidSeparator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to parameter #{0} of &apos;{1}&apos; type. All parameters should be of &apos;{2}&apos; type.
        /// </summary>
        internal static string errMethodBadParam {
            get {
                return ResourceManager.GetString("errMethodBadParam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to returns value of &apos;{0}&apos; type. Function should return &apos;{1}&apos; type.
        /// </summary>
        internal static string errMethodBadReturn {
            get {
                return ResourceManager.GetString("errMethodBadReturn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Method &quot;{0}&quot; cannot be founded (method non public?)..
        /// </summary>
        internal static string errMethodNotFounded {
            get {
                return ResourceManager.GetString("errMethodNotFounded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to should be standard runtime method (DynamicMethod is not supported)..
        /// </summary>
        internal static string errMethodNotRuntimeMethod {
            get {
                return ResourceManager.GetString("errMethodNotRuntimeMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to must be static.
        /// </summary>
        internal static string errMethodNotStatic {
            get {
                return ResourceManager.GetString("errMethodNotStatic", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to parameter #{0} is optional parameter. Optional parameters are not supported.
        /// </summary>
        internal static string errMethodParamOptional {
            get {
                return ResourceManager.GetString("errMethodParamOptional", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to parameter #{0} is out parameter. Out parameters are not supported.
        /// </summary>
        internal static string errMethodParamOut {
            get {
                return ResourceManager.GetString("errMethodParamOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to with same parameters is already in the list.
        /// </summary>
        internal static string errMethodSameParams {
            get {
                return ResourceManager.GetString("errMethodSameParams", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Culture &apos;{0}&apos; is neutral culture. It cannot be used for parse and formatting ..
        /// </summary>
        internal static string errNeutralCulture {
            get {
                return ResourceManager.GetString("errNeutralCulture", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incorrect number format:.
        /// </summary>
        internal static string errNumberFormat {
            get {
                return ResourceManager.GetString("errNumberFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Number constant overflow:.
        /// </summary>
        internal static string errNumberOverflow {
            get {
                return ResourceManager.GetString("errNumberOverflow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong range: Begin, End or Step is not finite value..
        /// </summary>
        internal static string errRangeNotFinite {
            get {
                return ResourceManager.GetString("errRangeNotFinite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tabulator object can be created only for the expression with one or two arguments..
        /// </summary>
        internal static string errTabulatorWrongArgs {
            get {
                return ResourceManager.GetString("errTabulatorWrongArgs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong range: iterations count doesn&apos;t fit in integer values range..
        /// </summary>
        internal static string errTooLongRange {
            get {
                return ResourceManager.GetString("errTooLongRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolved identifier:.
        /// </summary>
        internal static string errUnresolvedIdentifier {
            get {
                return ResourceManager.GetString("errUnresolvedIdentifier", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolved symbol:.
        /// </summary>
        internal static string errUnresolvedSymbol {
            get {
                return ResourceManager.GetString("errUnresolvedSymbol", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong arguments count: {0}. You should specify {1} argument(s) for the Evaluate() method..
        /// </summary>
        internal static string errWrongArgsCount {
            get {
                return ResourceManager.GetString("errWrongArgsCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to isn&apos;t got overload with {0} argument(s)..
        /// </summary>
        internal static string errWrongOverload {
            get {
                return ResourceManager.GetString("errWrongOverload", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong ranges count: {0}. You should specify {1} ranges(s) for the Tabulate() method..
        /// </summary>
        internal static string errWrongRangesCount {
            get {
                return ResourceManager.GetString("errWrongRangesCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong range step: invalid step sign..
        /// </summary>
        internal static string errWrongStepSign {
            get {
                return ResourceManager.GetString("errWrongStepSign", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to and.
        /// </summary>
        internal static string sAnd {
            get {
                return ResourceManager.GetString("sAnd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument.
        /// </summary>
        internal static string sArgument {
            get {
                return ResourceManager.GetString("sArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Constant.
        /// </summary>
        internal static string sConstant {
            get {
                return ResourceManager.GetString("sConstant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Function.
        /// </summary>
        internal static string sFunction {
            get {
                return ResourceManager.GetString("sFunction", resourceCulture);
            }
        }
    }
}