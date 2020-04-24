namespace Rassrotchka
{
	/// <summary>
	/// Содержит имена таблиц, хранимых процедур и функций базы данных
	/// </summary>
	public static class BaseElementName
	{
		/// <summary>
		/// Процедура формирует отчет на 20 число для руководства о прогнозе поступлений
		/// по рассрочкам и отсрочкам в разрезе платежей для руководства
		/// </summary>
		public static string ProcedGeNPayFromPlatej = "ProcedGeNPayFromPlatej";

		/// <summary>
		/// Процедура формирует отчет на 1 число о прогнозе поступлений
		/// по рассрочкам и отсрочкам в разрезе плательщиков и платежей
		/// </summary>
		public static string ProcedPredicGenPay = "ProcedPredicGenPay";
	}
}
