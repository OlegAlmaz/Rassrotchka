using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Rassrotchka.ValidationFiles
{
	/// <summary>
	/// Осуществляет проверку даты решения на вхождение в интервал 
	///от -35 дней до +6 дней от текущей даты
	/// </summary>
	public class ValidDateDecis :ValidationRule
	{
		public int DateMin { get; set; }
		public int DateMax { get; set; }

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			DateTime dateDecis = DateTime.Now;
			try
			{
				var s = (string) value;
				if (s != null && s.Length > 0)
				{
					dateDecis = DateTime.Parse((string) value);

				}
			}
			catch (Exception e)
			{
				return new ValidationResult(false, "Недопустимый символ или " + e.Message);
			}
			var dateMin = DateTime.Now.AddDays(DateMin);
			var dateMax = DateTime.Now.AddDays(DateMax);
			if (dateDecis < dateMin || dateDecis > dateMax)
			{
					return new ValidationResult(false, "Возможно дата решения указана неверно!");
			}
			return ValidationResult.ValidResult;
		}
	}
}
