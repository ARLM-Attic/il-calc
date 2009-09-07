using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
  public sealed class SerializationTests
  {
    #region Initialization

    readonly CalcContext<double> calc;

    public SerializationTests()
    {
      this.calc = new CalcContext<double>("x");

      Calc.Culture = CultureInfo.CurrentCulture;

      Calc.Constants.Add("pi", Math.PI);
      Calc.Constants.Add("e", Math.E);
      Calc.Constants.Add("fi", 1.234);

      Calc.Functions.ImportBuiltIn();
    }

    CalcContext<double> Calc
    {
      get { return this.calc; }
    }

    #endregion
    #region SerializationTests

    [TestMethod]
    public void InterpretSerializeTest()
    {
      const int Count = 10000;

      var gen = new ExprGenerator(Calc);
      var list1 = new List<double>();
      var list2 = new List<double>();
      var binFormatter = new BinaryFormatter
      {
        AssemblyFormat = FormatterAssemblyStyle.Simple,
        FilterLevel = TypeFilterLevel.Low
      };

      using (var tempMem = new MemoryStream())
      {
        foreach (string expr in gen.Generate(Count))
        {
          var a = Calc.CreateInterpret(expr);

          binFormatter.Serialize(tempMem, a);
          list1.Add(a.Evaluate(1.23));
        }

        tempMem.Position = 0;
        for (int i = 0; i < Count; i++)
        {
          var b = (Interpret<double>)
            binFormatter.Deserialize(tempMem);

          list2.Add(b.Evaluate(1.23));
        }
      }

      CollectionAssert.AreEqual(list1, list2);
    }

    [TestMethod]
    public void ContextSerializeTest()
    {
      var binFormatter = new BinaryFormatter
      {
        AssemblyFormat = FormatterAssemblyStyle.Simple,
        FilterLevel = TypeFilterLevel.Low
      };

      using (var tempMem = new MemoryStream())
      {
        var range1 = new ValueRange<double>(1, 200, 1.50);
        var exception1 = new SyntaxException("hehe");
        var exception2 = new InvalidRangeException("wtf?");

        binFormatter.Serialize(tempMem, Calc);
        binFormatter.Serialize(tempMem, range1);
        binFormatter.Serialize(tempMem, exception1);
        binFormatter.Serialize(tempMem, exception2);

        tempMem.Position = 0;

        var other = (CalcContext<double>) binFormatter.Deserialize(tempMem);

        Assert.AreEqual(Calc.Arguments.Count, other.Arguments.Count);
        Assert.AreEqual(Calc.Constants.Count, other.Constants.Count);
        Assert.AreEqual(Calc.Functions.Count, other.Functions.Count);
        Assert.AreEqual(Calc.OverflowCheck, other.OverflowCheck);
        Assert.AreEqual(Calc.Optimization, other.Optimization);
        Assert.AreEqual(Calc.IgnoreCase, other.IgnoreCase);
        Assert.AreEqual(Calc.Culture, other.Culture);

        var range2 = (ValueRange<double>)
          binFormatter.Deserialize(tempMem);

        Assert.AreEqual(range1, range2);

        var exception1D = (SyntaxException)
          binFormatter.Deserialize(tempMem);

        var exception2D = (InvalidRangeException)
          binFormatter.Deserialize(tempMem);

        Assert.AreEqual(exception1.Message, exception1D.Message);
        Assert.AreEqual(exception2.Message, exception2D.Message);
      }
    }

    #endregion
  }
}
