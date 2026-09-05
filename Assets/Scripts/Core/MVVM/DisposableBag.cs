using System;
using System.Collections.Generic;

public class DisposableBag : IDisposable
{
    private List<IDisposable> _disposables = new List<IDisposable>();

    public void Add(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    public void Dispose()
    {
        // 바구니에 담긴 모든 스위치의 Dispose()를 실행하여 합니다.
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();
    }
}

// 편의성을 위한 확장 메서드 (체이닝 문법용)
public static class DisposableExtensions
{
    public static void AddTo(this IDisposable disposable, DisposableBag bag)
    {
        bag.Add(disposable);
    }
}