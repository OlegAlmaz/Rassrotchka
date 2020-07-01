using System.Collections.Generic;

namespace Rassrotchka
{
	public class MonthName
	{
		/// <summary>
		/// Название месяца в именительном падеже
		/// </summary>
		public string Imenit;

		/// <summary>
		/// Название месяца в родительном падеже
		/// </summary>
		public string Roditel;

		public MonthName(string imenit, string rodit)
		{
			Imenit = imenit;
			Roditel = rodit;
		}

		/// <summary>
		/// Число месяца, например 03
		/// </summary>
		public string Numb { get; set; }
	}


	public class DictMonth : Dictionary<int, MonthName>
	{
		public DictMonth()
		{
			Add(1, new MonthName("январь", "январе"));
			Add(2, new MonthName("февраль", "феврале"));
			Add(3, new MonthName("март", "марте"));
			Add(4, new MonthName("апрель", "апреле"));
			Add(5, new MonthName("май", "мае"));
			Add(6, new MonthName("июнь", "июне"));
			Add(7, new MonthName("июль", "июле"));
			Add(8, new MonthName("август", "августе"));
			Add(9, new MonthName("сентябь", "сентябре"));
			Add(10, new MonthName("октябрь", "октябре"));
			Add(11, new MonthName("ноябрь", "ноябре"));
			Add(12, new MonthName("декабрь", "декабре"));
		}
	}
}