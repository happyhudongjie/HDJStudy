using UnityEngine;
using UnityEngine.Events;


[AddComponentMenu("NGUI/Interaction/SkillButton")]
public class SkillUIButton : UIButton
{
    public bool UseSwipeChangeTarget = false;
    public bool UseSwipChangeSkill = false;
    //点击释放时 点击的位置信息 -1表示在范围内 0表示出范围 1 表示左下角 2 表示右上角 
    int SwipedOutType = -1;
    //事件
    public class OnSwipeOutClass : UnityEvent<int> { };
    public class OnPressedHandler : UnityEvent{ }
    public class OnClickHandler : UnityEvent { }
    public OnSwipeOutClass OnSwipeOut  = new OnSwipeOutClass();
    public OnPressedHandler OnBtnPressed = new OnPressedHandler();
    public OnPressedHandler OnBtnUp = new OnPressedHandler();
    public OnClickHandler OnBtnClick= new OnClickHandler();

    protected override void OnEnable()
    {
        base.OnEnable();
        AddPressed();
    }

    protected override void OnDragOut()
    {
        if (isEnabled && (dragHighlight || UICamera.currentTouch.pressed == gameObject))
        {
            if (UICamera.currentTouch != null)
            {
                if (UseSwipChangeSkill)
                {
                    //切换技能
                    //Debug.LogError(" OnSwipeOut.Invoke(0);");
                    SwipedOutType = 0;
                }
                if (UseSwipeChangeTarget)
                {
                    if (UICamera.currentTouch.delta.x < 0 && UICamera.currentTouch.delta.y < 0)
                    {
                        //左下角
                        //Debug.LogError(" OnSwipeOut.Invoke(1);");
                        SwipedOutType = 1;
                    }
                    else if (UICamera.currentTouch.delta.x > 0 && UICamera.currentTouch.delta.y > 0)
                    {
                        //右上角
                        //Debug.LogError(" OnSwipeOut.Invoke(2);");
                        SwipedOutType = 2;
                    }
                }
            }
        }
    }

    protected override void OnClick()
    {
        base.OnClick();
        OnBtnClick.Invoke();
    }

    public void AddPressed()
    {
        UIEventListener listener = UIEventListener.Get(gameObject);
        if (listener == null)
        {
            Debug.LogError("there is no UIEventListener on " + gameObject.name);
            return;
        }
        listener.onPress = delegate (GameObject o, bool pressed)
        {
            OnPressed(pressed);
        };
    }

    public void OnPressed(bool pressed)
    {
        if (pressed)
        {
            OnBtnPressed.Invoke();
        }
        else
        {
            if (SwipedOutType != -1)
            {
                OnSwipeOut.Invoke(SwipedOutType);
                SwipedOutType = -1;
            }
            else
            {
                OnBtnUp.Invoke();
            }
        }
    }
}
