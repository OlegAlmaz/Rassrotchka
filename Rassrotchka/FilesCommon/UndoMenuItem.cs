using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Rassrotchka
{
	/// <summary>
	/// Класс, который аккумулирует изменения в строках таблицы
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class UndoMenuItem<T> : INotifyPropertyChanged
	{
		/// <summary>
		/// Позволяет быть активным элементу управления
		/// </summary>
		public bool IsEnabled
		{
			get { return _isEnabled; }
			private set
			{
				_isEnabled = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
				}
			}
		}

		/// <summary>
		/// Количество элементов в списки изменений для последующей отмены
		/// </summary>
		public int Count { get; private set; }

		public List<T> List
		{
			get { return _list; }
			set { _list = value; }
		}

		private List<T> _list;
		private bool _isEnabled;

		public UndoMenuItem()
		{
			List = new List<T>();
			Count = 0;
			IsEnabled = false;
		}

		public void Add(T t)
		{
			if (t != null)
			{
				_list.Add(t);
				Count = _list.Count;
				IsEnabled = true;
			}
		}

		/// <summary>
		/// удаление элемента из списка по индексу
		/// </summary>
		/// <param name="i">номер индекса удаляемого элемента списка</param>
		public void RemoveAt(int i)
		{
			if (i <= Count && i >= 0)
			{
				_list.RemoveAt(i);
				Count = _list.Count;
				if (Count == 0)
					IsEnabled = false;
			}
			else
				throw new ArgumentOutOfRangeException("Индекс вне диапазона. Количество элементов списка - " + Count);
		}

		public void Remote(T t)
		{
			if (t != null)
			{
				_list.Remove(t);
				Count = _list.Count;
				if (Count == 0)
					IsEnabled = false;
			}
		}

		internal void Clear()
		{
			List.Clear();
			Count = 0;
			IsEnabled = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}