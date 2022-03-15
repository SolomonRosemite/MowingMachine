using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MowingMachine.Models;

namespace MowingMachine.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class Benchy
{
    private List<Field>? _fieldsAsList;
    private IEnumerable<Field>? _fieldsAsEnumerable;
    
    [GlobalSetup]
    public void GlobalSetup()
    {
        _fieldsAsList = Enumerable.Range(1, 300).Select(size => new Field(FieldType.Grass, new Offset(size, size))).ToList();
        _fieldsAsEnumerable = Enumerable.Range(1, 300).Select(size => new Field(FieldType.Grass, new Offset(size, size)));
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(Offsets))]
    public void FirstOrDefaultLinqList(Offset target)
    {
        _fieldsAsList.FirstOrDefault(f => f.Offset == target);
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(Offsets))]
    public void FirstOrDefaultForLoopList(Offset target)
    {
        for (int i = 0; i < _fieldsAsList.Count; i++)
        {
            if (_fieldsAsList[i].Offset == target)
            {
                break;
            }
        }
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(Offsets))]
    public void FirstOrDefaultForEachList(Offset target)
    {
        foreach (var field in _fieldsAsList)
        {
            if (field.Offset == target)
            {
                break;
            }
        }
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(Offsets))]
    public void FirstOrDefaultLinqEnumerable(Offset target)
    {
        _fieldsAsEnumerable.FirstOrDefault(f => f.Offset == target);
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(Offsets))]
    public void FirstOrDefaultForLoopListEnumerable(Offset target)
    {
        using var enumerator = _fieldsAsEnumerable.GetEnumerator();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Offset == target)
            {
                break;
            }
        }
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(Offsets))]
    public void FirstOrDefaultForEachListEnumerable(Offset target)
    {
        foreach (var field in _fieldsAsEnumerable)
        {
            if (field.Offset == target)
            {
                break;
            }
        }
    }
    
    public IEnumerable<Offset> Offsets()
    {
        yield return new Offset(-5, -5);
        yield return new Offset(21, 21);
        yield return new Offset(126, 126);
        yield return new Offset(183, 183);
        yield return new Offset(262, 262);
        yield return new Offset(301, 301);
    }
}