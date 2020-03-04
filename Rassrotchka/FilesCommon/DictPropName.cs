using System.Collections.Generic;

namespace Rassrotchka.FilesCommon
{
	public class DictPropName : Dictionary<string, string>
	{
		public DictPropName()
		{
			Add("0", "Id_dpg");
			Add("1", "Kod_GNI");
			Add("2", "Name");
			Add("3", "Kod_Payer");
			Add("4", "Date_Decis");
			Add("5", "Numb_Decis");
			Add("51", "GniOrGKNS");
			Add("6", "Summa_Decis");
			Add("7", "Kod_Paying");
			Add("8", "Date_first");
			Add("9", "Date_end");
			Add("10", "Count_Mount");
			Add("11", "Summa_Payer");
			Add("111", "Date_prolong");
			Add("12", "Type_Decis");
			Add("13", "Note");
		}
	}
}