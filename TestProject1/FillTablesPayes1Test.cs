﻿using Rassrotchka.FilesCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject1
{
    
    
    /// <summary>
    ///Это класс теста для FillTablesPayes1Test, в котором должны
    ///находиться все модульные тесты FillTablesPayes1Test
    ///</summary>
	[TestClass()]
	public class FillTablesPayes1Test
	{


		private TestContext testContextInstance;

		/// <summary>
		///Получает или устанавливает контекст теста, в котором предоставляются
		///сведения о текущем тестовом запуске и обеспечивается его функциональность.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Дополнительные атрибуты теста
		// 
		//При написании тестов можно использовать следующие дополнительные атрибуты:
		//
		//ClassInitialize используется для выполнения кода до запуска первого теста в классе
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//TestInitialize используется для выполнения кода перед запуском каждого теста
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//TestCleanup используется для выполнения кода после завершения каждого теста
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///Тест для GetPay
		///</summary>
		[TestMethod()]
		public void GetPayTest()
		{
			DateTime dateFirst = new DateTime(2020, 3, 5); //
			DateTime dateEnd = new DateTime(2020, 3, 31);
			int expected = 1; // TODO: инициализация подходящего значения
			int actual = FillTablesPayes1.GetPay(dateFirst, dateEnd);
			Assert.AreEqual(expected, actual);
		}
	}
}
