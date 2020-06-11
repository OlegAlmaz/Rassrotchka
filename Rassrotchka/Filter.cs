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
        private short _kod_GNI;
        private long _kod_Payer;
        private string _name;
        #endregion

        private string _filter;
        private string[] _arrCond = new[]
                                        {
                                            "",//_close
                                            "",//_kod_GNI
                                            "",//_kod_Payer
                                            ""//_name
                                        };

        private const string _whereStr = "WHERE ";

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

        public short Kod_GNI { get => _kod_GNI; set => _kod_GNI = value; }

        public long Kod_Payer
        {
            get => _kod_Payer; set
            {
                if (_kod_Payer != value)
                {
                    _kod_Payer = value;
                    _arrCond[2] = _kod_Payer != 0 ? $"Kod_Payer = {_kod_Payer}" : string.Empty;
                }
            }
        }

        public string Name { get => _name; set => _name = value; }
        public string Filter { get => _filter; private set => _filter = value; }

        private void Onchanged()
        {
            _filter = string.Empty;
            int ind = 0;//todo уточнить, возможно ошибка с местом инициализации
            for (int i = 0; i < _arrCond.Length; i++)
            {
                if (_arrCond[i] != string.Empty)
                {
                    if (ind == 0)
                        _filter = $"WHERE {_arrCond[i]}";
                    else
                        _filter += $" AND {_arrCond[i]}";
                }
                ind++;
            }
        }

    }
}
