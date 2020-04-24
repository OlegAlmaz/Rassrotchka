using System.Windows.Input;

namespace Rassrotchka.Commands
{
	public class DataCommands
	{
		public static RoutedCommand Edit { get; set; }
		public static RoutedCommand Delete { get; set; }
		public static RoutedCommand UndoAll { get; set; }
		public static RoutedCommand Update { get; set; }
		public static RoutedCommand Download { get; set; }
		public static RoutedCommand FillData { get; set; }
		public static RoutedCommand Clean { get; set; }

		static DataCommands()
		{
			var inputs = new InputGestureCollection();
			inputs.Add(new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E"));
			Edit = new RoutedCommand("Edit", typeof(DataCommands), inputs);
			
			inputs = new InputGestureCollection();
			inputs.Add(new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D"));
			Delete = new RoutedCommand("Delete", typeof(DataCommands), inputs);

			inputs = new InputGestureCollection();
			inputs.Add(new KeyGesture(Key.R, ModifierKeys.Control, "Ctrl+R"));
			UndoAll = new RoutedCommand("UndoAll", typeof(DataCommands), inputs);

			inputs = new InputGestureCollection();
			inputs.Add(new KeyGesture(Key.Down, ModifierKeys.Control, "Ctrl+PageDown"));
			Update = new RoutedCommand("Update", typeof(DataCommands), inputs);

			inputs = new InputGestureCollection();
			inputs.Add(new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl+Q"));
			Download = new RoutedCommand("Download", typeof(DataCommands), inputs);

			inputs = new InputGestureCollection();
			inputs.Add(new KeyGesture(Key.B, ModifierKeys.Control, "Ctrl+B"));
			FillData = new RoutedCommand("FillData", typeof(DataCommands), inputs);

			inputs = new InputGestureCollection();
			inputs.Add(new KeyGesture(Key.J, ModifierKeys.Control, "Ctrl+J"));
			Clean = new RoutedCommand("Clean", typeof(DataCommands), inputs);
		}

	}
}
