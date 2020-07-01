using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rassrotchka
{
    public class FilterClass
    {
        #region Поля фильтра
        private string _close;
        private string _kod_GNI;
        private string _kod_Payer;
        private string _name;
        private readonly string[] _arrCond = new[]
                                        {
                                            "",//_close     0
                                            "",//_kod_GNI   1
                                            "",//_kod_Payer 2
                                            ""//_name       3
                                        };
        #endregion

        #region Свойства
        public string Close
        {
            get => _close;
            set
            {
                if (_close != value)
                {
                    _close = value;
                    _arrCond[0] = _close != string.Empty ? $"Close = {_close}" : string.Empty;
                }
            }
        }

        public string Kod_GNI
        {
            get => _kod_GNI;
            set
            {
                if (_kod_GNI != value)
                {
                    _kod_GNI = value;
                    _arrCond[1] = _kod_GNI != string.Empty ? $"Kod_GNI = {_kod_GNI}" : string.Empty;
                }
            }
        }

        public string Kod_Payer
        {
            get => _kod_Payer; set
            {
                if (_kod_Payer != value)
                {
                    _kod_Payer = value;
                    _arrCond[2] = _kod_Payer != string.Empty ? $"Kod_Payer = {_kod_Payer}" : string.Empty;
                }
            }
        }

        public string Name
        {
            get => _name;
            set 
            { if (_name != value)
                {
                    _name = value;
                    _arrCond[3] = _name != string.Empty ? $"Name LIKE %{_name}%" : string.Empty;
                }
            }
        }

        public string Filter { get; set; }
        #endregion
        /// <summary>
        /// Возвращает строку фильтра
        /// </summary>
        /// <returns>строка фильтра</returns>
        public string FilterString()
        {
            Filter = string.Empty;
            int cound = 1;//todo уточнить, возможно ошибка с местом инициализации
            for (int i = 0; i < _arrCond.Length; i++)
            {
                if (_arrCond[i] != string.Empty)
                {
                    if (cound == 1)
                        Filter = _arrCond[i];
                    else
                        Filter += $" AND {_arrCond[i]}";
                    cound++;
                }
            }
            return Filter;
        }
    }
}