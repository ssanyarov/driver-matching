# DriverMatching

Сетка N×M, в каждой клетке максимум один водитель.  
Найти 5 ближайших водителей к заказу (X, Y).

Реализации:
- BruteForceFinder
- RingGridFinder
- KdTreeFinder

## Tests
```bash
dotnet test
```

## Benchmarks
```bash
dotnet run -c Release --project bench/DriverMatching.Benchmarks
```

## Results
![Benchmark](screenshots/benchmark.png)
