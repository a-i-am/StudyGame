using UnityEngine;

public abstract class BaseView<TViewModel> : MonoBehaviour where TViewModel : BaseViewModel
{
protected TViewModel viewModel;
    
    // 이벤트 구독 해제를 자동화하기 위한 객체
    protected DisposableBag disposables = new DisposableBag();

    // View가 생성될 때 ViewModel을 주입받는 함수
    public virtual void Bind(TViewModel vm)
    {
        viewModel = vm;
    }

    // 오브젝트가 파괴될 때 자동으로 이벤트를 구독 해제하여 메모리 누수 방지
    protected virtual void OnDestroy()
    {
        disposables.Dispose(); 
        
        if (viewModel != null)
        {
            viewModel.Dispose();
        }
    }
}