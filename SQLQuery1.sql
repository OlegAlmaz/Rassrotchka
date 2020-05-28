SELECT     Id_dpg, Kod_GNI, Name, Kod_Payer, Date_Decis, Numb_Decis, GniOrGKNS, Summa_Decis, Kod_Paying, Date_first, Date_end, Count_Mount, 
                      Summa_Payer, Type_Decis, Date_prolong, Note
FROM         DebitPayGen
WHERE Kod_GNI IN (1, 2, 3)