// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project. 
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc. 
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File". 
// You do not need to add suppressions to this file manually. 

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "disbalance", Scope = "resource", Target = "ILCalc.Resource.resources")]

[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "ILCalc.Evaluator.#Evaluate0")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "ILCalc.Evaluator.#Evaluate1")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "ILCalc.Evaluator.#Evaluate2")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "ILCalc.Evaluator.#EvaluateN")]

[assembly: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "ILCalc.Evaluator.#Evaluate0")]
[assembly: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "ILCalc.Evaluator.#Evaluate1")]
[assembly: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "ILCalc.Evaluator.#Evaluate2")]
[assembly: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member", Target = "ILCalc.Evaluator.#EvaluateN")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "ILCalc.FunctionGroup")]

[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "ILCalc.Parser.#AmbiguousMatch(System.Int32,System.Collections.Generic.ICollection`1<ILCalc.Parser+Capture>)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "ILCalc.Parser.#IncorrectConstr(ILCalc.Parser+Item,ILCalc.Parser+Item,System.Int32)")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "ILCalc.TabRange")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type", Target = "ILCalc.TabRange+Enumerator")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a", Scope = "member", Target = "ILCalc.TabRange.#op_Equality(ILCalc.TabRange,ILCalc.TabRange)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b", Scope = "member", Target = "ILCalc.TabRange.#op_Equality(ILCalc.TabRange,ILCalc.TabRange)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a", Scope = "member", Target = "ILCalc.TabRange.#op_Inequality(ILCalc.TabRange,ILCalc.TabRange)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b", Scope = "member", Target = "ILCalc.TabRange.#op_Inequality(ILCalc.TabRange,ILCalc.TabRange)")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval", Scope = "type", Target = "ILCalc.EvalFunc0")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval", Scope = "type", Target = "ILCalc.EvalFunc1")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval", Scope = "type", Target = "ILCalc.EvalFunc2")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval", Scope = "type", Target = "ILCalc.EvalFuncN")]