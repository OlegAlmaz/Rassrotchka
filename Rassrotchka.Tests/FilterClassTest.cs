// <copyright file="FilterClassTest.cs" company="Microsoft">Copyright © Microsoft 2020</copyright>
using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rassrotchka;

namespace Rassrotchka.Tests
{
    /// <summary>Этот класс содержит параметризованные модульные тесты для FilterClass</summary>
    [PexClass(typeof(FilterClass))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class FilterClassTest
    {
        /// <summary>Тестовая заглушка для FilterString()</summary>
        [PexMethod]
        public string FilterStringTest([PexAssumeUnderTest]FilterClass target)
        {
            string result = target.FilterString();
            return result;
            // TODO: добавление проверочных утверждений в метод FilterClassTest.FilterStringTest(FilterClass)
        }
    }
}
