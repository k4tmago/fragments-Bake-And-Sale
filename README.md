# fragments-Bake-And-Sale
[`BakeryManager.cs`](fragments-Bake-And-Sale/BakeryManager.cs) to centralny kontroler logiki gry, który zarządza interakcjami gracza z otoczeniem oraz koordynuje pracę systemów produkcyjnych. Skrypt automatycznie aktualizuje asortyment pieczywa wraz z postępem dni i obsługuje dynamiczne UI dla punktów interakcji (lodówka, piec, śmietnik, kasa).

[`CustomerSpawner.cs`](fragments-Bake-And-Sale/CustomerSpawner.cs) odpowiada za generowanie klientów, ich losowy wygląd oraz logikę kolejki przy kasie. System zarządza zamówieniami (pojedyncze/podwójne) w oparciu o aktualny asortyment.
