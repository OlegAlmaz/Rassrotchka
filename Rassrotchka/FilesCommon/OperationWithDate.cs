using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rassrotchka.FilesCommon
{
	/// <summary>
	/// Предоставляет методы для обработки дат
	/// </summary>
    public static class OperationWithDate
    {
		/// <summary>
		/// Получает количество месяцев между датами, включая первый и последний месяц
		/// </summary>
		/// <param name="dateFirst">первая дата</param>
		/// <param name="dateEnd">последняя дата</param>
		/// <returns>количество месяцев (платежей в случае рассрочки), знак минус означает, что первый параметр (дата) больше (старше) второго</returns>
		public static int GetPay(DateTime dateFirst, DateTime dateEnd)
		{
			int deltaYar = dateEnd.Year - dateFirst.Year;
			int deltaMonth = dateEnd.Month - dateFirst.Month;
			int paysCount;
			if (dateFirst <= dateEnd)
				paysCount = deltaMonth + (deltaYar * 12) + 1;
			else
				paysCount = deltaMonth + (deltaYar * 12) - 1;
			return paysCount;
		}
	}
}
