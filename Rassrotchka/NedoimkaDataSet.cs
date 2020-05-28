namespace Rassrotchka.NedoimkaDataSetTableAdapters {
	partial class MonthPayTableAdapter
	{
		public string SelectedCommandText
		{
			get { return CommandCollection[0].CommandText; }
			set { CommandCollection[0].CommandText = value; }
		}
	}
    
    
    public partial class DebitPayGenTableAdapter {
	    public string SelectedCommandText
	    {
		    get { return CommandCollection[0].CommandText; }
		    set { CommandCollection[0].CommandText = value; }
	    }
    }
}
namespace Rassrotchka
{


	public partial class NedoimkaDataSet
	{
	}
}
