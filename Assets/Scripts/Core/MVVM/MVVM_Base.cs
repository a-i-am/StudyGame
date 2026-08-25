using System;
using UnityEngine;
public interface IModel
{

}

public abstract class BaseViewModel : IDisposable
{
    public abstract void Dispose();
}

public abstract class BaseView<TViewModel> : MonoBehaviour where TViewModel : BaseViewModel
{
    protected TViewModel viewModel;

    public virtual void Bind(TViewModel vm)
    {
        viewModel = vm;
    }

    protected virtual void OnDestroy()
    {
        if (viewModel != null)
        {
            viewModel.Dispose();
        }
    }
}